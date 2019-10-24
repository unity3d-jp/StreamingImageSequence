//
//  Drawer.m
//  Project
//
//  Created by Toshiyuki Mori on 2017/09/04.
//  Copyright © 2017 Unity Technologies. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <Metal/Metal.h>

#include "stdafx.h"
#include "../CommonLib/CommonLib.h"
#include "../Loader/Loader.h"
#include "Drawer.h"

#include "Unity/IUnityGraphicsMetal.h"



extern IUnityInterfaces* g_unity;
extern void* loadPNGFileAndAlloc(const charType* fileName, StReadResult* pResult);


#if SUPPORT_METAL

void UploadTextureToDeviceMetal(TexPointer unityTexture, StReadResult& tResult) {
    
    id<MTLTexture> tex = (__bridge id<MTLTexture>)unityTexture;
    
    MTLRegion r = MTLRegionMake3D(0,0,0, tResult.width,tResult.height,1);

    if (tResult.buffer && tex) {
        [tex replaceRegion:r mipmapLevel:0 withBytes:tResult.buffer bytesPerRow:tResult.width * 4];
    }
}


#endif

