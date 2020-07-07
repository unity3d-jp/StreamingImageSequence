
#include <gtest/gtest.h>
#include <iostream>
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
    UnloadAllImages();
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
    UnloadAllImages();
    TestUtility::CheckMemoryCleanup();
}

//----------------------------------------------------------------------------------------------------------------------

TEST(Loader, AutoUnloadUnusedImagesTest) {
    using namespace StreamingImageSequencePlugin;
    ImageCatalog& imageCatalog = ImageCatalog::GetInstance();

    const uint32_t imageType = CRITICAL_SECTION_TYPE_FULL_IMAGE;

    const uint32_t maxImages = TestUtility::CleanupAndLoadMaxImages(imageType);
    std::unordered_map<strType, ImageData> imageMap = imageCatalog.GetImageMap(imageType);

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
    UnloadAllImages();
    TestUtility::CheckMemoryCleanup();

}

//----------------------------------------------------------------------------------------------------------------------

TEST(Loader, StopLoadingRequiredImagesTest) {
    using namespace StreamingImageSequencePlugin;
    ImageCatalog& imageCatalog = ImageCatalog::GetInstance();
    const uint32_t imageType = CRITICAL_SECTION_TYPE_FULL_IMAGE;

    //Load initial images
    const uint32_t maxImages = TestUtility::CleanupAndLoadMaxImages(imageType);
    const bool initialImagesLoaded = TestUtility::CheckLoadedTestImageData(
        imageType, 0, 0, maxImages, READ_STATUS_SUCCESS);
    ASSERT_EQ(true, initialImagesLoaded) << "Initial images are not loaded in memory anymore";

    //Load next images in the same frame. This should fail.
    const int startIndex = maxImages;
    uint32_t numImages = 3;
    const bool processed = TestUtility::LoadTestImages(imageType, 0, startIndex, numImages);
    const bool laterImagesOutOfMemory = TestUtility::CheckLoadedTestImageData(
        imageType, 0, startIndex, numImages, READ_STATUS_OUT_OF_MEMORY);
    ASSERT_EQ(true, processed);
    ASSERT_EQ(true, laterImagesOutOfMemory) << "Later images are loaded, even though we are out of memory";

    //Unload
    UnloadAllImages();
    TestUtility::CheckMemoryCleanup();

}

//----------------------------------------------------------------------------------------------------------------------

TEST(Loader, OutOfMemoryTest) {

    using namespace StreamingImageSequencePlugin;
    ImageCatalog& imageCatalog = ImageCatalog::GetInstance();

    const uint32_t imageType = CRITICAL_SECTION_TYPE_FULL_IMAGE;

    const uint32_t maxImages = TestUtility::CleanupAndLoadMaxImages(imageType);
    std::unordered_map<strType, ImageData> imageMap = imageCatalog.GetImageMap(imageType);

    //This should unload image loaded in the frame 0
    int curFrame = 1;
    uint32_t startIndex = maxImages;
    uint32_t numImages = maxImages;
    imageMap = TestUtility::LoadAndCheckUnloadingOfUnusedImages(imageType, 
        curFrame, startIndex, numImages, imageMap
    );

    //This shouldn't alloc more image because we are processing the same frame and we are still out of memory
    startIndex += numImages;
    numImages = 1;
    bool processed = TestUtility::LoadTestImages(imageType, curFrame, startIndex, numImages);
    bool imagesOutOfMemory= TestUtility::CheckLoadedTestImageData( 
        imageType, curFrame, startIndex, numImages, READ_STATUS_OUT_OF_MEMORY);
    ASSERT_EQ(true, processed);
    ASSERT_EQ(true, imagesOutOfMemory) << "Later images are loaded, even though we are out of memory";

    //Advance frame and load previously failed images. This should deallocate the unused images (loaded in prev frames)
    ++curFrame;
    imageMap = TestUtility::LoadAndCheckUnloadingOfUnusedImages(imageType, 
        curFrame, startIndex, numImages, imageMap
    );
    const uint32_t lastIndexLoaded = (startIndex + numImages) - 1;

    //Try loading the early images back. This should move them to the end of the "order"
    ++curFrame;
    ASSERT_GE(startIndex, maxImages);
    startIndex = 0;
    numImages = maxImages;
    imageMap = TestUtility::LoadAndCheckUnloadingOfUnusedImages(imageType, 
        curFrame, startIndex, numImages, imageMap
    );

    //Now, try loading the later images. Since the later images should be in the beginning section of the order, they
    //should have been unloaded, and we shouldn't be able to load them in this frame
    startIndex += numImages;
    numImages = maxImages;
    ASSERT_GE(lastIndexLoaded, startIndex+numImages-1);
    processed = TestUtility::LoadTestImages(imageType, curFrame, startIndex, numImages);
    imagesOutOfMemory= TestUtility::CheckLoadedTestImageData( 
        imageType, curFrame, startIndex, numImages, READ_STATUS_OUT_OF_MEMORY);
    ASSERT_EQ(true, processed);
    ASSERT_EQ(true, imagesOutOfMemory) << "Later images are loaded, even though we are out of memory";

    //Unload
    UnloadAllImages();
    TestUtility::CheckMemoryCleanup();

}


//----------------------------------------------------------------------------------------------------------------------

int main(int argc, char** argv) {

    ::testing::InitGoogleTest(&argc, argv);
    return RUN_ALL_TESTS();
}


} //end namespace
 