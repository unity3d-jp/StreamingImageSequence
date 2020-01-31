#include "CCriticalSectionController.h"
#include "CCriticalSectionObject.h"

CCriticalSectionController::CCriticalSectionController(CCriticalSectionObject* cs)
	:m_cs(cs)
{
	m_cs->Enter();
}

CCriticalSectionController::~CCriticalSectionController()
{
	m_cs->Leave();
}

