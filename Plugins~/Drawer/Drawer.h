// The following ifdef block is the standard way of creating macros which make exporting 
// from a DLL simpler. All files within this DLL are compiled with the DRAWERWIN_EXPORTS
// symbol defined on the command line. This symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// DRAWERWIN_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.
#ifdef _WIN32
#  ifdef DRAWERWIN_EXPORTS
#  define DRAWERWIN_API __declspec(dllexport)
#  else
#  define DRAWERWIN_API __declspec(dllimport)
#  endif
#else
#  define DRAWERWIN_API 
#endif

#ifdef _WIN32
# include <d3d11.h>
# include <d3d9.h>
#endif
#include <thread>
#include <mutex>

#include "Unity/IUnityInterface.h"
#include "Unity/IUnityGraphics.h"

#include "../CommonLib/CommonLib.h"

extern "C"
{	UNITY_INTERFACE_EXPORT void UNITY_INTERFACE_API ResetAllLoadedTextures();
}

