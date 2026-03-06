; Inno Setup script
[Setup]
AppId={{A0E7A245-5C3E-47D7-B3FC-3A3BC9B4783A}
AppName=AceitesPro Facturación
AppVersion=1.0.0
AppPublisher=AceitesPro
DefaultDirName={autopf}\AceitesPro Facturacion
DefaultGroupName=AceitesPro Facturacion
OutputDir=..\Embotelladora.Facturacion.Desktop\bin\Release\Installer
OutputBaseFilename=AceitesPro-Facturacion-Setup
Compression=lzma
SolidCompression=yes
WizardStyle=modern
ArchitecturesInstallIn64BitMode=x64compatible

[Languages]
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"

[Files]
Source: "..\Embotelladora.Facturacion.Desktop\bin\Release\net9.0-windows\win-x64\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\AceitesPro Facturación"; Filename: "{app}\Embotelladora.Facturacion.Desktop.exe"
Name: "{autodesktop}\AceitesPro Facturación"; Filename: "{app}\Embotelladora.Facturacion.Desktop.exe"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "Crear acceso directo en el escritorio"; GroupDescription: "Accesos directos:"; Flags: unchecked

[Run]
Filename: "{app}\Embotelladora.Facturacion.Desktop.exe"; Description: "Ejecutar AceitesPro Facturación"; Flags: nowait postinstall skipifsilent
