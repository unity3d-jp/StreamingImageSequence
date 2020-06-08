#include "ImageCollection.h"

//Loader
#include "ImageMemoryAllocator.h"
#include "LoaderConstants.h"

//External
#include "External/stb/stb_image_resize.h"


namespace StreamingImageSequencePlugin {


const ImageData* ImageCollection::GetImage(const strType& imagePath) {
    std::map<strType, ImageData>::iterator pathIt = m_pathToImageMap.find(imagePath);

    if (m_pathToImageMap.end() == pathIt) {
        return nullptr;
    }

    ReorderImage(pathIt);
    return &pathIt->second;
}

//----------------------------------------------------------------------------------------------------------------------
std::map<strType, ImageData>::iterator ImageCollection::PrepareImage(const strType& imagePath) {
    
    ASSERT(m_pathToImageMap.find(imagePath) == m_pathToImageMap.end());

    auto it = m_pathToImageMap.insert({ imagePath, ImageData(nullptr,0,0,READ_STATUS_LOADING) });
    AddImageOrder(it.first);
    return it.first;
}

//----------------------------------------------------------------------------------------------------------------------

const ImageData* ImageCollection::AllocateImage(const strType& imagePath, const uint32_t w, const uint32_t h) {

    auto pathIt = m_pathToImageMap.find(imagePath);

    //Unload existing memory if it exists
    if (m_pathToImageMap.end() != pathIt) {
        m_memAllocator->Deallocate(&m_pathToImageMap[imagePath]);
    }  else {
        pathIt = PrepareImage(imagePath);
    }

    ImageData* imageData = &pathIt->second;
    imageData->Width = w;
    imageData->Height = h;

    const bool isAllocated = m_memAllocator->Allocate(&imageData->RawData, w, h);

    //[TODO-sin: 2020-6-5] Handle automatic memory deallocation
    if (!isAllocated)
        return nullptr;

    return imageData;
}

//----------------------------------------------------------------------------------------------------------------------

bool ImageCollection::ResizeImage(const strType& imagePath, const uint32_t w, const uint32_t h) {

    auto it = m_pathToImageMap.find(imagePath);
    ASSERT(it != m_pathToImageMap.end());

    //Allocate
    ImageData resizedImageData(nullptr,w,h,READ_STATUS_LOADING);
    const bool isAllocated = m_memAllocator->Allocate(&resizedImageData.RawData, w, h);

    //[TODO-sin: 2020-6-5] Handle automatic memory deallocation
    if (!isAllocated) {
        return false;
    }

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
    m_curFrameStartPos = m_orderedImageList.end();

    for (auto itr = m_pathToImageMap.begin(); itr != m_pathToImageMap.end(); ++itr) {
        ImageData* imageData = &(itr->second);
        m_memAllocator->Deallocate(imageData);
    }
    m_pathToImageMap.clear();

}

//----------------------------------------------------------------------------------------------------------------------

void ImageCollection::AdvanceFrame() {

    //This will imply that images next to this pos were added/used after this frame, 
    //while the prev nodes were added before and not used since
    m_curFrameStartPos = m_orderedImageList.end();
}

//----------------------------------------------------------------------------------------------------------------------

void ImageCollection::AddImageOrder(std::map<strType, ImageData>::iterator pathToImageIt) {
    auto orderIt = m_orderedImageList.insert(m_orderedImageList.end(), pathToImageIt);
    m_pathToOrderMap.insert({pathToImageIt->first, orderIt});
}
//----------------------------------------------------------------------------------------------------------------------

void ImageCollection::ReorderImage(std::map<strType, ImageData>::iterator pathToImageIt) {
    auto pathToOrderIt = m_pathToOrderMap.find(pathToImageIt->first);
    if (m_pathToOrderMap.end() == pathToOrderIt) {
        return;
    }

    //maintain the position of the current frame
    if (pathToOrderIt->second == m_curFrameStartPos) {
        ++m_curFrameStartPos;
    }

    //Move to the end
    m_orderedImageList.splice( m_orderedImageList.end(), m_orderedImageList, pathToOrderIt->second);

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

    if (orderIt == m_curFrameStartPos) {
        ++m_curFrameStartPos;
    }
    m_orderedImageList.erase(orderIt);
    m_pathToOrderMap.erase(pathToOrderIt);
}


} //end namespace
