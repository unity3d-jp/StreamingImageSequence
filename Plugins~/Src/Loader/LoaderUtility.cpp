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
//Returns whether the file has been processed, or is still processed  (inside readResultMap).

bool LoaderUtility::GetImageDataInto(const strType& imagePath, const uint32_t imageType, 
    ImageCatalog* imageCatalog, ImageData* pResult)
{
    using namespace StreamingImageSequencePlugin;
    ASSERT(pResult);
    pResult->CurrentReadStatus = READ_STATUS_NONE;
    
    const ImageData* imageData = imageCatalog->GetImage(imagePath, imageType);
    if (nullptr == imageData)
        return false;

    *pResult = *imageData;

    //if success, then the buffer must be not null
    ASSERT(pResult->CurrentReadStatus != READ_STATUS_SUCCESS || pResult->RawData);

    return true;


}

//----------------------------------------------------------------------------------------------------------------------
//Returns whether the file has been processed, or is still processed  (inside readResultMap).
bool LoaderUtility::LoadAndAllocImage(const strType& imagePath, const uint32_t imageType, ImageCatalog* imageCatalog) 
{
    using namespace StreamingImageSequencePlugin;
    ImageData readResult;

    const bool isProcessed = LoaderUtility::GetImageDataInto(imagePath, imageType, imageCatalog, &readResult );
    if (isProcessed) {
        return true;
    }

    const FileType fileType = LoaderUtility::CheckFileType(imagePath);
    if (FILE_TYPE_INVALID == fileType)
        return false;

    //Loading
    imageCatalog->PrepareImage(imagePath, imageType);

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
    , const uint32_t reqWidth, const uint32_t reqHeight) 
{
    //[TODO-sin: 2020-6-4] If the resized version of this tex is not loaded, but the full version is, we can probably
    //do some optimization by resizing the full version directly, instead of loading again.

    if (!LoaderUtility::LoadAndAllocImage(imagePath, imageType, imageCatalog))
        return false;

    ImageData imageData;
    if (!LoaderUtility::GetImageDataInto(imagePath, imageType, imageCatalog, &imageData ))
        return false;

    //Buffer is still null. Means, still loading
    if (NULL == imageData.RawData) {
        return true; 
    }

    //Already resized. 
    if (imageData.Width == reqWidth && imageData.Height == reqHeight)
        return true;

    imageCatalog->ResizeImage(imagePath, imageType, reqWidth, reqHeight);

    return true;
}



} //end namespace
