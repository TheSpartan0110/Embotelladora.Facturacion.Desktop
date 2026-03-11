; Inno Setup script generated for AceitesPro Facturación
; Adjust paths and filenames as needed

[Setup]
AppName=AceitesPro Facturación
AppVersion=1.0.0
DefaultDirName={autopf}\AceitesPro Facturación
DefaultGroupName=AceitesPro Facturación
OutputDir={#SourcePath}\Output
OutputBaseFilename=AceitesPro_Setup
Compression=lzma2
SolidCompression=yes

[Files]
; Include published x64 files (single-file exe). Change Source path if you prefer x86 or framework build
Source: "C:\Users\Crist\source\repos\Embotelladora.Facturacion.Desktop\publish\x64\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

; If you included a DB file and want to install it only if it doesn't exist on target machine:
; Source: "C:\Users\Crist\source\repos\Embotelladora.Facturacion.Desktop\publish\x64\AppDatabase.db"; DestDir: "{app}"; Flags: onlyifdoesntexist

[Icons]
Name: "{group}\AceitesPro Facturación"; Filename: "{app}\AceitesPro.exe"
Name: "{commondesktop}\AceitesPro Facturación"; Filename: "{app}\AceitesPro.exe"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "Crear acceso directo en el escritorio"

[Run]
; Run after install (disabled by default). Uncomment to launch app after install
; Filename: "{app}\Embotelladora.Facturacion.Desktop.exe"; Description: "Iniciar AceitesPro Facturación"; Flags: nowait postinstall skipifsilent

[Code]
// Add any custom Pascal Script code here if needed
