#include "stdafx.h"

//CommonLib
#include "CommonLib/CommonLib.h" //IMAGE_CS
#include "CommonLib/CriticalSectionController.h"

//Loader
#include "StreamingImageSequence/LoaderUtility.h"
#include "StreamingImageSequence/ImageCatalog.h"
#include "StreamingImageSequence/FileType.h"
#include "TGALoader.h"



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
//Returns the corresponding ImageData. Can be null
const ImageData* LoaderUtility::LoadAndAllocImage(const strType& imagePath, const uint32_t imageType, ImageCatalog* imageCatalog, 
    const int frame) 
{
    using namespace StreamingImageSequencePlugin;

    //Check if the image is already processed. If it does, just return if the image load doesn't have any error
    const ImageData* imageData = LoaderUtility::GetImageData(imagePath, imageType, imageCatalog, frame);
    if (nullptr!=imageData) {
        if (!LoaderUtility::IsImageLoadError(imageData->CurrentReadStatus))
            return imageData;
    } else {
        //prepare a placeholder for the image, before the actual load. This can also prevent loading for the same image
        imageData = imageCatalog->AddImage(imagePath, imageType, frame);
    }

    if (nullptr == imageData)
        return nullptr;

    const FileType fileType = LoaderUtility::CheckFileType(imagePath);
    switch (fileType) {	    
        case FILE_TYPE_TGA: {	
            imageData = imageCatalog->LoadImage(imagePath, imageType); 
            break;	
        }	
        case FILE_TYPE_PNG: {
#ifdef _WIN32
            //Faster to load using custom loader (GDI) on windows
            CriticalSectionController cs(IMAGE_CS(imageType));	
            LoadPNGFileAndAlloc(imagePath, imageType, imageCatalog);
#else
            imageData = imageCatalog->LoadImage(imagePath, imageType); 
#endif

            break;	
        }	
        default: { return nullptr; }	
    }
    
    return imageData;
}

//----------------------------------------------------------------------------------------------------------------------
//Returns the corresponding ImageData. Can be null
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

            {
                CriticalSectionController cs(IMAGE_CS(CRITICAL_SECTION_TYPE_FULL_IMAGE));
                //Add image and refresh
                if (imageCatalog->AddImageFromSrc(imagePath, CRITICAL_SECTION_TYPE_PREVIEW_IMAGE, frame, 
                                                  fullImageData, reqWidth, reqHeight)) 
                {
                    previewImageData = LoaderUtility::GetImageData(imagePath, CRITICAL_SECTION_TYPE_PREVIEW_IMAGE, 
                                                                   imageCatalog, frame);
                    return previewImageData;
                }
            }


            //fail
            return nullptr;
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
