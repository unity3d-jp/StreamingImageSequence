#include "stdafx.h"
#include "ImageCatalog.h"

#include "CommonLib/CommonLib.h"                    //IMAGE_CS
#include "CommonLib/CriticalSectionController.h"


namespace StreamingImageSequencePlugin {

ImageCatalog::ImageCatalog() : m_latestRequestFrame(0) {
    for (uint32_t imageType = 0; imageType < MAX_CRITICAL_SECTION_TYPE_IMAGES; ++imageType) {
        m_imageCollection[imageType].Init(static_cast<CriticalSectionType>(imageType), &m_memAllocator);
    }

}

//----------------------------------------------------------------------------------------------------------------------

//Wrapper for functions in ImageCollection 
const ImageData* ImageCatalog::GetImage(const strType& imagePath, const uint32_t imageType, const int frame) {
    ASSERT(imageType < MAX_CRITICAL_SECTION_TYPE_IMAGES);
    UpdateRequestFrame(frame);

    //[Note-sin: 2020-6-8] indicate if this image is for the current order by comparing frame and m_latestRequestFrame.
    //Also check overFlow when frame seems to be higher, but it's actually older
    const bool isOverFlow = (frame > 0 && m_latestRequestFrame < 0);

    return m_imageCollection[imageType].GetImage(imagePath, frame >= m_latestRequestFrame && !isOverFlow);
}

//----------------------------------------------------------------------------------------------------------------------

//Thread-safe
void ImageCatalog::Reset() {
    for (uint32_t imageType = 0; imageType < MAX_CRITICAL_SECTION_TYPE_IMAGES; ++imageType) {
        m_imageCollection[imageType].UnloadAllImages();
    }

    {
        CriticalSectionController cs0(IMAGE_CS(CRITICAL_SECTION_TYPE_FULL_IMAGE));
        CriticalSectionController cs1(IMAGE_CS(CRITICAL_SECTION_TYPE_PREVIEW_IMAGE));
        m_latestRequestFrame = 0;

    }
}

//Thread-safe
void ImageCatalog::ResetRequestFrame() {

    CriticalSectionController cs0(IMAGE_CS(CRITICAL_SECTION_TYPE_FULL_IMAGE));
    CriticalSectionController cs1(IMAGE_CS(CRITICAL_SECTION_TYPE_PREVIEW_IMAGE));

    for (uint32_t imageType = 0; imageType < MAX_CRITICAL_SECTION_TYPE_IMAGES; ++imageType) {
        m_imageCollection[imageType].ResetOrder();
    }
    m_latestRequestFrame = 0;
}

//----------------------------------------------------------------------------------------------------------------------

//Thread-safe
void ImageCatalog::UpdateRequestFrame(const int frame) {
    CriticalSectionController cs0(IMAGE_CS(CRITICAL_SECTION_TYPE_FULL_IMAGE));
    CriticalSectionController cs1(IMAGE_CS(CRITICAL_SECTION_TYPE_PREVIEW_IMAGE));


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
