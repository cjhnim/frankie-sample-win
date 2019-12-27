#include "stdafx.h"
#include "CppUnitTest.h"
#include <Windows.h>
#include <WinSafer.h>
#include <Sddl.h>


using namespace Microsoft::VisualStudio::CppUnitTestFramework;

namespace CreateProcessWithUserPermission
{
	static bool _IsNewProcessLaunched();

	TEST_CLASS(UnitTest1)
	{
	public:

		TEST_METHOD(TestMethod1)
		{
			// TODO: Your test code here
			Assert::IsTrue(_IsNewProcessLaunched());
		}
	};

	static bool _IsNewProcessLaunched()
	{
		// Create the restricted token.

		SAFER_LEVEL_HANDLE hLevel = NULL;
		if (!SaferCreateLevel(SAFER_SCOPEID_USER, SAFER_LEVELID_NORMALUSER, SAFER_LEVEL_OPEN, &hLevel, NULL))
		{
			return false;
		}

		HANDLE hRestrictedToken = NULL;
		if (!SaferComputeTokenFromLevel(hLevel, NULL, &hRestrictedToken, 0, NULL))
		{
			SaferCloseLevel(hLevel);
			return false;
		}

		SaferCloseLevel(hLevel);

		// Set the token to medium integrity.

		TOKEN_MANDATORY_LABEL tml = { 0 };
		tml.Label.Attributes = SE_GROUP_INTEGRITY;
		// alternatively, use CreateWellKnownSid(WinMediumLabelSid) instead...
		if (!ConvertStringSidToSid(TEXT("S-1-16-8192"), &(tml.Label.Sid)))
		{
			CloseHandle(hRestrictedToken);
			return false;
		}

		if (!SetTokenInformation(hRestrictedToken, TokenIntegrityLevel, &tml, sizeof(tml) + GetLengthSid(tml.Label.Sid)))
		{
			LocalFree(tml.Label.Sid);
			CloseHandle(hRestrictedToken);
			return false;
		}

		LocalFree(tml.Label.Sid);

		// Create startup info

		STARTUPINFO si = { 0 };
		si.cb = sizeof(si);
		si.lpDesktop = L"winsta0\\default";

		PROCESS_INFORMATION pi = { 0 };

		// Get the current executable's name
		TCHAR exePath[] = { TEXT("notepad.exe") };
		//GetModuleFileName(NULL, exePath, MAX_PATH);

		// Start the new (non-elevated) restricted process
		if (!CreateProcessAsUser(hRestrictedToken, NULL, exePath, NULL, NULL, TRUE, NORMAL_PRIORITY_CLASS, NULL, NULL, &si, &pi))
		{
			CloseHandle(hRestrictedToken);
			return false;
		}

		CloseHandle(hRestrictedToken);
		CloseHandle(pi.hThread);
		CloseHandle(pi.hProcess);

		return true;
	}
	
}