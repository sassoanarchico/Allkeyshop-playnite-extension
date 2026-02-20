# Changelog

Tutte le modifiche notevoli a questo progetto saranno documentate in questo file.

Il formato è basato su [Keep a Changelog](https://keepachangelog.com/it/1.0.0/),
e questo progetto aderisce a [Semantic Versioning](https://semver.org/lang/it/).

## [0.2.4] - 2025-07-24

### Aggiunto
- **Modifica soglia prezzo**: Aggiunto pulsante ✏ (modifica) nella colonna Azioni della tabella giochi monitorati. Cliccando si apre una finestra di dialogo per modificare la soglia di prezzo di un gioco. Supporta salvataggio, cancellazione soglia ("Cancella") e annullamento.
- **Notifiche Windows toast**: Le notifiche di alert prezzo, giochi gratis e aggiornamento prezzi ora appaiono anche come notifiche di sistema Windows (toast notification), oltre che all'interno di Playnite. Usa il pacchetto `Microsoft.Toolkit.Uwp.Notifications`. Le toast per aggiornamento prezzo vengono inviate solo per ribassi.

### Corretto
- `ShowInfo` e `ShowError` ora includono sia il titolo che il messaggio nel testo della notifica.

## [0.2.3] - 2025-02-20

### Corretto
- **Notifiche prezzo non funzionanti**: Le notifiche di alert prezzo ora vengono inviate correttamente quando il prezzo scende sotto la soglia configurata.
  - Il metodo `UpdatePrices()` nel plugin ora chiama `CheckPriceAlert()` dopo ogni aggiornamento prezzi e invia automaticamente le notifiche.
  - `PriceAlertsEnabled` ora è abilitato di default (`true` invece di `false`).
  - `NotifyPriceAlert()` riscritta: include il prezzo corrente, il venditore e la soglia nel testo della notifica (es. "💰 Cyberpunk 2077 - Prezzo sceso a 8.41€ (K4G) | Soglia: 10.00€").
  - Usa ID stabili per notifica (`allkeyshop-price-alert-{id}`) per evitare duplicati.
  - Click sulla notifica apre la pagina di acquisto.
- **Notifiche giochi gratis**: Il messaggio include ora la lista completa dei giochi trovati.
- **Notifiche aggiornamento prezzo**: Formato prezzo corretto (€ invece di `ToString("C")` che poteva usare altre valute).

### Aggiunto
- Alert prezzo anche dalla sidebar: il pulsante "Aggiorna Prezzi" e "Agg." (aggiorna singolo) ora verificano le soglie e inviano notifiche istantanee.
- Logging dettagliato per ogni notifica inviata (`logger.Info`).

## [0.2.2] - 2025-02-20

### Corretto
- **Tema e colori**: Riscrittura completa del sistema di stili per compatibilità ottimale con tutti i temi Playnite (scuro e chiaro).
  - Rimossi tutti i background espliciti dai DataGrid (`ControlBackgroundBrush`, `WindowBackgroundBrush`) che causavano conflitti con i temi. Ora usano `Background="Transparent"` per ereditare correttamente dallo sfondo del tema.
  - Aggiunti stili impliciti in `UserControl.Resources` per `DataGridRow`, `DataGridCell` e `DataGridColumnHeader` con trigger per hover e selezione che mantengono `TextBrush` come colore del testo in ogni stato.
  - Usati overlay semi-trasparenti neutri (`#18808080`, `#28808080`) per hover e selezione che funzionano sia su sfondo scuro che chiaro.
  - Aggiunto `Foreground="{DynamicResource TextBrush}"` sull'Expander per il testo dell'header.
  - Aggiunti `ListBoxItem` styles con trigger nella finestra di ricerca per mantenere i colori corretti durante la selezione.
  - Loading overlay ora con sfondo solido (`#CC000000`) e testo bianco esplicito, visibile su qualsiasi tema.
- **Pulsante elimina non visibile**: La colonna Azioni ora usa `Width="Auto"` con `MinWidth="140"` e pulsanti compatti (`FontSize="11"`, `Padding="4,1"`). Aggiunto `ScrollViewer.HorizontalScrollBarVisibility="Auto"` al DataGrid per consentire lo scorrimento orizzontale. Rinominato "Aggiorna" in "Agg." per risparmiare spazio. Pulsante "X" evidenziato in rosso (`#FF6B68`).
- **Giochi gratis (widget)**: Lo scraping dei giochi gratis ora usa il widget ufficiale AllKeyShop (`widget.allkeyshop.com`) invece della pagina blog `daily-game-deals`, recuperando correttamente tutti i giochi gratis disponibili (28+ titoli vs 6 articoli). Ogni gioco mostra piattaforma e tipo (es. "Steam - Free to keep", "Amazon - Free with Prime", "Xbox - Gamepass").

## [0.2.1] - 2025-07-10

### Corretto
- **Prezzi con commissioni**: Il prezzo mostrato ora include le commissioni di pagamento ("Lowest Fees"), calcolato come il minimo tra il prezzo con carta e il prezzo con PayPal. Aggiunta proprietà `LowestFeePrice` al modello `OfferInfo`.
- **Giochi gratis**: L'elenco dei giochi gratis ora viene popolato correttamente dalla pagina [Daily Game Deals](https://www.allkeyshop.com/blog/daily-game-deals/) di AllKeyShop, mostrando offerte gratuite, deal e promozioni attive. Aggiunto metodo `GetDailyGameDeals()` e `CheckForDailyDeals()`.
- **Tema/Font**: L'interfaccia si adatta correttamente al tema attivo di Playnite (scuro/chiaro). Risolto il problema di testo scuro su sfondo scuro. Tutti i componenti (DataGrid, ListBox, finestre) ora utilizzano le risorse tema di Playnite (`TextBrush`, `WindowBackgroundBrush`, `ControlBackgroundBrush`, `PopupBackgroundBrush`). Aggiunti stili per DataGrid ColumnHeader e Cell.

## [0.2.0] - 2025-07-09

### Aggiunto
- **Nuovo motore di scraping**: Riscrittura completa dello scraping basata sul JSON `gamePageTrans` integrato nelle pagine AllKeyShop, garantendo dati strutturati e affidabili.
- **Ricerca giochi**: Nuova finestra di ricerca (`SearchGameWindow`) che utilizza l'API di ricerca di AllKeyShop per trovare e aggiungere giochi da monitorare.
- **Prezzi Key e Account separati**: Distinzione tra offerte di tipo "key" e "account" con prezzi separati nella tabella di monitoraggio.
- **Migrazione database**: Aggiunta automatica delle nuove colonne (`KeyPrice`, `AccountPrice`, `BuyUrl`, `BestMerchant`, `IsAccountOffer`) al database SQLite esistente.
- **Modello OfferInfo completo**: Nuova classe `OfferInfo` con supporto per voucher, edizioni, regioni, commissioni (card/PayPal) e URL di acquisto diretto.

### Modificato
- Struttura dei modelli `GamePrice` e `WatchedGame` aggiornata con nuovi campi.
- `PriceService` aggiornato per il nuovo flusso di scraping.
- Vista sidebar aggiornata con colonne Key/Account/Migliore.

## [0.1.2] - 2025-07-08

### Corretto
- **Errore di caricamento XAML**: Risolto `XamlParseException` causato da riferimenti errati a `SystemColors.WindowTextColorKey` nei template XAML.
- **Vista sidebar**: Implementata la vista sidebar per il monitoraggio prezzi all'interno di Playnite.

## [0.1.1] - 2025-01-22

### Corretto
- **Scraping AllKeyShop**: Migliorati i selettori CSS per trovare correttamente prezzo e venditore
  - Aggiunti selettori multipli per gestire diverse strutture HTML del sito
  - Migliorata l'estrazione del prezzo per supportare vari formati (€12.99, 12,99€, $12.99, ecc.)
  - Aggiunto logging dettagliato per il debug dello scraping
- **Gestione LastUpdate**: Corretto il problema per cui la data di ultimo aggiornamento veniva mostrata anche quando il prezzo non era disponibile
  - LastUpdate viene aggiornato solo quando il prezzo è effettivamente disponibile
- **Grafica e Tema**: Rimossi tutti i colori hardcoded e adattata l'interfaccia al tema Playnite
  - Uso di DynamicResource con SystemColors per adattarsi automaticamente al tema scuro/chiaro
  - Stili per DataGrid e controlli che seguono la palette di colori del tema
  - Tutte le viste (PriceMonitorView, PriceMonitorWindow, SettingsView) ora si adattano al tema

### Migliorato
- **Scraping**: Aggiunti più selettori CSS per aumentare la probabilità di trovare i dati corretti
- **Logging**: Aggiunto logging dettagliato per tracciare il processo di scraping
- **Estrazione Prezzo**: Regex migliorata per estrarre prezzi da formati più vari

## [0.1.0] - 2024-12-19

### Aggiunto

#### Funzionalità Principali
- **Monitoraggio Prezzi**: Sistema completo per monitorare i prezzi dei giochi da AllKeyShop
  - Aggiunta di giochi da monitorare tramite interfaccia utente
  - Visualizzazione del prezzo più basso trovato su AllKeyShop
  - Aggiornamento automatico dei prezzi a intervalli configurabili
  - Click su un gioco per aprire direttamente la pagina AllKeyShop
  - Rimozione di giochi dalla lista di monitoraggio

- **Notifiche Giochi Gratis**: Sistema di notifiche per nuovi giochi gratis disponibili
  - Controllo automatico dei giochi gratis per piattaforme selezionate
  - Notifiche quando vengono trovati nuovi giochi gratis
  - Configurazione delle piattaforme da monitorare (Steam, Epic, GOG, Xbox, PlayStation, Nintendo Switch, Origin, Uplay, Battle.net)
  - Intervallo di controllo configurabile

- **Interfaccia Utente**
  - Vista principale (PriceMonitorView) con lista completa dei giochi monitorati
  - Finestra separata per il monitoraggio prezzi (PriceMonitorWindow)
  - Vista impostazioni completa con tutte le opzioni di configurazione
  - Indicatori di caricamento durante le operazioni
  - Stato vuoto quando non ci sono giochi monitorati

#### Servizi e Logica Business
- **AllKeyShopScraper**: Servizio di scraping per recuperare prezzi e giochi gratis
  - Scraping HTML con HtmlAgilityPack
  - Rate limiting per evitare ban
  - Cache dei risultati per ottimizzare le richieste
  - Gestione errori robusta

- **PriceService**: Servizio per la gestione dei prezzi
  - Aggiunta/rimozione giochi da monitorare
  - Aggiornamento prezzi singoli o multipli
  - Rilevamento giochi che necessitano aggiornamento
  - Supporto per soglie di prezzo (preparato per future feature)

- **FreeGamesService**: Servizio per la gestione dei giochi gratis
  - Controllo nuovi giochi gratis per piattaforma
  - Confronto con storico per rilevare nuovi giochi
  - Filtraggio per piattaforme abilitate

- **NotificationService**: Servizio per le notifiche Playnite
  - Notifiche per nuovi giochi gratis
  - Notifiche per alert prezzo (preparato per future feature)
  - Notifiche per aggiornamenti prezzi
  - Click sulle notifiche per aprire AllKeyShop

#### Persistenza Dati
- **Database SQLite**: Sistema di persistenza locale
  - Tabella `WatchedGames` per giochi monitorati
  - Tabella `FreeGamesHistory` per storico giochi gratis
  - Tabella `Settings` per impostazioni estensione
  - Operazioni thread-safe
  - Gestione automatica della creazione del database

#### Background Tasks
- **Timer Aggiornamento Prezzi**: Aggiornamento automatico periodico
  - Intervallo configurabile (default: 60 minuti)
  - Aggiornamento asincrono per non bloccare l'UI
  - Gestione errori con logging

- **Timer Controllo Giochi Gratis**: Controllo automatico periodico
  - Intervallo configurabile (default: 120 minuti)
  - Controllo solo per piattaforme abilitate
  - Notifiche automatiche per nuovi giochi

#### Configurazione
- **Impostazioni Estensione**:
  - Abilita/disabilita notifiche
  - Abilita/disabilita alert prezzo (preparato per future feature)
  - Configurazione intervallo aggiornamento prezzi
  - Configurazione intervallo controllo giochi gratis
  - Selezione piattaforme per notifiche giochi gratis
  - Salvataggio automatico delle impostazioni

#### Struttura Progetto
- Architettura modulare con separazione delle responsabilità
- Modelli dati ben definiti
- Servizi riutilizzabili
- Viste WPF responsive
- Utilities per operazioni comuni (HttpClientHelper)

#### Documentazione
- README.md completo con istruzioni di installazione e utilizzo
- Codice commentato e ben strutturato
- Gestione errori con logging dettagliato

### Note Tecniche
- Target Framework: .NET Framework 4.8
- Dipendenze: HtmlAgilityPack, Newtonsoft.Json, System.Data.SQLite.Core
- Compatibilità: Playnite 10.x o superiore
- Thread-safe per operazioni concorrenti
- Gestione errori robusta con try-catch appropriati
- Rate limiting per evitare problemi con AllKeyShop

### Preparato per Future Feature
- Sistema di alert per soglie prezzo (infrastruttura pronta)
- Storico prezzi nel tempo
- Supporto multi-valuta
- Integrazione con wishlist Steam
- Esportazione dati

### Note
- Lo scraping di AllKeyShop è soggetto alle politiche del sito
- I selettori HTML potrebbero dover essere aggiornati se AllKeyShop cambia la struttura
- Il database viene creato automaticamente alla prima esecuzione
- Le notifiche utilizzano il sistema nativo di Playnite
