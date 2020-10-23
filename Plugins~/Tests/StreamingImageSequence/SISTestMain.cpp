
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

//----------------------------------------------------------------------------------------------------------------------

void BenchmarkFunc(const uint32_t loopCount, bool (*func)(), const char* msg) {
    const clock_t t0 = clock(); 
    for (uint32_t i=0; i< loopCount;++i) {
        func();
    }
    const clock_t t1 = clock();
    const double elapsedSec = (t1 - t0) / (double)CLOCKS_PER_SEC;
    std::cerr << "[          ] " << msg << " LoopCount: " << loopCount << " Elapsed: " << elapsedSec << " sec" << std::endl;

};

#ifdef ENABLE_BENCHMARK
TEST(Loader, BenchmarkLoadSpeed) {
    BenchmarkFunc(1000, TestUtility::LoadAndUnloadTestFullPNGImage, "Loading Full PNG.");
    BenchmarkFunc(1000, TestUtility::LoadAndUnloadTestFullTGAImage, "Loading Full TGA.");
}
#endif

//----------------------------------------------------------------------------------------------------------------------

int main(int argc, char** argv) {

    ::testing::InitGoogleTest(&argc, argv);
    return RUN_ALL_TESTS();
}


} //end namespace
 