#pragma once

#include "Types.h"

namespace StreamingImageSequencePlugin {

class CriticalSectionObject;

class COMMONLIBWIN_API CriticalSectionController
{
	CriticalSectionObject* m_cs;
	CriticalSectionController(){};
public:
	CriticalSectionController(CriticalSectionObject* cs);
	virtual ~CriticalSectionController();
};

} // end namespace
