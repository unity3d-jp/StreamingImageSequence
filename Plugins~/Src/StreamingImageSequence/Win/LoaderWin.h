#include "stdafx.h"

#include <d3d11.h> //Must be included before gdiplus
#include <gdiplus.h>

namespace StreamingImageSequencePlugin {

class LoaderWin {
	Gdiplus::GdiplusStartupInput    startInput;
	ULONG_PTR                       token;

public:
	LoaderWin();
	~LoaderWin();
};

} //end namespace
