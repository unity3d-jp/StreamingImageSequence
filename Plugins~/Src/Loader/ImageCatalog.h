#pragma once

#include "stdafx.h"

//CommonLib
#include "CommonLib/Types.h" //strType
#include "CommonLib/CriticalSectionType.h" //MAX_CRITICAL_SECTION_TYPE_IMAGES

//Loader
#include "ImageData.h"

namespace StreamingImageSequencePlugin {

class ImageCatalog {
public:

    inline static ImageCatalog& GetInstance();
    
    //Returns null if not found
    const ImageData* GetImage(const strType& imagePath, const uint32_t imageType) const;

    void PrepareImage(const strType& imagePath, const uint32_t imageType);
    const ImageData* AllocateImage(const strType& imagePath,const uint32_t imageType,const uint32_t w,const uint32_t h);

    void ResizeImage(const strType& imagePath,const uint32_t imageType,const uint32_t w, const uint32_t h);

    void SetImageStatus(const strType& imagePath, const uint32_t imageType, const ReadStatus status);

    bool UnloadImage(const strType& imagePath, const uint32_t imageType);
    void UnloadAllImages();

    inline const std::map<strType, ImageData> GetImageCollection(const uint32_t imageType) const;
    inline size_t GetNumImages(const uint32_t imageType) const;
    inline uint64_t GetUsedMemory() const;
private:
    ImageCatalog();
    ImageCatalog(ImageCatalog const&) = delete;
    ImageCatalog& operator=(ImageCatalog const&) = delete;

    void UnloadImageData(ImageData* imageData);
    void IncUsedMemory(const uint64_t mem);
    void DecUsedMemory(const uint64_t mem);
    static uint32_t CalculateDataSize(const uint32_t w, const uint32_t h);

    uint64_t m_usedMemory;

    std::map<strType, ImageData> m_pathToImageMap[MAX_CRITICAL_SECTION_TYPE_IMAGES];


};

//----------------------------------------------------------------------------------------------------------------------

ImageCatalog& ImageCatalog::GetInstance() {
    static ImageCatalog catalog;
    return catalog;
}

const std::map<strType, ImageData> ImageCatalog::GetImageCollection(const uint32_t imageType) const { 
    ASSERT(imageType < MAX_CRITICAL_SECTION_TYPE_IMAGES);
    return m_pathToImageMap[imageType]; 
}

size_t ImageCatalog::GetNumImages(const uint32_t imageType) const { 
    ASSERT(imageType < MAX_CRITICAL_SECTION_TYPE_IMAGES);
    return m_pathToImageMap[imageType].size(); 
}

uint64_t ImageCatalog::GetUsedMemory() const { return m_usedMemory; }

} //end namespace

