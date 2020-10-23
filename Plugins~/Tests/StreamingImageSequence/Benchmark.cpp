
#include <gtest/gtest.h>

//LoaderTest
#include "Utilities/TestUtility.h"


#define ENABLE_BENCHMARK

namespace StreamingImageSequencePluginTest {

//----------------------------------------------------------------------------------------------------------------------

void BenchmarkFunc(const uint32_t loopCount, bool (*func)(), const char* msg) {
    const clock_t t0 = clock(); 
    for (uint32_t i=0; i< loopCount;++i) {
        func();
    }
    const clock_t t1 = clock();
    const double elapsedSec = (t1 - t0) / static_cast<double>(CLOCKS_PER_SEC);
    std::cerr << "[          ] " << msg << " LoopCount: " << loopCount << " Elapsed: " << elapsedSec << " sec" << std::endl;

};

#ifdef ENABLE_BENCHMARK
TEST(_3_Benchmark, BenchmarkLoadSpeed) {
    BenchmarkFunc(1000, TestUtility::LoadAndUnloadTestFullPNGImage, "Loading Full PNG.");
    BenchmarkFunc(1000, TestUtility::LoadAndUnloadTestFullTGAImage, "Loading Full TGA.");
}
#endif

//----------------------------------------------------------------------------------------------------------------------


} //end namespace
 