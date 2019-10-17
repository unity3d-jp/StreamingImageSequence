#include "stdafx.h"
#include "RenderAPI.h"
#include "PlatformBase.h"

// OpenGL 2 (legacy, deprecated) implementation of RenderAPI.


#if SUPPORT_OPENGL_LEGACY

#include "GLEW/glew.h"
#include "Drawer.h"

class RenderAPI_OpenGL2 : public RenderAPI
{
public:
	RenderAPI_OpenGL2();
	virtual ~RenderAPI_OpenGL2() { }

	virtual void ProcessDeviceEvent(UnityGfxDeviceEventType type, IUnityInterfaces* interfaces);

    virtual void  UploadTextureToDevice(TexPointer unityTexture, StReadResult& tResult);
private:
    void CreateResources();
	void ReleaseResources();
};


RenderAPI* CreateRenderAPI_OpenGL2()
{
	return new RenderAPI_OpenGL2();
}


RenderAPI_OpenGL2::RenderAPI_OpenGL2()
{
}

void RenderAPI_OpenGL2::CreateResources()
{
}

void RenderAPI_OpenGL2::ReleaseResources()
{
}

void RenderAPI_OpenGL2::ProcessDeviceEvent(UnityGfxDeviceEventType type, IUnityInterfaces* interfaces)
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

void  RenderAPI_OpenGL2::UploadTextureToDevice(TexPointer unityTexture, StReadResult& tResult)
{
    UploadTextureToDeviceOpenGL(unityTexture,  tResult);
}

#endif // #if SUPPORT_OPENGL_LEGACY
