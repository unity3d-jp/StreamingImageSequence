#pragma once

//CommonLib
#include "CommonLib/Types.h"

//Loader
#include <map>

#include "ImageData.h"

namespace StreamingImageSequencePlugin {

struct ImageData;

class ImageMemoryAllocator {
public:
    //return true if alloc is successful, false otherwise
    bool Allocate(uint8_t ** rawDataPtr, const uint32_t w, const uint32_t h);

    //return nullptr if not successful
    void* Allocate(const size_t memSize, bool forceAllocate = false);

    //Reallocate a previously allocated memory with a new size
    void* Reallocate(void* buffer, const size_t memSize, bool forceAllocate = false);

    bool Deallocate(ImageData* imageData);
    bool Deallocate(void* buffer);

    inline uint64_t GetUsedMemory() const;
    inline void SetMaxMemory(const uint64_t maxMemory);

    ImageMemoryAllocator();
    ~ImageMemoryAllocator();
private:

    bool IsMemoryAllocable(const size_t memSize) const;
    void* AllocateInternal(const size_t memSize);

    void IncUsedMem(const uint64_t mem);
    void DecUsedMem(const uint64_t mem);
    static size_t CalculateMemSize(const uint32_t w, const uint32_t h);

    uint64_t m_usedMemory;
    uint64_t m_maxMemory;
    float m_totalRAM;
    float m_inverseTotalRAM;

    std::map<void*, size_t> m_allocatedBuffers;

};

//----------------------------------------------------------------------------------------------------------------------

uint64_t ImageMemoryAllocator::GetUsedMemory() const { return m_usedMemory;  }
void ImageMemoryAllocator::SetMaxMemory(const uint64_t maxMemory) { m_maxMemory = maxMemory; }


} //end namespace

