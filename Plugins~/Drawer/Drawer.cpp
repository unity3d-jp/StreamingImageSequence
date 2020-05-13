// DrawerWin.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include "../CommonLib/CommonLib.h"
#include "../CommonLib/CriticalSectionController.h"
#include "../Loader/Loader.h"
#include "Drawer.h"

#pragma comment( lib, "opengl32.lib" )

using namespace std;

#ifdef _WIN32
#include "Unity/IUnityGraphicsD3D11.h"
#include "Unity/IUnityGraphicsD3D9.h"
using namespace Gdiplus;

#else
#endif

IUnityInterfaces* g_unity = nullptr;
static IUnityGraphics*   s_Graphics = nullptr;

//Note-sin: 2020-5-13 This was a function that was used to upload CPU texture data to GPU, but the D3D11 version 
//was causing crashes since it used ID3D11DeviceContext::UpdateSubresource() which requires GPU data as source, 
//while our design puts the memory of sequence images in CPU.

UNITY_INTERFACE_EXPORT void UNITY_INTERFACE_API ResetAllLoadedTexture() {
    using namespace StreamingImageSequencePlugin;

    CriticalSectionController cs2(INSTANCEID2TEXTURE_CS);
    CriticalSectionController cs1(INSTANCEID2FILENAME_CS);

    //Reset all textures
    for (uint32_t texType = 0; texType < MAX_CRITICAL_SECTION_TYPE_TEXTURES; ++texType) {
        CriticalSectionController cs0(TEXTURE_CS(texType));
        {
            for (auto itr = g_fileNameToPtrMap[texType].begin(); itr != g_fileNameToPtrMap[texType].end(); ++itr)
            {
                StReadResult readResult = itr->second;
                if (readResult.buffer)
                    NativeFree(readResult.buffer);
            }
            g_fileNameToPtrMap[texType].clear();
        }
    }

}

