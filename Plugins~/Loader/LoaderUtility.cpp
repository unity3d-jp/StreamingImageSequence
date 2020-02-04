#include "stdafx.h"

//Loader
#include "LoaderUtility.h"
#include "FileType.h"
#include "TGALoader.h"

//----------------------------------------------------------------------------------------------------------------------
//Forward declarations
void* loadPNGFileAndAlloc(const charType* fileName, StReadResult* pResult);

//----------------------------------------------------------------------------------------------------------------------

namespace StreamingImageSequencePlugin {

FileType LoaderUtility::CheckFileType(const charType* fileName) {
    using namespace StreamingImageSequencePlugin;
   
    const size_t length =
#if USE_WCHAR
        wcslen(fileName);
#else
        strlen(fileName);
#endif

    //Check extension validity
    if (length < 4 || fileName[length - 4] != '.')    {
        return FILE_TYPE_INVALID;
    }

#ifdef _WIN32    // TGA is only supported on Windows
    if ((fileName[length - 3] == 'T' || fileName[length - 3] == 't')
        && (fileName[length - 2] == 'G' || fileName[length - 2] == 'g')
        && (fileName[length - 1] == 'A' || fileName[length - 1] == 'a'))
    {
        return FILE_TYPE_TGA;
    }
#endif
    
    return FILE_TYPE_PNG;
}

//----------------------------------------------------------------------------------------------------------------------
//Returns whether the file has been processed, or is still processed  (inside readResultMap).

bool LoaderUtility::GetTextureInfo(const charType* fileName, StReadResult* pResult,
                                  std::map<strType, StReadResult>* readResultMap, const uint32_t textureType)
{
    using namespace StreamingImageSequencePlugin;
    ASSERT(pResult);
    pResult->readStatus = READ_STATUS_NONE;
    strType wstr(fileName);

    if (readResultMap->find(wstr) != readResultMap->end()) {
        *pResult = readResultMap->at(wstr);
            
        //if success, then the buffer must be not null
        ASSERT(pResult->readStatus != READ_STATUS_SUCCESS || pResult->buffer);
        return true;
    }
    return false;

}

//----------------------------------------------------------------------------------------------------------------------
//Returns whether the file has been processed, or is still processed  (inside readResultMap).
bool LoaderUtility::LoadAndAllocTexture(const charType* fileName, std::map<strType, StReadResult>* readResultMap, const uint32_t texType) 
{
    using namespace StreamingImageSequencePlugin;
    StReadResult readResult;

    const bool isProcessed = LoaderUtility::GetTextureInfo(fileName, &readResult, readResultMap, texType);
    if (isProcessed) {
        return true;
    }

    const FileType fileType = LoaderUtility::CheckFileType(fileName);
    if (FILE_TYPE_INVALID == fileType)
        return false;

    //Loading
    strType wstr(fileName);
    {
        readResult.readStatus = READ_STATUS_LOADING;
        (*readResultMap)[wstr] = readResult;
    }

    void* ptr = nullptr;
    switch (fileType) {
    case FILE_TYPE_TGA: {
        ptr = loadTGAFileAndAlloc(fileName, &readResult);
        break;
    }
    case FILE_TYPE_PNG: {
        ptr = loadPNGFileAndAlloc(fileName, &readResult);
        break;
    }
    default: break;
    }

    if (ptr == NULL) {
        return false;
    }

    readResult.readStatus = READ_STATUS_SUCCESS;
    (*readResultMap)[wstr] = readResult;

    return true;

}


} //end namespace
