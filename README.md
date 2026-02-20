AllKeyShop Playnite Extension
=============================

Estensione per Playnite che monitora i prezzi dei giochi da AllKeyShop, ti permette di creare una lista di giochi da tenere d'occhio e ti notifica quando escono nuovi giochi gratis sulle piattaforme supportate.

**Versione:** 0.1.1  
**Tipo:** Generic Plugin (app nella barra laterale)  
**ID:** `c1d2e3f4-a5b6-7890-cdef-1234567890ab`

## Funzionalità Attuali

### 1. App laterale AllKeyShop (Sidebar View)
- Mostra una vera e propria "app" nella barra laterale di Playnite.
- Quando clicchi sull'icona laterale, la schermata principale di Playnite viene sostituita con la vista `PriceMonitorView`.
- Dalla vista puoi:
  - **Vedere tutti i giochi monitorati**, con:
    - Nome gioco
    - Ultimo prezzo trovato
    - Venditore
    - Data/ora dell'ultimo aggiornamento
  - **Aggiungere nuovi giochi** alla lista di monitoraggio tramite il pulsante "Aggiungi Gioco".
  - **Aggiornare manualmente** i prezzi tramite il pulsante "Aggiorna".
  - **Rimuovere** un gioco dalla lista tramite il pulsante "Rimuovi".
  - **Aprire la pagina AllKeyShop** del gioco con doppio click sulla riga.

### 2. Monitoraggio Prezzi (background)
- Salva in un database locale SQLite la lista dei giochi monitorati.
- Aggiorna i prezzi periodicamente usando un `DispatcherTimer`.
- L'intervallo di aggiornamento è configurabile dalle impostazioni dell'estensione.
- Effettua uno **scraping** di AllKeyShop per trovare il prezzo migliore disponibile.

### 3. Notifiche Giochi Gratis
- Controlla periodicamente se sono disponibili **nuovi giochi gratis** su piattaforme supportate.
- Invia una notifica Playnite quando vengono trovati nuovi titoli gratuiti.
- Usa la `NotificationService` interna per mostrare notifiche localizzate.

### 4. Impostazioni Estensione
- Pagina di impostazioni dedicata accessibile da:
  - `Menu → Estensioni → Impostazioni estensioni → AllKeyShop Price Monitor`
- Possibilità di:
  - Abilitare/disabilitare le notifiche generali.
  - Abilitare/disabilitare gli alert di soglia prezzo.
  - Configurare:
    - Intervallo aggiornamento prezzi (in minuti).
    - Intervallo controllo giochi gratis (in minuti).
  - Selezionare le **piattaforme** da monitorare per i giochi gratis (Steam, Epic, GOG, Xbox, PlayStation, ecc.).
  - Aprire la vista di monitoraggio prezzi direttamente dalle impostazioni tramite il pulsante "Apri Monitor Prezzi".

## Installazione

### Metodo 1: File .pext (Raccomandato)

1. Scarica il file `AllKeyShopExtension-0.1.1.pext` dalla pagina delle release.
2. Apri Playnite.
3. Vai a `Menu → Estensioni → Installa estensione`.
4. Seleziona il file `.pext`.
5. Riavvia Playnite.

### Metodo 2: Installazione Manuale (sviluppo)

1. Compila il progetto:
   - Esegui `build.bat` (Windows) oppure `.\build.ps1` (PowerShell).
   - Vedi [BUILD.md](BUILD.md) per dettagli.
2. Copia la cartella `AllKeyShopExtension` nella cartella delle estensioni di Playnite:
   - `%AppData%\Playnite\Extensions\`
3. Verifica che in `extension.yaml`:
   - `Id` sia `c1d2e3f4-a5b6-7890-cdef-1234567890ab`.
   - `Type` sia `GenericPlugin`.
4. Riavvia Playnite.
5. L'estensione dovrebbe comparire:
   - Nelle impostazioni estensioni.
   - Nella barra laterale come app "AllKeyShop".

## Requisiti

- Playnite 10.x o superiore.
- .NET Framework 4.8.
- Connessione internet per lo scraping di AllKeyShop.

## Utilizzo

### Aprire l'app laterale AllKeyShop

- In Playnite, clicca sull'icona **AllKeyShop** nella barra laterale.
- Si aprirà la vista interna con la lista dei giochi monitorati.

### Aggiungere Giochi da Monitorare

1. Apri l'app laterale AllKeyShop (o la vista Price Monitor).
2. Clicca su **"Aggiungi Gioco"**.
3. Inserisci il nome del gioco così come vorresti cercarlo su AllKeyShop.
4. L'estensione aggiungerà il gioco al database locale e lancerà subito un aggiornamento del prezzo.

### Aggiornare i Prezzi Manualmente

1. Dalla vista Price Monitor clicca su **"Aggiorna"**.
2. L'estensione aggiornerà i prezzi per tutti i giochi monitorati.

### Configurare Notifiche Giochi Gratis

1. Vai alle impostazioni dell'estensione:
   - `Menu → Estensioni → Impostazioni estensioni → AllKeyShop Price Monitor`.
2. Seleziona le piattaforme da monitorare.
3. Configura gli intervalli di controllo.
4. Abilita le notifiche.

## Struttura Progetto

```
AllKeyShopExtension/
├── Models/              # Modelli dati (giochi monitorati, settings, ecc.)
├── Services/            # Logica di business (scraping, prezzi, notifiche)
├── Data/                # Accesso al database SQLite
├── Views/               # Viste WPF (settings, price monitor, ecc.)
├── Utilities/           # Helper utilities (HTTP, ecc.)
├── Plugin.cs            # Classe principale GenericPlugin
├── AssemblyInfo.cs
└── extension.yaml       # Manifest dell'estensione per Playnite
```

## Sviluppo

Per compilare il progetto, consulta [BUILD.md](BUILD.md).

### Dipendenze

- HtmlAgilityPack (scraping HTML).
- Newtonsoft.Json (serializzazione JSON).
- System.Data.SQLite.Core (database locale).
- Playnite.SDK (SDK Playnite).

## Note

- Lo scraping di AllKeyShop è soggetto a rate limiting per evitare ban.
- I dati sono salvati localmente in un database SQLite.
- Le notifiche utilizzano il sistema di notifiche di Playnite.
- I selettori HTML potrebbero dover essere aggiornati se AllKeyShop cambia la struttura.

## Changelog

Vedi [CHANGELOG.md](CHANGELOG.md) per la lista completa delle modifiche.

## Licenza

MIT License.

## Supporto

Per problemi o domande, apri una issue sul repository del progetto.
