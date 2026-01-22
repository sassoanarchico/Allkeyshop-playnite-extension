# Informazioni sui Percorsi

## Struttura Repository

La repository è ora organizzata con `AllKeyShopExtension/` come directory principale:

```
AllKeyShopExtension/
├── AllKeyShopExtension.csproj    # File progetto
├── extension.yaml                # Metadata estensione
├── build.bat                     # Script batch per build (Windows)
├── build.ps1                     # Script PowerShell unificato (compila + crea .pext)
├── bin/Release/                  # Output compilazione
├── obj/Release/                  # File temporanei build
└── [altri file sorgente]
```

## Percorso File .pext

Il file `.pext` viene creato nella directory principale della repository:

**Percorso completo:**
```
AllKeyShopExtension/AllKeyShopExtension-0.1.0.pext
```

**Percorso relativo (dalla root repository):**
```
./AllKeyShopExtension-0.1.0.pext
```

## Come Usare gli Script

### build.bat (Raccomandato per Windows)

Esegui dalla directory `AllKeyShopExtension/`:

```batch
build.bat
```

Oppure fai doppio click su `build.bat` nel file explorer.

### build.ps1 (PowerShell)

Esegui dalla directory `AllKeyShopExtension/`:

```powershell
cd AllKeyShopExtension
.\build.ps1
```

**Entrambi gli script:**
1. Verificano/rilevano automaticamente Playnite SDK
2. Compilano il progetto in `bin/Release/`
3. Raccolgono tutte le DLL necessarie (incluse dipendenze NuGet)
4. Creano il file `.pext` nella stessa directory dello script: `AllKeyShopExtension-0.1.0.pext`

**Nota:** Lo script legge automaticamente la versione da `extension.yaml`, quindi il nome del file `.pext` sarà sempre aggiornato.

## Installazione

Dopo aver creato il file `.pext`, copialo in:

```
%AppData%\Playnite\Extensions\
```

Esempio:
```
C:\Users\TuoNome\AppData\Roaming\Playnite\Extensions\AllKeyShopExtension-0.1.0.pext
```
