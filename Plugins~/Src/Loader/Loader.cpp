// Loader.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include "Loader.h"

//CommonLib
#include "CommonLib/CommonLib.h"
#include "CommonLib/CriticalSectionController.h"


//Loader
#include "TGALoader.h"
#include "FileType.h"
#include "LoaderUtility.h"
#include "ImageCatalog.h"


using namespace std;

LOADER_API std::map<strType, int>           g_scenePathToSceneStatus;


//----------------------------------------------------------------------------------------------------------------------

//Get the texture info and return the result inside ReadResult. Thread-safe
LOADER_API bool GetNativeTextureInfo(const charType* imagePath, StReadResult* readResult, const uint32_t imageType) {
    using namespace StreamingImageSequencePlugin;
    CriticalSectionController cs(TEXTURE_CS(imageType));
    return LoaderUtility::GetImageDataInto(imagePath, imageType, &ImageCatalog::GetInstance(), readResult);
}

//----------------------------------------------------------------------------------------------------------------------

//Returns if the imagePath can be loaded (). Thread-safe
LOADER_API bool LoadAndAllocFullTexture(const charType* imagePath) {

    using namespace StreamingImageSequencePlugin;
    const uint32_t imageType = CRITICAL_SECTION_TYPE_FULL_TEXTURE;
	CriticalSectionController cs(TEXTURE_CS(CRITICAL_SECTION_TYPE_FULL_TEXTURE));
	return LoaderUtility::LoadAndAllocImage(imagePath, imageType, &ImageCatalog::GetInstance());
}

//----------------------------------------------------------------------------------------------------------------------
//Returns if the imagePath can be loaded (). Thread-safe
LOADER_API bool LoadAndAllocPreviewTexture(const charType* imagePath, const uint32_t width, const uint32_t height) {
	using namespace StreamingImageSequencePlugin;
	const uint32_t imageType = CRITICAL_SECTION_TYPE_PREVIEW_TEXTURE;
	CriticalSectionController cs(TEXTURE_CS(imageType));
	return LoaderUtility::LoadAndAllocImage(imagePath, imageType, &ImageCatalog::GetInstance(),width,height);
}

//----------------------------------------------------------------------------------------------------------------------
// return succ:0 fail:-1
LOADER_API int   ResetNativeTexture(const charType* imagePath) {
    using namespace StreamingImageSequencePlugin;
	ImageCatalog& imageCatalog = ImageCatalog::GetInstance();

	//Reset all textures
	for (uint32_t imageType = 0; imageType < MAX_CRITICAL_SECTION_TYPE_TEXTURES; ++imageType) {
		CriticalSectionController cs0(TEXTURE_CS(imageType));
		imageCatalog.UnloadImage(imagePath, imageType);
	}


	return 0;

}

//----------------------------------------------------------------------------------------------------------------------
LOADER_API void ListLoadedTextures(const uint32_t imageType, void(*OnNextTexture)(const char*)) {
	using namespace StreamingImageSequencePlugin;
	ASSERT(imageType < MAX_CRITICAL_SECTION_TYPE_TEXTURES);

	const std::map<strType, StReadResult> images = ImageCatalog::GetInstance().GetImageCollection(imageType);
	for (auto itr = images.begin(); itr != images.end(); ++itr) {
		OnNextTexture(itr->first.c_str());
	}
}

//----------------------------------------------------------------------------------------------------------------------

LOADER_API uint32_t GetNumLoadedTextures(const uint32_t imageType) {
	using namespace StreamingImageSequencePlugin;
	ASSERT(imageType < MAX_CRITICAL_SECTION_TYPE_TEXTURES);
	return static_cast<uint32_t>(ImageCatalog::GetInstance().GetNumImages(imageType));
}

//----------------------------------------------------------------------------------------------------------------------

LOADER_API void   SetSceneStatus(const charType* scenePath, int sceneStatus)
{
	g_scenePathToSceneStatus[scenePath] = sceneStatus;

}
LOADER_API int    GetSceneStatus(const charType* scenePath)
{
	strType wstr(scenePath);
	if (g_scenePathToSceneStatus.find(wstr) != g_scenePathToSceneStatus.end())
	{
		return g_scenePathToSceneStatus[wstr];
	}
	return -1;	// not found;
}

//----------------------------------------------------------------------------------------------------------------------
LOADER_API void  ResetPlugin() {
	ResetAllLoadedTextures();
}

LOADER_API void  ResetAllLoadedTextures() {
	using namespace StreamingImageSequencePlugin;


	CriticalSectionController cs0(TEXTURE_CS(CRITICAL_SECTION_TYPE_FULL_TEXTURE));
	CriticalSectionController cs1(TEXTURE_CS(CRITICAL_SECTION_TYPE_PREVIEW_TEXTURE));

	ImageCatalog::GetInstance().UnloadAllImages();

}

