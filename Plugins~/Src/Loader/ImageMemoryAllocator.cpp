#include "ImageMemoryAllocator.h"

#include "CommonLib/MemoryUtility.h"

#include "LoaderConstants.h"

namespace StreamingImageSequencePlugin {

const uint64_t UNLIMITED_MEMORY = 0;
const uint64_t DEFAULT_MAX_MEMORY = UNLIMITED_MEMORY;

//----------------------------------------------------------------------------------------------------------------------

ImageMemoryAllocator::ImageMemoryAllocator() : m_usedMemory(0)
    , m_maxMemory(DEFAULT_MAX_MEMORY)
    , m_inverseTotalRAM(1.0f / MemoryUtility::GetTotalRAM())
{
#ifdef MAX_IMAGE_MEMORY
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

    if (m_maxMemory != UNLIMITED_MEMORY && (m_usedMemory + dataSize) > m_maxMemory)
        return false;

    const float MIN_AVAILABLE_RAM_RATIO = 0.1f;

    const float availableRAMRatio = MemoryUtility::GetAvailableRAM() * m_inverseTotalRAM;
    if (availableRAMRatio <= MIN_AVAILABLE_RAM_RATIO)
        return false;


    uint8_t*  buffer = static_cast<uint8_t*>(malloc(dataSize));
    if (nullptr == buffer) {
        return false;
    }

    std::memset(buffer,0,dataSize);
    *rawDataPtr = buffer;

    IncUsedMem(dataSize);

    return true;

}

//----------------------------------------------------------------------------------------------------------------------

void ImageMemoryAllocator::Deallocate(ImageData* imageData) {
    ASSERT(nullptr!=imageData);

    if (nullptr == imageData->RawData) {
        return;
    }

    const uint64_t mem = CalculateMemSize(imageData->Width, imageData->Height);
    ASSERT(m_usedMemory >= mem);
    DecUsedMem(mem);
    free(imageData->RawData);
    *imageData = ImageData(nullptr, 0, 0, READ_STATUS_IDLE);

}

//----------------------------------------------------------------------------------------------------------------------
void ImageMemoryAllocator::IncUsedMem(const uint64_t mem) {
    m_usedMemory += mem;
}

void ImageMemoryAllocator::DecUsedMem(const uint64_t mem) {
    m_usedMemory = (m_usedMemory >= mem) ? m_usedMemory - mem : 0;
}

//----------------------------------------------------------------------------------------------------------------------

uint32_t ImageMemoryAllocator::CalculateMemSize(const uint32_t w, const uint32_t h) {
    return w * h * LoaderConstants::NUM_BYTES_PER_TEXEL;
}



} //end namespace
