// The following ifdef block is the standard way of creating macros which make exporting 
// from a DLL simpler. All files within this DLL are compiled with the LOADERWIN_EXPORTS
// symbol defined on the command line. This symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// LOADERWIN_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.
#ifdef _WIN32
#  ifdef LOADERWIN_EXPORTS
#  define LOADERWIN_API __declspec(dllexport)
#  else
#  define LOADERWIN_API __declspec(dllimport)
#  endif
#else
#  define LOADERWIN_API
#endif

                                                                                                                        
#include "../CommonLib/CommonLib.h"


extern "C"
{
	LOADERWIN_API bool  LoadAndAlloc(const charType* ptr);
	LOADERWIN_API void   NativeFree(void* ptr);
	LOADERWIN_API bool  GetNativeTextureInfo(const charType* fileName, StReadResult* pResult);
	LOADERWIN_API int   ResetNativeTexture(const charType* fileName);
	LOADERWIN_API void  ResetPlugin();
	LOADERWIN_API void  DoneResetPlugin();
	LOADERWIN_API int   IsPluginResetting();
}
