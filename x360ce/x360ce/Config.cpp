/*  x360ce - XBOX360 Controler Emulator
*  Copyright (C) 2002-2010 ToCA Edit
*
*  x360ce is free software: you can redistribute it and/or modify it under the terms
*  of the GNU Lesser General Public License as published by the Free Software Found-
*  ation, either version 3 of the License, or (at your option) any later version.
*
*  x360ce is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
*  without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR
*  PURPOSE.  See the GNU General Public License for more details.
*
*  You should have received a copy of the GNU General Public License along with x360ce.
*  If not, see <http://www.gnu.org/licenses/>.
*/

#include "stdafx.h"
#include "globals.h"
#include "Utils.h"
#include "Config.h"
#include "DirectInput.h"

WORD wFakeAPI=0;
BOOL bInitBeep=0;

WORD wFakeWMI=0;
WORD wFakeDI=0;
WORD wFakeWinTrust=0;

WORD wFakeVID=0;
WORD wFakePID=0;

WORD wNativeMode=0;

static LPCTSTR buttonNames[10] = {
	_T("A"),
	_T("B"),
	_T("X"),
	_T("Y"),
	_T("Left Shoulder"),
	_T("Right Shoulder"),
	_T("Back"),
	_T("Start"),
	_T("Left Thumb"),
	_T("Right Thumb"),
};

static LPCTSTR povNames[4] = {
	_T("D-pad Up"),
	_T("D-pad Down"),
	_T("D-pad Left"),
	_T("D-pad Right")
};

static LPCTSTR axisNames[4] = {
	_T("Left Analog X"),
	_T("Left Analog Y"),
	_T("Right Analog X"),
	_T("Right Analog Y")
};
static LPCTSTR axisBNames[8] = {
	_T("Left Analog X+ Button"),
	_T("Left Analog X- Button"),
	_T("Left Analog Y+ Button"),
	_T("Left Analog Y- Button"),
	_T("Right Analog X+ Button"),
	_T("Right Analog X- Button"),
	_T("Right Analog Y+ Button"),
	_T("Right Analog Y- Button")
};
static LPCTSTR padNames[4] = {
	_T("PAD1"),
	_T("PAD2"),
	_T("PAD3"),
	_T("PAD4"),
};

GamepadMap GamepadMapping[4];

extern "C" void InitConfig(LPTSTR ininame) 
{
	LPTSTR pStr;
	TCHAR strPath[MAX_PATHW];
	extern TCHAR tstrConfigFile[MAX_PATHW];	
	GetModuleFileName (NULL, strPath, MAX_PATHW);
	pStr = _tcsrchr(strPath, _T('\\'));
	if (pStr != NULL)
		*(++pStr)=_T('\0'); 

	_stprintf_s(tstrConfigFile,_T("%s%s"),strPath, ininame);

	// Read global options
	bInitBeep = ReadUINTFromFile(_T("Options"), _T("UseInitBeep"));
	writelog = ReadUINTFromFile(_T("Options"), _T("Log"));

	//FakeAPI
	wFakeWMI = (WORD) ReadUINTFromFile(_T("FakeAPI"), _T("FakeWMI"));
	wFakeDI = (WORD) ReadUINTFromFile(_T("FakeAPI"), _T("FakeDI"));
	wFakeWinTrust = (WORD) ReadUINTFromFile(_T("FakeAPI"), _T("FakeWinTrust"));

	wFakeAPI = wFakeWMI + wFakeDI;

	if(wFakeAPI)
	{
		wFakeVID = (WORD) ReadUINTFromFile(_T("FakeAPI"), _T("FakeVID"));
		wFakePID = (WORD) ReadUINTFromFile(_T("FakeAPI"), _T("FakePID"));
	}

	// Read pad mappings
	for (WORD b = 0; b < 4; b++) 
		ReadPadConfig(b);
}


void ReadPadConfig(INT idx) {

	TCHAR section[5];
	_stprintf_s(section,_T("PAD%d"),idx+1);
	GamepadMap &PadMap = GamepadMapping[idx];

	TCHAR buffer[MAX_PATHW];

	Gamepad[idx].native = ReadUINTFromFile(section, _T("Native"));
	if(Gamepad[idx].native) 
	{ 
		wNativeMode = 1;
		return; 
	}

	ReadStringFromFile(section, _T("Product"), buffer, _T("0"));
	StringToGUID(&Gamepad[idx].product,buffer);

	Gamepad[idx].pid = HIWORD(Gamepad[idx].product.Data1);
	Gamepad[idx].vid = LOWORD(Gamepad[idx].product.Data1);

	ReadStringFromFile(section, _T("Instance"), buffer, _T("0"));
	StringToGUID(&Gamepad[idx].instance,buffer);

	if (Gamepad[idx].product.Data1 > 0) { PadMap.enabled = true; } else { return; }  

	Gamepad[idx].dwPadIndex = idx+1;

	Gamepad[idx].swapmotor = ReadUINTFromFile(section, _T("SwapMotor"));
	Gamepad[idx].tdeadzone = ReadUINTFromFile(section, _T("TriggerDeadzone"));
	Gamepad[idx].useforce = ReadUINTFromFile(section, _T("UseForceFeedback"));
	Gamepad[idx].gamepadtype = (BYTE)ReadUINTFromFile(section, _T("ControllerType"));
	Gamepad[idx].axistodpad = ReadUINTFromFile(section, _T("AxisToDPad"));
	Gamepad[idx].axistodpaddeadzone = ReadUINTFromFile(section, _T("AxisToDPadDeadZone"));
	Gamepad[idx].axistodpadoffset = ReadUINTFromFile(section, _T("AxisToDPadOffset"));
	Gamepad[idx].forcepercent = (FLOAT) ReadUINTFromFile(section, _T("ForcePercent")) * (FLOAT) 0.01;	
	for (INT i = 0; i < 10; ++i) PadMap.Button[i] = -1;
	for (INT i = 0; i < 2; ++i) PadMap.Trigger[i].type = NONE;
	PadMap.DpadPOV = -1;

	//TODO: rewrite this ...
	for (INT i=0; i<10; ++i) {
		if (ReadStringFromFile(section, buttonNames[i], buffer) > 0) {
			INT val = _tstoi(buffer);
			PadMap.Button[i] = val - 1;
		}
	}

	for (INT i=0; i<4; ++i) {
		if (ReadStringFromFile(section, povNames[i], buffer) > 0) {

			INT val = _tstoi(buffer);
			if(val > 0) 
			{
				PadMap.pov[i] = val - 1;
			} 
			else 
			{
				if(_tcsstr(buffer,_T("UP"))) PadMap.pov[i] = XINPUT_GAMEPAD_DPAD_UP;
				if(_tcsstr(buffer,_T("DOWN"))) PadMap.pov[i] = XINPUT_GAMEPAD_DPAD_DOWN;
				if(_tcsstr(buffer,_T("LEFT"))) PadMap.pov[i] = XINPUT_GAMEPAD_DPAD_LEFT;
				if(_tcsstr(buffer,_T("RIGHT"))) PadMap.pov[i] = XINPUT_GAMEPAD_DPAD_RIGHT;
			}
		}
	}

	for (INT i=0; i<4; ++i) {
		if (ReadStringFromFile(section, axisNames[i], buffer) > 0) {
			LPTSTR a = buffer;
			if (_totlower(*a) == 's') { // Slider
				PadMap.Axis[i].analogType = SLIDER;
				++a;
				PadMap.Axis[i].id = _ttoi(a);
			}else{
				// Axis
				PadMap.Axis[i].analogType = AXIS;
				PadMap.Axis[i].id = _ttoi(a);
			}
		}
	}

	for (INT i=0; i<4; ++i) {
		if (ReadStringFromFile(section, axisBNames[i*2],buffer) > 0) {
			INT ret = _tstoi(buffer);
			PadMap.Axis[i].hasDigital = true;
			PadMap.Axis[i].positiveButtonID = ret - 1;
		}
		if (ReadStringFromFile(section, axisBNames[i*2+1],buffer) > 0) {
			INT ret = _tstoi(buffer);
			PadMap.Axis[i].hasDigital = true;
			PadMap.Axis[i].negativeButtonID = ret - 1;
		}
	}
	if (ReadStringFromFile(section, _T("Left Trigger"), buffer) > 0) {
		LPTSTR a = buffer;
		if ((PadMap.Trigger[0].type = getTriggerType(a)) == DIGITAL) {
			PadMap.Trigger[0].id = _ttoi(a) - 1;
		} else {
			++a;
			PadMap.Trigger[0].id = _ttoi(a);
		}
	}

	if (ReadStringFromFile(section, _T("Right Trigger"), buffer) > 0) {
		LPTSTR a = buffer;
		if ((PadMap.Trigger[1].type = getTriggerType(a)) == DIGITAL) {
			PadMap.Trigger[1].id = _ttoi(a) - 1;
		} else {
			++a;
			PadMap.Trigger[1].id = _ttoi(a);
		}
	}

	if (UINT ret = ReadUINTFromFile(section, _T("D-pad POV")) > 0) {
		PadMap.DpadPOV = ret - 1;
	}
}

// NOTE: Letters corresponding to mapping types changed. Include in update notes.
MappingType getTriggerType(LPCTSTR s) {
	if (_totlower(*s) == 'a') return AXIS;	// Axis
	if (_totlower(*s) == 's') return SLIDER;	// Slider
	if (_totlower(*s) == 'x') return HAXIS;	// Half range axis
	if (_totlower(*s) == 'h') return HSLIDER;	// Half range slider
	return DIGITAL;							// Digital
}