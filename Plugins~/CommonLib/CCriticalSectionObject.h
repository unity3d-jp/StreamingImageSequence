#pragma once

#include "Types.h"


class COMMONLIBWIN_API CCriticalSectionObject
{
private:
#ifdef _WIN32
	CRITICAL_SECTION m_cs;
#else
    std::recursive_mutex m_cs;
#endif
	void operator =(const CCriticalSectionObject& src) {}
public:
	CCriticalSectionObject();
	virtual ~CCriticalSectionObject();

	void Enter();
	void Leave();
};
