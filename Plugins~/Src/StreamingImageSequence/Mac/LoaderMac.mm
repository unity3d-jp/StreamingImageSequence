//  Loader.mm
//  Created by Hiroki Omae on 2017/10/10.
//  Copyright © 2017 Unity Technologies. All rights reserved.

#import <Foundation/Foundation.h>
#include <ApplicationServices/ApplicationServices.h>

#include "CommonLib/CommonLib.h"

//Loader
#include "Loader.h"
#include "Loader/ImageCatalog.h"
#include "Loader/LoaderConstants.h"

#define DEBUG_MAC_DRAWING (0)

namespace StreamingImageSequencePlugin {


CGImageRef CGImageRefLoad(const char *filename) {
    NSString *path = [NSString stringWithUTF8String:filename];
    
    CGDataProviderRef dataProvider = CGDataProviderCreateWithFilename([path UTF8String]);
    CGImageRef image = CGImageCreateWithPNGDataProvider(dataProvider, NULL, NO, kCGRenderingIntentDefault);

    CGDataProviderRelease(dataProvider);
    
    return image;
}

//----------------------------------------------------------------------------------------------------------------------
void CGImageRefRetrievePixelData(const CGImageRef image, u32 width, u32 height, u8* output) {

    CGContextRef context = CGBitmapContextCreate(output,
                                                 width, height,
                                                 8, width * LoaderConstants::NUM_BYTES_PER_TEXEL,
                                                 CGImageGetColorSpace(image),
                                                 kCGImageAlphaPremultipliedLast);

    
    CGContextTranslateCTM(context, 0, height);
    CGContextScaleCTM(context, 1.0, -1.0);
#if DEBUG_MAC_DRAWING
    CGContextSetRGBFillColor(context, 1.0f,0.0f,0.0f,0.5f);
    CGContextFillRect(context, CGRectMake(0, 0, width, height));
#endif
    CGContextDrawImage(context,
                       CGRectMake(0.0, 0.0, (float)width, (float)height),
                       image);
    CGContextRelease(context);
}

//----------------------------------------------------------------------------------------------------------------------

void LoadPNGFileAndAlloc(const strType& imagePath, const uint32_t imageType, ImageCatalog* imageCatalog) {
        
    const CGImageRef image = CGImageRefLoad(imagePath.c_str());
    ReadStatus status = StreamingImageSequencePlugin::READ_STATUS_FAIL;
    if (nullptr!=image) {

        const u32 width = (u32) CGImageGetWidth(image);
        const u32 height = (u32) CGImageGetHeight(image);
        
        const ImageData* imageData = imageCatalog->AllocateImage(imagePath, imageType, width, height);
        if (nullptr == imageData) {
            status =StreamingImageSequencePlugin::READ_STATUS_OUT_OF_MEMORY;
        } else {
            u8* rawData = imageData->RawData;
            CGImageRefRetrievePixelData(image, width, height, rawData);
            status = StreamingImageSequencePlugin::READ_STATUS_SUCCESS;
        }
    }
    imageCatalog->SetImageStatus(imagePath,imageType, status);

    CGImageRelease(image);
    
}

//----------------------------------------------------------------------------------------------------------------------
void LoadTGAFileAndAlloc(const strType& imagePath, const uint32_t imageType, ImageCatalog*) {
    assert(false);   //Not implemented yet
}
//----------------------------------------------------------------------------------------------------------------------


} //end namespace
