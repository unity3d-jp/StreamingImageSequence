#include "ImageCollection.h"

//Loader
#include "ImageMemoryAllocator.h"
#include "LoaderConstants.h"

//External
#include "External/stb/stb_image_resize.h"


namespace StreamingImageSequencePlugin {


const ImageData* ImageCollection::GetImage(const strType& imagePath) const {

    if (m_pathToImageMap.find(imagePath) != m_pathToImageMap.end()) {
        return &(m_pathToImageMap.at(imagePath));
    }
    return nullptr;
}

//----------------------------------------------------------------------------------------------------------------------
void ImageCollection::PrepareImage(const strType& imagePath) {
    ASSERT(m_pathToImageMap.find(imagePath) == m_pathToImageMap.end());

    ImageData& imageData = m_pathToImageMap[imagePath];
    ASSERT(nullptr == imageData.RawData);

    imageData.CurrentReadStatus = READ_STATUS_LOADING;
}

//----------------------------------------------------------------------------------------------------------------------

const ImageData* ImageCollection::AllocateImage(const strType& imagePath, const uint32_t w, const uint32_t h) {

    //Unload existing memory if it exsits
    if (m_pathToImageMap.find(imagePath) != m_pathToImageMap.end()) {
        m_memAllocator->Deallocate(&m_pathToImageMap[imagePath]);
    }

    ImageData* imageData = &m_pathToImageMap[imagePath];
    const bool isAllocated = m_memAllocator->Allocate(imageData, w, h);
    //[TODO-sin: 2020-6-5] Handle automatic memory deallocation
    if (!isAllocated)
        return nullptr;

    return imageData;
}

//----------------------------------------------------------------------------------------------------------------------

bool ImageCollection::ResizeImage(const strType& imagePath, const uint32_t w, const uint32_t h) {
    ASSERT(m_pathToImageMap.find(imagePath) != m_pathToImageMap.end());

    //Allocate
    ImageData resizedImageData;
    const bool isAllocated = m_memAllocator->Allocate(&resizedImageData, w, h);

    //[TODO-sin: 2020-6-5] Handle automatic memory deallocation
    if (!isAllocated) {
        return false;
    }

    ImageData& prevImageData = m_pathToImageMap.at(imagePath);
    ASSERT(nullptr != prevImageData.RawData);
    ASSERT(READ_STATUS_SUCCESS == prevImageData.CurrentReadStatus);
    
    stbir_resize_uint8(prevImageData.RawData, prevImageData.Width, prevImageData.Height, 0,
        resizedImageData.RawData, w, h, 0, LoaderConstants::NUM_BYTES_PER_TEXEL);
    m_memAllocator->Deallocate(&prevImageData);

    //Register to map
    resizedImageData.CurrentReadStatus = READ_STATUS_SUCCESS;
    m_pathToImageMap[imagePath] = resizedImageData;
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

    if (m_pathToImageMap.find(imagePath) == m_pathToImageMap.end())
        return false;

    m_memAllocator->Deallocate(&m_pathToImageMap[imagePath]);
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
