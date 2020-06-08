#include "stdafx.h"
#include "ImageCatalog.h"



namespace StreamingImageSequencePlugin {

ImageCatalog::ImageCatalog() : m_latestRequestFrame(0) {
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

//----------------------------------------------------------------------------------------------------------------------
void ImageCatalog::UpdateRequestFrame(const int frame) {

    if (frame <= m_latestRequestFrame) {
        //overflow check
        const bool isOverflow = frame < 0 && m_latestRequestFrame >= 0;
        if (!isOverflow) {
            return;
        }
    }

    for (uint32_t imageType = 0; imageType < MAX_CRITICAL_SECTION_TYPE_IMAGES; ++imageType) {
        m_imageCollection[imageType].AdvanceOrder();
    }

    m_latestRequestFrame = frame;

}


} //end namespace
