// stdafx.h : include file for standard system include files,
// or project specific include files that are used frequently, but
// are changed infrequently
//

#ifndef STDAFX_H
#define STDAFX_H
#ifdef _WIN32
#include "targetver.h"

#define WIN32_LEAN_AND_MEAN             // Exclude rarely-used stuff from Windows headers
// Windows Header Files:
#include <windows.h>

#else

#include "sys/types.h"
#include "sys/sysinfo.h"

#endif // _WIN32

#endif

// TODO: reference additional headers your program requires here
