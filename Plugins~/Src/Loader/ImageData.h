#pragma once

#include "ReadStatus.h"

namespace StreamingImageSequencePlugin {

struct ImageData {
    uint8_t*  RawData;
    //uint32_t  DataSize;
    uint32_t  Width;
    uint32_t  Height;
    ReadStatus CurrentReadStatus;

    //Constructors
    ImageData();
    ImageData(uint8_t* _rawData, uint32_t _width, uint32_t _height, ReadStatus _readStatus);


};

} //end namespace

