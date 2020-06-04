#pragma once

#include "stdafx.h"

//CommonLib
#include "CommonLib/Types.h" //strType
#include "CommonLib/ReadResult.h"
#include "CommonLib/CriticalSectionType.h" //MAX_CRITICAL_SECTION_TYPE_TEXTURES

namespace StreamingImageSequencePlugin {

class ImageCatalog {
public:

    inline static ImageCatalog& GetInstance();
private:
    ImageCatalog();
    ImageCatalog(ImageCatalog const&) = delete;
    ImageCatalog& operator=(ImageCatalog const&) = delete;

    uint64_t m_allocatedMemory;

    std::map<strType, StReadResult> m_pathToImageMap[MAX_CRITICAL_SECTION_TYPE_TEXTURES];


};

//----------------------------------------------------------------------------------------------------------------------

ImageCatalog& ImageCatalog::GetInstance() {
    static ImageCatalog catalog;
    return catalog;
}

} //end namespace

