// DrawOverWindow.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include "../CommonLib/CommonLib.h"
//#include "../Loader/Loader.h"
#include "DrawOverWindow.h"

#pragma comment( lib, "winmm.lib" )
#pragma comment(lib, "gdiplus.lib")

using namespace Gdiplus;

std::map<int, CDrawOverWindow*>				 g_instanceIdToWindow;
std::map<int, int>                           g_instanceIdToLoadedFlags;
void InitLayeredWindow(HWND hWnd)
{
	/* The call to UpdateLayeredWindow() is what makes a non-rectangular
	* window possible. To enable per pixel alpha blending we pass in the
	* argument ULW_ALPHA, and provide a BLENDFUNCTION structure filled in
	* to do per pixel alpha blending.
	*/
	/*
	HDC hdc = NULL;

	if (hdc = GetDC(hWnd))
	{
		HGDIOBJ hPrevObj = NULL;
		POINT ptDest = { 0, 0 };
		POINT ptSrc = { 0, 0 };
		SIZE client = { g_image.width, g_image.height };
		BLENDFUNCTION blendFunc = { AC_SRC_OVER, 0, 255, AC_SRC_ALPHA };

		hPrevObj = SelectObject(g_image.hdc, g_image.hBitmap);
		ClientToScreen(hWnd, &ptDest);

		UpdateLayeredWindow(hWnd, hdc, &ptDest, &client,
			g_image.hdc, &ptSrc, 0, &blendFunc, ULW_ALPHA);

		SelectObject(g_image.hdc, hPrevObj);
		ReleaseDC(hWnd, hdc);
	}*/
}

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
	m_size(0),
	m_pColorArray(nullptr),
	m_pByteArray(nullptr),
	m_sLastPosX(posX),
	m_sLastPosY(posY),
	m_sLastWidth(width),
	m_sLastHeight(height),
	m_bIsModified(true)
{
	HBRUSH brush = CreateSolidBrush(RGB(0, 0, 255));
	SetClassLongPtr(m_hWnd, GCLP_HBRBACKGROUND, (LONG)brush);
}


CDrawOverWindow::~CDrawOverWindow()
{
	::DestroyWindow(m_hWnd);
	if (m_pColorArray)
	{
		delete[] m_pColorArray;
	}
	if (m_pByteArray)
	{
		delete[] m_pByteArray;
	}
}

bool CDrawOverWindow::IsModified(int posX, int posY, int width, int height)
{
	if ( m_sLastPosX != posX
		|| m_sLastPosY != posY
		|| m_sLastWidth != width
		|| m_sLastHeight != height
		|| m_bIsModified)
	{
		return true;
	}
	return false;
}
void CDrawOverWindow::SetNewCondition(int posX, int posY, int width, int height)
{
	m_sLastPosX = posX;
	m_sLastPosY = posY;
	m_sLastWidth = width;
	m_sLastHeight = height;
	m_bIsModified = false;
}


// This is an example of an exported variable
DRAWOVERWINDOW_API int nDrawOverWindow=0;

// This is an example of an exported function.
DRAWOVERWINDOW_API int fnDrawOverWindow(void)
{
	return 42;
}





DRAWOVERWINDOW_API void  ShowOverwrapWindow(int sInstanceId, int posX, int posY, int width, int height, int forceDraw)
{
	CDrawOverWindow*  pWindow = NULL;
	if (g_instanceIdToWindow.find(sInstanceId) == g_instanceIdToWindow.end())
	{
		pWindow = new CDrawOverWindow(posX, posY, width, height);
		g_instanceIdToWindow[sInstanceId] = pWindow;

	}
	pWindow = g_instanceIdToWindow[sInstanceId];
	ASSERT(pWindow);

	if (!pWindow->IsModified(posX, posY, width, height))
	{
		return;
	}
	pWindow->SetNewCondition(posX, posY, width, height);
	::SetWindowPos(pWindow->m_hWnd, HWND_TOPMOST, posX, posY, width, height, SWP_NOACTIVATE);


	if (!::IsWindowVisible(pWindow->m_hWnd))
	{
		::ShowWindow(pWindow->m_hWnd, SW_SHOW);
	}


	Gdiplus::GdiplusStartupInput    startInput;
	ULONG_PTR                       token;
	if (Gdiplus::GdiplusStartup(&token, &startInput, NULL) != Gdiplus::Ok)
	{

		return;
	}


	u32* pByte = pWindow->m_pColorArray;
	int length = pWindow->m_size;



	Gdiplus::Bitmap*    pBitmap = new Gdiplus::Bitmap(length, 1, length * 4, PixelFormat32bppARGB, (u8*)pByte);
	{
		Gdiplus::Graphics graphics(GetDC(pWindow->m_hWnd));


		graphics.DrawImage(pBitmap, 0,0, width, height );
	}

	delete pBitmap;

	Gdiplus::GdiplusShutdown(token);

	::UpdateWindow(pWindow->m_hWnd);
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

DRAWOVERWINDOW_API void  SetOverwrapWindowData(int sInstanceId, u32* byteArray, int size)
{
	if (g_instanceIdToWindow.find(sInstanceId) == g_instanceIdToWindow.end())
	{
		return;
	}
	CDrawOverWindow*  pWindow = g_instanceIdToWindow[sInstanceId];
	ASSERT(pWindow);

	pWindow->m_size = size;
	if (pWindow->m_pColorArray == nullptr)
	{
		pWindow->m_pColorArray = new u32[pWindow->m_size];
	}
	if (pWindow->m_pByteArray == nullptr)
	{ 
		pWindow->m_pByteArray = new u32[pWindow->m_size];
		for (int ii = 0; ii < pWindow->m_size;ii++)
		{
			pWindow->m_pByteArray[ii] = 0xCCCCCCCC;
		}
	}
	u32 uColorWhite = 0xffffffff;
	u32 uColorBlack = 0xff000000;
	for (int ii = 0; ii < pWindow->m_size; ii++)
	{
		pWindow->m_pColorArray[ii] = byteArray[ii] ? uColorWhite : uColorBlack;
		if (pWindow->m_pByteArray[ii] != byteArray[ii])
		{
			pWindow->m_pByteArray[ii] = byteArray[ii];
			pWindow->m_bIsModified = true;
		}
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