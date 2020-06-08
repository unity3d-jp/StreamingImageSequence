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

std::map<int, CDrawOverWindow*>				 g_instanceIdToWindow;
std::map<int, int>                           g_instanceIdToLoadedFlags;

CDrawOverWindow::CDrawOverWindow()
{
	ASSERT(0);
}

CDrawOverWindow::CDrawOverWindow(int posX, int posY, int width, int height)
	:m_hWnd(CreateWindowEx(
	WS_EX_LEFT | WS_EX_TOPMOST | WS_EX_TOOLWINDOW, TEXT("STATIC"), TEXT("STATIC"),
	WS_POPUP ,
	posX, posY, width, height,
	NULL, NULL, NULL, NULL)),
	m_sLastPosX(posX),
	m_sLastPosY(posY),
	m_sLastWidth(width),
	m_sLastHeight(height)
{
	HBRUSH brush = CreateSolidBrush(RGB(0, 0, 255));
	SetClassLongPtr(m_hWnd, GCLP_HBRBACKGROUND, (LONG)brush);
}


CDrawOverWindow::~CDrawOverWindow()
{
	::DestroyWindow(m_hWnd);
}



DRAWOVERWINDOW_API void  HideOverwrapWindow(int sInstanceId)
{
	if (g_instanceIdToWindow.find(sInstanceId) == g_instanceIdToWindow.end())
	{
		return;
	}
	CDrawOverWindow*  pWindow = g_instanceIdToWindow[sInstanceId];
	ASSERT(pWindow);
	::ShowWindow(pWindow->m_hWnd, SW_HIDE);

}

DRAWOVERWINDOW_API void  HideAllOverwrapWindows()
{

	for (auto it = g_instanceIdToWindow.begin();
		it != g_instanceIdToWindow.end();
		++it)
	{
		CDrawOverWindow*  pWindow = it->second;
		ASSERT(pWindow);
		::ShowWindow(pWindow->m_hWnd, SW_HIDE);
	}

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

	for (auto itr = g_instanceIdToWindow.begin(); itr != g_instanceIdToWindow.end(); ++itr)
	{
		CDrawOverWindow*  pWindow = itr->second;

		if (pWindow)
		{
			delete pWindow;
		}

	}

	g_instanceIdToWindow.clear();

	g_instanceIdToLoadedFlags.clear();

}