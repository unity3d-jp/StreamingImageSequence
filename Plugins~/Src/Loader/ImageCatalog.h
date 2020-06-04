#pragma once

#include "stdafx.h"

//CommonLib
#include "CommonLib/Types.h" //strType
#include "CommonLib/ReadResult.h"
#include "CommonLib/CriticalSectionType.h" //MAX_CRITICAL_SECTION_TYPE_TEXTURES

namespace StreamingImageSequencePlugin {

class ImageCatalog {
public:

    inline static ImageCatalog& GetInstance();
    
    //Returns null if not found
    const StReadResult* GetImage(const strType& imagePath, const uint32_t imageType) const;

    void AddImage(const strType& imagePath, const uint32_t imageType);
    void SetImage(const strType& imagePath, const uint32_t imageType, StReadResult* newImageData);

    bool UnloadImage(const strType& imagePath, const uint32_t imageType);
    void UnloadAllImages();

    inline const std::map<strType, StReadResult> GetImageCollection(const uint32_t imageType) const;
    inline size_t GetNumImages(const uint32_t imageType) const;
    inline uint64_t GetUsedMemory() const;
private:
    ImageCatalog();
    ImageCatalog(ImageCatalog const&) = delete;
    ImageCatalog& operator=(ImageCatalog const&) = delete;

    void IncUsedMemory(const uint64_t mem);
    void UnloadImageData(StReadResult* imageData);

    uint64_t m_usedMemory;

    std::map<strType, StReadResult> m_pathToImageMap[MAX_CRITICAL_SECTION_TYPE_TEXTURES];


};

//----------------------------------------------------------------------------------------------------------------------

ImageCatalog& ImageCatalog::GetInstance() {
    static ImageCatalog catalog;
    return catalog;
}

const std::map<strType, StReadResult> ImageCatalog::GetImageCollection(const uint32_t imageType) const { 
    ASSERT(imageType < MAX_CRITICAL_SECTION_TYPE_TEXTURES);
    return m_pathToImageMap[imageType]; 
}

size_t ImageCatalog::GetNumImages(const uint32_t imageType) const { 
    ASSERT(imageType < MAX_CRITICAL_SECTION_TYPE_TEXTURES);
    return m_pathToImageMap[imageType].size(); 
}

uint64_t ImageCatalog::GetUsedMemory() const { return m_usedMemory; }

} //end namespace

