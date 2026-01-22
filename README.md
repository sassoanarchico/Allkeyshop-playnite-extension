# AllKeyShop Playnite Extension

Estensione per Playnite che monitora i prezzi dei giochi da AllKeyShop e notifica l'utente sui giochi gratis disponibili.

**Versione:** 0.1.0

## Funzionalità

### 1. Monitoraggio Prezzi
- Aggiungi giochi da monitorare nella vista principale
- Visualizza il prezzo più basso trovato su AllKeyShop
- Aggiornamento automatico dei prezzi a intervalli configurabili
- Click su un gioco per aprire la pagina AllKeyShop

### 2. Notifiche Giochi Gratis
- Ricevi notifiche quando vengono trovati nuovi giochi gratis
- Configura le piattaforme da monitorare (Steam, Epic, GOG, Xbox, PlayStation, etc.)
- Intervallo di controllo configurabile

### 3. Impostazioni
- Abilita/disabilita notifiche
- Configura intervalli di aggiornamento
- Seleziona piattaforme per notifiche giochi gratis

## Installazione

### Metodo 1: File .pext (Raccomandato)

1. Scarica il file `AllKeyShopExtension-0.1.0.pext`
2. Apri Playnite
3. Vai a `Menu > Estensioni > Installa estensione`
4. Seleziona il file `.pext`
5. Riavvia Playnite

### Metodo 2: Installazione Manuale

1. Compila il progetto:
   - Esegui `build.bat` (Windows) oppure `.\build.ps1` (PowerShell)
   - Vedi [BUILD.md](BUILD.md) per dettagli
2. Copia la cartella `AllKeyShopExtension` nella cartella delle estensioni di Playnite:
   - `%AppData%\Playnite\Extensions\`
3. Riavvia Playnite
4. L'estensione dovrebbe apparire nelle impostazioni

## Requisiti

- Playnite (versione 10.x o superiore)
- .NET Framework 4.8
- Connessione internet per lo scraping di AllKeyShop

## Utilizzo

### Aprire la Vista Prezzi

Per aprire la finestra di monitoraggio prezzi:
- L'estensione può essere configurata per aprire automaticamente la vista
- Oppure puoi aggiungere un menu item personalizzato in Playnite
- La vista mostra tutti i giochi che stai monitorando con i relativi prezzi

### Aggiungere Giochi da Monitorare

1. Apri la vista Price Monitor
2. Clicca su "Aggiungi Gioco"
3. Inserisci il nome del gioco
4. Il prezzo verrà aggiornato automaticamente

### Configurare Notifiche Giochi Gratis

1. Vai alle Impostazioni dell'estensione
2. Seleziona le piattaforme da monitorare
3. Configura l'intervallo di controllo
4. Abilita le notifiche

## Struttura Progetto

```
AllKeyShopExtension/
├── Models/              # Modelli dati
├── Services/            # Servizi business logic
├── Data/                # Database SQLite
├── Views/               # Viste WPF
├── Utilities/           # Helper utilities
├── AllKeyShopExtension.cs
├── Plugin.cs
└── extension.yaml
```

## Sviluppo

Per compilare il progetto, consulta [BUILD.md](BUILD.md).

### Dipendenze

- HtmlAgilityPack (scraping HTML)
- Newtonsoft.Json (serializzazione JSON)
- System.Data.SQLite.Core (database locale)
- Playnite.SDK (SDK Playnite)

## Note

- Lo scraping di AllKeyShop è soggetto a rate limiting per evitare ban
- I dati sono salvati localmente in un database SQLite
- Le notifiche utilizzano il sistema di notifiche di Playnite
- I selettori HTML potrebbero dover essere aggiornati se AllKeyShop cambia la struttura

## Changelog

Vedi [CHANGELOG.md](CHANGELOG.md) per la lista completa delle modifiche.

## Licenza

MIT License

## Supporto

Per problemi o domande, apri una issue sul repository del progetto.
