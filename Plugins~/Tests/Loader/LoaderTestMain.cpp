
#include <gtest/gtest.h>

//CommonLib
#include "CommonLib/CriticalSectionType.h" //CRITICAL_SECTION_TYPE_FULL_IMAGE

//Loader
#include "Loader/Loader.h"
#include "Loader/LoaderUtility.h"
#include "Loader/ImageCatalog.h"

//----------------------------------------------------------------------------------------------------------------------

TEST(Loader, ResetPluginTest) {
    using namespace StreamingImageSequencePlugin;
    ResetPlugin();
    
    ASSERT_EQ(0, GetNumLoadedTextures(CRITICAL_SECTION_TYPE_FULL_IMAGE)) << "Some full textures are still loaded";
    ASSERT_EQ(0, GetNumLoadedTextures(CRITICAL_SECTION_TYPE_PREVIEW_IMAGE)) << "Some preview textures are still loaded";
}

//----------------------------------------------------------------------------------------------------------------------

TEST(Loader, LoadTextureTest) {
    using namespace StreamingImageSequencePlugin;
    const std::string filePath = "TestImage.png";
    ImageCatalog& imageCatalog = ImageCatalog::GetInstance();

    bool processed = LoadAndAllocFullImage(filePath.c_str());

    ASSERT_EQ(true, processed);

    ImageData result;
    processed = GetImageData(filePath.c_str(), CRITICAL_SECTION_TYPE_FULL_IMAGE, &result );
    ASSERT_EQ(true, processed);
    ASSERT_EQ(READ_STATUS_SUCCESS, result.CurrentReadStatus) << "Loading image failed";

    ASSERT_GT(imageCatalog.GetUsedMemory(), 0);

    //Unload
    imageCatalog.UnloadImage(filePath, CRITICAL_SECTION_TYPE_FULL_IMAGE);
    ASSERT_EQ(imageCatalog.GetUsedMemory(), 0);

    ASSERT_EQ(0, GetNumLoadedTextures(CRITICAL_SECTION_TYPE_FULL_IMAGE)) << "Some full images are still loaded";
    ASSERT_EQ(0, GetNumLoadedTextures(CRITICAL_SECTION_TYPE_PREVIEW_IMAGE)) << "Some preview images are still loaded";
}
//----------------------------------------------------------------------------------------------------------------------

int main(int argc, char** argv) {

    ::testing::InitGoogleTest(&argc, argv);
    return RUN_ALL_TESTS();
}


