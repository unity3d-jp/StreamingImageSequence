#include "ImageCollection.h"

//Loader
#include "ImageMemoryAllocator.h"
#include "LoaderConstants.h"

//External
#include "CommonLib/CommonLib.h" //IMAGE_CS
#include "CommonLib/CriticalSectionController.h"
#include "External/stb/stb_image_resize.h"


namespace StreamingImageSequencePlugin {


ImageCollection::ImageCollection()
    : m_memAllocator(nullptr)
    , m_curOrderStartPos(m_orderedImageList.end())
    , m_updateOrderStartPos(false)
    , m_csType(CRITICAL_SECTION_TYPE_FULL_IMAGE)
    , m_latestRequestFrame(0)
{

}
//----------------------------------------------------------------------------------------------------------------------

ImageCollection::~ImageCollection() {
    UnloadAllImagesUnsafe();
}


//----------------------------------------------------------------------------------------------------------------------
void ImageCollection::Init(CriticalSectionType csType, ImageMemoryAllocator* memAllocator) {
    m_csType       = csType;
    m_memAllocator = memAllocator;
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

//----------------------------------------------------------------------------------------------------------------------


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

    stbir_resize_uint8(src->RawData, src->Width, src->Height, 0,
        resizedImageData.RawData, w, h, 0, LoaderConstants::NUM_BYTES_PER_TEXEL);

    //Register to map
    resizedImageData.CurrentReadStatus = READ_STATUS_SUCCESS;
    pathIt->second = resizedImageData;

    return true;

}

//----------------------------------------------------------------------------------------------------------------------

//Non-Thread-safe
void ImageCollection::SetImageStatus(const strType& imagePath, const ReadStatus status) {

    //No need to sync. One imagePath should be processed by only one job
    //CriticalSectionController cs(IMAGE_CS(m_csType)); 

    ASSERT(m_pathToImageMap.find(imagePath) != m_pathToImageMap.end());
    ImageData& imageData = m_pathToImageMap.at(imagePath);
    imageData.CurrentReadStatus = status;

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
void ImageCollection::UnloadAllImages() {
    CriticalSectionController cs(IMAGE_CS(m_csType));
    UnloadAllImagesUnsafe();
}

//----------------------------------------------------------------------------------------------------------------------

//Thread-safe
void ImageCollection::ResetOrder() {
    CriticalSectionController cs(IMAGE_CS(m_csType));

    m_curOrderStartPos = m_orderedImageList.begin();
    m_updateOrderStartPos = false;
    m_latestRequestFrame = 0;
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


//----------------------------------------------------------------------------------------------------------------------
//returns true if one or more images are successfully unloaded
bool ImageCollection::UnloadUnusedImageUnsafe(const strType& imagePathToAllocate) {
    std::list<std::unordered_map<strType, ImageData>::iterator>::iterator orderIt = m_orderedImageList.begin();
    if (m_curOrderStartPos == orderIt)
        return false;

    const strType& imagePath = (*orderIt)->first;

    //if this happens, then we can't even allocate memory for one single image
    //ASSERT(imagePath != imagePathToAllocate);
    if (imagePath == imagePathToAllocate)
        return false;

    //Do processes inside UnloadImage((*orderIt)->first), without any checks
    ImageData* imageData = &(*orderIt)->second;

    m_memAllocator->Deallocate(imageData);
    m_orderedImageList.erase(orderIt);
    m_pathToOrderMap.erase(imagePath);
    m_pathToImageMap.erase(imagePath);

    return true;

}


} //end namespace

