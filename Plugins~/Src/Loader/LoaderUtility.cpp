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

const ImageData* LoaderUtility::GetImageData(const strType& imagePath, const uint32_t imageType, 
    ImageCatalog* imageCatalog, const int frame)
{
    using namespace StreamingImageSequencePlugin;
    
    const ImageData* imageData = imageCatalog->GetImage(imagePath, imageType, frame);
    if (nullptr == imageData) {
        return nullptr;
    }
    //if success, then the buffer must be not null
    ASSERT(imageData->CurrentReadStatus != READ_STATUS_SUCCESS || imageData->RawData);
    return imageData;


}

//----------------------------------------------------------------------------------------------------------------------
//Returns whether the file has been processed, or is still processed  (inside readResultMap).
const ImageData* LoaderUtility::LoadAndAllocImage(const strType& imagePath, const uint32_t imageType, ImageCatalog* imageCatalog, 
    const int frame) 
{
    using namespace StreamingImageSequencePlugin;

    const ImageData* imageData = LoaderUtility::GetImageData(imagePath, imageType, imageCatalog, frame);

    const FileType fileType = LoaderUtility::CheckFileType(imagePath);
    if (FILE_TYPE_INVALID == fileType)
        return nullptr;

    //Just return if the image load doesn't have any error
    if (nullptr!=imageData) {
        if (!LoaderUtility::IsImageLoadError(imageData->CurrentReadStatus))
            return imageData;

    } else {
        imageData = imageCatalog->AddImage(imagePath, imageType, frame);

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

    return imageData;

}

//----------------------------------------------------------------------------------------------------------------------
//Returns whether the file has been processed, or is still processed  (inside catalog).
const ImageData* LoaderUtility::LoadAndAllocImage(const strType& imagePath, const uint32_t imageType, ImageCatalog* imageCatalog
    , const uint32_t reqWidth, const uint32_t reqHeight, const int frame) 
{
    switch (imageType) {
        case CRITICAL_SECTION_TYPE_FULL_IMAGE: {
            return LoaderUtility::LoadAndAllocImage(imagePath, imageType, imageCatalog, frame);
        }
        case CRITICAL_SECTION_TYPE_PREVIEW_IMAGE: {
            const ImageData* previewImageData = LoaderUtility::GetImageData(imagePath, CRITICAL_SECTION_TYPE_PREVIEW_IMAGE, 
                                                                            imageCatalog, frame);

            //Just return if the image load doesn't have any error
            if (nullptr!=previewImageData && !LoaderUtility::IsImageLoadError(previewImageData->CurrentReadStatus))
                return previewImageData;


            //Load full image
            const ImageData* fullImageData = LoadAndAllocImage(imagePath, CRITICAL_SECTION_TYPE_FULL_IMAGE, 
                                                               imageCatalog, frame);
            if (nullptr == fullImageData)
                return nullptr;

            if ((fullImageData->CurrentReadStatus == READ_STATUS_LOADING))
                return fullImageData;

            imageCatalog->AddImageFromSrc(imagePath, CRITICAL_SECTION_TYPE_PREVIEW_IMAGE, frame, 
                                          fullImageData, reqWidth, reqHeight);
            return previewImageData;
        }
        default: {
            return nullptr;
        }
    }

    return nullptr;
}

//----------------------------------------------------------------------------------------------------------------------

bool LoaderUtility::IsImageLoadError(const ReadStatus readStatus) {
    //Except these statuses, it's an error
    return (!(READ_STATUS_LOADING == readStatus || READ_STATUS_SUCCESS == readStatus));

}




} //end namespace
