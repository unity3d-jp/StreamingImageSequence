#include "stdafx.h"

#include "CommonLib/CriticalSectionController.h"
#include "CommonLib/CriticalSectionObject.h"

namespace StreamingImageSequencePlugin {

CriticalSectionController::CriticalSectionController(CriticalSectionObject* cs)
	:m_cs(cs)
{
	m_cs->Enter();
}

CriticalSectionController::~CriticalSectionController()
{
	m_cs->Leave();
}

} //end namespace
