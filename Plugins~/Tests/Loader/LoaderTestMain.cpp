
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

    const uint32_t maxImages = TestUtility::CleanupAndLoadMaxImages(CRITICAL_SECTION_TYPE_FULL_IMAGE);
    std::map<strType, ImageData> imageMap = imageCatalog.GetImageMap(CRITICAL_SECTION_TYPE_FULL_IMAGE);

    //Test loading images in next frames
    int curFrame = 0;
    uint32_t startIndex = maxImages;
    uint32_t numImages = 3;
    imageMap = TestUtility::LoadAndCheckUnloadingOfUnusedImages(CRITICAL_SECTION_TYPE_FULL_IMAGE, 
        ++curFrame, startIndex, numImages, imageMap
    );

    startIndex += numImages;
    numImages = 5;
    imageMap = TestUtility::LoadAndCheckUnloadingOfUnusedImages(CRITICAL_SECTION_TYPE_FULL_IMAGE, 
        ++curFrame, startIndex, numImages, imageMap
    );

    startIndex += numImages;
    numImages = 7;
    imageMap = TestUtility::LoadAndCheckUnloadingOfUnusedImages(CRITICAL_SECTION_TYPE_FULL_IMAGE, 
        ++curFrame, startIndex, numImages, imageMap
    );

    startIndex += numImages;
    numImages = 9;
    imageMap = TestUtility::LoadAndCheckUnloadingOfUnusedImages(CRITICAL_SECTION_TYPE_FULL_IMAGE, 
        ++curFrame, startIndex, numImages, imageMap
    );


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
 