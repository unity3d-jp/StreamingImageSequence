
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
    const uint32_t imageType = CRITICAL_SECTION_TYPE_FULL_IMAGE;
    const bool processed = TestUtility::LoadTestImages(imageType, curFrame, 0, 1);
    ASSERT_EQ(true, processed);

    const bool readSuccessful = TestUtility::CheckLoadedTestImageData(imageType, curFrame, 0, 1, READ_STATUS_SUCCESS);
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
    const uint32_t imageType = CRITICAL_SECTION_TYPE_FULL_IMAGE;
    const bool processed = TestUtility::LoadTestImages(imageType, curFrame, 0, numImages);
    ASSERT_EQ(true, processed);

    const bool readSuccessful = TestUtility::CheckLoadedTestImageData(imageType, curFrame, 0, numImages, READ_STATUS_SUCCESS);
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

    const uint32_t imageType = CRITICAL_SECTION_TYPE_FULL_IMAGE;

    const uint32_t maxImages = TestUtility::CleanupAndLoadMaxImages(imageType);
    std::map<strType, ImageData> imageMap = imageCatalog.GetImageMap(imageType);

    //Test loading images in next frames
    int curFrame = 0;
    uint32_t startIndex = maxImages;
    uint32_t numImages = 3;
    imageMap = TestUtility::LoadAndCheckUnloadingOfUnusedImages(imageType, 
        ++curFrame, startIndex, numImages, imageMap
    );

    startIndex += numImages;
    numImages = 5;
    imageMap = TestUtility::LoadAndCheckUnloadingOfUnusedImages(imageType, 
        ++curFrame, startIndex, numImages, imageMap
    );

    startIndex += numImages;
    numImages = 7;
    imageMap = TestUtility::LoadAndCheckUnloadingOfUnusedImages(imageType, 
        ++curFrame, startIndex, numImages, imageMap
    );

    startIndex += numImages;
    numImages = 9;
    imageMap = TestUtility::LoadAndCheckUnloadingOfUnusedImages(imageType, 
        ++curFrame, startIndex, numImages, imageMap
    );

    //Unload
    imageCatalog.UnloadAllImages();
    TestUtility::CheckMemoryCleanup();

}

//----------------------------------------------------------------------------------------------------------------------

TEST(Loader, StopLoadingRequiredImagesTest) {
    using namespace StreamingImageSequencePlugin;
    ImageCatalog& imageCatalog = ImageCatalog::GetInstance();
    const uint32_t imageType = CRITICAL_SECTION_TYPE_FULL_IMAGE;

    const uint32_t maxImages = TestUtility::CleanupAndLoadMaxImages(imageType);

    const int startIndex = maxImages;
    uint32_t numImages = 3;
    const bool processed = TestUtility::LoadTestImages(imageType, 0, startIndex, numImages);
    ASSERT_EQ(true, processed);


    const bool initialImagesLoaded = TestUtility::CheckLoadedTestImageData(
        imageType, 0, 0, maxImages, READ_STATUS_SUCCESS);
    ASSERT_EQ(true, initialImagesLoaded) << "Initial images are not loaded in memory anymore";

    const bool laterImagesOutOfMemory = TestUtility::CheckLoadedTestImageData(
        imageType, 0, startIndex, numImages, READ_STATUS_OUT_OF_MEMORY);
    ASSERT_EQ(true, laterImagesOutOfMemory) << "Later images are loaded, even though we are out of memory";

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
 