#include "stdafx.h"
#include "RenderAPI.h"
#include "PlatformBase.h"

// Direct3D 9 implementation of RenderAPI.

#if SUPPORT_D3D9

#include <assert.h>
#include <d3d9.h>
#include "Unity/IUnityGraphicsD3D9.h"

#include "Drawer.h"

class RenderAPI_D3D9 : public RenderAPI
{
public:
	RenderAPI_D3D9();
	virtual ~RenderAPI_D3D9() { }
	virtual void ProcessDeviceEvent(UnityGfxDeviceEventType type, IUnityInterfaces* interfaces);

	virtual void UploadTextureToDevice(TexPointer unityTexture, StReadResult& tResult);



private:
	IDirect3DDevice9* m_Device;
};


RenderAPI* CreateRenderAPI_D3D9()
{
	return new RenderAPI_D3D9();
}


RenderAPI_D3D9::RenderAPI_D3D9()
	: m_Device(NULL)
{
}


void RenderAPI_D3D9::ProcessDeviceEvent(UnityGfxDeviceEventType type, IUnityInterfaces* interfaces)
{
	switch (type)
	{
	case kUnityGfxDeviceEventInitialize:
	{
		IUnityGraphicsD3D9* d3d = interfaces->Get<IUnityGraphicsD3D9>();
		m_Device = d3d->GetDevice();
	}
	// fall-through!
	case kUnityGfxDeviceEventAfterReset:
		break;
	case kUnityGfxDeviceEventBeforeReset:
	case kUnityGfxDeviceEventShutdown:

		break;
	}
}

void RenderAPI_D3D9::UploadTextureToDevice(TexPointer unityTexture, StReadResult& tResult)
{
	UploadTextureToDeviceD3D9(unityTexture, tResult);
}



#endif // #if SUPPORT_D3D9
