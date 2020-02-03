// CommonLibWin.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include "CommonLib.h"

#include "CriticalSectionObject.h"

using namespace std;

COMMONLIBWIN_API    map<strType, StReadResult>  g_fileNameToPtrMap;
COMMONLIBWIN_API    map<int, strType>           g_instanceIdToFileName;
COMMONLIBWIN_API    map<int, void*>             g_instanceIdToUnityTexturePointer;
COMMONLIBWIN_API    map<strType, int>           g_scenePathToSceneStatus;

//----------------------------------------------------------------------------------------------------------------------

