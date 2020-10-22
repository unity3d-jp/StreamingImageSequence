#include "TestUtility.h"

#include <gtest/gtest.h>
#include <cmath> //std::floor

//Loader
#include "StreamingImageSequence/Loader.h"
#include "StreamingImageSequence/ImageCatalog.h"


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
        const strType filePath = "PNGTestImage_" + TO_STR_TYPE(i) + ".png";
        processed = loadFunc(filePath.c_str(),frame);
    }

    return processed;
}

//----------------------------------------------------------------------------------------------------------------------

bool TestUtility::LoadAndUnloadTestFullPNGImage() {
    const char* filePath = "PNGTestImage_0.png";
    const bool loaded = LoadImage(filePath,StreamingImageSequencePlugin::CRITICAL_SECTION_TYPE_FULL_IMAGE, 0);
    if (!loaded)
        return false;

    UnloadImage(filePath);
    return true;
}

bool TestUtility::LoadAndUnloadTestFullTGAImage() {
    const char* filePath = "TGATestImage_0.tga";
    const bool loaded = LoadImage(filePath,StreamingImageSequencePlugin::CRITICAL_SECTION_TYPE_FULL_IMAGE, 0);
    if (!loaded)
        return false;

    UnloadImage(filePath);
    return true;
}

bool TestUtility::LoadInvalidTestPNGImage(const int frame) {
    return LoadImage("InvalidTestImage.png", StreamingImageSequencePlugin::CRITICAL_SECTION_TYPE_FULL_IMAGE, frame);
}

bool TestUtility::LoadInvalidTestTGAImage(const int frame) {
    return LoadImage("InvalidTestImage.tga", StreamingImageSequencePlugin::CRITICAL_SECTION_TYPE_FULL_IMAGE, frame);
}



bool TestUtility::LoadImage(const char* imagePath, const uint32_t imageType, const int frame) {

    using namespace StreamingImageSequencePlugin;
    const char* filePath = imagePath;
    const bool loaded = LoadAndAllocFullImage(filePath,frame);
    ImageData imageData;
    GetImageDataInto(filePath, imageType, frame, &imageData);
    return (nullptr != imageData.RawData);
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
        const strType filePath = "PNGTestImage_" + TO_STR_TYPE(i) + ".png";
        ImageData imageData;
        GetImageDataInto(filePath.c_str(), imageType, frame, &imageData );
        ret = (imageData.CurrentReadStatus == reqReadStatus);
    }

    return ret;
}
//----------------------------------------------------------------------------------------------------------------------

void TestUtility::CheckMemoryCleanup() {
    using namespace StreamingImageSequencePlugin;
    ASSERT(0 == ImageCatalog::GetInstance().GetUsedMemory());
    ASSERT(0 == GetNumLoadedImages(CRITICAL_SECTION_TYPE_FULL_IMAGE));
    ASSERT(0 == GetNumLoadedImages(CRITICAL_SECTION_TYPE_PREVIEW_IMAGE));
}

//----------------------------------------------------------------------------------------------------------------------

uint32_t TestUtility::FindNumDuplicateMapElements(
    const std::unordered_map<strType, StreamingImageSequencePlugin::ImageData>& map0, 
    const std::unordered_map<strType, StreamingImageSequencePlugin::ImageData>& map1) 
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
    UnloadAllImages(); //cleanup
    ImageCatalog& imageCatalog = ImageCatalog::GetInstance();

    const int curFrame = 0;
    uint32_t startIdx = 0;
    bool readSuccessful = true;

    while (readSuccessful) {
        bool processed = TestUtility::LoadTestImages(imageType, curFrame, startIdx, /*numImages= */1);
        ASSERT(true == processed);

        readSuccessful = TestUtility::CheckLoadedTestImageData(imageType, curFrame, 
            startIdx, /*numImages=*/1,READ_STATUS_SUCCESS);
        
        if (!readSuccessful)      
            break;
        ++startIdx;
    }    

    return startIdx;

}

//----------------------------------------------------------------------------------------------------------------------

std::unordered_map<strType, StreamingImageSequencePlugin::ImageData> TestUtility::LoadAndCheckUnloadingOfUnusedImages(
    const uint32_t imageType, const int frame, const uint32_t startTestImageIndex, const uint32_t numImages, 
    const std::unordered_map<strType, StreamingImageSequencePlugin::ImageData>& prevImageMap) 
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
    const std::unordered_map<strType, ImageData>& curImageMap = imageCatalog.GetImageMap(imageType);
    const uint32_t numDuplicates = TestUtility::FindNumDuplicateMapElements(prevImageMap, curImageMap);
    ASSERT(prevImageMap.size() - numImages == numDuplicates);

    //return a copy
    return curImageMap;
}

} //end namespace
