#pragma once

namespace StreamingImageSequencePlugin {

enum ReadStatus {
    READ_STATUS_UNAVAILABLE = -1,
    READ_STATUS_IDLE        = 0,
    READ_STATUS_LOADING,
    READ_STATUS_SUCCESS,
    READ_STATUS_FAIL,
    READ_STATUS_OUT_OF_MEMORY,
};

} //end namespace
