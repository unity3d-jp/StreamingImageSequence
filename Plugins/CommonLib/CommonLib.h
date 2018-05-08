// The following ifdef block is the standard way of creating macros which make exporting 
// from a DLL simpler. All files within this DLL are compiled with the COMMONLIBWIN_EXPORTS
// symbol defined on the command line. This symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// COMMONLIBWIN_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.
#ifndef COMMONLIB
#define COMMONLIB

#include "../Drawer/PlatformBase.h"
#include "../Drawer/Unity/IUnityGraphics.h"

#ifdef _WIN32
#  ifdef COMMONLIBWIN_EXPORTS
#  define COMMONLIBWIN_API __declspec(dllexport)
#  else
#  define COMMONLIBWIN_API __declspec(dllimport)
#  endif
#else
#  define COMMONLIBWIN_API
#endif

#ifdef _WIN32
#  include <d3d11.h>
#  include <objidl.h>
#  include <gdiplus.h>
#endif //_WIN32

#if SUPPORT_OPENGL_LEGACY
#  include "../Drawer/GLEW/glew.h"
#endif

#include <thread>
#include <mutex>
#include <string>
#include <map>

#include <stdio.h>
#include <stdlib.h>
#include <locale.h>
#include <string.h>


#ifdef _WIN32
  typedef UINT64 u64;
  typedef UINT32 u32;
  typedef INT32 s32;
  typedef UINT16 u16;
  typedef UINT8 u8;
#else
  typedef unsigned long u64;
  typedef unsigned int u32;
  typedef int s32;
  typedef short u16;
  typedef char u8;
#endif

#ifndef _WIN32
typedef wchar_t WCHAR;
#endif


#define  USE_WCHAR 0

#if USE_WCHAR
  typedef WCHAR charType;
  typedef std::wstring strType;
#else
  typedef char charType;
  typedef std::string strType;
#endif


#define BIT(x) (1<<x)
#if _DEBUG && defined(_WIN32)
#define ASSERT( xx_ )  (xx_) ? (void)0 : __debugbreak()
#else
#define ASSERT( xx_ )   (void)0 
#endif

typedef void* TexPointer;
typedef u32 ReadStatus;

struct StReadResult
{
    u8*  buffer;
    u32    width;
    u32    height;
    ReadStatus readStatus;
    
    StReadResult()
    {
        buffer = NULL;
        width = 0;
        height = 0;
        readStatus = 0;
    }
};

class COMMONLIBWIN_API CCriticalSectionObject
{
private:
#ifdef _WIN32
	CRITICAL_SECTION m_cs;
#else
    std::recursive_mutex m_cs;
#endif
	void operator =(const CCriticalSectionObject& src) {}
public:
	CCriticalSectionObject();
	virtual ~CCriticalSectionObject();

	void Enter();
	void Leave();
};

class COMMONLIBWIN_API CCriticalSectionController
{
	CCriticalSectionObject* m_cs;
	CCriticalSectionController(){};
public:
	CCriticalSectionController(CCriticalSectionObject* cs);
	virtual ~CCriticalSectionController();
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
extern COMMONLIBWIN_API std::map<int, void*>				 g_instanceIdToUnityTexturePointer;
extern COMMONLIBWIN_API std::map<strType, int>				 g_scenePathToSceneStatus;
extern COMMONLIBWIN_API	 CCommonLib                 g_CCommonLib;

enum {
	eCS_FileNameToPtr = 0,
	eCS_InstanceIdToFileName,
	eCS_InstanceIdToUnityTexturePtr,
	eCS_LoadingCounter,
	eCS_Resetting,
	eCS_MAX = 32
};

#define FILENAME2PTR_CS					(&g_CriticalSectionObjectArray[eCS_FileNameToPtr])
#define INSTANCEID2FILENAME_CS			(&g_CriticalSectionObjectArray[eCS_InstanceIdToFileName])
#define INSTANCEID2TEXTURE_CS			(&g_CriticalSectionObjectArray[eCS_InstanceIdToUnityTexturePtr])
#define LOADINGCOUNTER_CS			(&g_CriticalSectionObjectArray[eCS_LoadingCounter])
#define RESETTING_CS				(&g_CriticalSectionObjectArray[eCS_Resetting])
extern COMMONLIBWIN_API CCriticalSectionObject     g_CriticalSectionObjectArray[eCS_MAX];
extern COMMONLIBWIN_API	int						g_LoadingFileCounter;
extern COMMONLIBWIN_API	int						g_IsResetting;


#endif //#ifdef COMMONLIB
