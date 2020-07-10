#pragma once

#include "stdafx.h"

namespace StreamingImageSequencePlugin {

class MemoryUtility {
public:

    static DWORDLONG GetTotalRAM();
    static DWORDLONG GetUsedRAM();
    static DWORDLONG GetAvailableRAM();
    static float GetUsedRAMRatio();
    static float GetAvailableRAMRatio();

    
};


} //end namespace
