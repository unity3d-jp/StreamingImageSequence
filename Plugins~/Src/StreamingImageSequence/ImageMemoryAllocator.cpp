#include "StreamingImageSequence/ImageMemoryAllocator.h"

#include "CommonLib/MemoryUtility.h"

#include "LoaderConstants.h"

namespace StreamingImageSequencePlugin {

const uint64_t UNLIMITED_MEMORY = 0;
const uint64_t DEFAULT_MAX_MEMORY = UNLIMITED_MEMORY;

//----------------------------------------------------------------------------------------------------------------------

ImageMemoryAllocator::ImageMemoryAllocator() : m_usedMemory(0)
    , m_maxMemory(DEFAULT_MAX_MEMORY)
    , m_totalRAM(static_cast<float>(MemoryUtility::GetTotalRAM()))
    , m_inverseTotalRAM(1.0f / MemoryUtility::GetTotalRAM())
{
#ifdef MAX_IMAGE_MEMORY //overwrite for testing
    m_maxMemory = MAX_IMAGE_MEMORY;
#endif

}


ImageMemoryAllocator::~ImageMemoryAllocator() {
    ASSERT(0 == m_usedMemory);
}

//----------------------------------------------------------------------------------------------------------------------

bool ImageMemoryAllocator::Allocate(uint8_t ** rawDataPtr, const uint32_t w, const uint32_t h) {
    //Allocate
    const uint32_t dataSize = CalculateMemSize(w, h);

    uint8_t*  buffer = static_cast<uint8_t*>(Allocate(dataSize));
    if (nullptr == buffer) {
        return false;
    }

    *rawDataPtr = buffer;

    return true;

}

//----------------------------------------------------------------------------------------------------------------------
void* ImageMemoryAllocator::Allocate(const size_t memSize, bool forceAllocate) {

    if (!forceAllocate && m_maxMemory != UNLIMITED_MEMORY && (m_usedMemory + memSize) > m_maxMemory)
        return nullptr;

#if OSX
    //Mac (10.9+) compresses inactive memory automatically to free memory to be used by other application, and therefore
    //we can't directly use the amount of free RAM returned by the OS (the value is often near zero).
    //For now, we simply check if the number of used memory has exceeded the total RAM.
    if (m_usedMemory + dataSize > m_totalRAM)
        return false;

#else
    const float MIN_AVAILABLE_RAM_RATIO = 0.1f;
    const float availableRAMRatio = MemoryUtility::GetAvailableRAM() * m_inverseTotalRAM;
    if (!forceAllocate && availableRAMRatio <= MIN_AVAILABLE_RAM_RATIO)
        return nullptr;

    void* buffer = (malloc(memSize));
    if (nullptr==buffer) {
        return nullptr;
    }

    std::memset(buffer,0,memSize);
    m_allocatedBuffers[buffer] = memSize;
    IncUsedMem(memSize);

    return buffer;
}

//----------------------------------------------------------------------------------------------------------------------

void* ImageMemoryAllocator::Reallocate(void* buffer, const size_t memSize, bool forceAllocate) {

    if (nullptr == buffer) {
        return nullptr;
    }
    const auto allocatedBuffer = m_allocatedBuffers.find(buffer);
    if (m_allocatedBuffers.end() == allocatedBuffer ) {
        return nullptr;
    }

    const size_t prevSize = allocatedBuffer->second;

    void* newBuffer = Allocate(memSize, forceAllocate);
    if (nullptr == newBuffer)
        return nullptr;

    std::memcpy(newBuffer, buffer, min(prevSize, memSize));
    Deallocate(buffer);
    return newBuffer;
}


//----------------------------------------------------------------------------------------------------------------------

bool ImageMemoryAllocator::Deallocate(ImageData* imageData) {
    ASSERT(nullptr!=imageData);
    if (!Deallocate(imageData->RawData)) {
        return false;
    }

    *imageData = ImageData(nullptr, 0, 0, READ_STATUS_IDLE);
    return true;
}

//----------------------------------------------------------------------------------------------------------------------

bool ImageMemoryAllocator::Deallocate(void* buffer) {

    if (nullptr == buffer) {
        return false;
    }
    const auto allocatedBuffer = m_allocatedBuffers.find(buffer);
    if (m_allocatedBuffers.end() == allocatedBuffer ) {
        return false;
    }

    const size_t memSize = allocatedBuffer->second;
    ASSERT(m_usedMemory >= memSize);
    m_allocatedBuffers.erase(buffer);
    DecUsedMem(memSize);
    free(buffer);

    return true;
}


//----------------------------------------------------------------------------------------------------------------------
void ImageMemoryAllocator::IncUsedMem(const uint64_t mem) {
    m_usedMemory += mem;
}

void ImageMemoryAllocator::DecUsedMem(const uint64_t mem) {
    m_usedMemory = (m_usedMemory >= mem) ? m_usedMemory - mem : 0;
}

uint32_t ImageMemoryAllocator::CalculateMemSize(const uint32_t w, const uint32_t h) {
    return w * h * LoaderConstants::NUM_BYTES_PER_TEXEL;
}
//----------------------------------------------------------------------------------------------------------------------


} //end namespace
