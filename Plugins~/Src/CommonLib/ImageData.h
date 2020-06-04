#pragma once

#include "Types.h"
#include "ReadStatus.h"

namespace StreamingImageSequencePlugin {

struct ImageData {
    u8*  RawData;
    uint32_t DataSize;
    u32  Width;
    u32  Height;
    ReadStatus CurrentReadStatus;

    //Constructors
    ImageData();
    ImageData(u8* _rawData, uint32_t _dataSize, uint32_t _width, uint32_t _height, ReadStatus _readStatus);


};

} //end namespace

