// LoaderWin.cpp : Win implementation of Loader APIs
//

#include "stdafx.h"

#include "CommonLib/Types.h"
#include "CommonLib/ReadResult.h"

//External
#include "External/stb/stb_image_resize.h"

#pragma comment( lib, "winmm.lib" )
#pragma comment(lib, "gdiplus.lib")


//----------------------------------------------------------------------------------------------------------------------

void LoadPNGFileAndAlloc(const charType* fileName, StReadResult* pResult) {
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


	ASSERT(pResult->buffer);
}

//----------------------------------------------------------------------------------------------------------------------

void LoadPNGFileAndAllocWithSize(const charType* fileName, StReadResult* readResult, 
	const uint32_t width, const uint32_t height) 
{
	LoadPNGFileAndAlloc(fileName, readResult);
	ASSERT(readResult.buffer);

	//Already has the required size 
	if (readResult->width == width && readResult->height == height)
		return;

	{
		const uint64_t NUM_CHANNELS = 4;
		u8* resizedBuffer = (u8*)malloc(static_cast<uint32_t>(NUM_CHANNELS * width * height));

		stbir_resize_uint8(readResult->buffer, readResult->width, readResult->height, 0,
			resizedBuffer, width, height, 0, NUM_CHANNELS);
		free(readResult->buffer);
		readResult->buffer = resizedBuffer;
		readResult->width = width;
		readResult->height = height;
	}
}


