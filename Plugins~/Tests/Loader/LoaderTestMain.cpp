
#include <gtest/gtest.h>

//CommonLib
#include "CommonLib/CriticalSectionType.h" //CRITICAL_SECTION_TYPE_FULL_IMAGE

//Loader
#include "Loader/Loader.h"
#include "Loader/LoaderUtility.h"
#include "Loader/ImageCatalog.h"

//----------------------------------------------------------------------------------------------------------------------
bool LoadTestPreviewImage(const charType* ptr, const int frame) {
    return LoadAndAllocPreviewImage(ptr, 40, 25, frame);
}

//----------------------------------------------------------------------------------------------------------------------

bool LoadTestImages(const uint32_t imageType, const int frame, const uint32_t start, const uint32_t numImages) {

    //setup func pointer to the actual API
    bool (*loadFunc)(const char*, const int);
    if (StreamingImageSequencePlugin::CRITICAL_SECTION_TYPE_FULL_IMAGE == imageType) {
        loadFunc = &LoadAndAllocFullImage;
    } else {
        loadFunc = &LoadTestPreviewImage;
    }

    const uint32_t endIndex = start + numImages -1;
    ASSERT(endIndex < NUM_TEST_IMAGES);
    bool processed = true;
    for (uint32_t i = start; i <= endIndex && processed; ++i) {
        const strType filePath = "TestImage_" + TO_STR_TYPE(i) + ".png";
        processed = loadFunc(filePath.c_str(),frame);
    }

    return processed;
}

//----------------------------------------------------------------------------------------------------------------------
bool CheckLoadedTestImageData(const uint32_t imageType, const int frame, const uint32_t start, const uint32_t numImages) {
    using namespace StreamingImageSequencePlugin;
    const uint32_t endIndex = start + numImages -1;
    ASSERT(endIndex < NUM_TEST_IMAGES);
    bool ret = true;
    for (uint32_t i = start; i <= endIndex && ret; ++i) {
        const strType filePath = "TestImage_" + TO_STR_TYPE(i) + ".png";
        ImageData imageData;
        const bool processed = GetImageData(filePath.c_str(), imageType, frame, &imageData );
        ret = (processed && imageData.CurrentReadStatus == READ_STATUS_SUCCESS);
    }

    return ret;
}


//----------------------------------------------------------------------------------------------------------------------

TEST(Loader, ResetPluginTest) {
    using namespace StreamingImageSequencePlugin;
    const int curFrame = 0;
    const uint32_t numImages = 10;
    const bool processed = LoadTestImages(CRITICAL_SECTION_TYPE_FULL_IMAGE, curFrame, 0, numImages);
    ASSERT_EQ(true, processed);

    const ImageCatalog& imageCatalog = ImageCatalog::GetInstance();
    ASSERT_GT(imageCatalog.GetUsedMemory(), 0);

    //Unload
    ResetPlugin();
    ASSERT_EQ(imageCatalog.GetUsedMemory(), 0);
    ASSERT_EQ(0, GetNumLoadedTextures(CRITICAL_SECTION_TYPE_FULL_IMAGE)) << "Some full textures are still loaded";
    ASSERT_EQ(0, GetNumLoadedTextures(CRITICAL_SECTION_TYPE_PREVIEW_IMAGE)) << "Some preview textures are still loaded";
}

//----------------------------------------------------------------------------------------------------------------------

TEST(Loader, LoadSingleImageTest) {
    using namespace StreamingImageSequencePlugin;
    ImageCatalog& imageCatalog = ImageCatalog::GetInstance();

    const int curFrame = 0;
    const bool processed = LoadTestImages(CRITICAL_SECTION_TYPE_FULL_IMAGE, curFrame, 0, 1);
    ASSERT_EQ(true, processed);

    const bool readSuccessful = CheckLoadedTestImageData(CRITICAL_SECTION_TYPE_FULL_IMAGE, curFrame, 0, 1);
    ASSERT_EQ(true, readSuccessful) << "Loading image failed";

    ASSERT_GT(imageCatalog.GetUsedMemory(), 0);

    //Unload
    imageCatalog.UnloadAllImages();
    ASSERT_EQ(imageCatalog.GetUsedMemory(), 0);

    ASSERT_EQ(0, GetNumLoadedTextures(CRITICAL_SECTION_TYPE_FULL_IMAGE)) << "Some full images are still loaded";
    ASSERT_EQ(0, GetNumLoadedTextures(CRITICAL_SECTION_TYPE_PREVIEW_IMAGE)) << "Some preview images are still loaded";
}

//----------------------------------------------------------------------------------------------------------------------
TEST(Loader, LoadMultipleImagesTest) {
    using namespace StreamingImageSequencePlugin;
    ImageCatalog& imageCatalog = ImageCatalog::GetInstance();

    const int curFrame = 0;
    const uint32_t numImages = 10;
    const bool processed = LoadTestImages(CRITICAL_SECTION_TYPE_FULL_IMAGE, curFrame, 0, numImages);
    ASSERT_EQ(true, processed);

    const bool readSuccessful = CheckLoadedTestImageData(CRITICAL_SECTION_TYPE_FULL_IMAGE, curFrame, 0, numImages);
    ASSERT_EQ(true, readSuccessful) << "Loading image failed";

    ASSERT_GT(imageCatalog.GetUsedMemory(), 0);

    //Unload
    imageCatalog.UnloadAllImages();
    ASSERT_EQ(imageCatalog.GetUsedMemory(), 0);

    ASSERT_EQ(0, GetNumLoadedTextures(CRITICAL_SECTION_TYPE_FULL_IMAGE)) << "Some full images are still loaded";
    ASSERT_EQ(0, GetNumLoadedTextures(CRITICAL_SECTION_TYPE_PREVIEW_IMAGE)) << "Some preview images are still loaded";
}

//----------------------------------------------------------------------------------------------------------------------

int main(int argc, char** argv) {

    ::testing::InitGoogleTest(&argc, argv);
    return RUN_ALL_TESTS();
}


