// The following ifdef block is the standard way of creating macros which make exporting 
// from a DLL simpler. All files within this DLL are compiled with the DRAWOVERWINDOW_EXPORTS
// symbol defined on the command line. This symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// DRAWOVERWINDOW_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.
#if defined(DRAWOVERWINDOW_EXPORTS) || defined(PLUGIN_DLL_EXPORT)
#define DRAWOVERWINDOW_API __declspec(dllexport)
#else
#define DRAWOVERWINDOW_API __declspec(dllimport)
#endif



#include "../CommonLib/CommonLib.h"

extern "C"
{

	DRAWOVERWINDOW_API void  SetAllAreLoaded(int sInstanceId, int flag);
	DRAWOVERWINDOW_API int   GetAllAreLoaded(int sInstanceId);

	DRAWOVERWINDOW_API void ResetOverwrapWindows();

}
