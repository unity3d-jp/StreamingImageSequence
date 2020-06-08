// The following ifdef block is the standard way of creating macros which make exporting 
// from a DLL simpler. All files within this DLL are compiled with the DRAWOVERWINDOW_EXPORTS
// symbol defined on the command line. This symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// DRAWOVERWINDOW_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.
#if defined(DRAWOVERWINDOW_EXPORTS) || defined(PLUGIN_DLL_EXPORT)
#define DRAWOVERWINDOW_API __declspec(dllexport)
#else
#define DRAWOVERWINDOW_API __declspec(dllimport)
#endif



#include "../CommonLib/CommonLib.h"


// This class is exported from the DrawOverWindow.dll
class DRAWOVERWINDOW_API CDrawOverWindow {

	CDrawOverWindow(void);

public:
	HWND m_hWnd;
	int m_size;
	u32* m_pColorArray;
	u32*  m_pByteArray;
	int m_sLastPosX;
	int m_sLastPosY;
	int m_sLastWidth;
	int m_sLastHeight;
	bool m_bIsModified;
	CDrawOverWindow(int posX, int posY, int width, int height);
	virtual ~CDrawOverWindow();

	// TODO: add your methods here.
};

extern DRAWOVERWINDOW_API int nDrawOverWindow;

DRAWOVERWINDOW_API int fnDrawOverWindow(void);

extern "C"
{
	DRAWOVERWINDOW_API void  HideOverwrapWindow(int sInstanceId);
	DRAWOVERWINDOW_API void  HideAllOverwrapWindows();

	DRAWOVERWINDOW_API void  SetOverwrapWindowData(int sInstanceId, u32* byteArray, int size);

	DRAWOVERWINDOW_API void  SetAllAreLoaded(int sInstanceId, int flag);
	DRAWOVERWINDOW_API int   GetAllAreLoaded(int sInstanceId);

	DRAWOVERWINDOW_API void ResetOverwrapWindows();

}
