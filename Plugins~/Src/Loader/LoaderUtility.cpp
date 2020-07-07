#include "stdafx.h"

//Loader
#include "LoaderUtility.h"
#include "FileType.h"
#include "TGALoader.h"
#include "ImageCatalog.h"


//----------------------------------------------------------------------------------------------------------------------

namespace StreamingImageSequencePlugin {

FileType LoaderUtility::CheckFileType(const strType& imagePath) {
    using namespace StreamingImageSequencePlugin;
   
    const size_t length = imagePath.length();

    //Check extension validity
    if (length < 4 || imagePath[length - 4] != '.')    {
        return FILE_TYPE_INVALID;
    }

#ifdef _WIN32    // TGA is only supported on Windows
    if ((imagePath[length - 3] == 'T' || imagePath[length - 3] == 't')
        && (imagePath[length - 2] == 'G' || imagePath[length - 2] == 'g')
        && (imagePath[length - 1] == 'A' || imagePath[length - 1] == 'a'))
    {
        return FILE_TYPE_TGA;
    }
#endif
    
    return FILE_TYPE_PNG;
}

//----------------------------------------------------------------------------------------------------------------------

ImageData LoaderUtility::GetImageData(const strType& imagePath, const uint32_t imageType, 
    ImageCatalog* imageCatalog, const int frame)
{
    using namespace StreamingImageSequencePlugin;
    
    const ImageData* imageData = imageCatalog->GetImage(imagePath, imageType, frame);
    if (nullptr == imageData) {
        return ImageData(nullptr, 0, 0, READ_STATUS_UNAVAILABLE);
    }
    //if success, then the buffer must be not null
    ASSERT(imageData->CurrentReadStatus != READ_STATUS_SUCCESS || imageData->RawData);
    return *imageData;


}

//----------------------------------------------------------------------------------------------------------------------
//Returns whether the file has been processed, or is still processed  (inside readResultMap).
bool LoaderUtility::LoadAndAllocImage(const strType& imagePath, const uint32_t imageType, ImageCatalog* imageCatalog, 
    const int frame) 
{
    using namespace StreamingImageSequencePlugin;

    const ImageData imageData = LoaderUtility::GetImageData(imagePath, imageType, imageCatalog, frame);

    //Just return if the image load doesn't have any error
    if (!LoaderUtility::IsImageLoadError(imageData.CurrentReadStatus))
        return true;

    const FileType fileType = LoaderUtility::CheckFileType(imagePath);
    if (FILE_TYPE_INVALID == fileType)
        return false;

    //Loading
    if (READ_STATUS_UNAVAILABLE == imageData.CurrentReadStatus) {
        imageCatalog->PrepareImage(imagePath, imageType, frame);
    }

    switch (fileType) {
        case FILE_TYPE_TGA: {
            LoadTGAFileAndAlloc(imagePath, imageType, imageCatalog);
            break;
        }
        case FILE_TYPE_PNG: {
            LoadPNGFileAndAlloc(imagePath, imageType, imageCatalog);
            break;
        }
        default: { break; }
    }

    return true;

}

//----------------------------------------------------------------------------------------------------------------------
//Returns whether the file has been processed, or is still processed  (inside catalog).
bool LoaderUtility::LoadAndAllocImage(const strType& imagePath, const uint32_t imageType, ImageCatalog* imageCatalog
    , const uint32_t reqWidth, const uint32_t reqHeight, const int frame) 
{
    //[TODO-sin: 2020-6-4] If the resized version of this tex is not loaded, but the full version is, we can probably
    //do some optimization by resizing the full version directly, instead of loading again.

    if (!LoaderUtility::LoadAndAllocImage(imagePath, imageType, imageCatalog, frame))
        return false;

    const ImageData imageData = LoaderUtility::GetImageData(imagePath, imageType, imageCatalog, frame);
    if (LoaderUtility::IsImageLoadError(imageData.CurrentReadStatus))
        return false;


    //Still loading
    if (READ_STATUS_LOADING == imageData.CurrentReadStatus) {
        return true; 
    }

    //Already resized. 
    if (imageData.Width == reqWidth && imageData.Height == reqHeight)
        return true;

    imageCatalog->ResizeImage(imagePath, imageType, reqWidth, reqHeight);

    return true;
}

//----------------------------------------------------------------------------------------------------------------------

bool LoaderUtility::IsImageLoadError(const ReadStatus readStatus) {
    //Except these statuses, it's an error
    return (!(READ_STATUS_LOADING == readStatus || READ_STATUS_SUCCESS == readStatus));

}




} //end namespace
