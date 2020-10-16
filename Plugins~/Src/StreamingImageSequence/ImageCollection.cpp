#include "StreamingImageSequence/ImageCollection.h"

//Loader
#include "StreamingImageSequence/ImageMemoryAllocator.h"
#include "LoaderConstants.h"

//External
#include "CommonLib/CommonLib.h" //IMAGE_CS
#include "CommonLib/CriticalSectionController.h"


namespace StreamingImageSequencePlugin {

void* g_resizeBuffer[MAX_CRITICAL_SECTION_TYPE_IMAGES] = { nullptr };
size_t g_resizeBufferSize[MAX_CRITICAL_SECTION_TYPE_IMAGES];

//[TODO-sin: 2020-10-15] This is a hack. Change to a singleton
ImageMemoryAllocator* g_memAllocator = nullptr;

//Not thread safe
void* GetOrAllocateResizeBufferUnsafe(size_t memSize, void* context) {
    const uint32_t imageType = * (static_cast<uint32_t*>(context));
    ASSERT(imageType < MAX_CRITICAL_SECTION_TYPE_IMAGES);

    if (memSize <=g_resizeBufferSize[imageType]) {
        return g_resizeBuffer[imageType];
    }

    void* newResizeBuffer = g_memAllocator->Reallocate(g_resizeBuffer[imageType], memSize, /*forceAllocate=*/ true);
    if (nullptr != newResizeBuffer) {
        g_resizeBuffer[imageType] = newResizeBuffer;
        g_resizeBufferSize[imageType] = memSize;
    }

    return g_resizeBuffer[imageType];   
}

//not thread safe
inline void* AllocateImageRawData(size_t newSize) {
    return g_memAllocator->Allocate(newSize);
}

//not thread safe
inline void* ReallocateImageRawData(void* buffer, size_t newSize) {
    return g_memAllocator->Reallocate(buffer, newSize, /*forceAllocate = */ true);
}

//not thread safe
inline void FreeImageRawData(void* buffer) {
    g_memAllocator->Deallocate(buffer);
}


#define STB_IMAGE_IMPLEMENTATION
#define STBI_MALLOC(sz)           AllocateImageRawData(sz)
#define STBI_REALLOC(p,newsz)     ReallocateImageRawData(p,newsz)
#define STBI_FREE(p)              FreeImageRawData(p)
#define STBI_NO_JPEG
#include "stb/stb_image.h"

//----------------------------------------------------------------------------------------------------------------------


ImageCollection::ImageCollection()
    : m_memAllocator(nullptr)
    , m_curOrderStartPos(m_orderedImageList.end())
    , m_updateOrderStartPos(false)
    , m_csType(CRITICAL_SECTION_TYPE_FULL_IMAGE)
    , m_latestRequestFrame(0)
{
    stbi_set_flip_vertically_on_load(true);
}
//----------------------------------------------------------------------------------------------------------------------

ImageCollection::~ImageCollection() {
    ResetAllUnsafe();
}


//----------------------------------------------------------------------------------------------------------------------
void ImageCollection::Init(CriticalSectionType csType, ImageMemoryAllocator* memAllocator) {
    m_csType       = csType;
    m_memAllocator = memAllocator;

    //Allocate global resize buffer for this csType (imageType)
    if (nullptr==g_resizeBuffer[m_csType]) {
        g_resizeBufferSize[m_csType] = 1;
        g_resizeBuffer[m_csType] = m_memAllocator->Allocate(g_resizeBufferSize[m_csType]);
        g_memAllocator = memAllocator;
    }
}

//----------------------------------------------------------------------------------------------------------------------

//Thread-safe
const ImageData* ImageCollection::GetImage(const strType& imagePath, const int frame) {
    CriticalSectionController cs(IMAGE_CS(m_csType));

    UpdateRequestFrameUnsafe(frame);

    std::unordered_map<strType, ImageData>::iterator pathIt = m_pathToImageMap.find(imagePath);
    if (m_pathToImageMap.end() == pathIt) {
        return nullptr;
    }

    //Check if we have to update the order start pos first
    if (m_updateOrderStartPos) {
        MoveOrderStartPosToEndUnsafe();
    }

    const bool isForCurrentOrder = (frame >= m_latestRequestFrame);
    if (isForCurrentOrder) {
        ReorderImageUnsafe(pathIt);
    }

    return &pathIt->second;
}

//----------------------------------------------------------------------------------------------------------------------
//Thread-safe. May return null
const ImageData* ImageCollection::AddImage(const strType& imagePath, const int frame) {
    
    CriticalSectionController cs(IMAGE_CS(m_csType));

    UpdateRequestFrameUnsafe(frame);
    if (frame < m_latestRequestFrame)
        return nullptr;

    std::unordered_map<strType, ImageData>::iterator pathIt = m_pathToImageMap.find(imagePath);
    if (pathIt !=m_pathToImageMap.end())
        return &pathIt->second;

    pathIt = AddImageUnsafe(imagePath);
    return &pathIt->second;

}

//----------------------------------------------------------------------------------------------------------------------

//Thread-safe
const ImageData* ImageCollection::AllocateImage(const strType& imagePath, const uint32_t w, const uint32_t h) {

    CriticalSectionController cs(IMAGE_CS(m_csType));

    std::unordered_map<strType, ImageData>::iterator pathIt = m_pathToImageMap.find(imagePath);

    //Unload existing memory if it exists
    if (m_pathToImageMap.end() != pathIt) {
        m_memAllocator->Deallocate(&(pathIt->second));
    }  else {
        pathIt = AddImageUnsafe(imagePath);
    }

    ImageData* imageData = &pathIt->second;

    const bool isAllocated = AllocateRawDataUnsafe(&imageData->RawData, w, h, imagePath);
    if (!isAllocated)
        return nullptr;

    imageData->Width = w;
    imageData->Height = h;

    return imageData;
}

const ImageData* ImageCollection::LoadImage(const strType& imagePath) {
    CriticalSectionController cs(IMAGE_CS(m_csType));

    std::unordered_map<strType, ImageData>::iterator pathIt = m_pathToImageMap.find(imagePath);

    //Unload existing memory if it exists
    if (m_pathToImageMap.end() != pathIt) {
        m_memAllocator->Deallocate(&(pathIt->second));
    }  else {
        pathIt = AddImageUnsafe(imagePath);
    }

    ImageData* imageData = &pathIt->second;
    ReadStatus readStatus = LoadImageIntoUnsafe(imagePath, imageData);

    //unload old memory if failed to load
    bool unloadSuccessful = true;
    while (READ_STATUS_OUT_OF_MEMORY == readStatus && unloadSuccessful) {
        unloadSuccessful = UnloadUnusedImageUnsafe(imagePath);
        readStatus = LoadImageIntoUnsafe(imagePath, imageData);
    }

    return imageData;

}


//----------------------------------------------------------------------------------------------------------------------


//External/stb
#define STB_IMAGE_RESIZE_IMPLEMENTATION
#define STBIR_DEFAULT_FILTER_DOWNSAMPLE  STBIR_FILTER_CATMULLROM

//We free the resize buffer once when the program ends.
#define STBIR_MALLOC(size,context) GetOrAllocateResizeBufferUnsafe(size, context)
#define STBIR_FREE(ptr,context) 

#include "stb/stb_image_resize.h"

bool ImageCollection::AddImageFromSrc(const strType& imagePath, const int frame, const ImageData* src, 
                                           const uint32_t w, const uint32_t h)
{
    CriticalSectionController cs(IMAGE_CS(m_csType));

    UpdateRequestFrameUnsafe(frame);
    if (frame < m_latestRequestFrame)
        return false;

    auto pathIt = m_pathToImageMap.find(imagePath);

    //Unload existing memory if it exists
    if (m_pathToImageMap.end() != pathIt) {
        m_memAllocator->Deallocate(&(pathIt->second));
    }  else {
        pathIt = AddImageUnsafe(imagePath);
    }

    //Allocate
    ImageData resizedImageData(nullptr,w,h,READ_STATUS_LOADING);
    const bool isAllocated = AllocateRawDataUnsafe(&resizedImageData.RawData, w, h, imagePath);
    if (!isAllocated)
        return false;

    ASSERT(nullptr != src->RawData);
    ASSERT(READ_STATUS_SUCCESS == src->CurrentReadStatus);

    //stbir_resize_uint8(src->RawData, src->Width, src->Height, 0,
    //    resizedImageData.RawData, w, h, 0, LoaderConstants::NUM_BYTES_PER_TEXEL);

    stbir__resize_arbitrary(/*STBIR_MALLOC context = */ &m_csType, src->RawData, src->Width, src->Height, 0,
                            resizedImageData.RawData, w, h, 0,
                            0,0,1,1,nullptr, LoaderConstants::NUM_BYTES_PER_TEXEL,-1,0, 
                            STBIR_TYPE_UINT8, STBIR_FILTER_DEFAULT, STBIR_FILTER_DEFAULT,
                            STBIR_EDGE_CLAMP, STBIR_EDGE_CLAMP, STBIR_COLORSPACE_LINEAR);

    //Register to map
    resizedImageData.CurrentReadStatus = READ_STATUS_SUCCESS;
    resizedImageData.Format = src->Format;
    pathIt->second = resizedImageData;

    return true;
}

#undef STBIR_MALLOC
#undef STBIR_FREE

//----------------------------------------------------------------------------------------------------------------------

//Non thread-safe. Not required, because one imagePath should be processed by only one job
void ImageCollection::SetImageStatus(const strType& imagePath, const ReadStatus status) {

    ASSERT(m_pathToImageMap.find(imagePath) != m_pathToImageMap.end());
    ImageData& imageData = m_pathToImageMap.at(imagePath);
    imageData.CurrentReadStatus = status;

}

//Non thread-safe. Not required, because one imagePath should be processed by only one job
void ImageCollection::SetImageFormat(const strType& imagePath, const ImageFormat format) {
    ASSERT(m_pathToImageMap.find(imagePath) != m_pathToImageMap.end());
    ImageData& imageData = m_pathToImageMap.at(imagePath);
    imageData.Format = format;
}


//----------------------------------------------------------------------------------------------------------------------

//Thread-safe
bool ImageCollection::UnloadImage(const strType& imagePath) {
    CriticalSectionController cs(IMAGE_CS(m_csType));

    const std::unordered_map<strType,ImageData>::iterator it = m_pathToImageMap.find(imagePath);
    if (m_pathToImageMap.end() == it)
        return false;

    m_memAllocator->Deallocate(&(it->second));
    DeleteImageOrderUnsafe(it);
    m_pathToImageMap.erase(imagePath);
    return true;
}

//----------------------------------------------------------------------------------------------------------------------

//Thread-safe
void ImageCollection::ResetAll() {
    CriticalSectionController cs(IMAGE_CS(m_csType));
    ResetAllUnsafe();
}

//----------------------------------------------------------------------------------------------------------------------


//Thread-safe
void ImageCollection::ResetOrder() {
    CriticalSectionController cs(IMAGE_CS(m_csType));
    ResetOrderUnsafe();
}


//----------------------------------------------------------------------------------------------------------------------

std::unordered_map<strType, ImageData>::iterator ImageCollection::AddImageUnsafe(const strType& imagePath) {
    ASSERT(m_pathToImageMap.find(imagePath) == m_pathToImageMap.end());
    const auto it = m_pathToImageMap.insert({ imagePath, ImageData(nullptr,0,0,READ_STATUS_LOADING) });
    AddImageOrderUnsafe(it.first);

    ASSERT(m_orderedImageList.size() >= 1);
    if (m_curOrderStartPos == m_orderedImageList.end() || m_updateOrderStartPos) {
        MoveOrderStartPosToEndUnsafe();
    }

    return it.first;
}

//----------------------------------------------------------------------------------------------------------------------

void ImageCollection::AddImageOrderUnsafe(std::unordered_map<strType, ImageData>::iterator pathToImageIt) {
    auto orderIt = m_orderedImageList.insert(m_orderedImageList.end(), pathToImageIt);
    m_pathToOrderMap.insert({pathToImageIt->first, orderIt});
}

//----------------------------------------------------------------------------------------------------------------------

void ImageCollection::ReorderImageUnsafe(std::unordered_map<strType, ImageData>::iterator pathToImageIt) {
    const auto pathToOrderIt = m_pathToOrderMap.find(pathToImageIt->first);
    if (m_pathToOrderMap.end() == pathToOrderIt) {
        return;
    }

    //Make sure the startPos of the current "order" (frame) is still valid
    bool invalidStartPos = false;
    if (pathToOrderIt->second == m_curOrderStartPos) {
        ++m_curOrderStartPos;
        invalidStartPos = (m_curOrderStartPos == m_orderedImageList.end());
    }

    //Move to the end
    m_orderedImageList.splice( m_orderedImageList.end(), m_orderedImageList, pathToOrderIt->second);

    if (invalidStartPos) {
        MoveOrderStartPosToEndUnsafe();
    }
}

//----------------------------------------------------------------------------------------------------------------------


//Non-thread safe: remove the linked data in m_orderedImageList
void ImageCollection::DeleteImageOrderUnsafe(std::unordered_map<strType, ImageData>::iterator pathToImageIt) {
    //remove the linked data in the ordering structures
    const auto pathToOrderIt = m_pathToOrderMap.find(pathToImageIt->first);
    if (m_pathToOrderMap.end() == pathToOrderIt) {
        return;
    }

    const auto orderIt = pathToOrderIt->second;

    if (orderIt == m_curOrderStartPos) {
        ++m_curOrderStartPos;
    }
    m_orderedImageList.erase(orderIt);
    m_pathToOrderMap.erase(pathToOrderIt);


    //make sure the start pos is valid
    if (m_curOrderStartPos == m_orderedImageList.end() && !m_pathToOrderMap.empty()) {
        --m_curOrderStartPos;
    }

}

//----------------------------------------------------------------------------------------------------------------------

//Non-thread safe
void ImageCollection::MoveOrderStartPosToEndUnsafe() {
    ASSERT(m_orderedImageList.size() >= 1);
    m_curOrderStartPos = m_orderedImageList.end();
    --m_curOrderStartPos;
    m_updateOrderStartPos = false;

}

//----------------------------------------------------------------------------------------------------------------------

//Non-thread safe
void ImageCollection::UnloadAllImagesUnsafe() {
    m_pathToOrderMap.clear();
    m_orderedImageList.clear();
    m_curOrderStartPos = m_orderedImageList.end();

    for (auto itr = m_pathToImageMap.begin(); itr != m_pathToImageMap.end(); ++itr) {
        ImageData* imageData = &(itr->second);
        m_memAllocator->Deallocate(imageData);
    }
    m_pathToImageMap.clear();
}

void ImageCollection::ResetAllUnsafe() {
    UnloadAllImagesUnsafe();
    ResetOrderUnsafe();

    m_memAllocator->Deallocate(g_resizeBuffer[m_csType]);
    g_resizeBuffer[m_csType] = nullptr;
    g_resizeBufferSize[m_csType] = 0;
}


void ImageCollection::ResetOrderUnsafe() {
    m_curOrderStartPos = m_orderedImageList.begin();
    m_updateOrderStartPos = false;
    m_latestRequestFrame = 0;
}

//----------------------------------------------------------------------------------------------------------------------

//Non-Thread-safe
void ImageCollection::UpdateRequestFrameUnsafe(const int frame) {


    if (frame <= m_latestRequestFrame) {
        //overflow check
        const bool isOverflow = frame < 0 && m_latestRequestFrame >= 0;
        if (!isOverflow) {
            return;
        }
    }

    m_latestRequestFrame = frame;

    //Turn on the flag, so that at the next GetImage() or AddImage(), 
    //the related image would be the start pos of the current "order".
    //The prev nodes before this start pos, would be regarded as "unused" for this order, and thus safe to be unloaded
    m_updateOrderStartPos = true;
}

//----------------------------------------------------------------------------------------------------------------------

//Non-thread safe
bool ImageCollection::AllocateRawDataUnsafe(uint8_t** rawData,const uint32_t w,const uint32_t h,const strType& imagePath) 
{

    bool isAllocated = false;
    bool unloadSuccessful = true;
    while (!isAllocated && unloadSuccessful) {
        isAllocated = m_memAllocator->Allocate(rawData, w, h);
        if (!isAllocated) {
            unloadSuccessful = UnloadUnusedImageUnsafe(imagePath);
        }
    }

    return isAllocated;
}

ReadStatus ImageCollection::LoadImageIntoUnsafe(const strType& imagePath, ImageData* targetImageData) {

    int width, height, numComponents;
    unsigned char *data = stbi_load(imagePath.c_str(), &width, &height, &numComponents, /*requiredComponents = */4);
    if (nullptr!=data) {
        *targetImageData = ImageData(data, width, height, READ_STATUS_SUCCESS);
        targetImageData->Format = IMAGE_FORMAT_RGBA32;
    } else {
        if (0 == strcmp(stbi_failure_reason(),"outofmem")) {
            targetImageData->CurrentReadStatus = READ_STATUS_OUT_OF_MEMORY;
        } else {
            targetImageData->CurrentReadStatus = READ_STATUS_FAIL;
        }
    }
    return targetImageData->CurrentReadStatus;
}

//----------------------------------------------------------------------------------------------------------------------

//Frees up memory in order to allocate memory for imagePathContext.
//returns true if one or more images are successfully unloaded
bool ImageCollection::UnloadUnusedImageUnsafe(const strType& imagePathContext) {
    const std::list<std::unordered_map<strType, ImageData>::iterator>::iterator orderIt = m_orderedImageList.begin();
    if (m_curOrderStartPos == orderIt)
        return false;

    const strType& imagePath = (*orderIt)->first;

    //if this happens, then we can't even allocate memory for one single image
    //ASSERT(imagePath != imagePathContext);
    if (imagePath == imagePathContext)
        return false;

    //Do processes inside UnloadImage((*orderIt)->first), without any checks
    ImageData* imageData = &(*orderIt)->second;

    m_memAllocator->Deallocate(imageData);
    m_orderedImageList.erase(orderIt);
    m_pathToOrderMap.erase(imagePath);
    m_pathToImageMap.erase(imagePath);

    return true;

}

//----------------------------------------------------------------------------------------------------------------------


//undef STBI definitions
#undef STBI_MALLOC
#undef STBI_REALLOC
#undef STBI_FREE
#undef STBI_NO_JPEG

} //end namespace

