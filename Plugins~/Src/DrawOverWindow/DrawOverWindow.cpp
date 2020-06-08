// DrawOverWindow.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include "../CommonLib/CommonLib.h"
#include "DrawOverWindow.h"

#pragma comment( lib, "winmm.lib" )
#pragma comment(lib, "gdiplus.lib")

#include <d3d11.h> //Must be included before gdiplus
#include <gdiplus.h>

using namespace Gdiplus;

std::map<int, int>                           g_instanceIdToLoadedFlags;

CDrawOverWindow::CDrawOverWindow()
{
	ASSERT(0);
}



CDrawOverWindow::~CDrawOverWindow()
{
	::DestroyWindow(m_hWnd);
}



DRAWOVERWINDOW_API void  HideOverwrapWindow(int sInstanceId)
{

}

DRAWOVERWINDOW_API void  HideAllOverwrapWindows()
{


}








DRAWOVERWINDOW_API void   SetAllAreLoaded(int sInstanceId, int flag)
{

    g_instanceIdToLoadedFlags[sInstanceId] = flag;
}

DRAWOVERWINDOW_API int   GetAllAreLoaded(int sInstanceId)
{
	if (g_instanceIdToLoadedFlags.find(sInstanceId) == g_instanceIdToLoadedFlags.end())
	{
		return 0;

	}
	return g_instanceIdToLoadedFlags[sInstanceId] ;
}

DRAWOVERWINDOW_API void ResetOverwrapWindows()
{


	g_instanceIdToLoadedFlags.clear();

}