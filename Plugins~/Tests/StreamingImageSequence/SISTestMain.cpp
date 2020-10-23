
#include <gtest/gtest.h>
#include <iostream>
//CommonLib
#include "CommonLib/CriticalSectionType.h" //CRITICAL_SECTION_TYPE_FULL_IMAGE

//Loader
#include "StreamingImageSequence/Loader.h"
#include "StreamingImageSequence/LoaderUtility.h"
#include "StreamingImageSequence/ImageCatalog.h"

//LoaderTest
#include "Utilities/TestUtility.h"


//#define ENABLE_BENCHMARK

namespace StreamingImageSequencePluginTest {

int main(int argc, char** argv) {

    ::testing::InitGoogleTest(&argc, argv);
    return RUN_ALL_TESTS();
}


} //end namespace
 