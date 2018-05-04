#include "stdafx.h"
#include "RenderAPI.h"
#include "PlatformBase.h"

// Direct3D 12 implementation of RenderAPI.


#if SUPPORT_D3D12

#include <assert.h>
#include <d3d12.h>
#include "Unity/IUnityGraphicsD3D12.h"


class RenderAPI_D3D12 : public RenderAPI
{
public:
	RenderAPI_D3D12();
	virtual ~RenderAPI_D3D12() { }

	virtual void ProcessDeviceEvent(UnityGfxDeviceEventType type, IUnityInterfaces* interfaces);

	virtual void  UploadTextureToDevice(TexPointer unityTexture, StReadResult& tResult);




private:
	IUnityGraphicsD3D12v2* s_D3D12;

};


RenderAPI* CreateRenderAPI_D3D12()
{
	return new RenderAPI_D3D12();
}


const UINT kNodeMask = 0;


RenderAPI_D3D12::RenderAPI_D3D12()
	: s_D3D12(NULL)

{
}








void RenderAPI_D3D12::ProcessDeviceEvent(UnityGfxDeviceEventType type, IUnityInterfaces* interfaces)
{
	switch (type)
	{
	case kUnityGfxDeviceEventInitialize:
		s_D3D12 = interfaces->Get<IUnityGraphicsD3D12v2>();

		break;
	case kUnityGfxDeviceEventShutdown:

		break;
	}
}

void RenderAPI_D3D12::UploadTextureToDevice(TexPointer unityTexture, StReadResult& tResult)
{

}






#endif // #if SUPPORT_D3D12
