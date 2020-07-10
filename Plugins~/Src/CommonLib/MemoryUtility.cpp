
#include "stdafx.h"
#include "MemoryUtility.h"

namespace StreamingImageSequencePlugin {

#ifdef _WIN32

inline void GetGlobalMemoryStatusInto(MEMORYSTATUSEX* memStatus) {
    memStatus->dwLength = sizeof(MEMORYSTATUSEX);
    GlobalMemoryStatusEx(memStatus);
}

//----------------------------------------------------------------------------------------------------------------------
uint64_t MemoryUtility::GetTotalRAM() {
    MEMORYSTATUSEX memStatus;
    GetGlobalMemoryStatusInto(&memStatus);
    return memStatus.ullTotalPhys;    
}

uint64_t MemoryUtility::GetUsedRAM() {
    MEMORYSTATUSEX memStatus;
    GetGlobalMemoryStatusInto(&memStatus);
    return (memStatus.ullTotalPhys - memStatus.ullAvailPhys);
}

uint64_t MemoryUtility::GetAvailableRAM() {
    MEMORYSTATUSEX memStatus;
    GetGlobalMemoryStatusInto(&memStatus);
    return memStatus.ullAvailPhys;
   
}
float MemoryUtility::GetUsedRAMRatio() {
    MEMORYSTATUSEX memStatus;
    GetGlobalMemoryStatusInto(&memStatus);
    const float usedRAM = (static_cast<float>(memStatus.ullTotalPhys - memStatus.ullAvailPhys) 
                               / static_cast<float>(memStatus.ullTotalPhys));
    return usedRAM;
}

float MemoryUtility::GetAvailableRAMRatio() {
    MEMORYSTATUSEX memStatus;
    GetGlobalMemoryStatusInto(&memStatus);

    const float availableRAM= (static_cast<float>(memStatus.ullAvailPhys) 
                               / static_cast<float>(memStatus.ullTotalPhys));
    return availableRAM;
}

#else

DWORDLONG MemoryUtility::GetTotalRAM() {
    return 0;

}
DWORDLONG MemoryUtility::GetUsedRAM() {
    return 0;

}
DWORDLONG MemoryUtility::GetAvailableRAM() {
    return 0;

}
float MemoryUtility::GetUsedRAMRatio() {
    return 0;

}

float MemoryUtility::GetAvailableRAMRatio() {
    return 0;
}

#endif //_WIN32
} //end namespace
