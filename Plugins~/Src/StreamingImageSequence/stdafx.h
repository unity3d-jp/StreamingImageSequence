// stdafx.h : include file for standard system include files, or project specific include files 
// that are used frequently, but are changed infrequently

#ifndef STDAFX_H
#define STDAFX_H
#ifdef _WIN32
#include "targetver.h"

#define WIN32_LEAN_AND_MEAN             // Exclude rarely-used stuff from Windows headers

// Windows Header Files:
#include <windows.h>
#undef LoadImage //Avoid using LoadImage macro in Windows header file

#endif // _WIN32

#include <cstdint> //uint64_t
#include <unordered_map> //std::unordered_map

#endif // STDAFX_H

