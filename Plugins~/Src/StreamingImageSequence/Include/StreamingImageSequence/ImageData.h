#pragma once

#include "StreamingImageSequence/ReadStatus.h"
#include "StreamingImageSequence/ImageFormat.h"

namespace StreamingImageSequencePlugin {

struct ImageData {
    uint8_t*  RawData;
    //uint32_t  DataSize;
    uint32_t  Width;
    uint32_t  Height;
    ReadStatus CurrentReadStatus;
    ImageFormat Format;

    //Constructors
    explicit ImageData();
    explicit ImageData(uint8_t* _rawData, uint32_t _width, uint32_t _height, ReadStatus _readStatus);


};

} //end namespace

