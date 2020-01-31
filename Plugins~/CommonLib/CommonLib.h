// The following ifdef block is the standard way of creating macros which make exporting 
// from a DLL simpler. All files within this DLL are compiled with the COMMONLIBWIN_EXPORTS
// symbol defined on the command line. This symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// COMMONLIBWIN_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.
#ifndef COMMONLIB
#define COMMONLIB

#include "ReadStatus.h"
#include "Types.h"

#include "../Drawer/PlatformBase.h"
#include "../Drawer/Unity/IUnityGraphics.h"

#if SUPPORT_OPENGL_LEGACY
#  include "../Drawer/GLEW/glew.h"
#endif


typedef void* TexPointer;

struct StReadResult
{
    u8*  buffer;
    u32    width;
    u32    height;
    StreamingImageSequencePlugin::ReadStatus readStatus;
    
    StReadResult()
    {
        buffer = NULL;
        width = 0;
        height = 0;
        readStatus = StreamingImageSequencePlugin::READ_STATUS_NONE;
    }
};

// This class is exported from the CommonLibWin.dll
class COMMONLIBWIN_API CCommonLib {
#ifdef _WIN32
	Gdiplus::GdiplusStartupInput    startInput;
	ULONG_PTR                       token;
#endif

public:
	CCommonLib();
	virtual ~CCommonLib();
};



extern COMMONLIBWIN_API std::map<strType, StReadResult> g_fileNameToPtrMap;
extern COMMONLIBWIN_API std::map<int, strType>          g_instanceIdToFileName;
extern COMMONLIBWIN_API std::map<int, void*>            g_instanceIdToUnityTexturePointer;
extern COMMONLIBWIN_API std::map<strType, int>          g_scenePathToSceneStatus;
extern COMMONLIBWIN_API CCommonLib                      g_CCommonLib;

//----------------------------------------------------------------------------------------------------------------------

#include "CriticalSection.h"

#define FILENAME2PTR_CS             (StreamingImageSequencePlugin::CriticalSection::GetInstance().GetObject(StreamingImageSequencePlugin::CRITICAL_SECTION_TYPE_FILENAME_TO_PTR))
#define INSTANCEID2FILENAME_CS      (StreamingImageSequencePlugin::CriticalSection::GetInstance().GetObject(StreamingImageSequencePlugin::CRITICAL_SECTION_TYPE_INSTANCE_ID_TO_FILENAME))
#define INSTANCEID2TEXTURE_CS       (StreamingImageSequencePlugin::CriticalSection::GetInstance().GetObject(StreamingImageSequencePlugin::CRITICAL_SECTION_TYPE_INSTANCE_ID_TO_UNITY_TEXTURE_PTR))
#define LOADINGCOUNTER_CS           (StreamingImageSequencePlugin::CriticalSection::GetInstance().GetObject(StreamingImageSequencePlugin::CRITICAL_SECTION_TYPE_LOADING_COUNTER))
#define RESETTING_CS                (StreamingImageSequencePlugin::CriticalSection::GetInstance().GetObject(StreamingImageSequencePlugin::CRITICAL_SECTION_TYPE_RESETTING))


#endif //#ifdef COMMONLIB
