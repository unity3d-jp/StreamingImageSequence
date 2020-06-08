#pragma once

#include "CommonLib/Types.h"

//Loader
#include "FileType.h"

namespace StreamingImageSequencePlugin {

class ImageCatalog;
struct ImageData;

//Forward declarations
void LoadTGAFileAndAlloc(const strType& imagePath, const uint32_t imageType, ImageCatalog*);
void LoadPNGFileAndAlloc(const strType& imagePath, const uint32_t imageType, ImageCatalog*);

//[Note-sin: 2020-2-4] The functions in this class are not guaranteed to be thread-safe. 
//Use CriticalSectionController to guarantee thread safe when calling if required.
class LoaderUtility {
public:
    static FileType CheckFileType(const strType& imagePath);
    static bool GetImageDataInto(const strType& imagePath, const uint32_t imageType, ImageCatalog*,ImageData*);
    static bool LoadAndAllocImage(const strType& imagePath, const uint32_t imageType, ImageCatalog* textureCatalog );

    static bool LoadAndAllocImage(const strType& imagePath, const uint32_t imageType, ImageCatalog* textureCatalog,
        const uint32_t reqWidth, const uint32_t reqHeight);

};

} //end namespace

