// The following ifdef block is the standard way of creating macros which make exporting 
// from a DLL simpler. All files within this DLL are compiled with the DRAWERWIN_EXPORTS
// symbol defined on the command line. This symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// DRAWERWIN_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.
#ifdef _WIN32
#  ifdef DRAWERWIN_EXPORTS
#  define DRAWERWIN_API __declspec(dllexport)
#  else
#  define DRAWERWIN_API __declspec(dllimport)
#  endif
#else
#  define DRAWERWIN_API 
#endif

#ifdef _WIN32
# include <d3d11.h>
# include <d3d9.h>
#endif
#include <thread>
#include <mutex>

#include "Unity/IUnityInterface.h"
#include "Unity/IUnityGraphics.h"

#include "../CommonLib/CommonLib.h"


void UploadTextureToDeviceMetal(TexPointer unityTexture, StReadResult& tResult);
void UploadTextureToDeviceD3D11(TexPointer unityTexture, StReadResult& tResult);
void UploadTextureToDeviceD3D9(TexPointer unityTexture, StReadResult& tResult);
void UploadTextureToDeviceOpenGL(TexPointer unityTexture, StReadResult& tResult);

extern "C"
{

    UNITY_INTERFACE_EXPORT void UNITY_INTERFACE_API SetLoadedTextureToNativeTexture(const charType* fileName, void* texture, u32 uWidth, u32 height);
    
	UNITY_INTERFACE_EXPORT UnityRenderingEvent UNITY_INTERFACE_API GetRenderEventFunc();
	UNITY_INTERFACE_EXPORT void UNITY_INTERFACE_API SetNativeTexturePtr(void* texture, u32 uWidth, u32 height, s32 sObjectID);
	UNITY_INTERFACE_EXPORT void UNITY_INTERFACE_API SetLoadedTexture(const charType* fileName, s32 sObjectID);
	UNITY_INTERFACE_EXPORT void UNITY_INTERFACE_API ResetLoadedTexture(s32 sObjectID);
	UNITY_INTERFACE_EXPORT void UNITY_INTERFACE_API ResetAllLoadedTexture();

	UNITY_INTERFACE_EXPORT void UNITY_INTERFACE_API UnityPluginUnload();
	UNITY_INTERFACE_EXPORT void UNITY_INTERFACE_API UnityPluginLoad(IUnityInterfaces* unityInterfaces);


}

