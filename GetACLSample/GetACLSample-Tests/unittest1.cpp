#include "stdafx.h"
#include <windows.h>
#include <aclapi.h>
#include <sddl.h>
#include <stdio.h>

#include "CppUnitTest.h"
#include "../GetACLSample/AccessController.h"

using namespace Microsoft::VisualStudio::CppUnitTestFramework;

namespace GetACLSampleTests
{

	TEST_CLASS(UnitTest1)
	{
	public:

		TEST_METHOD(TestExternalMethodCall)
		{
			// TODO: Your test code here
			int result = sum(1, 1);

			Assert::AreEqual(2, result);

		}


		TEST_METHOD(TestGetCurrentDirectory)
		{
			TCHAR path[MAX_PATH];
			GetCurrentDirectory(sizeof path, path);
			Logger::WriteMessage(path);
		}

		TEST_METHOD(TestGetNamedSecurityInfo)
		{
			TCHAR pszObjName[] = TEXT("windows-version.txt");
			PACL pACL = NULL;
			PSECURITY_DESCRIPTOR pSD = NULL;

			DWORD dwRes = GetNamedSecurityInfo(pszObjName, SE_FILE_OBJECT,
				DACL_SECURITY_INFORMATION,
				NULL, NULL, &pACL, NULL, &pSD);

			Assert::AreEqual((int)ERROR_SUCCESS, (int)dwRes);

			PWSTR stringSD;
			ULONG stringSDLen = 0;

			BOOL bResult = ConvertSecurityDescriptorToStringSecurityDescriptor(
				pSD,
				SDDL_REVISION_1,
				OWNER_SECURITY_INFORMATION |
				GROUP_SECURITY_INFORMATION |
				DACL_SECURITY_INFORMATION |
				LABEL_SECURITY_INFORMATION |
				ATTRIBUTE_SECURITY_INFORMATION |
				SCOPE_SECURITY_INFORMATION,
				&stringSD,
				&stringSDLen
			);

			Logger::WriteMessage(stringSD);

			Assert::IsTrue(bResult);

			ULONG count = 0;
			PEXPLICIT_ACCESS explicitEntries = NULL;

			dwRes = GetExplicitEntriesFromAcl(pACL, &count, &explicitEntries);

			Logger::WriteMessage(stringSD);

			Assert::AreEqual((int)0, (int)count);
			Assert::AreEqual((int)ERROR_SUCCESS, (int)dwRes);

			if (stringSD != NULL)
				LocalFree(stringSD);
			if (explicitEntries != NULL)
				LocalFree(explicitEntries);

		}

	};
}