#pragma once

#include "Types.h"

class CCriticalSectionObject;

class COMMONLIBWIN_API CCriticalSectionController
{
	CCriticalSectionObject* m_cs;
	CCriticalSectionController(){};
public:
	CCriticalSectionController(CCriticalSectionObject* cs);
	virtual ~CCriticalSectionController();
};

