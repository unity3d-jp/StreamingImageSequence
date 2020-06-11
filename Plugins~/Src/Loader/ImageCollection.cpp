#include "ImageCollection.h"

//Loader
#include "ImageMemoryAllocator.h"
#include "LoaderConstants.h"

//External
#include "External/stb/stb_image_resize.h"


namespace StreamingImageSequencePlugin {


ImageCollection::ImageCollection() : m_curOrderStartPos(m_orderedImageList.end()), m_updateOrderStartPos(false) {

}

//----------------------------------------------------------------------------------------------------------------------
const ImageData* ImageCollection::GetImage(const strType& imagePath, const bool isForCurrentOrder) {
    std::map<strType, ImageData>::iterator pathIt = m_pathToImageMap.find(imagePath);

    if (m_pathToImageMap.end() == pathIt) {
        return nullptr;
    }

    //Check if we have to update the order start pos first
    if (m_updateOrderStartPos) {
        MoveOrderStartPosToEnd();
        m_updateOrderStartPos = false;
    }

    if (isForCurrentOrder)
        ReorderImageToEnd(pathIt);
    return &pathIt->second;
}

//----------------------------------------------------------------------------------------------------------------------
std::map<strType, ImageData>::iterator ImageCollection::PrepareImage(const strType& imagePath) {
    
    ASSERT(m_pathToImageMap.find(imagePath) == m_pathToImageMap.end());

    auto it = m_pathToImageMap.insert({ imagePath, ImageData(nullptr,0,0,READ_STATUS_LOADING) });
    AddImageOrder(it.first);

    ASSERT(m_orderedImageList.size() >= 1);
    if (m_curOrderStartPos == m_orderedImageList.end() || m_updateOrderStartPos) {
        MoveOrderStartPosToEnd();
        m_updateOrderStartPos = false;
    }

    return it.first;
}

//----------------------------------------------------------------------------------------------------------------------

const ImageData* ImageCollection::AllocateImage(const strType& imagePath, const uint32_t w, const uint32_t h) {

    auto pathIt = m_pathToImageMap.find(imagePath);

    //Unload existing memory if it exists
    if (m_pathToImageMap.end() != pathIt) {
        m_memAllocator->Deallocate(&(pathIt->second));
    }  else {
        pathIt = PrepareImage(imagePath);
    }

    ImageData* imageData = &pathIt->second;

    const bool isAllocated = AllocateRawData(&imageData->RawData, w, h, imagePath);
    if (!isAllocated)
        return nullptr;

    imageData->Width = w;
    imageData->Height = h;

    return imageData;
}

//----------------------------------------------------------------------------------------------------------------------

bool ImageCollection::ResizeImage(const strType& imagePath, const uint32_t w, const uint32_t h) {

    auto it = m_pathToImageMap.find(imagePath);
    ASSERT(it != m_pathToImageMap.end());

    //Allocate
    ImageData resizedImageData(nullptr,w,h,READ_STATUS_LOADING);
    const bool isAllocated = AllocateRawData(&resizedImageData.RawData, w, h, imagePath);
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
void ImageCollection::SetImageStatus(const strType& imagePath, const ReadStatus status) {
    ASSERT(m_pathToImageMap.find(imagePath) != m_pathToImageMap.end());

    ImageData& imageData = m_pathToImageMap.at(imagePath);
    imageData.CurrentReadStatus = status;

}

//----------------------------------------------------------------------------------------------------------------------
bool ImageCollection::UnloadImage(const strType& imagePath) {

    std::map<strType,ImageData>::iterator it = m_pathToImageMap.find(imagePath);
    if (m_pathToImageMap.end() == it)
        return false;

    m_memAllocator->Deallocate(&(it->second));
    DeleteImageOrder(it);
    m_pathToImageMap.erase(imagePath);
    return true;
}

//----------------------------------------------------------------------------------------------------------------------

void ImageCollection::UnloadAllImages() {
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

void ImageCollection::AdvanceOrder() {
    //Turn on the flag, so that at the next GetImage() or PrepareImage(), 
    //the related image would be the start pos of the current "order".
    //The prev nodes before this start pos, would be regarded as "unused" for this order, and thus safe to be unloaded
    m_updateOrderStartPos = true;
}

//----------------------------------------------------------------------------------------------------------------------

void ImageCollection::AddImageOrder(std::map<strType, ImageData>::iterator pathToImageIt) {
    auto orderIt = m_orderedImageList.insert(m_orderedImageList.end(), pathToImageIt);
    m_pathToOrderMap.insert({pathToImageIt->first, orderIt});
}
//----------------------------------------------------------------------------------------------------------------------

void ImageCollection::ReorderImageToEnd(std::map<strType, ImageData>::iterator pathToImageIt) {
    auto pathToOrderIt = m_pathToOrderMap.find(pathToImageIt->first);
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
        MoveOrderStartPosToEnd();
    }
}

//----------------------------------------------------------------------------------------------------------------------

//remove the linked data in m_orderedImageList
void ImageCollection::DeleteImageOrder(std::map<strType, ImageData>::iterator pathToImageIt) {
    //remove the linked data in the ordering structures
    auto pathToOrderIt = m_pathToOrderMap.find(pathToImageIt->first);
    if (m_pathToOrderMap.end() == pathToOrderIt) {
        return;
    }

    auto orderIt = pathToOrderIt->second;

    if (orderIt == m_curOrderStartPos) {
        ++m_curOrderStartPos;
    }
    m_orderedImageList.erase(orderIt);
    m_pathToOrderMap.erase(pathToOrderIt);


    //make sure the start pos is valid
    if (m_curOrderStartPos == m_orderedImageList.end() && m_pathToOrderMap.size() >= 1) {
        --m_curOrderStartPos;
    }

}

//----------------------------------------------------------------------------------------------------------------------
void ImageCollection::MoveOrderStartPosToEnd() {
    ASSERT(m_orderedImageList.size() >= 1);
    m_curOrderStartPos = m_orderedImageList.end();
    --m_curOrderStartPos;
}


//----------------------------------------------------------------------------------------------------------------------
bool ImageCollection::AllocateRawData(uint8_t** rawData,const uint32_t w,const uint32_t h,const strType& imagePath) 
{

    bool isAllocated = false;
    bool unloadSuccessful = true;
    while (!isAllocated && unloadSuccessful) {
        isAllocated = m_memAllocator->Allocate(rawData, w, h);
        if (!isAllocated) {
            unloadSuccessful = UnloadUnusedImage(imagePath);
        }
    }

    return isAllocated;
}


//----------------------------------------------------------------------------------------------------------------------
//returns true if one or more images are successfully unloaded
bool ImageCollection::UnloadUnusedImage(const strType& imagePathToAllocate) {
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
