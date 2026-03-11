Uso rápido para generar ejecutables e instalador

1) Requisitos:
- .NET 9 SDK instalado
- Inno Setup instalado (por defecto: C:\Program Files (x86)\Inno Setup 6\ISCC.exe)

2) Pasos automáticos (PowerShell):
- Abrir PowerShell y ejecutar:
  cd C:\Users\Crist\source\repos\Embotelladora.Facturacion.Desktop
  .\scripts\publish_and_build_installer.ps1

3) Resultado:
- Ejecutables publicados:
  publish\x64\
  publish\x86\
- Instalador generado en:
  installer\Output\AceitesPro_Setup.exe

4) Notas:
- Si ISCC no está en la ruta por defecto, exporta variable ISCC_PATH con la ruta a ISCC.exe:
  $env:ISCC_PATH = "C:\Program Files (x86)\Inno Setup 6\ISCC.exe"

- Edita installer\setup.iss si necesitas cambiar qué archivos incluir o el nombre del exe.
- Revisa riesgos de incluir base de datos en el instalador (puede sobrescribir datos de usuario). Para no sobrescribir, usa flag 'onlyifdoesntexist' en la sección [Files].

5) Soporte:
- Si quieres, adapto el script para publicar sólo x64, o para firmar los ejecutables antes de crear el instalador.
