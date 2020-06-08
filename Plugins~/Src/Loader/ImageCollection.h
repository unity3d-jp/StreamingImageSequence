#pragma once

//CommonLib
#include "CommonLib/Types.h"

//Loader
#include "ImageData.h"

namespace StreamingImageSequencePlugin {

class ImageMemoryAllocator;

class ImageCollection {
public:
    inline void SetMemoryAllocator(ImageMemoryAllocator*);

    //return null if not found
    const ImageData* GetImage(const strType& imagePath) const;

    const ImageData* AllocateImage(const strType& imagePath, const uint32_t w, const uint32_t h);
    bool ResizeImage(const strType& imagePath, const uint32_t w, const uint32_t h);

    void PrepareImage(const strType& imagePath);
    void SetImageStatus(const strType& imagePath, const ReadStatus status);
    bool UnloadImage(const strType& imagePath);
    void UnloadAllImages();

    inline const std::map<strType, ImageData> GetImageMap() const;
    inline size_t GetNumImages() const;

private:

    ImageMemoryAllocator*           m_memAllocator;
    std::map<strType, ImageData>    m_pathToImageMap;



};
void ImageCollection::SetMemoryAllocator(ImageMemoryAllocator* memAllocator) { m_memAllocator = memAllocator; }

inline const std::map<strType, ImageData> ImageCollection::GetImageMap() const { return m_pathToImageMap;  }
inline size_t ImageCollection::GetNumImages() const { return m_pathToImageMap.size(); }


} //end namespace

