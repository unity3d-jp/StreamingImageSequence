#pragma once

#include "CommonLib/Types.h"

//Loader
#include "FileType.h"
#include "ReadStatus.h"

namespace StreamingImageSequencePlugin {

class ImageCatalog;
struct ImageData;

//Forward declarations
void LoadPNGFileAndAlloc(const strType& imagePath, const uint32_t imageType, ImageCatalog*);

//[Note-sin: 2020-2-4] The functions in this class are not guaranteed to be thread-safe. 
//Use CriticalSectionController to guarantee thread safe when calling if required.
class LoaderUtility {
public:
    static FileType CheckFileType(const strType& imagePath);
    static const ImageData* GetImageData(const strType& imagePath, const uint32_t imageType,ImageCatalog*,const int);
    static const ImageData* LoadAndAllocImage(const strType& imagePath, const uint32_t imageType, ImageCatalog*, const int frame);
    static const ImageData* LoadAndAllocImage(const strType& imagePath, const uint32_t imageType, ImageCatalog*,
        const uint32_t reqWidth, const uint32_t reqHeight, const int frame);

private:
    static bool IsImageLoadError(const ReadStatus );

};

} //end namespace

