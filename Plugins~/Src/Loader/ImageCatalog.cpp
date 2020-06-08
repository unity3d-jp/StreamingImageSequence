#include "stdafx.h"
#include "ImageCatalog.h"



namespace StreamingImageSequencePlugin {

ImageCatalog::ImageCatalog() {
    for (uint32_t imageType = 0; imageType < MAX_CRITICAL_SECTION_TYPE_IMAGES; ++imageType) {
        m_imageCollection[imageType].SetMemoryAllocator(&m_memAllocator);
    }

}
//----------------------------------------------------------------------------------------------------------------------

void ImageCatalog::UnloadAllImages() {
    for (uint32_t imageType = 0; imageType < MAX_CRITICAL_SECTION_TYPE_IMAGES; ++imageType) {
        m_imageCollection[imageType].UnloadAllImages();
    }
}




} //end namespace
