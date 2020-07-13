
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

    int mib[2];
    uint64_t totalRAM;
    mib[0] = CTL_HW;
    mib[1] = HW_MEMSIZE;
    length = sizeof(uint64_t);
    sysctl(mib, 2, &totalRAM, &length, NULL, 0);
    return totalRAM;

}
uint64_t MemoryUtility::GetUsedRAM() {

    vm_size_t pageSize;
    mach_port_t machPort;
    mach_msg_type_number_t count;
    vm_statistics64_data_t vmStats;

    machPort = mach_host_self();
    count = sizeof(vmStats) / sizeof(natural_t);
    if (KERN_SUCCESS == host_pageSize(machPort, &pageSize) &&
        KERN_SUCCESS == host_statistics64(machPort, HOST_VM_INFO,
                                          (host_info64_t)&vmStats, &count))
    {

        const uint64_t usedRAM = ((int64_t)vmStats.active_count +
                                 (int64_t)vmStats.inactive_count +
                                 (int64_t)vmStats.wire_count) *  (int64_t)pageSize;
        return usedRAM;
    }
    return 0;

}
uint64_t MemoryUtility::GetAvailableRAM() {
    vm_size_t pageSize;
    mach_port_t machPort;
    mach_msg_type_number_t count;
    vm_statistics64_data_t vmStats;

    machPort = mach_host_self();
    count = sizeof(vmStats) / sizeof(natural_t);
    if (KERN_SUCCESS == host_pageSize(machPort, &pageSize) &&
        KERN_SUCCESS == host_statistics64(machPort, HOST_VM_INFO,
                                          (host_info64_t)&vmStats, &count))
    {
        const uint64_t freeRAM = (int64_t)vmStats.free_count * (int64_t)pageSize;
        return freeRAM;
    }

    return 0;
}

float MemoryUtility::GetUsedRAMRatio() {
    const uint64_t usedRAM  = GetUsedRAM();
    const uint64_t totalRAM = GetTotalRAM();
    const float usedRAM = (static_cast<float>(usedRAM) / static_cast<float>(totalRAM));
    return usedRAM;

}

float MemoryUtility::GetAvailableRAMRatio() {
    const uint64_t availableRAM = GetAvailableRAM();
    const uint64_t totalRAM = GetTotalRAM();
    const float availableRAMRatio = (static_cast<float>(availableRAM) / static_cast<float>(totalRAM));
    return availableRAMRatio;
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
