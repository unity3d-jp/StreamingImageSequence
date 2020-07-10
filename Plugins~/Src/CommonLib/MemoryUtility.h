#pragma once

#include <cstdint>

namespace StreamingImageSequencePlugin {

class MemoryUtility {
public:

    static uint64_t GetTotalRAM();
    static uint64_t GetUsedRAM();
    static uint64_t GetAvailableRAM();
    static float GetUsedRAMRatio();
    static float GetAvailableRAMRatio();

    
};


} //end namespace
