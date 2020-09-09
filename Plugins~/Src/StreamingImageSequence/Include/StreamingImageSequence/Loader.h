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

                                                                                                                        
#include "CommonLib/CommonLib.h"

namespace StreamingImageSequencePlugin {
struct ImageData;
} //end namespace

extern "C"
{
	LOADER_API bool  LoadAndAllocFullImage(const charType* ptr, const int frame);
	LOADER_API bool  LoadAndAllocPreviewImage(const charType* ptr, const uint32_t width, const uint32_t height, const int frame);
	LOADER_API void  GetImageDataInto(const charType* imagePath, const uint32_t imageType, const int frame
		, StreamingImageSequencePlugin::ImageData* readResult);
	LOADER_API int   UnloadImage(const charType* fileName);
	LOADER_API void  UnloadAllImages();

	LOADER_API void  ListLoadedImages(const uint32_t imageType, void(* OnNextTexture)(const char*));
	LOADER_API int  GetImageLoadOrder(const uint32_t imageType);
	LOADER_API uint32_t GetNumLoadedImages(const uint32_t imageType);

	LOADER_API void  SetMaxImagesMemory(const uint32_t maxImageMemoryMB);
	LOADER_API int GetUsedImagesMemory();


	LOADER_API void  ResetPlugin();
	LOADER_API void  ResetImageLoadOrder();
}

//----------------------------------------------------------------------------------------------------------------------

