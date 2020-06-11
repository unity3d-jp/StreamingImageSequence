
#include "stdafx.h"
#include "../CommonLib/CommonLib.h"

//Loader
#include "TGALoader.h"
#include "Loader/ImageCatalog.h"
#include "Loader/LoaderConstants.h"


namespace StreamingImageSequencePlugin {

extern "C"
{
	static void GetColorPalette(u8* pPaletteData_, u8 uColorMapEntrySize_, u32 uIndex_, StTGA_COLOR* pPalette)
	{
		switch (uColorMapEntrySize_)
		{
		case 15:
		case 16:
		{
			u16 *pixel = (u16*)(pPaletteData_ + 2 * uIndex_);
			//
			// http://msdn.microsoft.com/ja-jp/library/cc371597.aspx
			// RGB565
			//u8 rgbRed = (u8)((s32)((*pixel & 0xf800) >> 11) * 255 / 31);
			// RGB555
			pPalette->rgbRed = (u8)((s32)((*pixel & 0x7c00) >> 10) * 255 / 31);
			pPalette->rgbGreen = (u8)((s32)((*pixel & 0x03e0) >> 5) * 255 / 31);
			pPalette->rgbBlue = (u8)((s32)((*pixel & 0x001f) >> 0) * 255 / 31);
			pPalette->rgbAlpha = 0xff;
		}
		break;
		case 24:
		{
			u8* p = pPaletteData_ + 3 * uIndex_;
			pPalette->rgbBlue = *p++;
			pPalette->rgbGreen = *p++;
			pPalette->rgbRed = *p++;
			pPalette->rgbAlpha = 0xff;
		}
		break;
		case 32:
		{

			u8* p = pPaletteData_ + 4 * uIndex_;
			pPalette->rgbBlue = *p++;
			pPalette->rgbGreen = *p++;
			pPalette->rgbRed = *p++;
			pPalette->rgbAlpha = *p++;
		}
		break;
		}
	}

//----------------------------------------------------------------------------------------------------------------------
	void loadTGAFileAndAlloc(const strType& imagePath, const uint32_t imageType,
		StreamingImageSequencePlugin::ImageCatalog* imageCatalog)
	{
		HANDLE hh =
#if USE_WCHAR
			CreateFileW(pName,
				GENERIC_READ,
				FILE_SHARE_READ,
				NULL,
				OPEN_EXISTING,
				FILE_ATTRIBUTE_NORMAL,
				NULL);
#else
			CreateFileA(imagePath.c_str(),
				GENERIC_READ,
				FILE_SHARE_READ,
				NULL,
				OPEN_EXISTING,
				FILE_ATTRIBUTE_NORMAL,
				NULL);
#endif
		s32 sSizByteFile = GetFileSize(hh, NULL);

		//[TODO-sin: 2020-6-5] Should probably move this allocation to imageCatalog
		u8* pBuffer = (u8*)malloc(sSizByteFile);
		ASSERT(pBuffer != nullptr);

		s32 sReadDone;
		BOOL bRead = ::ReadFile(hh, pBuffer, sSizByteFile, (LPDWORD)&sReadDone, NULL);
		CloseHandle(hh);

		StTGA_HEADER* ptHeader = (StTGA_HEADER*)pBuffer;
		const u32  uRGBAByte = LoaderConstants::NUM_BYTES_PER_TEXEL;
		bool bIsRLE = false;
		u32  uImageType = ptHeader->_uImageType;
		if (uImageType >= 8) {
			uImageType -= 8;
			bIsRLE = true;
		}

		const u32 uBitPerPixel = ptHeader->_uBitPerPixel;

		const ImageData* imageData = imageCatalog->AllocateImage(imagePath, imageType, ptHeader->_uWidth, ptHeader->_uHeight);
		if (nullptr == imageData) {
			free(pBuffer);
			imageCatalog->SetImageStatus(imagePath, imageType,READ_STATUS_OUT_OF_MEMORY);
			return;
		}

		s32 sSizeByteTgaHeader = sizeof(StTGA_HEADER);
		s32 sColorMapEntrySizeByte = (ptHeader->_uColorMapEntrySize + 1) / 8;
		u8* pSrc = &pBuffer[sSizeByteTgaHeader + ptHeader->_uIDFiledLength + (ptHeader->_uColorMapLength[1] * 256 + ptHeader->_uColorMapLength[0]) * sColorMapEntrySizeByte];

		s32  sRLE_Count = 0;
		s32  sREL_Repeating = 0;
		bool bReadNextPixel = 1;
		u8   uRawData[uRGBAByte];

		u8*  pDst       = imageData->RawData;
		u8*  pImageData = imageData->RawData;
		s32  yy = -1;

		// On Unity Windows, Texture coordinate is upside down.
		ptHeader->_uDescripter ^= (BIT(5));

		for (u32 uPixel = 0; uPixel < (u32)(ptHeader->_uHeight * ptHeader->_uWidth); uPixel++)
		{
			if ((uPixel % ptHeader->_uWidth) == 0)
			{
				yy++;
				// upper left
				if (0 != (ptHeader->_uDescripter & BIT(5)) && 0 == (ptHeader->_uDescripter & BIT(4)))
				{
					pDst = &pImageData[yy * ptHeader->_uWidth * uRGBAByte];
				}
				// lower left( most common)
				else if (0 == (ptHeader->_uDescripter & BIT(5)) && 0 == (ptHeader->_uDescripter & BIT(4)))
				{
					pDst = &pImageData[(ptHeader->_uHeight - yy - 1) * ptHeader->_uWidth * uRGBAByte];
				}
				// upper right
				else if (0 != (ptHeader->_uDescripter & BIT(5)) && 0 != (ptHeader->_uDescripter & BIT(4)))
				{
					pDst = &pImageData[(yy + 1) * ptHeader->_uWidth * uRGBAByte - uRGBAByte];
				}
				// lower right
				else
				{
					pDst = &pImageData[(ptHeader->_uHeight - yy) * ptHeader->_uWidth * uRGBAByte - uRGBAByte];
				}
			}

			if (bIsRLE)
			{
				if (sRLE_Count == 0)
				{
					//	
					s32 sRLE_cmd = *pSrc++;
					sRLE_Count = 1 + (sRLE_cmd & 127);
					sREL_Repeating = sRLE_cmd >> 7;
					bReadNextPixel = true;
				}
				else if (!sREL_Repeating)
				{
					bReadNextPixel = true;
				}
			}
			else
			{
				bReadNextPixel = true;
			}
			if (bReadNextPixel)
			{

				for (u32 jj = 0; jj < uBitPerPixel / 8; ++jj)
				{
					uRawData[jj] = *pSrc++;
				}
			}
			switch (uBitPerPixel)
			{
			case 8:
			{
				if (1 == uImageType)  // Index Color
				{
					StTGA_COLOR stPalette;
					GetColorPalette(pBuffer + sSizeByteTgaHeader + ptHeader->_uIDFiledLength, ptHeader->_uColorMapEntrySize, uRawData[0], &stPalette);
					*(pDst + 0) = stPalette.rgbBlue;
					*(pDst + 1) = stPalette.rgbGreen;
					*(pDst + 2) = stPalette.rgbRed;
					*(pDst + 3) = stPalette.rgbAlpha;
				}
				else if (3 == uImageType) // Black/White
				{
					*(pDst + 0) = uRawData[0];
					*(pDst + 1) = uRawData[0];
					*(pDst + 2) = uRawData[0];
					*(pDst + 3) = 0xFF;
				}
				else
				{
					ASSERT(0);  // not allowed
				}
			}
			break;
			case 16:
			{
				u16 *pixel = (u16*)&uRawData[0];
				//
				// http://msdn.microsoft.com/ja-jp/library/cc371597.aspx
				// RGB565
				//u8 rgbRed = (u8)((s32)((*pixel & 0xf800) >> 11) * 255 / 31);
				// RGB555
				u8 rgbRed = (u8)((s32)((*pixel & 0x7c00) >> 10) * 255 / 31);
				u8 rgbGreen = (u8)((s32)((*pixel & 0x03e0) >> 5) * 255 / 31);
				u8 rgbBlue = (u8)((s32)((*pixel & 0x001f) >> 0) * 255 / 31);
				*(pDst + 0) = rgbBlue;
				*(pDst + 1) = rgbGreen;
				*(pDst + 2) = rgbRed;
				*(pDst + 3) = 0xFF;
			}
			break;
			case 24:
			{
				*(pDst + 0) = uRawData[0];
				*(pDst + 1) = uRawData[1];
				*(pDst + 2) = uRawData[2];
				*(pDst + 3) = 0xFF;
			}
			break;
			case 32:
			{
				*(pDst + 0) = uRawData[0];
				*(pDst + 1) = uRawData[1];
				*(pDst + 2) = uRawData[2];
				*(pDst + 3) = uRawData[3];
			}
			break;
			default:
				ASSERT(0);
				break;
			}
			bReadNextPixel = false;

			--sRLE_Count;
			if (0 == (ptHeader->_uDescripter & BIT(4)))
			{
				pDst += uRGBAByte;
			}
			else
			{
				pDst -= uRGBAByte;
			}
		}

		free(pBuffer);

		imageCatalog->SetImageStatus(imagePath, imageType, READ_STATUS_SUCCESS);
	}
};


} //end namespace
