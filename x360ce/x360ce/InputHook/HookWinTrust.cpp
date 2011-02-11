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
#include "Utilities\Log.h"
#include <Softpub.h>
#include "InputHook.h"

//EasyHook
TRACED_HOOK_HANDLE		hHookWinVerifyTrust = NULL;

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
LONG (WINAPI *OriginalWinVerifyTrust)(HWND hwnd, GUID *pgActionID,LPVOID pWVTData) = NULL;
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

LONG WINAPI HookWinVerifyTrust(HWND hwnd, GUID *pgActionID,LPVOID pWVTData)
{
	if(!InputHook_Config()->bEnabled) return HookWinVerifyTrust(hwnd,pgActionID,pWVTData);
	WriteLog(LOG_HOOKWT,L"HookWinVerifyTrust");

	UNREFERENCED_PARAMETER(hwnd);
	UNREFERENCED_PARAMETER(pgActionID);
	UNREFERENCED_PARAMETER(pWVTData);
	return 0;
}

void HookWinTrust()
{
	if(!OriginalWinVerifyTrust) {
		OriginalWinVerifyTrust = WinVerifyTrust;
		hHookWinVerifyTrust = new HOOK_TRACE_INFO(); 
		WriteLog(LOG_IHOOK,L"HookWinTrust:: Hooking");

		LhInstallHook(OriginalWinVerifyTrust,HookWinVerifyTrust,static_cast<PVOID>(NULL),hHookWinVerifyTrust);
		LhSetExclusiveACL(ACLEntries, 0, hHookWinVerifyTrust);
	}
}

void HookWinTrustClean()
{
	SAFE_DELETE(hHookWinVerifyTrust);
}