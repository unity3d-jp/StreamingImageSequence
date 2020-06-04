#include "stdafx.h"

//CommonLib
#include "CommonLib/CriticalSectionType.h" //MAX_CRITICAL_SECTION_TYPE_TEXTURES

//Loader
#include "LoaderUtility.h"
#include "FileType.h"
#include "TGALoader.h"
#include "ImageCatalog.h"

//External
#include "External/stb/stb_image_resize.h"


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
    const ImageCatalog* imageCatalog, StReadResult* pResult)
{
    using namespace StreamingImageSequencePlugin;
    ASSERT(pResult);
    pResult->readStatus = READ_STATUS_NONE;
    
    const StReadResult* readResult = imageCatalog->GetImage(imagePath, imageType);
    if (nullptr == readResult)
        return false;

    *pResult = *readResult;

    //if success, then the buffer must be not null
    ASSERT(pResult->readStatus != READ_STATUS_SUCCESS || pResult->buffer);

    return true;


}

//----------------------------------------------------------------------------------------------------------------------
//Returns whether the file has been processed, or is still processed  (inside readResultMap).
bool LoaderUtility::LoadAndAllocImage(const strType& imagePath, const uint32_t imageType, ImageCatalog* imageCatalog) 
{
    using namespace StreamingImageSequencePlugin;
    StReadResult readResult;

    const bool isProcessed = LoaderUtility::GetImageDataInto(imagePath, imageType, imageCatalog, &readResult );
    if (isProcessed) {
        return true;
    }

    const FileType fileType = LoaderUtility::CheckFileType(imagePath);
    if (FILE_TYPE_INVALID == fileType)
        return false;

    //Loading
    imageCatalog->AddImage(imagePath, imageType);

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

    StReadResult readResult;
    if (!LoaderUtility::GetImageDataInto(imagePath, imageType, imageCatalog, &readResult ))
        return false;

    //Buffer is still null. Means, still loading
    if (NULL == readResult.buffer) {
        return true; 
    }

    //Already resized. 
    if (readResult.width == reqWidth && readResult.height == reqHeight)
        return true;

    {
        const uint64_t numChannels = 4;
        const uint64_t BUFFER_SIZE = numChannels * reqWidth * reqHeight;
        u8* resizedBuffer = (u8*)malloc(static_cast<uint32_t>(BUFFER_SIZE));

        stbir_resize_uint8(readResult.buffer, readResult.width, readResult.height, 0,
            resizedBuffer, reqWidth, reqHeight, 0, numChannels);
        readResult.buffer = resizedBuffer;
        readResult.width = reqWidth;
        readResult.height = reqHeight;

        imageCatalog->SetImage(imagePath, imageType, &readResult);
    }

    return true;
}



} //end namespace
