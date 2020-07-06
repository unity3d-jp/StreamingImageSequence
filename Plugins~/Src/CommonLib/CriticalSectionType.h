#pragma once

#include "CriticalSectionObject.h"

namespace StreamingImageSequencePlugin {

enum CriticalSectionType {
    //Images section. Must start from 0
    CRITICAL_SECTION_TYPE_FULL_IMAGE = 0,
    CRITICAL_SECTION_TYPE_PREVIEW_IMAGE,

    //Others
    CRITICAL_SECTION_TYPE_MAX = 32
};

const uint32_t MAX_CRITICAL_SECTION_TYPE_IMAGES = CRITICAL_SECTION_TYPE_PREVIEW_IMAGE + 1;

} //end namespace
