#region Copyright (C) 2018 Helmut Wahrmann

/* 
 *  Copyright (C) 2018 Helmut Wahrmann
 *  https://github.com/hwahrmann/NwLUADebugProject
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 3, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

#endregion

#**********************************************************************************************************#
#
#   For building the installer on your own you need:
#       1. Latest NSIS version from http://nsis.sourceforge.net/Download
#       2. DotNotChecker plugin from https://github.com/ReVolly/NsisDotNetChecker
#
#**********************************************************************************************************#

Name "NwLUADebugProject"

SetCompressor /SOLID lzma

# Defines
!define REGKEY "SOFTWARE\Helmut Wahrmann\$(^Name)"
!define VERSION 1.0
!define AUTHOR "Helmut Wahrmann"
!define URL https://github.com/hwahrmann/NwLUADebugProject

# MUI defines
!define MUI_HEADERIMAGE_BITMAP "contrib\Graphics\Header.bmp"
!define MUI_FINISHPAGE_NOAUTOCLOSE
!define MUI_FINISHPAGE_TEXT "Please make sure that you set the LUA_PATH and LUA_CPATH in your Eclipse Environment. Instructions are found in the Documentation."
!define MUI_UNFINISHPAGE_NOAUTOCLOSE

# Included files
!include Sections.nsh
!include MUI2.nsh
!include LogicLib.nsh
!include InstallOptions.nsh
!include "DotNetChecker.nsh"

# Installer pages
!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_LICENSE "contrib\License.txt"
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH
!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES

# Installer languages
!insertmacro MUI_LANGUAGE English

# Installer attributes
OutFile "bin\NwLUADebugProject_setup.exe"
InstallDir "$PROGRAMFILES\NwLUADebugProject"
CRCCheck on
XPStyle on
ShowInstDetails show
VIProductVersion 1.0.0.0
VIAddVersionKey ProductName "Netwitness LUA Debugger Project"
VIAddVersionKey ProductVersion "${VERSION}"
VIAddVersionKey Author "${AUTHOR}"
VIAddVersionKey FileVersion "${VERSION}"
VIAddVersionKey FileDescription ""
VIAddVersionKey LegalCopyright ""
InstallDirRegKey HKLM "${REGKEY}" Path
ShowUninstDetails show

BrandingText  "$(^Name) ${VERSION} by ${AUTHOR}"

# Installer sections
Section -Main SEC0000
	
	!insertmacro CheckNetFramework 461
	
    SetOverwrite on
    
    # Bin Dir including external binaries
    SetOutPath $INSTDIR\bin
    File /r ..\Externals\bin\*
    File ..\NwLuaDebugHelper\bin\Release\NwLuaDebugHelper.dll
    
    # Lualibs
    SetOutPath $INSTDIR\lualibs
    File /r ..\Externals\lualibs\*
    File ..\nw-api\nw-api.lua
    
SectionEnd

Section -post SEC0001
    WriteRegStr HKLM "${REGKEY}" Path $INSTDIR
    SetOutPath $INSTDIR
    WriteUninstaller $INSTDIR\uninstall.exe
    
    WriteRegStr HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\$(^Name)" DisplayName "$(^Name)"
    WriteRegStr HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\$(^Name)" DisplayVersion "${VERSION}"
    WriteRegStr HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\$(^Name)" Publisher "${AUTHOR}"
    WriteRegStr HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\$(^Name)" URLInfoAbout "${URL}"
    WriteRegStr HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\$(^Name)" DisplayIcon $INSTDIR\uninstall.exe
    WriteRegStr HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\$(^Name)" UninstallString $INSTDIR\uninstall.exe
    WriteRegDWORD HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\$(^Name)" NoModify 1
    WriteRegDWORD HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\$(^Name)" NoRepair 1
SectionEnd

# Uninstaller sections
Section "Uninstall"
    RmDir /r /REBOOTOK $INSTDIR\bin
    RmDir /r /REBOOTOK $INSTDIR\lualibs
    
    DeleteRegValue HKLM "${REGKEY}\Components" Main
        DeleteRegKey HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\$(^Name)"
    Delete /REBOOTOK $INSTDIR\uninstall.exe
    DeleteRegKey /IfEmpty HKLM "${REGKEY}\Components"
    DeleteRegKey /IfEmpty HKLM "${REGKEY}"
    RmDir /REBOOTOK $INSTDIR
SectionEnd

# Installer functions
Function .onInit

	; Check for old verion and do uninstall
	ReadRegStr $R0 HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\$(^Name)" "UninstallString"
	StrCmp $R0 "" done
 
	MessageBox MB_OKCANCEL|MB_ICONEXCLAMATION "$(^Name) is already installed. $\n$\nClick `OK` to remove the  previous version or `Cancel` to cancel this upgrade." IDOK uninst
	Abort
 
	;Run the uninstaller
	uninst:
		ClearErrors
		Exec $INSTDIR\uninstall.exe
 
	done:

FunctionEnd