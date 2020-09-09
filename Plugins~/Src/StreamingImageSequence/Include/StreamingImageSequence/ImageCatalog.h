#pragma once

//CommonLib
#include "CommonLib/Types.h" //strType
#include "CommonLib/CriticalSectionType.h" //MAX_CRITICAL_SECTION_TYPE_IMAGES

//SIS
#include "StreamingImageSequence/ImageData.h"
#include "ImageCollection.h"
#include "ImageMemoryAllocator.h"

namespace StreamingImageSequencePlugin {

class ImageCatalog {
public:

    inline static ImageCatalog& GetInstance();

    //Wrapper for functions in ImageCollection
    inline const ImageData* GetImage(const strType& imagePath, const uint32_t imageType, const int frame);
    inline const ImageData* AddImage(const strType& imagePath, const uint32_t imageType, const int frame);
    inline bool AddImageFromSrc(const strType& imagePath,const uint32_t imageType, const int frame, const ImageData*, 
                                const uint32_t w,const uint32_t h);
    inline const ImageData* AllocateImage(const strType& imagePath,const uint32_t imageType,const uint32_t w,const uint32_t h);
    inline void SetImageStatus(const strType& imagePath, const uint32_t imageType, const ReadStatus status);
    inline bool UnloadImage(const strType& imagePath, const uint32_t imageType);
    inline const std::unordered_map<strType, ImageData>& GetImageMap(const uint32_t imageType) const;
    inline int GetLatestFrame(const uint32_t imageType) const;
    inline size_t GetNumImages(const uint32_t imageType) const;

    void Reset();
    void ResetOrder();

    inline uint64_t GetUsedMemory() const;
    inline void SetMaxMemory(uint64_t maxMemory);
private:
    ImageCatalog();
    ImageCatalog(ImageCatalog const&) = delete;
    ImageCatalog& operator=(ImageCatalog const&) = delete;

    ImageMemoryAllocator m_memAllocator;
    ImageCollection m_imageCollection[MAX_CRITICAL_SECTION_TYPE_IMAGES];
};

//----------------------------------------------------------------------------------------------------------------------

ImageCatalog& ImageCatalog::GetInstance() {
    static ImageCatalog catalog;
    return catalog;
}

//----------------------------------------------------------------------------------------------------------------------
//Wrapper for functions in ImageCollection 
const ImageData* ImageCatalog::GetImage(const strType& imagePath, const uint32_t imageType, const int frame) {
    ASSERT(imageType < MAX_CRITICAL_SECTION_TYPE_IMAGES);
    return m_imageCollection[imageType].GetImage(imagePath, frame);
}




const ImageData* ImageCatalog::AddImage(const strType& imagePath, const uint32_t imageType, const int frame) {
    ASSERT(imageType < MAX_CRITICAL_SECTION_TYPE_IMAGES);
    return m_imageCollection[imageType].AddImage(imagePath, frame);
}

bool ImageCatalog::AddImageFromSrc(const strType& imagePath,const uint32_t imageType, const int frame, const ImageData* src,
                                   const uint32_t w,const uint32_t h)
{
    ASSERT(imageType < MAX_CRITICAL_SECTION_TYPE_IMAGES);
    return m_imageCollection[imageType].AddImageFromSrc(imagePath, frame, src, w, h);
}


const ImageData* ImageCatalog::AllocateImage(const strType& imagePath, const uint32_t imageType, const uint32_t w, const uint32_t h) {
    ASSERT(imageType < MAX_CRITICAL_SECTION_TYPE_IMAGES);
    return m_imageCollection[imageType].AllocateImage(imagePath, w, h);
}

void ImageCatalog::SetImageStatus(const strType& imagePath, const uint32_t imageType, const ReadStatus status) {

    ASSERT(imageType < MAX_CRITICAL_SECTION_TYPE_IMAGES);
    m_imageCollection[imageType].SetImageStatus(imagePath, status);
}
bool ImageCatalog::UnloadImage(const strType& imagePath, const uint32_t imageType) {
    ASSERT(imageType < MAX_CRITICAL_SECTION_TYPE_IMAGES);

    return m_imageCollection[imageType].UnloadImage(imagePath);
}

const std::unordered_map<strType, ImageData>& ImageCatalog::GetImageMap(const uint32_t imageType) const { 
    ASSERT(imageType < MAX_CRITICAL_SECTION_TYPE_IMAGES);
    return m_imageCollection[imageType].GetImageMap();
}

//----------------------------------------------------------------------------------------------------------------------
size_t ImageCatalog::GetNumImages(const uint32_t imageType) const { 
    ASSERT(imageType < MAX_CRITICAL_SECTION_TYPE_IMAGES);
    return m_imageCollection[imageType].GetNumImages();
}

int ImageCatalog::GetLatestFrame(const uint32_t imageType) const { 
    ASSERT(imageType < MAX_CRITICAL_SECTION_TYPE_IMAGES);
    return m_imageCollection[imageType].GetLatestRequestFrame();
}

uint64_t ImageCatalog::GetUsedMemory() const { return m_memAllocator.GetUsedMemory(); }

void ImageCatalog::SetMaxMemory(uint64_t maxMemory) { m_memAllocator.SetMaxMemory(maxMemory); }


} //end namespace

