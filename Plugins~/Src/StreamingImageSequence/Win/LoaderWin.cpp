#include "stdafx.h"

//External
//#include "External/stb/stb_image.h"


//CommonLib
#include "CommonLib/Types.h"

//SIS
#include "TGALoader.h"
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

	//    unsigned char *data = stbi_load(filename, &x, &y, &n, 0);
	//    // ... process data if not NULL ...
	//    // ... x = width, y = height, n = # 8-bit components per pixel ...
	//    // ... replace '0' with '1'..'4' to force that many components per pixel
	//    // ... but 'n' will always be the number that it would have been if you said 0
	//    stbi_image_free(data)
	//
	//
	//const uint32_t FORCED_NUM_COMPONENTS = 4;
	//int width, height, numComponents;
	//unsigned char *data = stbi_load(imagePath.c_str(), &width, &height, &numComponents, FORCED_NUM_COMPONENTS);




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
	if (pBitmap ) {
		status = pBitmap->GetLastStatus();
	}
	ASSERT(status == Gdiplus::Ok);
	if (status == Gdiplus::Ok)	{
		const u32 width = pBitmap->GetWidth();
		const u32 height = pBitmap->GetHeight();
		const uint32_t dataSize = sizeof(u32) * width * height;

		const ImageData* imageData = imageCatalog->AllocateImage(imagePath, imageType, width, height);
		if (nullptr == imageData) {
			imageCatalog->SetImageStatus(imagePath, imageType,READ_STATUS_OUT_OF_MEMORY);
			return;
		}

		u8* pBuffer = imageData->RawData;
		ASSERT(pBuffer !=nullptr);
		u32* pImage = (u32*)pBuffer;

		Gdiplus::BitmapData bitmapData;
		pBitmap->LockBits(&Gdiplus::Rect(0, 0, width, height), Gdiplus::ImageLockModeWrite, PixelFormat32bppARGB, &bitmapData);
		u32 *pRawBitmapOrig = (u32*)bitmapData.Scan0;   // for easy access and indexing
		for (u32 yy = 0; yy < height; yy++) {
			memcpy(&pImage[yy*width], &pRawBitmapOrig[(height - 1 - yy) * bitmapData.Stride / 4], width*sizeof(u32));

		}

		//memcpy(pImage, bitmapData.Scan0, width*height*sizeof(UINT32));
		pBitmap->UnlockBits(&bitmapData);
		delete pBitmap;

		imageCatalog->SetImageStatus(imagePath, imageType,READ_STATUS_SUCCESS);

	} else {
		imageCatalog->SetImageStatus(imagePath, imageType,READ_STATUS_FAIL);
	}

}

//----------------------------------------------------------------------------------------------------------------------
void LoadTGAFileAndAlloc(const strType& imagePath, const uint32_t imageType, ImageCatalog* imageCatalog) {

	loadTGAFileAndAlloc(imagePath, imageType, imageCatalog);
}

} // end namespace
