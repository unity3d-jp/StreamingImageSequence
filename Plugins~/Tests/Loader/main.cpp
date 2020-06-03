
#include <gtest/gtest.h>
#include "Loader/Loader.h"

TEST(TestSuiteName, TestName) {
    ASSERT_EQ(0,1) << "Vectors x and y are of unequal length";
}


int main(int argc, char** argv)
{

    ResetPlugin();

    ::testing::InitGoogleTest(&argc, argv);
    return RUN_ALL_TESTS();
}


