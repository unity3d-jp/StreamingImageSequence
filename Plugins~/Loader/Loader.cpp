// Loader.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include "Loader.h"

//CommonLib
#include "CommonLib/CommonLib.h"
#include "CommonLib/CriticalSectionController.h"


//Loader
#include "TGALoader.h"
#include "FileType.h"
#include "LoaderUtility.h"

#pragma comment( lib, "winmm.lib" )
#pragma comment(lib, "gdiplus.lib")

using namespace std;

//Forward declarations
void* loadPNGFileAndAlloc(const charType* fileName, StReadResult* pResult);
void* loadPNGFileAndAlloc(const charType* fileName, StReadResult* pResult, u32 requiredWidth, u32 requiredHeight);

//----------------------------------------------------------------------------------------------------------------------

int g_LoadingFileCounter = 0;
int g_IsResetting;


//----------------------------------------------------------------------------------------------------------------------

#define INC_LOADINGCOUNTER() {\
	CriticalSectionController cs2(LOADINGCOUNTER_CS);\
	g_LoadingFileCounter++;\
}
#define DEC_LOADINGCOUNTER() {\
	CriticalSectionController cs2(LOADINGCOUNTER_CS);\
	g_LoadingFileCounter--;\
}

//----------------------------------------------------------------------------------------------------------------------

LOADERWIN_API bool GetNativeTextureInfo(const charType* fileName, StReadResult* readResult, const uint32_t textureType) {
    using namespace StreamingImageSequencePlugin;
    return LoaderUtility::GetTextureInfo(fileName, readResult, &g_fileNameToPtrMap[textureType], textureType );
}

//----------------------------------------------------------------------------------------------------------------------

//Returns if the fileName can be loaded ()
LOADERWIN_API bool LoadAndAlloc(const charType* fileName, int textureType) {
    using namespace StreamingImageSequencePlugin;
    StReadResult readResult;
    
    bool isProcessed = LoaderUtility::GetTextureInfo(fileName, &readResult, &g_fileNameToPtrMap[textureType], textureType);
    if (isProcessed) {
        return true;
    }
    
    const FileType fileType = LoaderUtility::CheckFileType(fileName);
    if (FILE_TYPE_INVALID ==fileType)
        return false;
    
    //Loading
    strType wstr(fileName);
    {
        CriticalSectionController cs(TEXTURE_CS(textureType));
        readResult.readStatus = READ_STATUS_LOADING;
        g_fileNameToPtrMap[textureType][wstr] = readResult;
    }
    
    void *ptr = nullptr;
    INC_LOADINGCOUNTER();
    switch (fileType) {
        case FILE_TYPE_TGA: {
            ptr = loadTGAFileAndAlloc(fileName, &readResult);
            break;
        }
        case FILE_TYPE_PNG: {
            ptr = loadPNGFileAndAlloc(fileName, &readResult);
            break;
        }
        default: break;
    }
    DEC_LOADINGCOUNTER();

    if(ptr == NULL) {
        return false;
    }
    
    {
		CriticalSectionController cs(TEXTURE_CS(textureType));
		readResult.readStatus = READ_STATUS_SUCCESS;
        g_fileNameToPtrMap[textureType][wstr] = readResult;
    }

    return true;
}


//----------------------------------------------------------------------------------------------------------------------

LOADERWIN_API void   NativeFree(void* ptr) {
	free(ptr);
}


//----------------------------------------------------------------------------------------------------------------------
// return succ:0 fail:-1
LOADERWIN_API int   ResetNativeTexture(const charType* fileName) {
    using namespace StreamingImageSequencePlugin;

	//Reset all textures
	for (uint32_t texType = 0; texType < MAX_CRITICAL_SECTION_TYPE_TEXTURES; ++texType) {
		StReadResult readResult;
		GetNativeTextureInfo(fileName, &readResult, texType);

		//Check
		if (!readResult.buffer || readResult.readStatus != READ_STATUS_SUCCESS) {
			continue;
		}

		CriticalSectionController cs0(TEXTURE_CS(texType));
		{
			strType wstr(fileName);
			NativeFree(readResult.buffer);
			g_fileNameToPtrMap[texType].erase(wstr);
		}
	}


	return 0;

}

//----------------------------------------------------------------------------------------------------------------------
LOADERWIN_API void ListLoadedTextures(const uint32_t textureType, void(*OnNextTexture)(const char*)) {
	using namespace StreamingImageSequencePlugin;
	ASSERT(textureType < MAX_CRITICAL_SECTION_TYPE_TEXTURES);

	for (auto itr = g_fileNameToPtrMap[textureType].begin(); itr != g_fileNameToPtrMap[textureType].end(); ++itr) {
		OnNextTexture(itr->first.c_str());
	}

}

//----------------------------------------------------------------------------------------------------------------------

LOADERWIN_API void   SetSceneStatus(const charType* scenePath, int sceneStatus)
{
	g_scenePathToSceneStatus[scenePath] = sceneStatus;

}
LOADERWIN_API int    GetSceneStatus(const charType* scenePath)
{
	strType wstr(scenePath);
	if (g_scenePathToSceneStatus.find(wstr) != g_scenePathToSceneStatus.end())
	{
		return g_scenePathToSceneStatus[wstr];
	}
	return -1;	// not found;
}

LOADERWIN_API void  ResetPlugin()
{
	StreamingImageSequencePlugin::CriticalSectionController cs2(RESETTING_CS);
	g_IsResetting = 1;
}

LOADERWIN_API void  DoneResetPlugin()
{
	StreamingImageSequencePlugin::CriticalSectionController cs2(RESETTING_CS);
	g_IsResetting = 0;
}

LOADERWIN_API int   IsPluginResetting()
{
	StreamingImageSequencePlugin::CriticalSectionController cs2(RESETTING_CS);
	return g_IsResetting ;
}
