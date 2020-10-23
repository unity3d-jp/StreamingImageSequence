
#include <gtest/gtest.h>

//CommonLib
#include "CommonLib/CriticalSectionType.h" //CRITICAL_SECTION_TYPE_FULL_IMAGE

//Loader
#include "StreamingImageSequence/Loader.h"
#include "StreamingImageSequence/LoaderUtility.h"
#include "StreamingImageSequence/ImageCatalog.h"

//LoaderTest
#include "Utilities/TestUtility.h"


//#define ENABLE_BENCHMARK

namespace StreamingImageSequencePluginTest {

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
    const uint32_t numImages = 3;
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

TEST(Loader, IgnoreLateRequests) {
    using namespace StreamingImageSequencePlugin;
    ImageCatalog& imageCatalog = ImageCatalog::GetInstance();

    const uint32_t imageType = CRITICAL_SECTION_TYPE_FULL_IMAGE;

    const uint32_t maxImages = TestUtility::CleanupAndLoadMaxImages(imageType);
    std::unordered_map<strType, ImageData> imageMap = imageCatalog.GetImageMap(imageType);

    //Test loading images in next frames
    int curFrame = 10;
    uint32_t startIndex = maxImages;
    uint32_t numImages = 3;
    imageMap = TestUtility::LoadAndCheckUnloadingOfUnusedImages(imageType, 
                                                                ++curFrame, startIndex, numImages, imageMap
    );

    //Load next images, which are late requests. This should fail.
    curFrame = 0;
    startIndex += numImages;
    numImages = 9;
    const bool processed = TestUtility::LoadTestImages(imageType, curFrame, startIndex, numImages);
    ASSERT_EQ(false, processed) << "Late requests are still processed, even though they are late";

    //Unload
    UnloadAllImages();
    TestUtility::CheckMemoryCleanup();

}


//----------------------------------------------------------------------------------------------------------------------

TEST(Loader, ResetImageLoadOrder) {
    using namespace StreamingImageSequencePlugin;
    ImageCatalog& imageCatalog = ImageCatalog::GetInstance();

    const uint32_t imageType = CRITICAL_SECTION_TYPE_FULL_IMAGE;

    const uint32_t maxImages = TestUtility::CleanupAndLoadMaxImages(imageType);
    std::unordered_map<strType, ImageData> imageMap = imageCatalog.GetImageMap(imageType);

    //Test loading images in next frames
    int curFrame = 10;
    uint32_t startIndex = maxImages;
    uint32_t numImages = 3;
    imageMap = TestUtility::LoadAndCheckUnloadingOfUnusedImages(imageType, 
                                                                ++curFrame, startIndex, numImages, imageMap
    );

    //Reset
    ResetImageLoadOrder();

    //Load next images after resetting for frame 0. This should fail.
    curFrame = 0;
    startIndex += numImages;
    numImages = 9;
    const bool processed = TestUtility::LoadTestImages(imageType, curFrame, startIndex, numImages);
    const bool laterImagesOutOfMemory = TestUtility::CheckLoadedTestImageData(
        imageType, 0, startIndex, numImages, READ_STATUS_OUT_OF_MEMORY);
    ASSERT_EQ(true, processed);
    ASSERT_EQ(true, laterImagesOutOfMemory) << "Later images are loaded, even though we are out of memory";

    //Request for frame 1, should be prioritized
    curFrame = 1;
    startIndex += numImages;
    numImages = 5;
    imageMap = TestUtility::LoadAndCheckUnloadingOfUnusedImages(imageType, 
                                                                ++curFrame, startIndex, numImages, imageMap
    );


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


} //end namespace
 