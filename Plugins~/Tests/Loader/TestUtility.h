#include "CommonLib/Types.h" //charType

//Loader
#include "Loader/ImageData.h"

namespace StreamingImageSequencePluginTest {

class TestUtility {

public:
    static bool LoadTestImages(const uint32_t imageType, const int frame, const uint32_t start, const uint32_t );
    static bool CheckLoadedTestImageData(const uint32_t imageType, const int frame, const uint32_t start, 
        const uint32_t numImages, const StreamingImageSequencePlugin::ReadStatus reqReadStatus);
    static void CheckMemoryCleanup();

    static uint32_t FindNumDuplicateMapElements(
            const std::map<strType, StreamingImageSequencePlugin::ImageData>& map0, 
            const std::map<strType, StreamingImageSequencePlugin::ImageData>& map1        
    );

    static std::map<strType, StreamingImageSequencePlugin::ImageData> LoadAndCheckUnloadingOfUnusedImages(
        const uint32_t imageType, const int frame, const uint32_t startTestImageIndex, const uint32_t numImages,
        const std::map<strType, StreamingImageSequencePlugin::ImageData>& prevImageMap);

    static uint32_t CleanupAndLoadMaxImages(const uint32_t imageType);


};

} // end namespace
