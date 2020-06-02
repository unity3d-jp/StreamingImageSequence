// The following ifdef block is the standard way of creating macros which make exporting 
// from a DLL simpler. All files within this DLL are compiled with the LOADERWIN_EXPORTS
// symbol defined on the command line. This symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// LOADER_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.
#ifdef _WIN32
#  if defined(LOADERWIN_EXPORTS) || defined(PLUGIN_DLL_EXPORT)
#  define LOADER_API __declspec(dllexport)
#  else
#  define LOADER_API __declspec(dllimport)
#  endif
#else
#  define LOADER_API
#endif

                                                                                                                        
#include "../CommonLib/CommonLib.h"

LOADER_API    std::map<strType, StReadResult>  g_fileNameToPtrMap[StreamingImageSequencePlugin::MAX_CRITICAL_SECTION_TYPE_TEXTURES];


extern "C"
{
	LOADER_API bool  LoadAndAllocFullTexture(const charType* ptr);
	LOADER_API bool  LoadAndAllocPreviewTexture(const charType* ptr, const uint32_t width, const uint32_t height);
	LOADER_API void  NativeFree(void* ptr);
	LOADER_API bool  GetNativeTextureInfo(const charType* fileName, StReadResult* pResult, const uint32_t textureType);
	LOADER_API int   ResetNativeTexture(const charType* fileName);

	LOADER_API void  ListLoadedTextures(const uint32_t textureType, void(* OnNextTexture)(const char*));

	LOADER_API void  ResetAllLoadedTextures();

	LOADER_API void  ResetPlugin();
}
