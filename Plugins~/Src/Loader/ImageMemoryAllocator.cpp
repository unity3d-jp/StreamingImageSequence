#include "ImageMemoryAllocator.h"

#include "LoaderConstants.h"

namespace StreamingImageSequencePlugin {

ImageMemoryAllocator::~ImageMemoryAllocator() {
    ASSERT(0 == m_usedMemory);
}

//----------------------------------------------------------------------------------------------------------------------

bool ImageMemoryAllocator::Allocate(uint8_t ** rawDataPtr, const uint32_t w, const uint32_t h) {
    //Allocate
    const uint32_t dataSize = CalculateMemSize(w, h);
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

    if (nullptr != imageData->RawData) {
        const uint64_t mem = CalculateMemSize(imageData->Width, imageData->Height);
        ASSERT(m_usedMemory >= mem);
        DecUsedMem(mem);
        free(imageData->RawData);
    }

    *imageData = ImageData(nullptr, 0, 0, READ_STATUS_NONE);

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
