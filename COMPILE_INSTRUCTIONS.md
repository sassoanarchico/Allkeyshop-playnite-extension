# Istruzioni per Compilare e Creare il File .pext

## Passo 1: Verificare Playnite SDK

Lo script di build cerca automaticamente Playnite SDK nel percorso standard:
- `%LOCALAPPDATA%\Playnite\Playnite.SDK.dll`

Se Playnite è installato in un percorso diverso, puoi impostare la variabile d'ambiente `PlaynitePath`:

### Windows PowerShell (Sessione Corrente)
```powershell
$env:PlaynitePath = "C:\Path\To\Playnite"
```

### Windows PowerShell (Permanente)
```powershell
[System.Environment]::SetEnvironmentVariable('PlaynitePath', 'C:\Path\To\Playnite', 'User')
```

### Windows CMD
```cmd
set PlaynitePath=C:\Path\To\Playnite
```

**Nota:** Se Playnite è installato nel percorso standard (`%LOCALAPPDATA%\Playnite`), non è necessario impostare la variabile.

## Passo 2: Compilare il Progetto

### Opzione A: Usa lo Script Batch (Raccomandato per Windows)

Esegui semplicemente:
```batch
build.bat
```

Oppure fai doppio click su `build.bat` nel file explorer.

### Opzione B: Usa lo Script PowerShell

```powershell
.\build.ps1
```

Questi script:
1. Verificano che PlaynitePath sia impostato (o lo trovano automaticamente)
2. Compilano il progetto in modalità Release
3. Creano la struttura dell'estensione
4. Generano il file `.pext` nella stessa directory dello script

### Opzione C: Compilazione Manuale

1. **Ripristina i pacchetti NuGet:**
   ```powershell
   dotnet restore AllKeyShopExtension.csproj
   ```

2. **Compila il progetto:**
   ```powershell
   dotnet build AllKeyShopExtension.csproj -c Release
   ```

3. **Crea il file .pext:**
   ```powershell
   .\create-pext.ps1
   ```

## Passo 3: Verificare il File .pext

Dopo la compilazione, dovresti avere un file chiamato:
- `AllKeyShopExtension-0.1.0.pext`

Questo file può essere installato direttamente in Playnite.

## Struttura del File .pext

Un file `.pext` è essenzialmente un file ZIP con questa struttura:

```
AllKeyShopExtension/
├── extension.yaml          # Metadati dell'estensione
├── AllKeyShopExtension.dll # DLL principale
├── HtmlAgilityPack.dll     # Dipendenza
├── Newtonsoft.Json.dll     # Dipendenza
└── System.Data.SQLite.*.dll # Dipendenze SQLite
```

## Installazione in Playnite

1. Apri Playnite
2. Vai a `Menu > Estensioni > Installa estensione`
3. Seleziona il file `AllKeyShopExtension-0.1.0.pext`
4. Riavvia Playnite
5. L'estensione dovrebbe apparire nelle impostazioni

## Risoluzione Problemi

### Errore: "Playnite.SDK.dll non trovato"

**Causa:** Playnite SDK non è stato trovato nel percorso standard o la variabile `PlaynitePath` non è impostata correttamente.

**Soluzione:**
1. Verifica che Playnite sia installato
2. Controlla se il file esiste in `%LOCALAPPDATA%\Playnite\Playnite.SDK.dll`
3. Se Playnite è installato altrove, trova il percorso di installazione
4. Imposta la variabile `PlaynitePath` con il percorso corretto (se necessario)
5. Verifica che il file `Playnite.SDK.dll` esista in quella directory

### Errore: "Impossibile risolvere il riferimento"

**Causa:** I pacchetti NuGet non sono stati scaricati.

**Soluzione:**
```powershell
dotnet restore AllKeyShopExtension.csproj
```

### Errore di Compilazione

**Causa:** Potrebbero esserci errori nel codice o dipendenze mancanti.

**Soluzione:**
1. Controlla i log di compilazione per dettagli
2. Verifica di avere .NET SDK 6.0 o superiore
3. Assicurati che tutti i pacchetti NuGet siano stati scaricati

## Verifica della Compilazione

Dopo una compilazione riuscita, dovresti vedere:

```
✓ Build completato con successo!
File creato: AllKeyShopExtension-0.1.0.pext
```

Il file `.pext` sarà nella directory principale del progetto.
