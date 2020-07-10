
#include <gtest/gtest.h>

//CommonLib
#include "CommonLib/MemoryUtility.h" 

namespace StreamingImageSequencePluginTest {

//----------------------------------------------------------------------------------------------------------------------

TEST(CommonLib, GetTotalRAM) {
    using namespace StreamingImageSequencePlugin;
    const DWORDLONG totalRAM = MemoryUtility::GetTotalRAM();
    ASSERT_GT(totalRAM, 0);
}

//----------------------------------------------------------------------------------------------------------------------

TEST(CommonLib, GetUsedRAM) {
    using namespace StreamingImageSequencePlugin;
    const DWORDLONG usedRAM = MemoryUtility::GetUsedRAM();
    ASSERT_GT(usedRAM, 0);
}

//----------------------------------------------------------------------------------------------------------------------

TEST(CommonLib, GetAvailableRAM) {
    using namespace StreamingImageSequencePlugin;
    const DWORDLONG availableRAM = MemoryUtility::GetAvailableRAM();
    ASSERT_GT(availableRAM, 0);
}

//----------------------------------------------------------------------------------------------------------------------

TEST(CommonLib, GetUsedRAMRatio) {
    using namespace StreamingImageSequencePlugin;
    const float usedRAMRatio = MemoryUtility::GetUsedRAMRatio();
    ASSERT_GT(usedRAMRatio, 0.0f);
    ASSERT_LT(usedRAMRatio, 1.0f);
}

//----------------------------------------------------------------------------------------------------------------------

TEST(CommonLib, GetAvailableRAMRatio) {
    using namespace StreamingImageSequencePlugin;
    const float availableRAMRatio = MemoryUtility::GetAvailableRAMRatio();
    ASSERT_GT(availableRAMRatio, 0);
    ASSERT_LT(availableRAMRatio, 1.0f);
}



} //end namespace
 