
#include <gtest/gtest.h>
#include <iostream>
//CommonLib
#include "CommonLib/CriticalSectionType.h" //CRITICAL_SECTION_TYPE_FULL_IMAGE

//Loader
#include "StreamingImageSequence/Loader.h"
#include "StreamingImageSequence/LoaderUtility.h"
#include "StreamingImageSequence/ImageCatalog.h"

//LoaderTest
#include "TestUtility.h"


//#define ENABLE_BENCHMARK

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
TEST(Loader, LoadInvalidImageTest) {
    using namespace StreamingImageSequencePlugin;
    ImageCatalog& imageCatalog = ImageCatalog::GetInstance();

    const int curFrame = 0;
    const uint32_t numImages = 10;
    const uint32_t imageType = CRITICAL_SECTION_TYPE_FULL_IMAGE;
    const bool processed = TestUtility::LoadTestImages(imageType, curFrame, 0, numImages);
    ASSERT_EQ(true, processed);

    const bool invalidReadSuccessful = TestUtility::LoadInvalidTestImage();
    ASSERT_EQ(false, invalidReadSuccessful);

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

//----------------------------------------------------------------------------------------------------------------------

void BenchmarkFunc(const uint32_t loopCount, bool (*func)(), const char* msg) {
    const clock_t t0 = clock(); 
    for (uint32_t i=0; i< loopCount;++i) {
        func();
    }
    const clock_t t1 = clock();
    const double elapsedSec = (t1 - t0) / (double)CLOCKS_PER_SEC;
    std::cerr << "[          ] " << msg << " LoopCount: " << loopCount << " Elapsed: " << elapsedSec << " sec" << std::endl;

};

#ifdef ENABLE_BENCHMARK
TEST(Loader, BenchmarkLoadSpeed) {
    BenchmarkFunc(1000, TestUtility::LoadAndUnloadTestFullPNGImage, "Loading Full PNG.");
    BenchmarkFunc(1000, TestUtility::LoadAndUnloadTestFullTGAImage, "Loading Full TGA.");
}
#endif

//----------------------------------------------------------------------------------------------------------------------

int main(int argc, char** argv) {

    ::testing::InitGoogleTest(&argc, argv);
    return RUN_ALL_TESTS();
}


} //end namespace
 