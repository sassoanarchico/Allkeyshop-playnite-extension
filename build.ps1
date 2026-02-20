# Script unificato di build per AllKeyShop Extension
# Compila il progetto e crea il file .pext nella stessa directory dello script

param(
    [string]$Configuration = "Release",
    [string]$PlaynitePath = $null,
    [string]$Version = $null
)

# Ottieni la directory dello script
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptDir

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  AllKeyShop Extension Build Script" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Leggi la versione da extension.yaml se non specificata
if ([string]::IsNullOrEmpty($Version)) {
    if (Test-Path "extension.yaml") {
        $yamlContent = Get-Content "extension.yaml" -Raw
        if ($yamlContent -match "Version:\s*([0-9]+\.[0-9]+\.[0-9]+)") {
            $Version = $matches[1]
            Write-Host "Versione letta da extension.yaml: $Version" -ForegroundColor Cyan
        } else {
            $Version = "0.1.1"
            Write-Host "Versione non trovata in extension.yaml, uso default: $Version" -ForegroundColor Yellow
        }
    } else {
        $Version = "0.1.1"
        Write-Host "extension.yaml non trovato, uso versione default: $Version" -ForegroundColor Yellow
    }
}

$extensionName = "AllKeyShopExtension"
$pextFile = "$extensionName-$Version.pext"

Write-Host "Nome file .pext: $pextFile" -ForegroundColor Cyan
Write-Host ""

# Determina il percorso di Playnite
Write-Host "`n[1/4] Verificando Playnite SDK..." -ForegroundColor Green
if ([string]::IsNullOrEmpty($PlaynitePath)) {
    # Prova prima con LOCALAPPDATA (percorso standard)
    $localAppDataPath = Join-Path $env:LOCALAPPDATA "Playnite"
    if (Test-Path (Join-Path $localAppDataPath "Playnite.SDK.dll")) {
        $PlaynitePath = $localAppDataPath
        Write-Host "  [OK] Trovato Playnite SDK in: $PlaynitePath" -ForegroundColor Cyan
    } elseif (-not [string]::IsNullOrEmpty($env:PlaynitePath)) {
        # Usa la variabile d'ambiente se impostata
        $PlaynitePath = $env:PlaynitePath
        Write-Host "  [OK] Usando PlaynitePath dalla variabile d'ambiente: $PlaynitePath" -ForegroundColor Cyan
    } else {
        Write-Host "  [ERRORE] Playnite.SDK.dll non trovato!" -ForegroundColor Red
        Write-Host "  Cercato in: $localAppDataPath" -ForegroundColor Yellow
        Write-Host "  Imposta la variabile con: `$env:PlaynitePath = 'C:\Path\To\Playnite'" -ForegroundColor Yellow
        exit 1
    }
}

if (-not (Test-Path (Join-Path $PlaynitePath "Playnite.SDK.dll"))) {
    Write-Host "  [ERRORE] Playnite.SDK.dll non trovato in $PlaynitePath" -ForegroundColor Red
    exit 1
}

# Compila il progetto
Write-Host "`n[2/4] Compilando il progetto..." -ForegroundColor Green
$env:PlaynitePath = $PlaynitePath
dotnet build AllKeyShopExtension.csproj -c $Configuration /p:PlaynitePath="$PlaynitePath" --no-incremental

if ($LASTEXITCODE -ne 0) {
    Write-Host "  [ERRORE] Compilazione fallita!" -ForegroundColor Red
    exit 1
}

Write-Host "  [OK] Compilazione completata" -ForegroundColor Cyan

# Prepara la directory per il .pext
Write-Host "`n[3/4] Preparando file per il .pext..." -ForegroundColor Green

$outputDir = "bin\$Configuration"
$tempDir = "temp_pext_$([Guid]::NewGuid().ToString().Substring(0,8))"
New-Item -ItemType Directory -Force -Path $tempDir | Out-Null

try {
    # Copia extension.yaml
    if (Test-Path "extension.yaml") {
        Copy-Item "extension.yaml" -Destination $tempDir -Force
        Write-Host "  [OK] extension.yaml copiato" -ForegroundColor Cyan
    } else {
        Write-Host "  [AVVISO] extension.yaml non trovato" -ForegroundColor Yellow
    }

    # Cerca e copia le DLL compilate
    $dllFiles = @(
        "AllKeyShopExtension.dll",
        "HtmlAgilityPack.dll",
        "Newtonsoft.Json.dll",
        "System.Data.SQLite.dll"
    )

    $foundDlls = 0
    foreach ($dll in $dllFiles) {
        # Cerca prima nella directory di output
        $sourcePath = Join-Path $outputDir $dll
        if (-not (Test-Path $sourcePath)) {
            # Cerca ricorsivamente nella directory di output
            $found = Get-ChildItem -Path $outputDir -Filter $dll -Recurse -ErrorAction SilentlyContinue | Select-Object -First 1
            if ($found) {
                $sourcePath = $found.FullName
            } else {
                # Cerca nei pacchetti NuGet
                $nugetPath = Get-ChildItem -Path "$env:USERPROFILE\.nuget\packages" -Recurse -Filter $dll -ErrorAction SilentlyContinue | Select-Object -First 1
                if ($nugetPath) {
                    $sourcePath = $nugetPath.FullName
                }
            }
        }

        if (Test-Path $sourcePath) {
            Copy-Item $sourcePath -Destination $tempDir -Force
            Write-Host "  [OK] $dll copiato" -ForegroundColor Cyan
            $foundDlls++
        } else {
            Write-Host "  [AVVISO] $dll non trovato" -ForegroundColor Yellow
        }
    }

    # Cerca anche le DLL SQLite native (x64/x86)
    $sqliteNative = Get-ChildItem -Path $outputDir -Filter "SQLite.Interop.dll" -Recurse -ErrorAction SilentlyContinue
    foreach ($native in $sqliteNative) {
        $relativePath = $native.FullName.Substring((Resolve-Path $outputDir).Path.Length + 1)
        $destPath = Join-Path $tempDir $relativePath
        $destDir = Split-Path -Parent $destPath
        if (-not (Test-Path $destDir)) {
            New-Item -ItemType Directory -Force -Path $destDir | Out-Null
        }
        Copy-Item $native.FullName -Destination $destPath -Force
        Write-Host "  [OK] $relativePath copiato" -ForegroundColor Cyan
    }

    if ($foundDlls -eq 0) {
        Write-Host "  [ERRORE] Nessuna DLL trovata!" -ForegroundColor Red
        exit 1
    }

    # Crea il file .pext (zip) nella directory dello script
    Write-Host "`n[4/4] Creando file .pext..." -ForegroundColor Green
    
    if (Test-Path $pextFile) {
        Remove-Item $pextFile -Force
        Write-Host "  [INFO] File esistente rimosso" -ForegroundColor Yellow
    }

    Add-Type -AssemblyName System.IO.Compression.FileSystem
    [System.IO.Compression.ZipFile]::CreateFromDirectory($tempDir, $pextFile)

    $fileSize = [math]::Round((Get-Item $pextFile).Length / 1KB, 2)
    Write-Host "  [OK] File .pext creato: $pextFile ($fileSize KB)" -ForegroundColor Cyan
    Write-Host "  [INFO] Percorso completo: $(Resolve-Path $pextFile)" -ForegroundColor Gray

} finally {
    # Rimuovi directory temporanea
    if (Test-Path $tempDir) {
        Remove-Item $tempDir -Recurse -Force
    }
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "  Build completato con successo!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "`nFile .pext creato in: $(Resolve-Path $pextFile)" -ForegroundColor Cyan
Write-Host ""
