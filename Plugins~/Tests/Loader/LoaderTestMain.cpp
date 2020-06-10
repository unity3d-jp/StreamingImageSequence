
#include <gtest/gtest.h>

//CommonLib
#include "CommonLib/CriticalSectionType.h" //CRITICAL_SECTION_TYPE_FULL_IMAGE

//Loader
#include "Loader/Loader.h"
#include "Loader/LoaderUtility.h"
#include "Loader/ImageCatalog.h"

//LoaderTest
#include "TestUtility.h"

namespace StreamingImageSequencePluginTest {

//----------------------------------------------------------------------------------------------------------------------

TEST(Loader, ResetPluginTest) {
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

TEST(Loader, LoadSingleImageTest) {
    using namespace StreamingImageSequencePlugin;
    ImageCatalog& imageCatalog = ImageCatalog::GetInstance();

    const int curFrame = 0;
    const bool processed = TestUtility::LoadTestImages(CRITICAL_SECTION_TYPE_FULL_IMAGE, curFrame, 0, 1);
    ASSERT_EQ(true, processed);

    const bool readSuccessful = TestUtility::CheckLoadedTestImageData(CRITICAL_SECTION_TYPE_FULL_IMAGE, curFrame, 0, 1);
    ASSERT_EQ(true, readSuccessful) << "Loading image failed";

    ASSERT_GT(imageCatalog.GetUsedMemory(), 0);

    //Unload
    imageCatalog.UnloadAllImages();
    TestUtility::CheckMemoryCleanup();
}

//----------------------------------------------------------------------------------------------------------------------
TEST(Loader, LoadMultipleImagesTest) {
    using namespace StreamingImageSequencePlugin;
    ImageCatalog& imageCatalog = ImageCatalog::GetInstance();

    const int curFrame = 0;
    const uint32_t numImages = 10;
    const bool processed = TestUtility::LoadTestImages(CRITICAL_SECTION_TYPE_FULL_IMAGE, curFrame, 0, numImages);
    ASSERT_EQ(true, processed);

    const bool readSuccessful = TestUtility::CheckLoadedTestImageData(CRITICAL_SECTION_TYPE_FULL_IMAGE, curFrame, 0, numImages);
    ASSERT_EQ(true, readSuccessful) << "Loading image failed";

    ASSERT_GT(imageCatalog.GetUsedMemory(), 0);

    //Unload
    imageCatalog.UnloadAllImages();
    TestUtility::CheckMemoryCleanup();
}

//----------------------------------------------------------------------------------------------------------------------

TEST(Loader, AutoUnloadUnusedImagesTest) {
    using namespace StreamingImageSequencePlugin;
    ImageCatalog& imageCatalog = ImageCatalog::GetInstance();

    int curFrame = 0;
    bool processed = TestUtility::LoadTestImages(CRITICAL_SECTION_TYPE_FULL_IMAGE, curFrame, 0, 1);
    ASSERT_EQ(true, processed);

    const uint64_t reqMemForOneImage = imageCatalog.GetUsedMemory();
    ASSERT_GT(reqMemForOneImage, 0);
    const uint32_t numImagesLimit = static_cast<uint32_t>(std::floor( static_cast<float>(MAX_IMAGE_MEMORY) / reqMemForOneImage));
    ASSERT(numImagesLimit < NUM_TEST_IMAGES);

    //Load the remaining images
    processed = TestUtility::LoadTestImages(CRITICAL_SECTION_TYPE_FULL_IMAGE, curFrame, 1, numImagesLimit-1);
    bool readSuccessful = TestUtility::CheckLoadedTestImageData(CRITICAL_SECTION_TYPE_FULL_IMAGE, curFrame, 0, numImagesLimit);
    ASSERT_EQ(true, processed);
    ASSERT_EQ(true, readSuccessful) << "Loading image failed";
    std::map<strType, ImageData> images_0 = imageCatalog.GetImageMap(CRITICAL_SECTION_TYPE_FULL_IMAGE);

    //Load images for frame 1
    ++curFrame;
    uint32_t startIndex = numImagesLimit;
    const uint32_t numImages_1 = 3;
    ASSERT(startIndex + numImages_1 -1 < NUM_TEST_IMAGES);
    processed = TestUtility::LoadTestImages(CRITICAL_SECTION_TYPE_FULL_IMAGE, curFrame, startIndex, numImages_1);
    readSuccessful = TestUtility::CheckLoadedTestImageData(CRITICAL_SECTION_TYPE_FULL_IMAGE, curFrame, startIndex, numImages_1);
    ASSERT_EQ(true, processed);
    ASSERT_EQ(true, readSuccessful) << "Loading image failed";
    std::map<strType, ImageData> images_1 = imageCatalog.GetImageMap(CRITICAL_SECTION_TYPE_FULL_IMAGE);
    uint32_t numDuplicates = TestUtility::FindNumDuplicateMapElements(images_0, images_1);
    ASSERT_EQ(images_0.size() - numImages_1, numDuplicates) << "Error in unloading unused images";

    //Load images for frame 2
    ++curFrame;
    startIndex += numImages_1;
    const uint32_t numImages_2 = 5;
    ASSERT(startIndex + numImages_2 -1 < NUM_TEST_IMAGES);
    processed = TestUtility::LoadTestImages(CRITICAL_SECTION_TYPE_FULL_IMAGE, curFrame, startIndex, numImages_2);
    readSuccessful = TestUtility::CheckLoadedTestImageData(CRITICAL_SECTION_TYPE_FULL_IMAGE, curFrame, startIndex, numImages_2);
    ASSERT_EQ(true, processed);
    ASSERT_EQ(true, readSuccessful) << "Loading image failed";
    std::map<strType, ImageData> images_2 = imageCatalog.GetImageMap(CRITICAL_SECTION_TYPE_FULL_IMAGE);
    numDuplicates = TestUtility::FindNumDuplicateMapElements(images_1, images_2);
    ASSERT_EQ(images_1.size() - numImages_2, numDuplicates) << "Error in unloading unused images";


    //Unload
    imageCatalog.UnloadAllImages();
    TestUtility::CheckMemoryCleanup();


}

//----------------------------------------------------------------------------------------------------------------------

int main(int argc, char** argv) {

    ::testing::InitGoogleTest(&argc, argv);
    return RUN_ALL_TESTS();
}


} //end namespace
 