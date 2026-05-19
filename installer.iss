[Setup]
; App Information
AppName=Generic Inventory System
AppVersion=1.0
AppPublisher=Softio
AppPublisherURL=https://softio.com
AppSupportURL=https://softio.com
AppUpdatesURL=https://softio.com

; Default installation folder
DefaultDirName={autopf}\Generic Inventory System
DefaultGroupName=Generic Inventory System

; Output settings
OutputDir=.\InstallerOutput
OutputBaseFilename=GenericInventorySystem_Setup_v1.0

; Compression
Compression=lzma
SolidCompression=yes

; Require admin rights to install to Program Files
PrivilegesRequired=admin

; Setup Icon (Optional - will use default if not specified)
SetupIconFile=Assets\inventory_ico.ico

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"

[Files]
; Main executable
Source: "publish-output\GenericInventorySystem.exe"; DestDir: "{app}"; Flags: ignoreversion

; Configuration file
Source: "publish-output\appsettings.json"; DestDir: "{app}"; Flags: ignoreversion

; Folders
Source: "publish-output\Assets\*"; DestDir: "{app}\Assets"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "publish-output\wwwroot\*"; DestDir: "{app}\wwwroot"; Flags: ignoreversion recursesubdirs createallsubdirs

; Catch any other files in publish-output (like sqlite dlls if any exist outside single-file)
Source: "publish-output\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs; Excludes: "GenericInventorySystem.exe,appsettings.json,Assets,wwwroot,Plugins"

[Dirs]
Name: "{app}"; Permissions: users-modify
Name: "{app}\Plugins"; Permissions: users-modify

[Icons]
; Start Menu Icon
Name: "{group}\Generic Inventory System"; Filename: "{app}\GenericInventorySystem.exe"; IconFilename: "{app}\Assets\inventory_ico.ico"
; Desktop Icon
Name: "{autodesktop}\Generic Inventory System"; Filename: "{app}\GenericInventorySystem.exe"; IconFilename: "{app}\Assets\inventory_ico.ico"; Tasks: desktopicon

[Run]
; Launch application after installation
Filename: "{app}\GenericInventorySystem.exe"; Description: "{cm:LaunchProgram,Generic Inventory System}"; Flags: nowait postinstall skipifsilent
