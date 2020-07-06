#include "ImageCollection.h"

//Loader
#include "ImageMemoryAllocator.h"
#include "LoaderConstants.h"

//External
#include "CommonLib/CommonLib.h" //IMAGE_CS
#include "CommonLib/CriticalSectionController.h"
#include "External/stb/stb_image_resize.h"


namespace StreamingImageSequencePlugin {


ImageCollection::ImageCollection(CriticalSectionType csType)
    : m_curOrderStartPos(m_orderedImageList.end())
    , m_updateOrderStartPos(false)
    , m_csType( csType)
{

}

//----------------------------------------------------------------------------------------------------------------------

//Thread-safe
const ImageData* ImageCollection::GetImage(const strType& imagePath, const bool isForCurrentOrder) {
    CriticalSectionController cs(IMAGE_CS(m_csType));
    std::map<strType, ImageData>::iterator pathIt = m_pathToImageMap.find(imagePath);
    if (m_pathToImageMap.end() == pathIt) {
        return nullptr;
    }

    //Check if we have to update the order start pos first
    if (m_updateOrderStartPos) {
        MoveOrderStartPosToEndUnsafe();
    }

    if (isForCurrentOrder) {
        ReorderImageUnsafe(pathIt);
    }

    return &pathIt->second;
}

//----------------------------------------------------------------------------------------------------------------------
//Thread-safe
std::map<strType, ImageData>::iterator ImageCollection::PrepareImage(const strType& imagePath) {
    
    CriticalSectionController cs(IMAGE_CS(m_csType));
    return PrepareImageUnsafe(imagePath);
}

//----------------------------------------------------------------------------------------------------------------------

//Thread-safe
const ImageData* ImageCollection::AllocateImage(const strType& imagePath, const uint32_t w, const uint32_t h) {

    CriticalSectionController cs(IMAGE_CS(m_csType));

    auto pathIt = m_pathToImageMap.find(imagePath);

    //Unload existing memory if it exists
    if (m_pathToImageMap.end() != pathIt) {
        m_memAllocator->Deallocate(&(pathIt->second));
    }  else {
        pathIt = PrepareImageUnsafe(imagePath);
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

//Thread-safe
bool ImageCollection::ResizeImage(const strType& imagePath, const uint32_t w, const uint32_t h) {

    CriticalSectionController cs(IMAGE_CS(m_csType));

    auto it = m_pathToImageMap.find(imagePath);
    ASSERT(it != m_pathToImageMap.end());

    //Allocate
    ImageData resizedImageData(nullptr,w,h,READ_STATUS_LOADING);
    const bool isAllocated = AllocateRawDataUnsafe(&resizedImageData.RawData, w, h, imagePath);
    if (!isAllocated)
        return false;


    ImageData& imageData = it->second;
    ASSERT(nullptr != imageData.RawData);
    ASSERT(READ_STATUS_SUCCESS == imageData.CurrentReadStatus);

    
    stbir_resize_uint8(imageData.RawData, imageData.Width, imageData.Height, 0,
        resizedImageData.RawData, w, h, 0, LoaderConstants::NUM_BYTES_PER_TEXEL);
    m_memAllocator->Deallocate(&imageData);

    //Register to map
    resizedImageData.CurrentReadStatus = READ_STATUS_SUCCESS;
    it->second = resizedImageData;

    return true;
}

//----------------------------------------------------------------------------------------------------------------------

//Thread-safe
void ImageCollection::SetImageStatus(const strType& imagePath, const ReadStatus status) {
    CriticalSectionController cs(IMAGE_CS(m_csType));

    ASSERT(m_pathToImageMap.find(imagePath) != m_pathToImageMap.end());
    ImageData& imageData = m_pathToImageMap.at(imagePath);
    imageData.CurrentReadStatus = status;

}

//----------------------------------------------------------------------------------------------------------------------

//Thread-safe
bool ImageCollection::UnloadImage(const strType& imagePath) {
    CriticalSectionController cs(IMAGE_CS(m_csType));

    const std::map<strType,ImageData>::iterator it = m_pathToImageMap.find(imagePath);
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

//Thread-safe
void ImageCollection::AdvanceOrder() {
    CriticalSectionController cs(IMAGE_CS(m_csType));
    //Turn on the flag, so that at the next GetImage() or PrepareImage(), 
    //the related image would be the start pos of the current "order".
    //The prev nodes before this start pos, would be regarded as "unused" for this order, and thus safe to be unloaded
    m_updateOrderStartPos = true;
}


//----------------------------------------------------------------------------------------------------------------------

std::map<strType, ImageData>::iterator ImageCollection::PrepareImageUnsafe(const strType& imagePath) {
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

void ImageCollection::AddImageOrderUnsafe(std::map<strType, ImageData>::iterator pathToImageIt) {
    auto orderIt = m_orderedImageList.insert(m_orderedImageList.end(), pathToImageIt);
    m_pathToOrderMap.insert({pathToImageIt->first, orderIt});
}

//----------------------------------------------------------------------------------------------------------------------

void ImageCollection::ReorderImageUnsafe(std::map<strType, ImageData>::iterator pathToImageIt) {
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
void ImageCollection::DeleteImageOrderUnsafe(std::map<strType, ImageData>::iterator pathToImageIt) {
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
    std::list<std::map<strType, ImageData>::iterator>::iterator orderIt = m_orderedImageList.begin();
    if (m_curOrderStartPos == orderIt)
        return false;

    const strType& imagePath = (*orderIt)->first;

    //This should not be happening. The image that we want to allocate should not be located at the start of the list
    ASSERT(imagePath != imagePathToAllocate);
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
