#include "CommonLib/Types.h" //charType

//Loader
#include "Loader/ImageData.h"

namespace StreamingImageSequencePluginTest {

class TestUtility {



public:
    static bool LoadTestImages(const uint32_t imageType, const int frame, const uint32_t start, const uint32_t );
    static bool CheckLoadedTestImageData(const uint32_t imageType, const int frame, const uint32_t, const uint32_t );
    static void CheckMemoryCleanup();

    static uint32_t TestUtility::FindNumDuplicateMapElements(
            const std::map<strType, StreamingImageSequencePlugin::ImageData>& map0, 
            const std::map<strType, StreamingImageSequencePlugin::ImageData>& map1        
    );

};

} // end namespace
