#pragma once
#include <list>

//CommonLib
#include "CommonLib/Types.h"

//Loader
#include "ImageData.h"
#include "CommonLib/CriticalSectionType.h"

namespace StreamingImageSequencePlugin {

class ImageMemoryAllocator;

class ImageCollection {
public:
    ImageCollection();
    void Init(CriticalSectionType csType, ImageMemoryAllocator*);

    //return null if not found
    const ImageData* GetImage(const strType& imagePath, const bool isForCurrentOrder);

    const ImageData* AllocateImage(const strType& imagePath, const uint32_t w, const uint32_t h);
    bool ResizeImage(const strType& imagePath, const uint32_t w, const uint32_t h);

    std::map<strType, ImageData>::iterator PrepareImage(const strType& imagePath);
    void SetImageStatus(const strType& imagePath, const ReadStatus status);
    bool UnloadImage(const strType& imagePath);
    void UnloadAllImages();

    inline const std::map<strType, ImageData>& GetImageMap() const;
    inline size_t GetNumImages() const;

    void AdvanceOrder();

private:

    std::map<strType, ImageData>::iterator PrepareImageUnsafe(const strType& imagePath);
    void AddImageOrderUnsafe(std::map<strType, ImageData>::iterator);
    void ReorderImageUnsafe(std::map<strType, ImageData>::iterator);
    void DeleteImageOrderUnsafe(std::map<strType, ImageData>::iterator);
    void MoveOrderStartPosToEndUnsafe();

    //This will unload unused image if memory is not enough
    bool AllocateRawDataUnsafe(uint8_t** rawData, const uint32_t w, const uint32_t h, const strType& imagePath);
    bool UnloadUnusedImageUnsafe(const strType& imagePath); //returns true if one or more images are successfully unloaded

    ImageMemoryAllocator*           m_memAllocator{};
    std::map<strType, ImageData>    m_pathToImageMap;

    //Ordering structure
    std::map<strType, std::list<std::map<strType, ImageData>::iterator>::iterator> m_pathToOrderMap;
    std::list<std::map<strType, ImageData>::iterator>           m_orderedImageList;
    std::list<std::map<strType, ImageData>::iterator>::iterator m_curOrderStartPos;
    bool m_updateOrderStartPos;

    CriticalSectionType m_csType;

};

inline const std::map<strType, ImageData>& ImageCollection::GetImageMap() const { return m_pathToImageMap;  }
inline size_t ImageCollection::GetNumImages() const { return m_pathToImageMap.size(); }


} //end namespace

