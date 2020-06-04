#pragma once

#include "Types.h"
#include "ReadStatus.h"

struct StReadResult
{
    u8*  buffer;
    uint32_t DataSize;
    u32  width;
    u32  height;
    StreamingImageSequencePlugin::ReadStatus readStatus;
    
    StReadResult() : DataSize(0) {
        buffer = NULL;
        width = 0;
        height = 0;
        readStatus = StreamingImageSequencePlugin::READ_STATUS_NONE;
    }

    StReadResult(u8* _data, uint32_t _dataSize, uint32_t _width, uint32_t _height, 
        StreamingImageSequencePlugin::ReadStatus _readStatus) 
        : buffer(_data)
        , DataSize(_dataSize)
        , width (_width)
        , height (_height)
        , readStatus(_readStatus)
    {
    }

};

