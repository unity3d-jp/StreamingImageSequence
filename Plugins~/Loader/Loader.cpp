// LoaderWin.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include "../CommonLib/CommonLib.h"
#include "Loader.h"
#include "TGALoader.h"

#pragma comment( lib, "winmm.lib" )
#pragma comment(lib, "gdiplus.lib")

using namespace std;

void* loadPNGFileAndAlloc(const charType* fileName, StReadResult* pResult);

// This is the constructor of a class that has been exported.
// see LoaderWin.h for the class definition
CLoaderWin::CLoaderWin()
{
	return;
}

#define INC_LOADINGCOUNTER() {\
	CCriticalSectionController cs2(LOADINGCOUNTER_CS);\
	g_LoadingFileCounter++;\
}
#define DEC_LOADINGCOUNTER() {\
	CCriticalSectionController cs2(LOADINGCOUNTER_CS);\
	g_LoadingFileCounter--;\
}


LOADERWIN_API void*  LoadAndAlloc(const charType* fileName)
{

	StReadResult tResult;
	strType wstr(fileName);
	{
		CCriticalSectionController cs(FILENAME2PTR_CS);
		{
			if (g_fileNameToPtrMap.find(wstr) != g_fileNameToPtrMap.end())
			{
				return g_fileNameToPtrMap[wstr].buffer;
			}
		}

	}

	size_t length =
#if USE_WCHAR
		wcslen(fileName);
#else
		strlen(fileName);
#endif

	if (length < 4)
	{
		return NULL;
	}
	if (fileName[length - 4] != '.')
	{
		return NULL;
	}


	{
		CCriticalSectionController cs(FILENAME2PTR_CS);
		{
			tResult = g_fileNameToPtrMap[wstr];
			tResult.readStatus = 1; // loading
			g_fileNameToPtrMap[wstr] = tResult;
		}
	}
	void *ptr = nullptr;

	INC_LOADINGCOUNTER();
#ifdef _WIN32	// not neccesary for now
	if ((fileName[length - 3] == 'T' || fileName[length - 3] == 't')
		&& (fileName[length - 2] == 'G' || fileName[length - 2] == 'g')
		&& (fileName[length - 1] == 'A' || fileName[length - 1] == 'a'))
	{
		ptr = loadTGAFileAndAlloc(fileName, &tResult);
	}
	else
#endif
	{
		ptr = loadPNGFileAndAlloc(fileName, &tResult);
        if(ptr == NULL) {
            return NULL;
        }
	}
	DEC_LOADINGCOUNTER();
	{
		CCriticalSectionController cs(FILENAME2PTR_CS);
		{
			tResult.readStatus = 2; // loaded
			g_fileNameToPtrMap[wstr] = tResult;
		}
	}

	return ptr;
}

LOADERWIN_API void   NativeFree(void* ptr)
{
	free(ptr);
}

LOADERWIN_API void*  GetNativTextureInfo(const charType* fileName, StReadResult* pResult)
{
	{
		CCriticalSectionController cs(FILENAME2PTR_CS);
		{
			ASSERT(pResult);
			pResult->readStatus = 0;
			strType wstr(fileName);

			if (g_fileNameToPtrMap.find(wstr) != g_fileNameToPtrMap.end())
			{
				*pResult = g_fileNameToPtrMap[wstr];
				if (pResult->readStatus == 2)
				{
					ASSERT(pResult->buffer);
				} 
				return pResult->buffer;
			}
		}
	}
	return NULL;
}

// return succ:0 fail:-1
LOADERWIN_API int   ResetNativeTexture(const charType* fileName)
{
	StReadResult tReadResult;
	void* ptr = GetNativTextureInfo(fileName, &tReadResult);
	if (!ptr)
	{
		return -1;
	}

	if (tReadResult.readStatus != 2)
	{
		return -1;
	}


	{
		CCriticalSectionController cs(FILENAME2PTR_CS);
		{
			strType wstr(fileName);
			if (g_fileNameToPtrMap.find(wstr) != g_fileNameToPtrMap.end())
			{
				NativeFree(tReadResult.buffer);
				g_fileNameToPtrMap.erase(wstr);
			}
		}
	}

	return 0;

}
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
	CCriticalSectionController cs2(RESETTING_CS);
	g_IsResetting = 1;
}

LOADERWIN_API void  DoneResetPlugin()
{
	CCriticalSectionController cs2(RESETTING_CS);
	g_IsResetting = 0;
}

LOADERWIN_API int   IsPluginResetting()
{
	CCriticalSectionController cs2(RESETTING_CS);
	return g_IsResetting ;
}
#ifdef _WIN32
void* loadPNGFileAndAlloc(const charType* fileName, StReadResult* pResult)
{
	u8* pBuffer = NULL;
    


# if USE_WCHAR
	Gdiplus::Bitmap*    pBitmap = Gdiplus::Bitmap::FromFile(fileName);
# else
	const int size = 8192;
	WCHAR wlocal[size] = { 0x00 };


	int ret = MultiByteToWideChar(
		CP_ACP,
		MB_PRECOMPOSED,
		fileName,
		strlen(fileName),
		wlocal,
		size);
	Gdiplus::Bitmap*    pBitmap = Gdiplus::Bitmap::FromFile(wlocal);

# endif
	Gdiplus::Status status = Gdiplus::FileNotFound;
	if (pBitmap )
	{
		status = pBitmap->GetLastStatus();
	}
	ASSERT(status == Gdiplus::Ok);
	if (status == Gdiplus::Ok)
	{

		u32 width = pBitmap->GetWidth();
		u32 height = pBitmap->GetHeight();

		pBuffer = (u8*)malloc(sizeof(u32)* width * height);
		ASSERT(pBuffer !=nullptr);
		u32* pImage = (u32*)pBuffer;

		pResult->width = width;
		pResult->height = height;
		pResult->buffer = pBuffer;

		Gdiplus::BitmapData bitmapData;
		pBitmap->LockBits(&Gdiplus::Rect(0, 0, width, height), Gdiplus::ImageLockModeWrite, PixelFormat32bppARGB, &bitmapData);
		u32 *pRawBitmapOrig = (u32*)bitmapData.Scan0;   // for easy access and indexing
		for (u32 yy = 0; yy < height; yy++)
		{
			memcpy(&pImage[yy*width], &pRawBitmapOrig[(height - 1 - yy) * bitmapData.Stride / 4], width*sizeof(u32));

		}

		//memcpy(pImage, bitmapData.Scan0, width*height*sizeof(UINT32));
		pBitmap->UnlockBits(&bitmapData);
		delete pBitmap;
	}
	else
	{
		ASSERT(0);
	}


	ASSERT(pBuffer);
	return pBuffer; //  pBuffer;
}
#endif

