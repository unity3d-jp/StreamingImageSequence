
#include <gtest/gtest.h>

//CommonLib
#include "CommonLib/CriticalSectionType.h" //CRITICAL_SECTION_TYPE_FULL_TEXTURE

//Loader
#include "Loader/Loader.h"
#include "Loader/LoaderUtility.h"

//----------------------------------------------------------------------------------------------------------------------

TEST(Loader, ResetPluginTest) {
    using namespace StreamingImageSequencePlugin;
    ResetPlugin();
    
    ASSERT_EQ(0, GetNumLoadedTextures(CRITICAL_SECTION_TYPE_FULL_TEXTURE)) << "Some full textures are still loaded";
    ASSERT_EQ(0, GetNumLoadedTextures(CRITICAL_SECTION_TYPE_PREVIEW_TEXTURE)) << "Some preview textures are still loaded";
}

//----------------------------------------------------------------------------------------------------------------------

TEST(Loader, LoadTextureTest) {
    using namespace StreamingImageSequencePlugin;
    const std::string filePath = "TestImage.png";
    bool processed = LoadAndAllocFullTexture(filePath.c_str());

    ASSERT_EQ(true, processed);

    StReadResult result;
    processed = GetNativeTextureInfo(filePath.c_str(), &result, CRITICAL_SECTION_TYPE_FULL_TEXTURE);
    ASSERT_EQ(true, processed);
    ASSERT_EQ(READ_STATUS_SUCCESS, result.readStatus) << "Loading image failed";

    //Unload
    LoaderUtility::UnloadTexture(filePath.c_str(), CRITICAL_SECTION_TYPE_FULL_TEXTURE);

    ASSERT_EQ(0, GetNumLoadedTextures(CRITICAL_SECTION_TYPE_PREVIEW_TEXTURE)) << "Some preview textures are still loaded";
}
//----------------------------------------------------------------------------------------------------------------------

int main(int argc, char** argv) {

    ::testing::InitGoogleTest(&argc, argv);
    return RUN_ALL_TESTS();
}


