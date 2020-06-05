//  Loader.mm
//  Created by Hiroki Omae on 2017/10/10.
//  Copyright © 2017 Unity Technologies. All rights reserved.

#import <Foundation/Foundation.h>
#include <ApplicationServices/ApplicationServices.h>

#include "CommonLib/CommonLib.h"

//Loader
#include "Loader.h"
#include "Loader/ImageCatalog.h"

#define DEBUG_MAC_DRAWING (0)

namespace StreamingImageSequencePlugin {

const uint32_t NUM_CHANNELS = 4;

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
                                                 8, width * NUM_CHANNELS,
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
    ImageData imageData(nullptr,0,0,0,StreamingImageSequencePlugin::READ_STATUS_FAIL);
    if (nullptr!=image) {

        const u32 width = (u32) CGImageGetWidth(image);
        const u32 height = (u32) CGImageGetHeight(image);
        const uint32_t dataSize = width * height* NUM_CHANNELS;
        u8* rawData = (u8*)malloc(dataSize);
        
        if(nullptr!=rawData) {
            memset(rawData,0,dataSize);

            CGImageRefRetrievePixelData(image, width, height, rawData);

            imageData.Width  = width;
            imageData.Height = height;
            imageData.RawData  = rawData;
            imageData.DataSize = dataSize;
            imageData.CurrentReadStatus =StreamingImageSequencePlugin::READ_STATUS_SUCCESS;
        }
    }
    imageCatalog->SetImage(imagePath,imageType, &imageData);

    CGImageRelease(image);
    
}

//----------------------------------------------------------------------------------------------------------------------
void LoadTGAFileAndAlloc(const strType& imagePath, const uint32_t imageType, ImageCatalog*) {
    assert(false);   //Not implemented yet
}
//----------------------------------------------------------------------------------------------------------------------


} //end namespace
