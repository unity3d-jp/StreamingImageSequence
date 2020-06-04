#include "stdafx.h"
#include "ImageCatalog.h"


namespace StreamingImageSequencePlugin {

ImageCatalog::ImageCatalog() : m_usedMemory(0){

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

void ImageCatalog::AddImage(const strType& imagePath, const uint32_t imageType) {
    ASSERT(imageType < MAX_CRITICAL_SECTION_TYPE_IMAGES);

    std::map<strType, ImageData>& curMap = m_pathToImageMap[imageType];
    ASSERT(curMap.find(imagePath) == curMap.end());

    ImageData& imageData = curMap[imagePath];
    imageData.CurrentReadStatus = READ_STATUS_LOADING;
}

//----------------------------------------------------------------------------------------------------------------------

void ImageCatalog::SetImage(const strType& imagePath, const uint32_t imageType, ImageData* newImageData) {

    ASSERT(imageType < MAX_CRITICAL_SECTION_TYPE_IMAGES);

    std::map<strType, ImageData>& curMap = m_pathToImageMap[imageType];
    ASSERT(curMap.find(imagePath) != curMap.end());

    ImageData& imageData = curMap.at(imagePath);
    //Deallocate existing image
    if (nullptr != imageData.RawData) {
        UnloadImageData(&imageData);
    }  

    imageData = *newImageData;
    IncUsedMemory(imageData.DataSize);
}

//----------------------------------------------------------------------------------------------------------------------
bool ImageCatalog::UnloadImage(const strType& imagePath, const uint32_t imageType) {
    ASSERT(imageType < MAX_CRITICAL_SECTION_TYPE_IMAGES);

    std::map<strType, ImageData>& curMap = m_pathToImageMap[imageType];
    if (curMap.find(imagePath) == curMap.end())
        return false;

    ImageData& imageData = curMap.at(imagePath);
    //Check if the loading progress is still ongoing
    if (!imageData.RawData|| imageData.CurrentReadStatus != READ_STATUS_SUCCESS) {
        return false;
    }

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
void ImageCatalog::IncUsedMemory(const uint64_t mem) {
    m_usedMemory += mem;
}

//----------------------------------------------------------------------------------------------------------------------
void ImageCatalog::UnloadImageData(ImageData* imageData) {
    ASSERT(nullptr!=imageData);
    const uint64_t mem = imageData->DataSize;
    ASSERT(m_usedMemory >= mem);
    ASSERT(nullptr!=imageData->RawData);

    free(imageData->RawData);

    imageData->RawData = nullptr;
    imageData->DataSize = 0;

    m_usedMemory = (m_usedMemory >= mem) ? m_usedMemory - mem : 0;
}

}
