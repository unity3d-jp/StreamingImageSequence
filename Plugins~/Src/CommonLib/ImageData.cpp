#include "stdafx.h"

#include "ImageData.h"

namespace StreamingImageSequencePlugin {

ImageData::ImageData() 
    : RawData(NULL)
    , DataSize(0)
    , Width (0)
    , Height (0)
    , CurrentReadStatus(READ_STATUS_NONE)

{
}

//----------------------------------------------------------------------------------------------------------------------

ImageData::ImageData(u8* _rawData, uint32_t _dataSize, uint32_t _width, uint32_t _height, ReadStatus _readStatus) 
    : RawData(_rawData)
    , DataSize(_dataSize)
    , Width (_width)
    , Height (_height)
    , CurrentReadStatus(_readStatus)
{
}

} //end namespace
