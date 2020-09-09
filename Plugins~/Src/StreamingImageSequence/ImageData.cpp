#include "stdafx.h"

#include "ImageData.h"

namespace StreamingImageSequencePlugin {

ImageData::ImageData() 
    : RawData(NULL)
    , Width (0)
    , Height (0)
    , CurrentReadStatus(READ_STATUS_IDLE)

{
}

//----------------------------------------------------------------------------------------------------------------------

ImageData::ImageData(uint8_t* _rawData, uint32_t _width, uint32_t _height, ReadStatus _readStatus) 
    : RawData(_rawData)
    , Width (_width)
    , Height (_height)
    , CurrentReadStatus(_readStatus)
{
}

} //end namespace
