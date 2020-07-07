#pragma once

#include "stdafx.h"

//CommonLib
#include "CommonLib/Types.h" //strType
#include "CommonLib/CriticalSectionType.h" //MAX_CRITICAL_SECTION_TYPE_IMAGES

//Loader
#include "ImageData.h"
#include "ImageCollection.h"
#include "ImageMemoryAllocator.h"

namespace StreamingImageSequencePlugin {

class ImageCatalog {
public:

    inline static ImageCatalog& GetInstance();
    const ImageData* GetImage(const strType& imagePath, const uint32_t imageType, const int frame);

    //Wrapper for functions in ImageCollection
    inline const ImageData* PrepareImage(const strType& imagePath, const uint32_t imageType, const int frame);
    inline const ImageData* AllocateImage(const strType& imagePath,const uint32_t imageType,const uint32_t w,const uint32_t h);
    inline bool CopyImageFromSrc(const strType& imagePath,const uint32_t imageType, const ImageData*, 
                                 const uint32_t w,const uint32_t h);
    inline void ResizeImage(const strType& imagePath,const uint32_t imageType,const uint32_t w, const uint32_t h);
    inline void SetImageStatus(const strType& imagePath, const uint32_t imageType, const ReadStatus status);
    inline bool UnloadImage(const strType& imagePath, const uint32_t imageType);
    inline const std::unordered_map<strType, ImageData>& GetImageMap(const uint32_t imageType) const;
    inline size_t GetNumImages(const uint32_t imageType) const;

    void Reset();

    inline uint64_t GetUsedMemory() const;
private:
    ImageCatalog();
    ImageCatalog(ImageCatalog const&) = delete;
    ImageCatalog& operator=(ImageCatalog const&) = delete;

    void UpdateRequestFrame(const int );

    ImageMemoryAllocator m_memAllocator;
    ImageCollection m_imageCollection[MAX_CRITICAL_SECTION_TYPE_IMAGES];
    int m_latestRequestFrame;
};

//----------------------------------------------------------------------------------------------------------------------

ImageCatalog& ImageCatalog::GetInstance() {
    static ImageCatalog catalog;
    return catalog;
}

//----------------------------------------------------------------------------------------------------------------------

const ImageData* ImageCatalog::PrepareImage(const strType& imagePath, const uint32_t imageType, const int frame) {
    ASSERT(imageType < MAX_CRITICAL_SECTION_TYPE_IMAGES);
    UpdateRequestFrame(frame);
    const std::unordered_map<strType, ImageData>::const_iterator it = m_imageCollection[imageType].PrepareImage(imagePath);
    return &it->second;
}

const ImageData* ImageCatalog::AllocateImage(const strType& imagePath, const uint32_t imageType, const uint32_t w, const uint32_t h) {
    ASSERT(imageType < MAX_CRITICAL_SECTION_TYPE_IMAGES);
    return m_imageCollection[imageType].AllocateImage(imagePath, w, h);
}
bool ImageCatalog::CopyImageFromSrc(const strType& imagePath,const uint32_t imageType, const ImageData* src,
                                    const uint32_t w,const uint32_t h)
{
    ASSERT(imageType < MAX_CRITICAL_SECTION_TYPE_IMAGES);
    return m_imageCollection[imageType].CopyImageFromSrc(imagePath, src, w, h);
}

void ImageCatalog::ResizeImage(const strType& imagePath, const uint32_t imageType, const uint32_t w, const uint32_t h) {
    ASSERT(imageType < MAX_CRITICAL_SECTION_TYPE_IMAGES);
    m_imageCollection[imageType].ResizeImage(imagePath, w, h);
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

uint64_t ImageCatalog::GetUsedMemory() const { return m_memAllocator.GetUsedMemory(); }



} //end namespace

