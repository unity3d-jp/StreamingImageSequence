
#include <gtest/gtest.h>

//CommonLib
#include "CommonLib/CriticalSectionType.h" //CRITICAL_SECTION_TYPE_FULL_IMAGE

//Loader
#include "StreamingImageSequence/Loader.h"
#include "StreamingImageSequence/ImageCatalog.h"

//LoaderTest
#include "Utilities/TestUtility.h"

namespace StreamingImageSequencePluginTest {

//----------------------------------------------------------------------------------------------------------------------

TEST(BasicImageLoad, ResetPluginTest) {
    using namespace StreamingImageSequencePlugin;
    const int curFrame = 0;
    const uint32_t numImages = 10;
    const bool processed = TestUtility::LoadTestImages(CRITICAL_SECTION_TYPE_FULL_IMAGE, curFrame, 0, numImages);
    ASSERT_EQ(true, processed);

    const ImageCatalog& imageCatalog = ImageCatalog::GetInstance();
    ASSERT_GT(imageCatalog.GetUsedMemory(), 0);

    //Unload
    ResetPlugin();
    TestUtility::CheckMemoryCleanup();
}

//----------------------------------------------------------------------------------------------------------------------

TEST(BasicImageLoad, LoadSingleImageTest) {
    using namespace StreamingImageSequencePlugin;
    ImageCatalog& imageCatalog = ImageCatalog::GetInstance();

    const int curFrame = 0;
    const uint32_t imageType = CRITICAL_SECTION_TYPE_FULL_IMAGE;
    const bool processed = TestUtility::LoadTestImages(imageType, curFrame, 0, 1);
    ASSERT_EQ(true, processed);

    const bool readSuccessful = TestUtility::CheckLoadedTestImageData(imageType, curFrame, 0, 1, READ_STATUS_SUCCESS);
    ASSERT_EQ(true, readSuccessful) << "Loading image failed";

    ASSERT_GT(imageCatalog.GetUsedMemory(), 0);

    //Unload
    UnloadAllImages();
    TestUtility::CheckMemoryCleanup();
}

//----------------------------------------------------------------------------------------------------------------------
TEST(BasicImageLoad, LoadMultipleImagesTest) {
    using namespace StreamingImageSequencePlugin;
    ImageCatalog& imageCatalog = ImageCatalog::GetInstance();

    const int curFrame = 0;
    const uint32_t numImages = 10;
    const uint32_t imageType = CRITICAL_SECTION_TYPE_FULL_IMAGE;
    const bool processed = TestUtility::LoadTestImages(imageType, curFrame, 0, numImages);
    ASSERT_EQ(true, processed);

    const bool readSuccessful = TestUtility::CheckLoadedTestImageData(imageType, curFrame, 0, numImages, READ_STATUS_SUCCESS);
    ASSERT_EQ(true, readSuccessful) << "Loading image failed";

    ASSERT_GT(imageCatalog.GetUsedMemory(), 0);

    //Unload
    UnloadAllImages();
    TestUtility::CheckMemoryCleanup();
}
//----------------------------------------------------------------------------------------------------------------------
TEST(BasicImageLoad, LoadInvalidImageTest) {
    using namespace StreamingImageSequencePlugin;
    ImageCatalog& imageCatalog = ImageCatalog::GetInstance();

    //load initial files for preparation
    int curFrame = 0;
    const uint32_t numImages = 10;
    const uint32_t imageType = CRITICAL_SECTION_TYPE_FULL_IMAGE;
    const bool processed = TestUtility::LoadTestImages(imageType, curFrame, 0, numImages);
    ASSERT_EQ(true, processed);
    bool readSuccessful = TestUtility::CheckLoadedTestImageData(imageType, curFrame, 0, numImages, READ_STATUS_SUCCESS);
    ASSERT_EQ(true, readSuccessful) << "Loading image failed";

    //load invalid images
    ++curFrame;
    readSuccessful = TestUtility::LoadInvalidTestPNGImage(curFrame);
    ASSERT_EQ(false, readSuccessful);
    readSuccessful= TestUtility::LoadInvalidTestTGAImage(curFrame);
    ASSERT_EQ(false, readSuccessful);

    //make sure the previously loaded images are still loaded
    readSuccessful = TestUtility::CheckLoadedTestImageData(imageType, curFrame, 0, numImages, READ_STATUS_SUCCESS);
    ASSERT_EQ(true, readSuccessful) << "Loaded images were unloaded";

    ASSERT_GT(imageCatalog.GetUsedMemory(), 0);

    //Unload
    UnloadAllImages();
    TestUtility::CheckMemoryCleanup();
}

//----------------------------------------------------------------------------------------------------------------------

TEST(BasicImageLoad, LoadUnvailableImageTest) {
    using namespace StreamingImageSequencePlugin;
    ImageCatalog& imageCatalog = ImageCatalog::GetInstance();

    //load initial files for preparation
    int curFrame = 0;
    const uint32_t numImages = 10;
    const uint32_t imageType = CRITICAL_SECTION_TYPE_FULL_IMAGE;
    const bool processed = TestUtility::LoadTestImages(imageType, curFrame, 0, numImages);
    ASSERT_EQ(true, processed);
    bool readSuccessful = TestUtility::CheckLoadedTestImageData(imageType, curFrame, 0, numImages, READ_STATUS_SUCCESS);
    ASSERT_EQ(true, readSuccessful) << "Loading image failed";

    //load unavailable images
    ++curFrame;
    const char* imagePath = "ThisFileDoesNotExist.png";
    readSuccessful = TestUtility::LoadImage(imagePath, CRITICAL_SECTION_TYPE_FULL_IMAGE, curFrame);
    ASSERT_EQ(false, readSuccessful);
    readSuccessful = TestUtility::LoadImage(imagePath, CRITICAL_SECTION_TYPE_PREVIEW_IMAGE, curFrame);
    ASSERT_EQ(false, readSuccessful);


    //make sure the previously loaded images are still loaded
    readSuccessful = TestUtility::CheckLoadedTestImageData(imageType, curFrame, 0, numImages, READ_STATUS_SUCCESS);
    ASSERT_EQ(true, readSuccessful) << "Loaded images were unloaded";

    ASSERT_GT(imageCatalog.GetUsedMemory(), 0);

    //Unload
    UnloadAllImages();
    TestUtility::CheckMemoryCleanup();
}



} //end namespace
 