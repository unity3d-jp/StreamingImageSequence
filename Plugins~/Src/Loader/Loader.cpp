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

//----------------------------------------------------------------------------------------------------------------------

//Get the texture info and return the result inside ReadResult. Thread-safe
LOADER_API void GetImageDataInto(const charType* imagePath, const uint32_t imageType, const int frame
	, StreamingImageSequencePlugin::ImageData* readResult) 
{
    using namespace StreamingImageSequencePlugin;
    CriticalSectionController cs(TEXTURE_CS(imageType));
	ImageCatalog& imageCatalog = ImageCatalog::GetInstance();
	*readResult = LoaderUtility::GetImageData(imagePath, imageType, &imageCatalog, frame);
}

//----------------------------------------------------------------------------------------------------------------------

//Returns if the imagePath can be loaded (). Thread-safe
LOADER_API bool LoadAndAllocFullImage(const charType* imagePath, const int frame) {

    using namespace StreamingImageSequencePlugin;
    const uint32_t imageType = CRITICAL_SECTION_TYPE_FULL_IMAGE;
	CriticalSectionController cs(TEXTURE_CS(CRITICAL_SECTION_TYPE_FULL_IMAGE));
	ImageCatalog& imageCatalog = ImageCatalog::GetInstance();
	return LoaderUtility::LoadAndAllocImage(imagePath, imageType, &imageCatalog, frame);
}

//----------------------------------------------------------------------------------------------------------------------
//Returns if the imagePath can be loaded (). Thread-safe
LOADER_API bool LoadAndAllocPreviewImage(const charType* imagePath, const uint32_t width, const uint32_t height, const int frame) {
	using namespace StreamingImageSequencePlugin;
	const uint32_t imageType = CRITICAL_SECTION_TYPE_PREVIEW_IMAGE;
	CriticalSectionController cs(TEXTURE_CS(imageType));
	ImageCatalog& imageCatalog = ImageCatalog::GetInstance();
	return LoaderUtility::LoadAndAllocImage(imagePath, imageType, &imageCatalog,width,height, frame);
}

//----------------------------------------------------------------------------------------------------------------------
// return succ:0 fail:-1
LOADER_API int   UnloadImage(const charType* imagePath) {
    using namespace StreamingImageSequencePlugin;
	ImageCatalog& imageCatalog = ImageCatalog::GetInstance();

	//Reset all textures
	for (uint32_t imageType = 0; imageType < MAX_CRITICAL_SECTION_TYPE_IMAGES; ++imageType) {
		CriticalSectionController cs0(TEXTURE_CS(imageType));
		imageCatalog.UnloadImage(imagePath, imageType);
	}


	return 0;

}

//----------------------------------------------------------------------------------------------------------------------
LOADER_API void  UnloadAllImages() {
	using namespace StreamingImageSequencePlugin;

	CriticalSectionController cs0(TEXTURE_CS(CRITICAL_SECTION_TYPE_FULL_IMAGE));
	CriticalSectionController cs1(TEXTURE_CS(CRITICAL_SECTION_TYPE_PREVIEW_IMAGE));

	ImageCatalog::GetInstance().Reset();
}


//----------------------------------------------------------------------------------------------------------------------
LOADER_API void ListLoadedImages(const uint32_t imageType, void(*OnNextTexture)(const char*)) {
	using namespace StreamingImageSequencePlugin;
	ASSERT(imageType < MAX_CRITICAL_SECTION_TYPE_IMAGES);

	const std::map<strType, ImageData> images = ImageCatalog::GetInstance().GetImageMap(imageType);
	for (auto itr = images.begin(); itr != images.end(); ++itr) {
		OnNextTexture(itr->first.c_str());
	}
}

//----------------------------------------------------------------------------------------------------------------------

LOADER_API uint32_t GetNumLoadedTextures(const uint32_t imageType) {
	using namespace StreamingImageSequencePlugin;
	ASSERT(imageType < MAX_CRITICAL_SECTION_TYPE_IMAGES);
	return static_cast<uint32_t>(ImageCatalog::GetInstance().GetNumImages(imageType));
}

//----------------------------------------------------------------------------------------------------------------------
LOADER_API void  ResetPlugin() {
	UnloadAllImages();
}

