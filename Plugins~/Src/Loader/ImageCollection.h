#pragma once
#include <list>

//CommonLib
#include "CommonLib/Types.h"

//Loader
#include <unordered_map>

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

    std::unordered_map<strType, ImageData>::const_iterator AddImage(const strType& imagePath);
    bool AddImageFromSrc(const strType& imagePath, const ImageData* src, const uint32_t w, const uint32_t h);

    void SetImageStatus(const strType& imagePath, const ReadStatus status);
    bool UnloadImage(const strType& imagePath);
    void UnloadAllImages();

    inline const std::unordered_map<strType, ImageData>& GetImageMap() const;
    inline size_t GetNumImages() const;

    void AdvanceOrder();

private:

    std::unordered_map<strType, ImageData>::iterator PrepareImageUnsafe(const strType& imagePath);
    void AddImageOrderUnsafe(std::unordered_map<strType, ImageData>::iterator);
    void ReorderImageUnsafe(std::unordered_map<strType, ImageData>::iterator);
    void DeleteImageOrderUnsafe(std::unordered_map<strType, ImageData>::iterator);
    void MoveOrderStartPosToEndUnsafe();

    //This will unload unused image if memory is not enough
    bool AllocateRawDataUnsafe(uint8_t** rawData, const uint32_t w, const uint32_t h, const strType& imagePath);
    bool UnloadUnusedImageUnsafe(const strType& imagePath); //returns true if one or more images are successfully unloaded

    ImageMemoryAllocator*           m_memAllocator;
    std::unordered_map<strType, ImageData>    m_pathToImageMap;

    //Ordering structure
    std::unordered_map<strType, std::list<std::unordered_map<strType, ImageData>::iterator>::iterator> m_pathToOrderMap;
    std::list<std::unordered_map<strType, ImageData>::iterator>           m_orderedImageList;
    std::list<std::unordered_map<strType, ImageData>::iterator>::iterator m_curOrderStartPos;
    bool m_updateOrderStartPos;

    CriticalSectionType m_csType;

};

inline const std::unordered_map<strType, ImageData>& ImageCollection::GetImageMap() const { return m_pathToImageMap;  }
inline size_t ImageCollection::GetNumImages() const { return m_pathToImageMap.size(); }


} //end namespace

