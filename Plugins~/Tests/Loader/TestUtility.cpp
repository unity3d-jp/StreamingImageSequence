#include "TestUtility.h"

#include <gtest/gtest.h>

//Loader
#include "Loader/Loader.h"
#include "Loader/ImageCatalog.h"


namespace StreamingImageSequencePluginTest {


bool LoadTestPreviewImage(const charType* ptr, const int frame) {
    return LoadAndAllocPreviewImage(ptr, 40, 25, frame);
}
//----------------------------------------------------------------------------------------------------------------------

bool TestUtility::LoadTestImages(const uint32_t imageType, const int frame, const uint32_t start, const uint32_t numImages) 
{
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
bool TestUtility::CheckLoadedTestImageData(const uint32_t imageType, const int frame, const uint32_t start, 
    const uint32_t numImages, const StreamingImageSequencePlugin::ReadStatus reqReadStatus) 
{
    using namespace StreamingImageSequencePlugin;
    const uint32_t endIndex = start + numImages -1;
    ASSERT(endIndex < NUM_TEST_IMAGES);
    bool ret = true;
    for (uint32_t i = start; i <= endIndex && ret; ++i) {
        const strType filePath = "TestImage_" + TO_STR_TYPE(i) + ".png";
        ImageData imageData;
        const bool processed = GetImageData(filePath.c_str(), imageType, frame, &imageData );
        ret = (processed && imageData.CurrentReadStatus == reqReadStatus);
    }

    return ret;
}
//----------------------------------------------------------------------------------------------------------------------

void TestUtility::CheckMemoryCleanup() {
    using namespace StreamingImageSequencePlugin;
    ASSERT(0 == ImageCatalog::GetInstance().GetUsedMemory());
    ASSERT(0 == GetNumLoadedTextures(CRITICAL_SECTION_TYPE_FULL_IMAGE));
    ASSERT(0 == GetNumLoadedTextures(CRITICAL_SECTION_TYPE_PREVIEW_IMAGE));
}

//----------------------------------------------------------------------------------------------------------------------

uint32_t TestUtility::FindNumDuplicateMapElements(
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

//returns maximum number of images that can be loaded in memory
uint32_t TestUtility::CleanupAndLoadMaxImages(const uint32_t imageType) {
    using namespace StreamingImageSequencePlugin;
    ImageCatalog& imageCatalog = ImageCatalog::GetInstance();
    imageCatalog.UnloadAllImages(); //cleanup

    const int curFrame = 0;
    bool processed = TestUtility::LoadTestImages(imageType, curFrame, 0, 1);
    ASSERT(true == processed);

    const uint64_t reqMemForOneImage = imageCatalog.GetUsedMemory();
    ASSERT(reqMemForOneImage >  0);
    const uint32_t maxImages = static_cast<uint32_t>(std::floor( static_cast<float>(MAX_IMAGE_MEMORY) / reqMemForOneImage));
    ASSERT(maxImages < NUM_TEST_IMAGES);

    //Load the remaining images to fill memory to max
    processed = TestUtility::LoadTestImages(imageType, curFrame, 1, maxImages-1);
    bool readSuccessful = TestUtility::CheckLoadedTestImageData(imageType, curFrame, 0, maxImages,READ_STATUS_SUCCESS);

    ASSERT(processed);
    ASSERT(readSuccessful);

    return maxImages;


}

//----------------------------------------------------------------------------------------------------------------------

std::map<strType, StreamingImageSequencePlugin::ImageData> TestUtility::LoadAndCheckUnloadingOfUnusedImages(
    const uint32_t imageType, const int frame, const uint32_t startTestImageIndex, const uint32_t numImages, 
    const std::map<strType, StreamingImageSequencePlugin::ImageData>& prevImageMap) 
{
    //This function assumes that we have reached a state where it's not possible to allocate an image unless we unload.

    using namespace StreamingImageSequencePlugin;

    ImageCatalog& imageCatalog = ImageCatalog::GetInstance();

    ASSERT(startTestImageIndex + numImages -1 < NUM_TEST_IMAGES);
    const bool processed = TestUtility::LoadTestImages(imageType, frame, startTestImageIndex, numImages);
    const bool readSuccessful = TestUtility::CheckLoadedTestImageData(imageType, frame, 
        startTestImageIndex, numImages, READ_STATUS_SUCCESS
    );
    ASSERT(processed);
    ASSERT(readSuccessful);
    const std::map<strType, ImageData>& curImageMap = imageCatalog.GetImageMap(imageType);
    uint32_t numDuplicates = TestUtility::FindNumDuplicateMapElements(prevImageMap, curImageMap);
    ASSERT(prevImageMap.size() - numImages == numDuplicates);

    //return a copy
    return curImageMap;
}

} //end namespace
