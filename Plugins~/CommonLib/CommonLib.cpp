// CommonLibWin.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include "CommonLib.h"
#ifdef _WIN32
#pragma comment(lib, "gdiplus.lib")
#endif

#include "CCriticalSectionObject.h"

using namespace std;
COMMONLIBWIN_API	int							g_LoadingFileCounter = 0;
COMMONLIBWIN_API	int							g_IsResetting = 0;
COMMONLIBWIN_API    CCriticalSectionObject   g_CriticalSectionObjectArray[eCS_MAX];
COMMONLIBWIN_API    map<strType, StReadResult> g_fileNameToPtrMap;
COMMONLIBWIN_API    map<int, strType>          g_instanceIdToFileName;
COMMONLIBWIN_API    map<int, void*>            g_instanceIdToUnityTexturePointer;
COMMONLIBWIN_API    map<strType, int>			g_scenePathToSceneStatus;
COMMONLIBWIN_API	CCommonLib                 g_CCommonLib;


// This is an example of an exported variable
COMMONLIBWIN_API int nCommonLibWin=0;

// This is an example of an exported function.
COMMONLIBWIN_API int fnCommonLibWin(void)
{
	return 42;
}

// This is the constructor of a class that has been exported.
// see CommonLibWin.h for the class definition
CCommonLib::CCommonLib()
{
#ifdef _WIN32
	int status = Gdiplus::GdiplusStartup(&token, &startInput, NULL);
#endif
}

CCommonLib::~CCommonLib()
{
#ifdef _WIN32
	Gdiplus::GdiplusShutdown(token);
#endif
}
