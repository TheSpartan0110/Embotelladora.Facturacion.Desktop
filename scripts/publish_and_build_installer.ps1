# PowerShell script to publish the app (x64/x86) and build Inno Setup installer
# Run this from an elevated PowerShell if needed. Adjust paths as necessary.

$solutionDir = "C:\Users\Crist\source\repos\Embotelladora.Facturacion.Desktop"
$projectDir = Join-Path $solutionDir "Embotelladora.Facturacion.Desktop"
$proj = Join-Path $projectDir "Embotelladora.Facturacion.Desktop.csproj"

$outRoot = Join-Path $solutionDir "publish"
$outX64 = Join-Path $outRoot "x64"
$outX86 = Join-Path $outRoot "x86"
$outFramework = Join-Path $outRoot "framework"
$installerDir = Join-Path $solutionDir "installer"
$setupIssPath = Join-Path $installerDir "setup.iss"
$outputInstallerDir = Join-Path $installerDir "Output"
$outputBaseFilename = "AceitesPro_Setup"

# Ensure directories
New-Item -ItemType Directory -Force -Path $outX64, $outX86, $outFramework, $installerDir, $outputInstallerDir | Out-Null

Write-Host "Project: $proj"
Write-Host "Publishing..."

# Publish x64 self-contained single-file
dotnet publish `"$proj`" -c Release -r win-x64 -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:PublishTrimmed=false -o `"$outX64`"
if ($LASTEXITCODE -ne 0) { Write-Error "dotnet publish x64 failed"; exit $LASTEXITCODE }

# Publish x86 self-contained single-file
dotnet publish `"$proj`" -c Release -r win-x86 -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:PublishTrimmed=false -o `"$outX86`"
if ($LASTEXITCODE -ne 0) { Write-Error "dotnet publish x86 failed"; exit $LASTEXITCODE }

 # Publish framework-dependent (optional)
 dotnet publish `"$proj`" -c Release -f net9.0-windows -o `"$outFramework`"

# Copy additional resources (if present)
$resources = @(
    "invoice_settings.json",
    "ManualUsuario.html",
    "AppDatabase.db",
    "Embotelladora.Facturacion.Desktop.db"
)
foreach ($res in $resources) {
    $src = Join-Path $projectDir $res
    if (Test-Path $src) {
        Copy-Item -Path $src -Destination $outX64 -Force -Recurse
        Copy-Item -Path $src -Destination $outX86 -Force -Recurse
        Write-Host "Copied $res"
    }
}

# Rename executables to AceitesPro.exe
if (Test-Path "$outX64\AceitesPro.exe") { Remove-Item "$outX64\AceitesPro.exe" }
Rename-Item -Path "$outX64\Embotelladora.Facturacion.Desktop.exe" -NewName "AceitesPro.exe" -Force
if (Test-Path "$outX86\AceitesPro.exe") { Remove-Item "$outX86\AceitesPro.exe" }
Rename-Item -Path "$outX86\Embotelladora.Facturacion.Desktop.exe" -NewName "AceitesPro.exe" -Force

# If you want to include a specific DB path used by the app, copy it manually or adjust above.

# Remove old installer if exists
if (Test-Path $outputInstallerDir\$outputBaseFilename.exe) { Remove-Item $outputInstallerDir\$outputBaseFilename.exe }

# Ensure setup.iss is present in installer folder
if (-not (Test-Path $setupIssPath)) {
    Write-Error "setup.iss not found at $setupIssPath. Create or adjust installer/setup.iss and re-run this script.";
    exit 1
}

# Locate Inno ISCC.exe
 $defaultIscc = "C:\Program Files (x86)\Inno Setup 6\ISCC.exe"
$iscc = $env:ISCC_PATH
if ([string]::IsNullOrWhiteSpace($iscc)) { $iscc = $defaultIscc }

if (-not (Test-Path $iscc)) {
    Write-Error "Inno Setup compiler not found at '$iscc'. Set ISCC_PATH environment variable or install Inno Setup.";
    exit 2
}

Write-Host "Building installer with ISCC: $iscc"
 & $iscc $setupIssPath
if ($LASTEXITCODE -ne 0) { Write-Error "ISCC failed"; exit $LASTEXITCODE }

Write-Host "Installer created in: $outputInstallerDir"
Write-Host "Publish and build completed successfully."

# End of script
