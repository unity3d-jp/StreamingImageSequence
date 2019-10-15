
#include "RenderAPI.h"
#include "PlatformBase.h"


// Metal implementation of RenderAPI.


#if SUPPORT_METAL

#include "Unity/IUnityGraphicsMetal.h"
#import <Metal/Metal.h>
#include "Drawer.h"

class RenderAPI_Metal : public RenderAPI
{
public:
	RenderAPI_Metal();
	virtual ~RenderAPI_Metal() { }
	
    virtual void  UploadTextureToDevice(TexPointer unityTexture, StReadResult& tResult);
    virtual void ProcessDeviceEvent(UnityGfxDeviceEventType type, IUnityInterfaces* interfaces);

private:
    void CreateResources();
    void ReleaseResources();
private:
	IUnityGraphicsMetal*	m_MetalGraphics;
};


RenderAPI* CreateRenderAPI_Metal()
{
	return new RenderAPI_Metal();
}





RenderAPI_Metal::RenderAPI_Metal()
{
}

void RenderAPI_Metal::ProcessDeviceEvent(UnityGfxDeviceEventType type, IUnityInterfaces* interfaces)
{
    switch (type)
    {
        case kUnityGfxDeviceEventInitialize:
        {
            CreateResources();
            break;
        }
        case kUnityGfxDeviceEventShutdown:
            ReleaseResources();
            break;
    }
}

void  RenderAPI_Metal::UploadTextureToDevice(TexPointer unityTexture, StReadResult& tResult)
{
    UploadTextureToDeviceMetal(unityTexture,  tResult);
}

void RenderAPI_Metal::CreateResources()
{
}

void RenderAPI_Metal::ReleaseResources()
{
}







#endif // #if SUPPORT_METAL
