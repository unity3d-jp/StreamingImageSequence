#ifndef __TGALOADER_H__
#define __TGALOADER_H__


struct StTGA_HEADER
{
	u8  _uIDFiledLength; //IDフィールド長
	u8  _uColorMapType;    //カラーマップ有無（0=なし、1=あり）
	u8  _uImageType;	//画像形式  0,1,2,3,9,10,11
	u8 _uColorMapIndex[2];
	u8 _uColorMapLength[2];
	u8  _uColorMapEntrySize;

	u16 _uPosX;
	u16 _uPosY;
	u16 _uWidth;
	u16 _uHeight;
	u8  _uBitPerPixel;

	u8  _uDescripter;   // bit0-3 属性 4 格納方法 5 格納方法 6,7(インターリーブ使用不可)
};

struct StTGA_COLOR
{
	u8  rgbBlue;
	u8  rgbGreen;
	u8  rgbRed;
	u8  rgbAlpha;
};

extern "C"
{
	void* loadTGAFileAndAlloc(const charType* pName, StReadResult* pReadResult);
}
#endif //__TGALOADER_H__