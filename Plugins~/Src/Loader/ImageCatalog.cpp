#include "stdafx.h"
#include "ImageCatalog.h"

#include "LoaderConstants.h" //NUM_BYTES_PER_TEXEL

//External
#include "External/stb/stb_image_resize.h"


namespace StreamingImageSequencePlugin {

ImageCatalog::ImageCatalog() : m_usedMemory(0) {

}

//----------------------------------------------------------------------------------------------------------------------


const ImageData* ImageCatalog::GetImage(const strType& imagePath, const uint32_t imageType) const {
    ASSERT(imageType < MAX_CRITICAL_SECTION_TYPE_IMAGES);

    const std::map<strType, ImageData>& curMap = m_pathToImageMap[imageType];

    if (curMap.find(imagePath) != curMap.end()) {
        return &(curMap.at(imagePath));
    }
    return nullptr;

}


//----------------------------------------------------------------------------------------------------------------------

void ImageCatalog::PrepareImage(const strType& imagePath, const uint32_t imageType) {
    ASSERT(imageType < MAX_CRITICAL_SECTION_TYPE_IMAGES);

    std::map<strType, ImageData>& curMap = m_pathToImageMap[imageType];
    ASSERT(curMap.find(imagePath) == curMap.end());

    ImageData& imageData = curMap[imagePath];
    ASSERT(nullptr == imageData.RawData);

    imageData.CurrentReadStatus = READ_STATUS_LOADING;
}

//----------------------------------------------------------------------------------------------------------------------
const ImageData* ImageCatalog::AllocateImage(const strType& imagePath, const uint32_t imageType, const uint32_t w, const uint32_t h) {
    ASSERT(imageType < MAX_CRITICAL_SECTION_TYPE_IMAGES);

    //Unload existing memory if it exsits
    std::map<strType, ImageData>& curMap = m_pathToImageMap[imageType];
    if (curMap.find(imagePath) != curMap.end()) {
        UnloadImageData(&curMap[imagePath]);
    }

    //Allocate
    const uint32_t dataSize = CalculateDataSize(w, h);
    uint8_t*  buffer = static_cast<uint8_t*>(malloc(dataSize));
    //[TODO-sin: 2020-6-5] Handle automatic memory deallocation
    if (nullptr == buffer) {
        return nullptr;
    }
    
    //memset(buffer,0,dataSize);

    ImageData& imageData = curMap[imagePath];
    imageData.Width = w;
    imageData.Height = h;
    imageData.CurrentReadStatus = READ_STATUS_LOADING;
    imageData.RawData = buffer;

    IncUsedMemory(dataSize);

    return &imageData;
}

//----------------------------------------------------------------------------------------------------------------------
void ImageCatalog::ResizeImage(const strType& imagePath, const uint32_t imageType, const uint32_t w, const uint32_t h) {
    ASSERT(imageType < MAX_CRITICAL_SECTION_TYPE_IMAGES);
    std::map<strType, ImageData>& curMap = m_pathToImageMap[imageType];
    ASSERT(curMap.find(imagePath) != curMap.end());

    //Allocate
    const uint32_t dataSize = CalculateDataSize(w, h);
    
    uint8_t*  resizedBuffer = static_cast<uint8_t*>(malloc(dataSize));
    //[TODO-sin: 2020-6-5] Handle automatic memory deallocation
    if (nullptr == resizedBuffer) {
        return;
    }

    ImageData& imageData = curMap.at(imagePath);
    ASSERT(nullptr != imageData.RawData);
    ASSERT(READ_STATUS_SUCCESS == imageData.CurrentReadStatus);

    uint8_t* prevRawData = imageData.RawData;
    const uint32_t prevDataSize = CalculateDataSize(imageData.Width, imageData.Height);

    stbir_resize_uint8(imageData.RawData, imageData.Width, imageData.Height, 0,
        resizedBuffer, w, h, 0, LoaderConstants::NUM_BYTES_PER_TEXEL);
    imageData.RawData = resizedBuffer;
    imageData.Width   = w;
    imageData.Height  = h;

    free(prevRawData);
    DecUsedMemory(prevDataSize);

}


//----------------------------------------------------------------------------------------------------------------------
void ImageCatalog::SetImageStatus(const strType& imagePath, const uint32_t imageType, const ReadStatus status) {

    ASSERT(imageType < MAX_CRITICAL_SECTION_TYPE_IMAGES);

    std::map<strType, ImageData>& curMap = m_pathToImageMap[imageType];
    ASSERT(curMap.find(imagePath) != curMap.end());

    ImageData& imageData = curMap.at(imagePath);
    imageData.CurrentReadStatus = status;
}
//----------------------------------------------------------------------------------------------------------------------
bool ImageCatalog::UnloadImage(const strType& imagePath, const uint32_t imageType) {
    ASSERT(imageType < MAX_CRITICAL_SECTION_TYPE_IMAGES);

    std::map<strType, ImageData>& curMap = m_pathToImageMap[imageType];
    if (curMap.find(imagePath) == curMap.end())
        return false;

    ImageData& imageData = curMap.at(imagePath);

    UnloadImageData(&imageData);
    m_pathToImageMap[imageType].erase(imagePath);
    return true;

}
//----------------------------------------------------------------------------------------------------------------------

void ImageCatalog::UnloadAllImages() {
    for (uint32_t imageType = 0; imageType < MAX_CRITICAL_SECTION_TYPE_IMAGES; ++imageType) {
        std::map<strType, ImageData>& curMap = m_pathToImageMap[imageType];

        for (auto itr = curMap.begin(); itr != curMap.end(); ++itr) {
            ImageData* imageData = &(itr->second);
            UnloadImageData(imageData);
        }
        curMap.clear();
    }

}



//----------------------------------------------------------------------------------------------------------------------
void ImageCatalog::UnloadImageData(ImageData* imageData) {
    ASSERT(nullptr!=imageData);

    if (nullptr != imageData->RawData) {
        const uint64_t mem = CalculateDataSize(imageData->Width, imageData->Height);
        ASSERT(m_usedMemory >= mem);
        DecUsedMemory(mem);
        free(imageData->RawData);
    }

    *imageData = ImageData(nullptr, 0, 0, READ_STATUS_NONE);
}

//----------------------------------------------------------------------------------------------------------------------
void ImageCatalog::IncUsedMemory(const uint64_t mem) {
    m_usedMemory += mem;
}

void ImageCatalog::DecUsedMemory(const uint64_t mem) {
    m_usedMemory = (m_usedMemory >= mem) ? m_usedMemory - mem : 0;
}

//----------------------------------------------------------------------------------------------------------------------

uint32_t ImageCatalog::CalculateDataSize(const uint32_t w, const uint32_t h) {
    return w * h * LoaderConstants::NUM_BYTES_PER_TEXEL;
}


} //end namespace
