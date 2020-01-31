#include "CCriticalSectionObject.h"

CCriticalSectionObject::CCriticalSectionObject()
{
#ifdef _WIN32
	InitializeCriticalSection(&m_cs);
#endif
}

CCriticalSectionObject::~CCriticalSectionObject()
{
#ifdef _WIN32
	DeleteCriticalSection(&m_cs);
#endif
}

void CCriticalSectionObject::Enter()
{
#ifdef _WIN32
	EnterCriticalSection(&m_cs);
#else
    m_cs.lock();
#endif
}

void CCriticalSectionObject::Leave()
{
#ifdef _WIN32
	LeaveCriticalSection(&m_cs);
#else
    m_cs.unlock();
#endif
}


