#include "ImageCollection.h"

//Loader
#include "ImageMemoryAllocator.h"
#include "LoaderConstants.h"

//External
#include "External/stb/stb_image_resize.h"


namespace StreamingImageSequencePlugin {


const ImageData* ImageCollection::GetImage(const strType& imagePath) const {
    std::map<strType, ImageData>::const_iterator pathIt = m_pathToImageMap.find(imagePath);

    if (m_pathToImageMap.end() == pathIt) {
        return nullptr;
    }

    return &pathIt->second;
}

//----------------------------------------------------------------------------------------------------------------------
std::map<strType, ImageData>::iterator ImageCollection::PrepareImage(const strType& imagePath) {
    
    ASSERT(m_pathToImageMap.find(imagePath) == m_pathToImageMap.end());

    auto it = m_pathToImageMap.insert({ imagePath, ImageData(nullptr,0,0,READ_STATUS_LOADING) });
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
    m_pathToImageMap.erase(imagePath);
    return true;
}

//----------------------------------------------------------------------------------------------------------------------

void ImageCollection::UnloadAllImages() {

    for (auto itr = m_pathToImageMap.begin(); itr != m_pathToImageMap.end(); ++itr) {
        ImageData* imageData = &(itr->second);
        m_memAllocator->Deallocate(imageData);
    }
    m_pathToImageMap.clear();

}



} //end namespace
