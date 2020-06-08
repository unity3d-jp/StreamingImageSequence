#pragma once

//CommonLib
#include "CommonLib/Types.h"

//Loader
#include "ImageData.h"

namespace StreamingImageSequencePlugin {

struct ImageData;

class ImageMemoryAllocator {
public:
    //return true if alloc is successful, false otherwise
    bool Allocate(uint8_t ** rawDataPtr, const uint32_t w, const uint32_t h);
    void Deallocate(ImageData* imageData);

    inline uint64_t GetUsedMemory() const;

    ~ImageMemoryAllocator();
private:
    void IncUsedMem(const uint64_t mem);
    void DecUsedMem(const uint64_t mem);
    static uint32_t CalculateMemSize(const uint32_t w, const uint32_t h);

    uint64_t m_usedMemory;

};

//----------------------------------------------------------------------------------------------------------------------

uint64_t ImageMemoryAllocator::GetUsedMemory() const { return m_usedMemory;  }


} //end namespace

