:: Copyright (c) Microsoft Corporation. All rights reserved.
:: This code is licensed under the Visual Studio SDK license terms.
:: THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
:: ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
:: IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
:: PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

:: Build the binaries then extract their registry attributes as a WiX include file

MSBuild ..\IronPython.sln /p:Configuration=Release /p:RegisterOutputPackage=false
..\..\..\Tools\Bin\regpkg.exe /codebase /root:Software\Microsoft\VisualStudio\9.0 /wixfile:.\IronPythonConsoleWindow.generated.wxi ..\bin\Release\IronPythonConsoleWindow.dll 
..\..\..\Tools\Bin\regpkg.exe /codebase /root:Software\Microsoft\VisualStudio\9.0 /wixfile:.\IronPython.LanguageService.generated.wxi ..\bin\Release\IronPython.LanguageService.dll
..\..\..\Tools\Bin\regpkg.exe /codebase /root:Software\Microsoft\VisualStudio\9.0 /wixfile:.\PythonProject.generated.wxi ..\bin\Release\PythonProject.dll

