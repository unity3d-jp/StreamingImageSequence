#include "stdafx.h"
#include "RenderAPI.h"
#include "PlatformBase.h"
#include "Drawer.h"
// Direct3D 11 implementation of RenderAPI.

#if SUPPORT_D3D11

#include <assert.h>
#include <d3d11.h>
#include "Unity/IUnityGraphicsD3D11.h"


class RenderAPI_D3D11 : public RenderAPI
{
public:
	RenderAPI_D3D11();
	virtual ~RenderAPI_D3D11() { }

	virtual void ProcessDeviceEvent(UnityGfxDeviceEventType type, IUnityInterfaces* interfaces);

	virtual void UploadTextureToDevice(TexPointer unityTexture, StReadResult& tResult);

private:
	void CreateResources();
	void ReleaseResources();

private:
	ID3D11Device* m_Device;

};


RenderAPI* CreateRenderAPI_D3D11()
{
	return new RenderAPI_D3D11();
}






RenderAPI_D3D11::RenderAPI_D3D11()
	: m_Device(NULL)

{
}


void RenderAPI_D3D11::ProcessDeviceEvent(UnityGfxDeviceEventType type, IUnityInterfaces* interfaces)
{
	switch (type)
	{
	case kUnityGfxDeviceEventInitialize:
	{
		IUnityGraphicsD3D11* d3d = interfaces->Get<IUnityGraphicsD3D11>();
		m_Device = d3d->GetDevice();
		CreateResources();
		break;
	}
	case kUnityGfxDeviceEventShutdown:
		ReleaseResources();
		break;
	}
}


void RenderAPI_D3D11::CreateResources()
{
}


void RenderAPI_D3D11::ReleaseResources()
{
}

void RenderAPI_D3D11::UploadTextureToDevice(TexPointer unityTexture, StReadResult& tResult)
{
	UploadTextureToDeviceD3D11(unityTexture, tResult);
}


#endif // #if SUPPORT_D3D11
