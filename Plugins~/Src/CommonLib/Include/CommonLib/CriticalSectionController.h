#pragma once

#include "CommonLib/Types.h"

namespace StreamingImageSequencePlugin {

class CriticalSectionObject;

class COMMONLIB_API CriticalSectionController {
public:
	CriticalSectionController(CriticalSectionObject* cs);
	virtual ~CriticalSectionController();

private:
	CriticalSectionObject* m_cs;
};

} // end namespace
