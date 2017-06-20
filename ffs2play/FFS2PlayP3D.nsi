; HM NIS Edit Wizard helper defines
!define PRODUCT_NAME "FFSTracker P3D"
!getdllversion ".\bin\x64\P3D-Release\ffs2play.exe" expv_
!define PRODUCT_VERSION "${expv_1}.${expv_2}.${expv_3}"
!define PRODUCT_PUBLISHER "ffsimulateur2.fr (c)2017"
!define PRODUCT_WEB_SITE "http://www.ffsimulateur2.fr"
!define PRODUCT_DIR_REGKEY "Software\Microsoft\Windows\CurrentVersion\App Paths\FFS2PlayP3D"
!define PRODUCT_UNINST_KEY "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_NAME}"
!define PRODUCT_UNINST_ROOT_KEY "HKLM"
!define PRODUCT_STARTMENU_REGVAL "NSIS:StartMenuDir"
 
; MUI 1.67 compatible ------
!include "MUI2.nsh"
!include "FileAssociation.nsh"
!include "PostExec.nsh"
; MUI Settings
!define MUI_ABORTWARNING
!define MUI_ICON ".\images\ffstracker_48x48.ico"
!define MUI_UNICON ".\images\ffstracker_48x48.ico"
!define MUI_WELCOMEFINISHPAGE_BITMAP ".\images\splash.bmp"
 
; Language Selection Dialog Settings
!define MUI_LANGDLL_REGISTRY_ROOT "${PRODUCT_UNINST_ROOT_KEY}"
!define MUI_LANGDLL_REGISTRY_KEY "${PRODUCT_UNINST_KEY}"
;!define MUI_LANGDLL_REGISTRY_VALUENAME "NSIS:Language"
  
; Welcome page
!insertmacro MUI_PAGE_WELCOME
; License page
!define MUI_LICENSEPAGE_CHECKBOX
!insertmacro MUI_PAGE_LICENSE ".\readme.txt"
; Directory page
!insertmacro MUI_PAGE_DIRECTORY
; Instfiles page
!insertmacro MUI_PAGE_INSTFILES
; Finish page
!define MUI_FINISHPAGE_RUN "$INSTDIR\ffs2play.exe"
!define MUI_FINISHPAGE_SHOWREADME "$INSTDIR\readme.txt"
!insertmacro MUI_PAGE_FINISH
 
; Uninstaller pages
!insertmacro MUI_UNPAGE_INSTFILES
 
; Language files
!insertmacro MUI_LANGUAGE "French"
 
; MUI end ------
 
Name "${PRODUCT_NAME} ${PRODUCT_VERSION}"
!define OutFile "FFS2PlayP3D_${expv_1}_${expv_2}_${expv_3}.exe"
OutFile ${OUTFILE}
InstallDir "$PROGRAMFILES64\FFS2PlayP3D"
InstallDirRegKey HKLM "${PRODUCT_DIR_REGKEY}" ""
ShowInstDetails show
ShowUnInstDetails show
 
Function .onInit
  !insertmacro MUI_LANGDLL_DISPLAY
FunctionEnd
 
Section "SectionPrincipale" SEC01
  SetOutPath "$INSTDIR"
  SetOverwrite on
  File ".\bin\x64\P3D-Release\ffs2play.exe"
  CreateDirectory "$SMPROGRAMS\FFS2PlayP3D"
  CreateShortCut "$SMPROGRAMS\FFS2PlayP3D\ffstracker.lnk" "$INSTDIR\ffs2play.exe"
  CreateShortCut "$DESKTOP\FFS2PlayP3D.lnk" "$INSTDIR\ffs2play.exe"
  File ".\bin\x64\P3D-Release\LockheedMartin.Prepar3D.SimConnect.dll"
  File ".\bin\x64\P3D-Release\Open.Nat.dll"
  File ".\bin\x64\P3D-Release\System.Data.SQLite.dll"
  File ".\bin\x64\P3D-Release\x64\SQLite.Interop.dll"
  File ".\bin\x64\P3D-Release\SharpCompress.dll"
  File ".\bin\x64\P3D-Release\protobuf-net.dll"
  File ".\bin\x64\P3D-Release\ENG.WMOCodes.Decoders.dll"
  File ".\bin\x64\P3D-Release\ENG.WMOCodes.dll"
  File ".\bin\x64\P3D-Release\ENG.WMOCodes.Downloaders.dll"
  File ".\bin\x64\P3D-Release\ENG.WMOCodes.Formatters.dll"
  File ".\bin\x64\P3D-Release\ENG.WMOCodes.Formatters.InfoFormatter.dll"
  File ".\bin\x64\P3D-Release\ENG.WMOCodes.Formatters.ShortInfo.dll"
  File ".\bin\x64\P3D-Release\ESystem.dll"
  File ".\bin\x64\P3D-Release\ESystem.Extensions.dll"
  File "readme.txt"
  ${registerExtension} "$INSTDIR\ffstracker.exe"  ".pirep" "FFSTracker pirep"
SectionEnd
 
Section -AdditionalIcons
  WriteIniStr "$INSTDIR\${PRODUCT_NAME}.url" "InternetShortcut" "URL" "${PRODUCT_WEB_SITE}"
  CreateShortCut "$SMPROGRAMS\FFS2PlayP3D\Website.lnk" "$INSTDIR\${PRODUCT_NAME}.url"
  CreateShortCut "$SMPROGRAMS\FFS2PlayP3D\Uninstall.lnk" "$INSTDIR\uninst.exe"
SectionEnd
 
Section -Post
  WriteUninstaller "$INSTDIR\uninst.exe"
  WriteRegStr HKLM "${PRODUCT_DIR_REGKEY}" "" "$INSTDIR\ffs2play.exe"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "DisplayName" "$(^Name)"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "UninstallString" "$INSTDIR\uninst.exe"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "DisplayIcon" "$INSTDIR\ffs2play.exe"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "DisplayVersion" "${PRODUCT_VERSION}"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "URLInfoAbout" "${PRODUCT_WEB_SITE}"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "Publisher" "${PRODUCT_PUBLISHER}"
SectionEnd
 
 
Function un.onUninstSuccess
  HideWindow
  MessageBox MB_ICONINFORMATION|MB_OK "$(^Name) a été désinstallé avec succès de votre ordinateur."
FunctionEnd
 
Function un.onInit
!insertmacro MUI_UNGETLANGUAGE
  MessageBox MB_ICONQUESTION|MB_YESNO|MB_DEFBUTTON2 "Êtes-vous certains de vouloir désinstaller totalement $(^Name) et tous ses composants ?" IDYES +2
  Abort
FunctionEnd
 
Section Uninstall
  RMDir /r "$SMPROGRAMS\FFS2PlayP3D"
  RMDir /r "$INSTDIR"
  ;${unregisterExtension} ".pirep" "FFSTracker pirep"
  DeleteRegKey ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}"
  DeleteRegKey HKLM "${PRODUCT_DIR_REGKEY}"
  SetAutoClose true
SectionEnd
!finalize '7z.exe a -mx9 "FFS2PlayP3D_${expv_1}_${expv_2}_${expv_3}.zip" ${OUTFILE}'