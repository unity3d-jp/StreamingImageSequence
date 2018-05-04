// DrawerWin.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include  "RenderAPI.h"
#include "../CommonLib/CommonLib.h"
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
static RenderAPI*        s_CurrentAPI = NULL;
static UnityGfxRenderer  s_DeviceType = kUnityGfxRendererNull;






void UpdateTexture(int sEventID)
{
	if (IsPluginResetting())
	{
		return;
	}

	strType wstr;

	{
		CCriticalSectionController cs(INSTANCEID2FILENAME_CS);
		{
			if (g_instanceIdToFileName.find(sEventID) == g_instanceIdToFileName.end())
			{
				return; // not found.
			}
			wstr = g_instanceIdToFileName[sEventID];

		}
	}
	{
		CCriticalSectionController cs(FILENAME2PTR_CS);

		if (g_fileNameToPtrMap.find(wstr) == g_fileNameToPtrMap.end())
		{
			return; // not found.
		}
	}

	StReadResult tResult;
	if (!GetNativTextureInfo(wstr.c_str(), &tResult))
	{
		return; // not found.
	}

	if (tResult.readStatus != 2)
	{
		return;
	}
	TexPointer  unityTexture = nullptr;
	{
		CCriticalSectionController cs(INSTANCEID2TEXTURE_CS);
		{
			if (g_instanceIdToUnityTexturePointer.find(sEventID) == g_instanceIdToUnityTexturePointer.end())
			{
				return; // not found.
			}
			unityTexture = (TexPointer)g_instanceIdToUnityTexturePointer[sEventID];
		}
	}
	if (nullptr == unityTexture)
	{
		return;
	}

    if (s_CurrentAPI)
    {
        
        s_CurrentAPI->UploadTextureToDevice(unityTexture, tResult);
    }

}

#if SUPPORT_D3D11
void UploadTextureToDeviceD3D11(TexPointer unityTexture, StReadResult& tResult) {
    
    D3D11_BOX box;
    box.front = 0;
    box.back = 1;
    box.left = 0;
    box.right = tResult.width;
    box.top = 0;
    box.bottom = tResult.height;
    if (!g_unity)
    {
        return;
    }
    auto device = g_unity->Get<IUnityGraphicsD3D11>()->GetDevice();
    ID3D11DeviceContext* context;
    device->GetImmediateContext(&context);
    if (tResult.buffer && unityTexture) {
        context->UpdateSubresource(reinterpret_cast<ID3D11Texture2D*>(unityTexture), 0, &box, tResult.buffer, tResult.width * 4, tResult.height * 4);
    }
}
#endif

#if SUPPORT_OPENGL_LEGACY || SUPPORT_OPENGL_UNIFIED
void UploadTextureToDeviceOpenGL(TexPointer unityTexture, StReadResult& tResult) {

  GLuint gltex = (GLuint)(size_t)(unityTexture);
  glBindTexture (GL_TEXTURE_2D, gltex);
  int texWidth, texHeight;
  glGetTexLevelParameteriv (GL_TEXTURE_2D, 0, GL_TEXTURE_WIDTH, &texWidth);
  glGetTexLevelParameteriv (GL_TEXTURE_2D, 0, GL_TEXTURE_HEIGHT, &texHeight);

  glTexSubImage2D (GL_TEXTURE_2D, 0, 0, 0, texWidth, texHeight, GL_RGBA, GL_UNSIGNED_BYTE, tResult.buffer);

}
#endif

#if SUPPORT_D3D9
void UploadTextureToDeviceD3D9(TexPointer unityTexture, StReadResult& tResult) {
	D3DLOCKED_RECT tLockedRect;

	IDirect3DTexture9* pTexture = reinterpret_cast<IDirect3DTexture9*>(unityTexture);
	DWORD uFlag = D3DLOCK_DISCARD;
	HRESULT res = pTexture->LockRect(0,&tLockedRect, NULL, uFlag);
	if (res == S_OK)
	{
		memcpy(tLockedRect.pBits, tResult.buffer, tResult.width * tResult.height * 4);
		pTexture->UnlockRect(0);
	}
}
#endif



static void UNITY_INTERFACE_API OnRenderEvent(int eventID)
{
	UpdateTexture(eventID);
}

UNITY_INTERFACE_EXPORT UnityRenderingEvent UNITY_INTERFACE_API GetRenderEventFunc()
{
	return OnRenderEvent;
}



UNITY_INTERFACE_EXPORT void UNITY_INTERFACE_API SetNativeTexturePtr(void* texture, u32 uWidth, u32 uHeight, s32 sObjectID)
{
	CCriticalSectionController cs(INSTANCEID2TEXTURE_CS);
	g_instanceIdToUnityTexturePointer[sObjectID] = reinterpret_cast<TexPointer>(texture);

}

UNITY_INTERFACE_EXPORT void UNITY_INTERFACE_API SetLoadedTexture(const charType* fileName, s32 sObjectID)
{
	StReadResult tReulst;
	if (GetNativTextureInfo(fileName, &tReulst))
	{
		CCriticalSectionController cs(INSTANCEID2FILENAME_CS);
		{
			strType wstr(fileName);
			g_instanceIdToFileName[sObjectID] = wstr;

		}

	}
}

UNITY_INTERFACE_EXPORT void UNITY_INTERFACE_API ResetLoadedTexture(s32 sObjectID)
{
	CCriticalSectionController cs(INSTANCEID2TEXTURE_CS);//
	{
		g_instanceIdToUnityTexturePointer.erase(sObjectID);
	}

}

UNITY_INTERFACE_EXPORT void UNITY_INTERFACE_API ResetAllLoadedTexture()
{

	CCriticalSectionController cs2(INSTANCEID2TEXTURE_CS);
	CCriticalSectionController cs1(INSTANCEID2FILENAME_CS);
	CCriticalSectionController cs0(FILENAME2PTR_CS);
	{


		for (auto itr = g_fileNameToPtrMap.begin(); itr != g_fileNameToPtrMap.end(); ++itr) 
		{
			StReadResult tResult = itr->second;
			if (tResult.buffer)
				NativeFree(tResult.buffer);
		}

		g_instanceIdToUnityTexturePointer.clear();
		g_instanceIdToFileName.clear();
		g_fileNameToPtrMap.clear();
	}
}

static void UNITY_INTERFACE_API OnGraphicsDeviceEvent(UnityGfxDeviceEventType eventType)
{
    // Create graphics API implementation upon initialization
    if (eventType == kUnityGfxDeviceEventInitialize)
    {
        ASSERT(s_CurrentAPI == nullptr);
        s_DeviceType = s_Graphics->GetRenderer();
        s_CurrentAPI = CreateRenderAPI(s_DeviceType);
    }
    
	// Let the implementation process the device related events
	if (s_CurrentAPI)
	{
		s_CurrentAPI->ProcessDeviceEvent(eventType, g_unity);
	}
   
    // Cleanup graphics API implementation upon shutdown
    if (eventType == kUnityGfxDeviceEventShutdown)
    {
        delete s_CurrentAPI;
        s_CurrentAPI = NULL;
        s_DeviceType = kUnityGfxRendererNull;
    }
}

UNITY_INTERFACE_EXPORT void UNITY_INTERFACE_API UnityPluginUnload()
{
    s_Graphics->UnregisterDeviceEventCallback(OnGraphicsDeviceEvent);

}
UNITY_INTERFACE_EXPORT void UNITY_INTERFACE_API UnityPluginLoad(IUnityInterfaces* unityInterfaces)
{
	g_unity = unityInterfaces;
	s_Graphics = g_unity->Get<IUnityGraphics>();
	s_Graphics->RegisterDeviceEventCallback(OnGraphicsDeviceEvent);
	
	// Run OnGraphicsDeviceEvent(initialize) manually on plugin load
	OnGraphicsDeviceEvent(kUnityGfxDeviceEventInitialize);
}



