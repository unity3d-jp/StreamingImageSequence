
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

void CheckMemoryCleanup() {
    using namespace StreamingImageSequencePlugin;
    ASSERT(0 == ImageCatalog::GetInstance().GetUsedMemory());
    ASSERT(0 == GetNumLoadedTextures(CRITICAL_SECTION_TYPE_FULL_IMAGE));
    ASSERT(0 == GetNumLoadedTextures(CRITICAL_SECTION_TYPE_PREVIEW_IMAGE));
}

//----------------------------------------------------------------------------------------------------------------------

uint32_t FindNumDuplicateMapElements(
    const std::map<strType, StreamingImageSequencePlugin::ImageData>& map0, 
    const std::map<strType, StreamingImageSequencePlugin::ImageData>& map1) 
{
    uint32_t ret = 0;
    for (auto itr = map0.begin(); itr != map0.end(); ++itr) {
        if (map1.find(itr->first)!=map1.end()) {
            ++ret;
        }

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
    CheckMemoryCleanup();
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
    CheckMemoryCleanup();
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
    CheckMemoryCleanup();
}

//----------------------------------------------------------------------------------------------------------------------

TEST(Loader, AutoUnloadUnusedImagesTest) {
    using namespace StreamingImageSequencePlugin;
    ImageCatalog& imageCatalog = ImageCatalog::GetInstance();

    int curFrame = 0;
    bool processed = LoadTestImages(CRITICAL_SECTION_TYPE_FULL_IMAGE, curFrame, 0, 1);
    ASSERT_EQ(true, processed);

    const uint64_t reqMemForOneImage = imageCatalog.GetUsedMemory();
    ASSERT_GT(reqMemForOneImage, 0);
    const uint32_t numImagesLimit = static_cast<uint32_t>(std::floor( static_cast<float>(MAX_IMAGE_MEMORY) / reqMemForOneImage));
    ASSERT(numImagesLimit < NUM_TEST_IMAGES);

    //Load the remaining images
    processed = LoadTestImages(CRITICAL_SECTION_TYPE_FULL_IMAGE, curFrame, 1, numImagesLimit-1);
    bool readSuccessful = CheckLoadedTestImageData(CRITICAL_SECTION_TYPE_FULL_IMAGE, curFrame, 0, numImagesLimit);
    ASSERT_EQ(true, processed);
    ASSERT_EQ(true, readSuccessful) << "Loading image failed";
    std::map<strType, ImageData> images_0 = imageCatalog.GetImageMap(CRITICAL_SECTION_TYPE_FULL_IMAGE);

    //Load images for frame 1
    ++curFrame;
    uint32_t startIndex = numImagesLimit;
    const uint32_t numImages_1 = 3;
    ASSERT(startIndex + numImages_1 -1 < NUM_TEST_IMAGES);
    processed = LoadTestImages(CRITICAL_SECTION_TYPE_FULL_IMAGE, curFrame, startIndex, numImages_1);
    readSuccessful = CheckLoadedTestImageData(CRITICAL_SECTION_TYPE_FULL_IMAGE, curFrame, startIndex, numImages_1);
    ASSERT_EQ(true, processed);
    ASSERT_EQ(true, readSuccessful) << "Loading image failed";
    std::map<strType, ImageData> images_1 = imageCatalog.GetImageMap(CRITICAL_SECTION_TYPE_FULL_IMAGE);
    uint32_t numDuplicates = FindNumDuplicateMapElements(images_0, images_1);
    ASSERT_EQ(images_0.size() - numImages_1, numDuplicates) << "Error in unloading unused images";

    //Load images for frame 2
    ++curFrame;
    startIndex += numImages_1;
    const uint32_t numImages_2 = 5;
    ASSERT(startIndex + numImages_2 -1 < NUM_TEST_IMAGES);
    processed = LoadTestImages(CRITICAL_SECTION_TYPE_FULL_IMAGE, curFrame, startIndex, numImages_2);
    readSuccessful = CheckLoadedTestImageData(CRITICAL_SECTION_TYPE_FULL_IMAGE, curFrame, startIndex, numImages_2);
    ASSERT_EQ(true, processed);
    ASSERT_EQ(true, readSuccessful) << "Loading image failed";
    std::map<strType, ImageData> images_2 = imageCatalog.GetImageMap(CRITICAL_SECTION_TYPE_FULL_IMAGE);
    numDuplicates = FindNumDuplicateMapElements(images_1, images_2);
    numDuplicates = FindNumDuplicateMapElements(images_1, images_2);
    ASSERT_EQ(images_1.size() - numImages_2, numDuplicates) << "Error in unloading unused images";


    //Unload
    imageCatalog.UnloadAllImages();
    CheckMemoryCleanup();


}

//----------------------------------------------------------------------------------------------------------------------

int main(int argc, char** argv) {

    ::testing::InitGoogleTest(&argc, argv);
    return RUN_ALL_TESTS();
}


