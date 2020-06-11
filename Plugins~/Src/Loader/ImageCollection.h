#pragma once
#include <list>

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
    const ImageData* GetImage(const strType& imagePath, const bool isForCurrentOrder);

    const ImageData* AllocateImage(const strType& imagePath, const uint32_t w, const uint32_t h);
    bool ResizeImage(const strType& imagePath, const uint32_t w, const uint32_t h);

    std::map<strType, ImageData>::iterator PrepareImage(const strType& imagePath);
    void SetImageStatus(const strType& imagePath, const ReadStatus status);
    bool UnloadImage(const strType& imagePath);
    void UnloadAllImages();

    inline const std::map<strType, ImageData> GetImageMap() const;
    inline size_t GetNumImages() const;

    void AdvanceOrder();

private:

    void AddImageOrder(std::map<strType, ImageData>::iterator);
    void ReorderImageToEnd(std::map<strType, ImageData>::iterator);
    void DeleteImageOrder(std::map<strType, ImageData>::iterator);

    //This will unload unused image if memory is not enough
    bool AllocateRawData(uint8_t** rawData, const uint32_t w, const uint32_t h, const strType& imagePath);
    bool UnloadUnusedImage(const strType& imagePath); //returns true if one or more images are successfully unloaded

    ImageMemoryAllocator*           m_memAllocator;
    std::map<strType, ImageData>    m_pathToImageMap;

    //Ordering structure
    std::map<strType, std::list<std::map<strType, ImageData>::iterator>::iterator> m_pathToOrderMap;
    std::list<std::map<strType, ImageData>::iterator>           m_orderedImageList;
    std::list<std::map<strType, ImageData>::iterator>::iterator m_curOrderStartPos;

};

void ImageCollection::SetMemoryAllocator(ImageMemoryAllocator* memAllocator) { m_memAllocator = memAllocator; }

inline const std::map<strType, ImageData> ImageCollection::GetImageMap() const { return m_pathToImageMap;  }
inline size_t ImageCollection::GetNumImages() const { return m_pathToImageMap.size(); }


} //end namespace

