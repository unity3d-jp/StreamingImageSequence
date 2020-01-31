#pragma once

#include "CommonLib/Types.h"
#include "CommonLib/ReadResult.h"
#include "FileType.h"

namespace StreamingImageSequencePlugin {

class LoaderUtility {
public:
    static FileType CheckFileType(const charType* fileName);

    static bool GetTextureInfo(const charType* fileName, StReadResult* pResult,
                                      std::map<strType, StReadResult>* readResultMap);

};

} //end namespace

