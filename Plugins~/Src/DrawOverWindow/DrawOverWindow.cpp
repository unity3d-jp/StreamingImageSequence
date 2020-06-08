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

