#include "stdafx.h"
#include "Loader.h"

//CommonLib
#include "CommonLib/CriticalSectionController.h"


//Loader
#include "LoaderUtility.h"
#include "ImageCatalog.h"

//----------------------------------------------------------------------------------------------------------------------

//Get the texture info and return the result inside ReadResult. Thread-safe
LOADER_API void GetImageDataInto(const charType* imagePath, const uint32_t imageType, const int frame
	, StreamingImageSequencePlugin::ImageData* readResult) 
{
    using namespace StreamingImageSequencePlugin;
	ImageCatalog& imageCatalog = ImageCatalog::GetInstance();
	const ImageData* imageData = (LoaderUtility::GetImageData(imagePath, imageType, &imageCatalog, frame));
	if (nullptr != imageData) {
		*readResult = *imageData;
	} else {
		*readResult = ImageData(nullptr, 0, 0, READ_STATUS_UNAVAILABLE);
	}
}

//----------------------------------------------------------------------------------------------------------------------

//Returns if the imagePath can be loaded (). Thread-safe
LOADER_API bool LoadAndAllocFullImage(const charType* imagePath, const int frame) {

    using namespace StreamingImageSequencePlugin;
    const uint32_t imageType = CRITICAL_SECTION_TYPE_FULL_IMAGE;
	ImageCatalog& imageCatalog = ImageCatalog::GetInstance();
	const ImageData* imageData = LoaderUtility::LoadAndAllocImage(imagePath, imageType, &imageCatalog, frame);
	return (nullptr!=imageData);
}

//----------------------------------------------------------------------------------------------------------------------
//Returns if the imagePath can be loaded (). Thread-safe
LOADER_API bool LoadAndAllocPreviewImage(const charType* imagePath, const uint32_t width, const uint32_t height, const int frame) {
	using namespace StreamingImageSequencePlugin;
	const uint32_t imageType = CRITICAL_SECTION_TYPE_PREVIEW_IMAGE;
	ImageCatalog& imageCatalog = ImageCatalog::GetInstance();
	const ImageData* imageData = LoaderUtility::LoadAndAllocImage(imagePath, imageType, &imageCatalog,width,height, frame);
	return (nullptr!=imageData);
}

//----------------------------------------------------------------------------------------------------------------------
// return success:0 fail:-1
LOADER_API int   UnloadImage(const charType* imagePath) {
    using namespace StreamingImageSequencePlugin;
	ImageCatalog& imageCatalog = ImageCatalog::GetInstance();

	//Reset all textures
	for (uint32_t imageType = 0; imageType < MAX_CRITICAL_SECTION_TYPE_IMAGES; ++imageType) {
		imageCatalog.UnloadImage(imagePath, imageType);
	}


	return 0;

}

//----------------------------------------------------------------------------------------------------------------------
LOADER_API void  UnloadAllImages() {
	using namespace StreamingImageSequencePlugin;

	ImageCatalog::GetInstance().Reset();
}


//----------------------------------------------------------------------------------------------------------------------
LOADER_API void ListLoadedImages(const uint32_t imageType, void(*OnNextTexture)(const char*)) {
	using namespace StreamingImageSequencePlugin;
	ASSERT(imageType < MAX_CRITICAL_SECTION_TYPE_IMAGES);

	const std::unordered_map<strType, ImageData> images = ImageCatalog::GetInstance().GetImageMap(imageType);
	for (auto itr = images.begin(); itr != images.end(); ++itr) {
		OnNextTexture(itr->first.c_str());
	}
}

//----------------------------------------------------------------------------------------------------------------------
LOADER_API int  GetLatestFrame(const uint32_t imageType) {
	using namespace StreamingImageSequencePlugin;
	ASSERT(imageType < MAX_CRITICAL_SECTION_TYPE_IMAGES);
	const int latestFrame = ImageCatalog::GetInstance().GetLatestFrame(imageType);
	return latestFrame;
}

//----------------------------------------------------------------------------------------------------------------------

LOADER_API uint32_t GetNumLoadedTextures(const uint32_t imageType) {
	using namespace StreamingImageSequencePlugin;
	ASSERT(imageType < MAX_CRITICAL_SECTION_TYPE_IMAGES);
	return static_cast<uint32_t>(ImageCatalog::GetInstance().GetNumImages(imageType));
}

//----------------------------------------------------------------------------------------------------------------------

LOADER_API void  SetMaxImagesMemory(const uint32_t maxImageMemoryMB) {
	using namespace StreamingImageSequencePlugin;
	ImageCatalog& imageCatalog = ImageCatalog::GetInstance();
	imageCatalog.SetMaxMemory(maxImageMemoryMB * 1024 * 1024); //to bytes
}

//----------------------------------------------------------------------------------------------------------------------
LOADER_API void  ResetPlugin() {
	UnloadAllImages();
	ResetImageLoadOrder();
}

LOADER_API void  ResetImageLoadOrder() {
	using namespace StreamingImageSequencePlugin;
	ImageCatalog& imageCatalog = ImageCatalog::GetInstance();
	imageCatalog.ResetOrder();
}

