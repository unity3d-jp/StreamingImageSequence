#include "stdafx.h"

//CommonLib
#include "CommonLib/Types.h"

//SIS
#include "StreamingImageSequence/ImageCatalog.h"
#include "StreamingImageSequence/ImageData.h"
#include "LoaderWin.h"

#pragma comment( lib, "winmm.lib" )
#pragma comment(lib, "gdiplus.lib")

//----------------------------------------------------------------------------------------------------------------------

namespace StreamingImageSequencePlugin {
	LoaderWin::LoaderWin() {
		int status = Gdiplus::GdiplusStartup(&token, &startInput, NULL);
	}

	LoaderWin::~LoaderWin() {
		Gdiplus::GdiplusShutdown(token);
	}



//This is a dummy variable to init and shutdown GDI on Windows
StreamingImageSequencePlugin::LoaderWin		g_loaderWin;

//----------------------------------------------------------------------------------------------------------------------

void LoadPNGFileAndAlloc(const strType& imagePath, const uint32_t imageType, 
	StreamingImageSequencePlugin::ImageCatalog* imageCatalog) 
{

# if USE_WCHAR
	Gdiplus::Bitmap*    pBitmap = Gdiplus::Bitmap::FromFile(fileName);
# else
	const int size = 8192;
	WCHAR wlocal[size] = { 0x00 };


	int ret = MultiByteToWideChar(
		CP_ACP,
		MB_PRECOMPOSED,
		imagePath.c_str(),
		static_cast<int>(imagePath.length()),
		wlocal,
		size);
	Gdiplus::Bitmap* bitmap = Gdiplus::Bitmap::FromFile(wlocal);

# endif
	Gdiplus::Status status = Gdiplus::FileNotFound;
	if (bitmap ) {
		status = bitmap->GetLastStatus();
	}

	if (Gdiplus::Ok!=status)	{
		imageCatalog->SetImageStatus(imagePath, imageType,READ_STATUS_FAIL);
		return;
	}

	const u32 width = bitmap->GetWidth();
	const u32 height = bitmap->GetHeight();

	const ImageData* imageData = imageCatalog->AllocateImage(imagePath, imageType, width, height);
	if (nullptr == imageData) {
		imageCatalog->SetImageStatus(imagePath, imageType,READ_STATUS_OUT_OF_MEMORY);
		delete(bitmap);
		return;
	}

	Gdiplus::BitmapData bitmapData;
	bitmapData.Width = width;
	bitmapData.Height = height;
    bitmapData.Stride = 4 * bitmapData.Width;
	bitmapData.PixelFormat = PixelFormat32bppARGB;
	bitmapData.Scan0 = static_cast<VOID*>(imageData->RawData);
	bitmapData.Reserved = NULL;
	bitmap->LockBits(&Gdiplus::Rect(0, 0, width, height), 
					 Gdiplus::ImageLockModeRead | Gdiplus::ImageLockModeUserInputBuf, 
					 PixelFormat32bppARGB,&bitmapData);

	const u32 heightMinusOneMulWidth = (height - 1) * width;
	const u32 memSizePerRow = width * sizeof(u32);
	u32* rawData = reinterpret_cast<u32*>(imageData->RawData);

	//invert by swapping
	const u32 halfHeight = static_cast<u32>(height * 0.5f);
	//[TODO-sin: 2020-10-16] Make this temporary buffer shared per CS_TYPE. Also use ImageAllocator
	u8* tempBuffer = new u8[memSizePerRow];
	for (u32 yy = 0; yy < halfHeight; ++yy) {
		const u32 startIndex = yy * width;
		const u32 endIndex = heightMinusOneMulWidth - startIndex; //From: (height - 1 - yy) * width;
		memcpy(tempBuffer, &rawData[startIndex], memSizePerRow);
		memcpy(&rawData[startIndex], &rawData[endIndex], memSizePerRow);
		memcpy(&rawData[endIndex], tempBuffer, memSizePerRow);
	}
	free(tempBuffer);

	//memcpy(pImage, bitmapData.Scan0, width*height*sizeof(UINT32));
	bitmap->UnlockBits(&bitmapData);
	delete bitmap;

	imageCatalog->SetImageStatus(imagePath, imageType,READ_STATUS_SUCCESS);
	imageCatalog->SetImageFormat(imagePath, imageType,IMAGE_FORMAT_BGRA32);

}

} // end namespace
