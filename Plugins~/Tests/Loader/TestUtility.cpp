#include "TestUtility.h"

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
    const uint32_t numImages) 
{
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

} //end namespace
