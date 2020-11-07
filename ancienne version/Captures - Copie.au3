; #INFORMATIONS# ====================================================================
;
;                                  ~ Captures ~
;                                ~~~~~~~~~~~~~~~~
;
; Title ...........: Captures
; Langue ..........: Français
; Langage .........: AutoIt 3
; Description .....: Captures
; Author(s) .......: TheRake66
; App version .....: 3.1
; AutoIt version ..: 3.4.14.5
;
;
;                             ~ Informations legales ~
;                           ~~~~~~~~~~~~~~~~~~~~~~~~~~~~
;
;  (c) Bustos Thibault - 2019
;
;  Interdiction de copier, modifier et / ou distribuer ce logiciel sans l'accord de 
;  l'auteur. Il est également interdit de le vendre pour n'importe quelle monnaie.
;
; ===================================================================================






; #SOURCE-CODE# =====================================================================
; ###################################################################################
#Region Startup

; --------------------------------------------------------------
#AutoIt3Wrapper_Icon=icon.ico
#AutoIt3Wrapper_Res_Description=Captures
#AutoIt3Wrapper_Res_Fileversion=3.1
#AutoIt3Wrapper_Res_ProductName=Captures
#AutoIt3Wrapper_Res_ProductVersion=3.1
#AutoIt3Wrapper_Res_Legalcopyright=© Bustos Thibault - 2019

Global $VERSION = "3.1"
Global $COPYRIGHT = "© Bustos Thibault - 2019"
; --------------------------------------------------------------


; --------------------------------------------------------------
#include <ScreenCapture.au3>
; --------------------------------------------------------------


; --------------------------------------------------------------
$i = ProcessList(@ScriptName)
If $i[0][0] > 1 Then
	Exit
EndIf

Opt("TrayMenuMode", 1+2)
; --------------------------------------------------------------


; --------------------------------------------------------------
Global $ATSTART = IniRead(@AppDataDir & "\Captures.ini", "Options", "Start", "1")
If $ATSTARt = 1 Then FileCreateShortcut(@ScriptFullPath, @StartupDir & "\" & @ScriptName & ".lnk")

Global $SCREENFOLDER = IniRead(@AppDataDir & "\Captures.ini", "Options", "Folder", @UserProfileDir & "\Pictures\Captures")
If not FileExists($SCREENFOLDER & "\") Then $SCREENFOLDER = @UserProfileDir & "\Pictures\Captures"

Global $SCREENFILE = IniRead(@AppDataDir & "\Captures.ini", "Options", "File", ".PNG")
If not $SCREENFILE = ".PNG" And not $SCREENFILE = ".JPG" not $SCREENFILE = ".BMP" Then Global $SCREENFILE = ".PNG" 

Global $SCREENKEY = IniRead(@AppDataDir & "\Captures.ini", "Options", "KeyScreen", "F9")
If not $SCREENKEY = "F1" And not $SCREENKEY = "F2" And not $SCREENKEY = "F3" And not $SCREENKEY = "F4" And not $SCREENKEY = "F5" And not $SCREENKEY = "F6" And not $SCREENKEY = "F7" And not $SCREENKEY = "F8" And not $SCREENKEY = "F9" And not $SCREENKEY = "F10" And not $SCREENKEY = "F11" And not $SCREENKEY = "F12" And not $SCREENKEY = "F13" Then Global $SCREENKEY = "F9"

Global $SCREENCUTKEY = IniRead(@AppDataDir & "\Captures.ini", "Options", "KeyCut", "F10")
If not $SCREENCUTKEY = "F1" And not $SCREENCUTKEY = "F2" And not $SCREENCUTKEY = "F3" And not $SCREENCUTKEY = "F4" And not $SCREENCUTKEY = "F5" And not $SCREENCUTKEY = "F6" And not $SCREENCUTKEY = "F7" And not $SCREENCUTKEY = "F8" And not $SCREENCUTKEY = "F9" And not $SCREENCUTKEY = "F10" And not $SCREENCUTKEY = "F11" And not $SCREENCUTKEY = "F12" And not $SCREENCUTKEY = "F13" Then Global $SCREENCUTKEY = "F10"

Global $SCREENWINKEY = IniRead(@AppDataDir & "\Captures.ini", "Options", "KeyWin", "F8")
If not $SCREENWINKEY = "F1" And not $SCREENWINKEY = "F2" And not $SCREENWINKEY = "F3" And not $SCREENWINKEY = "F4" And not $SCREENWINKEY = "F5" And not $SCREENWINKEY = "F6" And not $SCREENWINKEY = "F7" And not $SCREENWINKEY = "F8" And not $SCREENWINKEY = "F9" And not $SCREENWINKEY = "F10" And not $SCREENWINKEY = "F11" And not $SCREENWINKEY = "F12" And not $SCREENWINKEY = "F13" Then Global $SCREENWINKEY = "F8"
; --------------------------------------------------------------

#EndRegion Startup
; ###################################################################################






; ###################################################################################
#Region Gui_Main

; --------------------------------------------------------------
$Gui_Main = GUICreate("Captures", 480, 155, 0, 0, -1, 0x00000080)
	GUISetFont(9)
	GUISetBkColor(0xFFFFFF)
; --------------------------------------------------------------


; --------------------------------------------------------------
$Gui_Main_Menu_Options = GUICtrlCreateMenu("Options")
$Gui_Main_Menu_Options_Path = GUICtrlCreateMenuItem("Changer le dossier des captures d'écrans", $Gui_Main_Menu_Options)
$Gui_Main_Menu_Options_Start = GUICtrlCreateMenuItem("Lancer au démarrage de Windows", $Gui_Main_Menu_Options)
If $ATSTART = 1 Then
	GUICtrlSetState(-1, 1)
EndIf
GUICtrlCreateMenuItem("", $Gui_Main_Menu_Options)
$Gui_Main_Menu_Options_Quit = GUICtrlCreateMenuItem("Fermer Captures", $Gui_Main_Menu_Options)

$Gui_Main_Menu_File = GUICtrlCreateMenu("Format")
$Gui_Main_Menu_File_PNG = GUICtrlCreateMenuItem(".PNG", $Gui_Main_Menu_File, 0, 1)
If $SCREENFILE = ".PNG" Then GUICtrlSetState(-1, 1)
$Gui_Main_Menu_File_JPG= GUICtrlCreateMenuItem(".JPG", $Gui_Main_Menu_File, 1, 1)
If $SCREENFILE = ".JPG" Then GUICtrlSetState(-1, 1)
$Gui_Main_Menu_File_BMP = GUICtrlCreateMenuItem(".BMP", $Gui_Main_Menu_File, 2, 1)
If $SCREENFILE = ".BMP" Then GUICtrlSetState(-1, 1)

$Gui_Main_Menu_WinScreen = GUICtrlCreateMenu("Clé pour fenêtrer")
For $i = 1 To 12 Step + 1
	Assign("Gui_Main_Menu_KeyWin_F" & $i, GUICtrlCreateMenuItem("F" & $i, $Gui_Main_Menu_WinScreen, $i-1, 1))
	If $SCREENWINKEY = "F" & $i Then GUICtrlSetState(-1, 1)
Next

$Gui_Main_Menu_KeyScreen = GUICtrlCreateMenu("Clé pour capturer")
For $i = 1 To 12 Step + 1
	Assign("Gui_Main_Menu_KeyScreen_F" & $i, GUICtrlCreateMenuItem("F" & $i, $Gui_Main_Menu_KeyScreen, $i-1, 1))
	If $SCREENKEY = "F" & $i Then GUICtrlSetState(-1, 1)
Next

$Gui_Main_Menu_KeyCut = GUICtrlCreateMenu("Clé pour découper")
For $i = 1 To 12 Step + 1
	Assign("Gui_Main_Menu_KeyCut_F" & $i, GUICtrlCreateMenuItem("F" & $i, $Gui_Main_Menu_KeyCut, $i-1, 1))
	If $SCREENCUTKEY = "F" & $i Then GUICtrlSetState(-1, 1)
Next

$Gui_Main_Menu_Help = GUICtrlCreateMenu("?")
$Gui_Main_Menu_Help_APropos = GUICtrlCreateMenuItem("À propos..", $Gui_Main_Menu_Help)
; --------------------------------------------------------------


; --------------------------------------------------------------
FileInstall("jpg.jpg", @TempDir & "\jpg.jpg", 1)
GUICtrlCreatePic(@TempDir & "\jpg.jpg", 5, 20, 64, 55, 128)
FileDelete(@TempDir & "\jpg.jpg")
; --------------------------------------------------------------


; --------------------------------------------------------------
$Gui_Main_Button_Win = GUICtrlCreateButton("Fenêtrer une capture d'écran", 75, 10, 400, 25)
$Gui_Main_Button_Screen = GUICtrlCreateButton("Prendre une capture d'écran", 75, 35, 400, 25)
$Gui_Main_Button_Cut = GUICtrlCreateButton("Découper une capture d'écran", 75, 60, 400, 25)

GUICtrlCreateGroup(" Dossier des captures d'écrans ", 0, 95, 480, 30)

$Gui_Main_Input_Folder = GUICtrlCreateInput("", 0, 115, 480, 20, 0x0001+0x0080+0x0800)
	GUICtrlSetData(-1, $SCREENFOLDER)
; --------------------------------------------------------------


; --------------------------------------------------------------
__Tray_Load()
; --------------------------------------------------------------

#EndRegion Gui
; ###################################################################################






; ###################################################################################
#Region Brain


; --------------------------------------------------------------
HotKeySet("{" & $SCREENKEY & "}", "__Screen_Capture")
HotKeySet("{" & $SCREENWINKEY & "}", "__Screen_Win")
HotKeySet("{" & $SCREENCUTKEY & "}", "__Screen_Cut")

TrayTip("Captures", "Captures vient de se lancer." & @Crlf & _
					"(dans la zone de notifications)", -1, 1)
; --------------------------------------------------------------



While 1

	; --------------------------------------------------------------
	$GUIMSG = GUIGetMsg()
	$TRAYMSG = TrayGetMsg()
	; --------------------------------------------------------------
	
	
	; --------------------------------------------------------------
	Switch $TRAYMSG
	
		Case $Gui_Tray_Screen
			__Screen_Capture()
	
		Case $Gui_Tray_Win
			__Screen_Win()
	
		Case $Gui_Tray_Cut
			__Screen_Cut()
	
		Case $Gui_Tray_View
			GUISetState(@SW_SHOW, $Gui_Main)
			
		Case $Gui_Tray_Quit
			GUIDelete($Gui_Main)
			Exit
		
			
	EndSwitch
	; --------------------------------------------------------------
	
	
	; --------------------------------------------------------------
	Switch $GUIMSG
	
		Case -3
			GUISetState(@SW_HIDE, $Gui_Main)
			
		Case $Gui_Main_Menu_Options_Quit
			Exit
			
		Case $Gui_Main_Menu_Options_Path
			$a = FileSelectFolder("Choissiez le chemins des captures d'écrans...", $SCREENFOLDER, 0, "", $Gui_Main)
			If $a <> "" Then
				Global $SCREENFOLDER = $a
				IniWrite(@AppDataDir & "\Captures.ini", "Options", "Folder", $SCREENFOLDER)
				GUICtrlSetData($Gui_Main_Input_Folder, $SCREENFOLDER)
			EndIf
			
		Case $Gui_Main_Menu_Options_Start
			If GUICtrlRead($Gui_Main_Menu_Options_Start) = 68 Then
				FileCreateShortcut(@ScriptFullPath, @StartupDir & "\" & @ScriptName & ".lnk")
				IniWrite(@AppDataDir & "\Captures.ini", "Options", "Start", "1")
				GUICtrlSetState($Gui_Main_Menu_Options_Start, 1)
			Else
				FileDelete(@StartupDir & "\" & @ScriptName & ".lnk")
				IniWrite(@AppDataDir & "\Captures.ini", "Options", "Start", "0")
				GUICtrlSetState($Gui_Main_Menu_Options_Start, 4)
			EndIf
				
		Case $Gui_Main_Menu_Help_APropos
			MsgBox(0+64, "À propos...", "Captures" & @Crlf & _
										"" & @Crlf & _
										"Auteur : Bustos Thibault" & @Crlf & _
										"Version : " & $VERSION & @Crlf & _
										"Copyright : " & $COPYRIGHT & @Crlf & _
										"Langage : AutoIt 3" & @Crlf & _
										"" & @Crlf & _
										"Me joindre : thibault.bustos1234@gmail.com")
	
		Case $Gui_Main_Button_Win
			GUISetState(@SW_HIDE, $Gui_Main)
			Sleep(500)
			__Screen_Win()
			GUISetState(@SW_SHOW, $Gui_Main)
	
		Case $Gui_Main_Button_Screen
			GUISetState(@SW_HIDE, $Gui_Main)
			Sleep(500)
			__Screen_Capture()
			GUISetState(@SW_SHOW, $Gui_Main)
			
		Case $Gui_Main_Button_Cut
			GUISetState(@SW_HIDE, $Gui_Main)
			Sleep(500)
			__Screen_Cut()
			GUISetState(@SW_SHOW, $Gui_Main)
	
	EndSwitch
	; --------------------------------------------------------------
	
	
	; --------------------------------------------------------------
	; Case $Gui_Main_Menu_File_~
	Dim $a[3] = ["PNG", "JPG", "BMP"]
	For $i = 0 To 2 Step + 1
		Switch $GUIMSG
		
			Case Eval("Gui_Main_Menu_File_" & $a[$i])
				IniWrite(@AppDataDir & "\Captures.ini", "Options", "File", "." & $a[$i])
				Global $SCREENFILE = "." & $a[$i]
				
		EndSwitch
	Next
	
	; Case $Gui_Main_Menu_KeyWin_F~
	For $i = 1 To 12 Step + 1
		Switch $GUIMSG

			Case Eval("Gui_Main_Menu_KeyWin_F" & $i)
				IniWrite(@AppDataDir & "\Captures.ini", "Options", "KeyWin", "F" & $i)
				HotKeySet("{" & $SCREENWINKEY & "}")
				Global $SCREENWINKEY = "F" & $i
				HotKeySet("{" & $SCREENWINKEY & "}", "__Screen_Capture")
				__Tray_Load()
				
		EndSwitch
	Next
	
	; Case $Gui_Main_Menu_KeyScreen_F~
	For $i = 1 To 12 Step + 1
		Switch $GUIMSG

			Case Eval("Gui_Main_Menu_KeyScreen_F" & $i)
				IniWrite(@AppDataDir & "\Captures.ini", "Options", "KeyScreen", "F" & $i)
				HotKeySet("{" & $SCREENKEY & "}")
				Global $SCREENKEY = "F" & $i
				HotKeySet("{" & $SCREENKEY & "}", "__Screen_Capture")
				__Tray_Load()
				
		EndSwitch
	Next

	; Case $Gui_Main_Menu_KeyCut_F~
	For $i = 1 To 12 Step + 1
		Switch $GUIMSG
		
			Case Eval("Gui_Main_Menu_KeyCut_F" & $i)
				IniWrite(@AppDataDir & "\Captures.ini", "Options", "KeyCut", "F" & $i)
				HotKeySet("{" & $SCREENCUTKEY & "}")
				Global $SCREENCUTKEY = "F" & $i
				HotKeySet("{" & $SCREENCUTKEY & "}", "__Screen_Cut")
				__Tray_Load()
				
		EndSwitch
	Next
	; --------------------------------------------------------------
	
WEnd




#EndRegion Brain
; ###################################################################################






; ###################################################################################
#Region Function

; --------------------------------------------------------------
Func __Tray_Load()
		
		Dim $a[7] = ["Gui_Tray_Win", "Gui_Tray_Screen", "Gui_Tray_Cut", "Gui_Tray_Sep1", "Gui_Tray_View", "Gui_Tray_Sep2", "Gui_Tray_Quit"]
		For $i = 0 To 6 Step + 1
			TrayItemDelete(Eval($a[$i]))
		Next

		Global $Gui_Tray_Win = TrayCreateItem("Fenêtrer une capture d'écran	" & $SCREENWINKEY)
		Global $Gui_Tray_Screen = TrayCreateItem("Prendre une capture d'écran	" & $SCREENKEY)
		Global $Gui_Tray_Cut = TrayCreateItem("Découper une capture d'écran	" & $SCREENCUTKEY)
		Global $Gui_Tray_Sep1 = TrayCreateItem("")
		Global $Gui_Tray_View = TrayCreateItem("Afficher Captures")
		Global $Gui_Tray_Sep2 = TrayCreateItem("")
		Global $Gui_Tray_Quit = TrayCreateItem("Fermer Captures")
	
EndFunc
; --------------------------------------------------------------


; --------------------------------------------------------------
Func __Screen_Win()
	
	DirCreate($SCREENFOLDER)
	_ScreenCapture_CaptureWnd($SCREENFOLDER & "\" & @Year & @Mon & @MDay & "_" & @Hour & @Min & @Sec & @MSec & $SCREENFILE, WinGetHandle("[ACTIVE]"), 0, 0, -1, -1, False)
	
	If @error = 0 Then
		TrayTip("Captures", "Fenêtrage d'écran faites.", -1, 1)
	Else
		TrayTip("Captures", "Erreur de sauvegarde de la capture d'écran !", -1, 3)
	EndIf
	
EndFunc
; --------------------------------------------------------------



; --------------------------------------------------------------
Func __Screen_Capture()
	
	GUISetState(@SW_HIDE, $Gui_Main)

	$a = DLLCall("user32.dll", "int", "GetSystemMetrics", "int", 76)
	$b = DLLCall("user32.dll", "int", "GetSystemMetrics", "int", 77)
	$c = DLLCall("user32.dll", "int", "GetSystemMetrics", "int", 78)
	$d = DLLCall("user32.dll", "int", "GetSystemMetrics", "int", 79)
	
	DirCreate($SCREENFOLDER)
	_ScreenCapture_Capture($SCREENFOLDER & "\" & @Year & @Mon & @MDay & "_" & @Hour & @Min & @Sec & @MSec & $SCREENFILE, $a[0], $b[0], $c[0]+$a[0], $d[0]+$b[0], False)
	
	If @error = 0 Then
		TrayTip("Captures", "Capture d'écran faites.", -1, 1)
	Else
		TrayTip("Captures", "Erreur de sauvegarde de la fenêtre d'écran !", -1, 3)
	EndIf
	
EndFunc
; --------------------------------------------------------------


; --------------------------------------------------------------
Func __Screen_Cut()

	GUISetState(@SW_HIDE, $Gui_Main)

	$a = DLLCall("user32.dll", "int", "GetSystemMetrics", "int", 76)
	$b = DLLCall("user32.dll", "int", "GetSystemMetrics", "int", 77)
	$c = DLLCall("user32.dll", "int", "GetSystemMetrics", "int", 78)
	$d = DLLCall("user32.dll", "int", "GetSystemMetrics", "int", 79)
	
	; ---------------
	$Gui_ToCut = GUICreate("", $c[0], $d[0], $a[0], $b[0], 0x80000000)
		GUISetBkColor(0x000000)
		
	$Gui_ToCut_Label_ToGet = GUICtrlCreateLabel("", 0, 0, $c[0], $d[0])
		GUICtrlSetTip(-1, "Cliquez pour sélectionnez le premier point de coupe.", "", 1, 1)
		GUICtrlSetCursor(-1, 3)

	GUISetState(@SW_SHOW, $Gui_ToCut)
	WinSetTrans($Gui_ToCut, "", 50)
	WinActivate($Gui_ToCut)
	; ---------------
	
	Do
	Until GUIGetMsg() = $Gui_ToCut_Label_ToGet
	$MousePose1 = MouseGetPos()
	
	GUICtrlSetTip($Gui_ToCut_Label_ToGet, "Cliquez pour sélectionnez le deuxième point de coupe.", "", 1, 1)
	
	Do
	Until GUIGetMsg() = $Gui_ToCut_Label_ToGet
	$MousePose2 = MouseGetPos()
	
	
	If $MousePose1[0] < $MousePose2[0] Then
		$MousePose1_X = $MousePose1[0]
		$MousePose2_X = $MousePose2[0]
		$GuiPosX = $MousePose1[0]
	Else
		$MousePose1_X = $MousePose2[0]
		$MousePose2_X = $MousePose1[0]
		$GuiPosX = $MousePose2[0]
	EndIf
	
	If $MousePose1[1] < $MousePose2[1] Then
		$MousePose1_Y = $MousePose1[1]
		$MousePose2_Y = $MousePose2[1]
		$GuiPosY = $MousePose1[1]
	Else
		$MousePose1_Y = $MousePose2[1]
		$MousePose2_Y = $MousePose1[1]
		$GuiPosY = $MousePose2[1]
	EndIf
	
	; ---------------
	$Gui_ToCut2 = GUICreate("", $MousePose2_X-$MousePose1_X, $MousePose2_Y-$MousePose1_Y, $GuiPosX, $GuiPosY, 0x00800000+0x80000000)
		GUISetBkColor(0xFFFFFF)
	
	GUISetState(@SW_SHOW, $Gui_ToCut2)
	WinSetTrans($Gui_ToCut2, "", 150)
	Sleep(500)
	; ---------------
	
	GUIDelete($Gui_ToCut2)
	GUIDelete($Gui_ToCut)
	
	DirCreate($SCREENFOLDER)
	_ScreenCapture_Capture($SCREENFOLDER & "\" & @Year & @Mon & @MDay & "_" & @Hour & @Min & @Sec & @MSec & $SCREENFILE, $MousePose1_X, $MousePose1_Y, $MousePose2_X, $MousePose2_Y, False)
	
	If @error = 0 Then
		TrayTip("Captures", "Découpe de la capture d'écran faites.", -1, 1)
	Else
		TrayTip("Captures", "Erreur de sauvegarde de la découpe de la capture d'écran !", -1, 3)
	EndIf

EndFunc
; --------------------------------------------------------------

#EndRegion Function
; ###################################################################################
; ===================================================================================



