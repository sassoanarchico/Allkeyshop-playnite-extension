# Script per creare il file .pext manualmente
# Usa questo script se hai già compilato il progetto manualmente

param(
    [string]$SourceDir = "bin\Release",
    [string]$Version = "0.1.0"
)

$extensionName = "AllKeyShopExtension"
$pextFile = "$extensionName-$Version.pext"

Write-Host "Creando file .pext: $pextFile" -ForegroundColor Green

# Verifica che la directory esista
if (-not (Test-Path $SourceDir)) {
    Write-Host "ERRORE: Directory $SourceDir non trovata!" -ForegroundColor Red
    Write-Host "Assicurati di aver compilato il progetto prima." -ForegroundColor Yellow
    exit 1
}

# Crea directory temporanea
$tempDir = "temp_pext_$([Guid]::NewGuid().ToString().Substring(0,8))"
New-Item -ItemType Directory -Force -Path $tempDir | Out-Null

try {
    # Copia extension.yaml
    if (Test-Path "extension.yaml") {
        Copy-Item "extension.yaml" -Destination $tempDir -Force
        Write-Host "[OK] extension.yaml copiato" -ForegroundColor Cyan
    } else {
        Write-Host "AVVISO: extension.yaml non trovato" -ForegroundColor Yellow
    }

    # Copia DLL compilate
    $dllFiles = @(
        "AllKeyShopExtension.dll",
        "HtmlAgilityPack.dll",
        "Newtonsoft.Json.dll"
    )

    # Cerca anche le dipendenze SQLite
    $sqliteFiles = Get-ChildItem -Path $SourceDir -Filter "*SQLite*" -Recurse -ErrorAction SilentlyContinue
    foreach ($file in $sqliteFiles) {
        $dllFiles += $file.Name
    }

    foreach ($dll in $dllFiles) {
        $sourcePath = Join-Path $SourceDir $dll
        if (Test-Path $sourcePath) {
            Copy-Item $sourcePath -Destination $tempDir -Force
            Write-Host "[OK] $dll copiato" -ForegroundColor Cyan
        } else {
            # Cerca ricorsivamente
            $found = Get-ChildItem -Path $SourceDir -Filter $dll -Recurse -ErrorAction SilentlyContinue | Select-Object -First 1
            if ($found) {
                Copy-Item $found.FullName -Destination $tempDir -Force
                Write-Host "[OK] $dll copiato (trovato in $($found.DirectoryName))" -ForegroundColor Cyan
            } else {
                Write-Host "AVVISO: $dll non trovato" -ForegroundColor Yellow
            }
        }
    }

    # Crea il file .pext (zip)
    if (Test-Path $pextFile) {
        Remove-Item $pextFile -Force
        Write-Host "File esistente rimosso" -ForegroundColor Yellow
    }

    Add-Type -AssemblyName System.IO.Compression.FileSystem
    [System.IO.Compression.ZipFile]::CreateFromDirectory($tempDir, $pextFile)

    Write-Host "`n[OK] File .pext creato con successo: $pextFile" -ForegroundColor Green
    Write-Host "Dimensione: $([math]::Round((Get-Item $pextFile).Length / 1KB, 2)) KB" -ForegroundColor Cyan

} finally {
    # Rimuovi directory temporanea
    if (Test-Path $tempDir) {
        Remove-Item $tempDir -Recurse -Force
    }
}
