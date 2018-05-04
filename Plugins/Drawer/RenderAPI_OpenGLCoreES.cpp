#include "stdafx.h"
#include "RenderAPI.h"
#include "PlatformBase.h"

// OpenGL Core profile (desktop) or OpenGL ES (mobile) implementation of RenderAPI.
// Supports several flavors: Core, ES2, ES3


#if SUPPORT_OPENGL_UNIFIED

#include <assert.h>
#if UNITY_IPHONE
#	include <OpenGLES/ES2/gl.h>
#elif UNITY_ANDROID || UNITY_WEBGL
#	include <GLES2/gl2.h>
#else
#	include "GLEW/glew.h"
#endif
#include "Drawer.h"


class RenderAPI_OpenGLCoreES : public RenderAPI
{
public:
	RenderAPI_OpenGLCoreES(UnityGfxRenderer apiType);
	virtual ~RenderAPI_OpenGLCoreES() { }
	virtual void ProcessDeviceEvent(UnityGfxDeviceEventType type, IUnityInterfaces* interfaces);

    virtual void  UploadTextureToDevice(TexPointer unityTexture, StReadResult& tResult);

private:
	UnityGfxRenderer m_APIType;
	void CreateResources();
	void ReleaseResources();
};


RenderAPI* CreateRenderAPI_OpenGLCoreES(UnityGfxRenderer apiType)
{
	return new RenderAPI_OpenGLCoreES(apiType);
}




RenderAPI_OpenGLCoreES::RenderAPI_OpenGLCoreES(UnityGfxRenderer apiType)
	: m_APIType(apiType)
{
}

void RenderAPI_OpenGLCoreES::CreateResources()
{
}

void RenderAPI_OpenGLCoreES::ReleaseResources()
{
}

void RenderAPI_OpenGLCoreES::ProcessDeviceEvent(UnityGfxDeviceEventType type, IUnityInterfaces* interfaces)
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

void  RenderAPI_OpenGLCoreES::UploadTextureToDevice(TexPointer unityTexture, StReadResult& tResult)
{
	UploadTextureToDeviceOpenGL(unityTexture, tResult);
}


#endif // #if SUPPORT_OPENGL_UNIFIED
