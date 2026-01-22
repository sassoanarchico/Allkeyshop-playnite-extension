# Istruzioni per la Compilazione

## Prerequisiti

1. **Playnite SDK**: Assicurati di avere Playnite installato e di conoscere il percorso di installazione
2. **.NET SDK**: Versione 6.0 o superiore
3. **PowerShell**: Per eseguire lo script di build

## Configurazione

Lo script di build cerca automaticamente Playnite SDK nel percorso standard:
- `%LOCALAPPDATA%\Playnite\Playnite.SDK.dll`

Se Playnite è installato in un percorso diverso, puoi impostare la variabile d'ambiente `PlaynitePath`:

**Windows PowerShell:**
```powershell
$env:PlaynitePath = "C:\Path\To\Playnite"
```

**Windows CMD:**
```cmd
set PlaynitePath=C:\Path\To\Playnite
```

**Permanente (PowerShell):**
```powershell
[System.Environment]::SetEnvironmentVariable('PlaynitePath', 'C:\Path\To\Playnite', 'User')
```

## Compilazione

### Metodo 1: Script Batch (Raccomandato per Windows)

Esegui semplicemente:
```batch
build.bat
```

Oppure fai doppio click su `build.bat` nel file explorer.

### Metodo 2: Script PowerShell

```powershell
.\build.ps1
```

### Metodo 3: Manuale

1. Imposta la variabile d'ambiente `PlaynitePath`:
   ```powershell
   $env:PlaynitePath = "C:\Program Files\Playnite"
   ```

2. Compila il progetto:
   ```powershell
   dotnet build AllKeyShopExtension.csproj -c Release
   ```

3. Crea il file .pext manualmente:
   - Copia tutti i file necessari in una cartella
   - Comprimi la cartella in un file .zip
   - Rinomina l'estensione da .zip a .pext

## Struttura File .pext

Un file .pext è essenzialmente uno zip con questa struttura:

```
AllKeyShopExtension/
├── extension.yaml
├── AllKeyShopExtension.dll
├── HtmlAgilityPack.dll
├── Newtonsoft.Json.dll
├── System.Data.SQLite.dll
└── (altre dipendenze)
```

## Installazione

1. Copia il file `.pext` nella cartella delle estensioni di Playnite:
   - `%AppData%\Playnite\Extensions\`
   
2. Riavvia Playnite

3. L'estensione dovrebbe apparire nelle impostazioni

## Risoluzione Problemi

### Errore: "Playnite.SDK.dll non trovato"
- Verifica che la variabile `PlaynitePath` sia impostata correttamente
- Verifica che Playnite sia installato nel percorso specificato
- Assicurati che il file `Playnite.SDK.dll` esista in quella directory

### Errore: "Impossibile risolvere il riferimento"
- Esegui `dotnet restore` prima di compilare
- Verifica che tutti i pacchetti NuGet siano stati scaricati

### Errore di compilazione
- Verifica di avere .NET SDK 6.0 o superiore
- Controlla i log di compilazione per dettagli
