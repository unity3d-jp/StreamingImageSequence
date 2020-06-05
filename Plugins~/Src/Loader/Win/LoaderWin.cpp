#include "stdafx.h"

//CommonLib
#include "CommonLib/Types.h"

//Loader
#include "Loader/TGALoader.h"
#include "Loader/ImageCatalog.h"
#include "Loader/ImageData.h"
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
	u8* pBuffer = NULL;

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

		const u32 width = pBitmap->GetWidth();
		const u32 height = pBitmap->GetHeight();
		const uint32_t dataSize = sizeof(u32) * width * height;

		pBuffer = (u8*)malloc(dataSize);
		ASSERT(pBuffer !=nullptr);
		u32* pImage = (u32*)pBuffer;

		ImageData imageData(pBuffer, dataSize, width, height, StreamingImageSequencePlugin::READ_STATUS_SUCCESS);

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

		imageCatalog->SetImage(imagePath, imageType, &imageData);

	}
	else
	{
		ImageData imageData(nullptr, 0,0,0,READ_STATUS_FAIL);
		imageCatalog->SetImage(imagePath, imageType, &imageData);
		ASSERT(0);
	}

}

//----------------------------------------------------------------------------------------------------------------------
void LoadTGAFileAndAlloc(const strType& imagePath, const uint32_t imageType, ImageCatalog* imageCatalog) {

	loadTGAFileAndAlloc(imagePath, imageType, imageCatalog);
}

} // end namespace
