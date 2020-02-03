#include "stdafx.h"
#include "LoaderUtility.h"

#include "FileType.h"
#include "CommonLib/CriticalSection.h"
#include "CommonLib/CriticalSectionController.h"

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
    {
        CriticalSectionController cs(CriticalSection::GetInstance().GetObject(
            static_cast<CriticalSectionType>(textureType))
        );
        ASSERT(pResult);
        pResult->readStatus = READ_STATUS_NONE;
        strType wstr(fileName);

        if (readResultMap->find(wstr) != readResultMap->end()) {
            *pResult = readResultMap->at(wstr);
            
            //if success, then the buffer must be not null
            ASSERT(pResult->readStatus != READ_STATUS_SUCCESS || pResult->buffer);
            return true;
        }
    }
    return false;

}

} //end namespace
