
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

#elif OSX

uint64_t MemoryUtility::GetTotalRAM() {

    struct sysinfo memInfo;
    sysinfo (&memInfo);

    uint64_t totalRAM = memInfo.totalram;
    totalRAM *= memInfo.mem_unit;
    return totalRAM;

}
uint64_t MemoryUtility::GetUsedRAM() {
    struct sysinfo memInfo;
    sysinfo (&memInfo);

    uint64_t usedRAM = memInfo.totalram - memInfo.freeram;
    usedRAM *= memInfo.mem_unit;

    return usedRAM;

}
uint64_t MemoryUtility::GetAvailableRAM() {
    struct sysinfo memInfo;
    sysinfo (&memInfo);

    uint64_t availableRAM = memInfo.freeram;
    availableRAM *= memInfo.mem_unit;
    return 0;

}
float MemoryUtility::GetUsedRAMRatio() {
    struct sysinfo memInfo;
    sysinfo (&memInfo);
    const float usedRAM = (static_cast<float>(memInfo.totalram - memInfo.freeram)
                           / static_cast<float>(memInfo.totalram));
    return usedRAM;

}

float MemoryUtility::GetAvailableRAMRatio() {
    struct sysinfo memInfo;
    sysinfo (&memInfo);
    const float availableRAM = (static_cast<float>(memInfo.freeram)
                               / static_cast<float>(memInfo.totalram));
    return availableRAM;
}

#else

uint64_t MemoryUtility::GetTotalRAM() {

    struct sysinfo memInfo;
    sysinfo (&memInfo);

    uint64_t totalRAM = memInfo.totalram;
    totalRAM *= memInfo.mem_unit;
    return totalRAM;

}
uint64_t MemoryUtility::GetUsedRAM() {
    struct sysinfo memInfo;
    sysinfo (&memInfo);

    uint64_t usedRAM = memInfo.totalram - memInfo.freeram;
    usedRAM *= memInfo.mem_unit;

    return usedRAM;

}
uint64_t MemoryUtility::GetAvailableRAM() {
    struct sysinfo memInfo;
    sysinfo (&memInfo);

    uint64_t availableRAM = memInfo.freeram;
    availableRAM *= memInfo.mem_unit;
    return 0;

}
float MemoryUtility::GetUsedRAMRatio() {
    struct sysinfo memInfo;
    sysinfo (&memInfo);
    const float usedRAM = (static_cast<float>(memInfo.totalram - memInfo.freeram) 
                           / static_cast<float>(memInfo.totalram));
    return usedRAM;

}

float MemoryUtility::GetAvailableRAMRatio() {
    struct sysinfo memInfo;
    sysinfo (&memInfo);
    const float availableRAM = (static_cast<float>(memInfo.freeram) 
                               / static_cast<float>(memInfo.totalram));
    return availableRAM;
}

#endif //_WIN32
} //end namespace
