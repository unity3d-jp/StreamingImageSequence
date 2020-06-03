
#include <gtest/gtest.h>

#include "CommonLib/CriticalSectionType.h" //CRITICAL_SECTION_TYPE_FULL_TEXTURE
#include "Loader/Loader.h"

TEST(Loader, ResetPluginTest) {
    using namespace StreamingImageSequencePlugin;
    ResetPlugin();
    
    ASSERT_EQ(0, GetNumLoadedTextures(CRITICAL_SECTION_TYPE_FULL_TEXTURE)) << "Some full textures are still loaded";
    ASSERT_EQ(0, GetNumLoadedTextures(CRITICAL_SECTION_TYPE_PREVIEW_TEXTURE)) << "Some preview textures are still loaded";
}

//----------------------------------------------------------------------------------------------------------------------

int main(int argc, char** argv) {
//    LoadAndAllocFullTexture()

    ::testing::InitGoogleTest(&argc, argv);
    return RUN_ALL_TESTS();
}


