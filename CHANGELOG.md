# Changelog

Tutte le modifiche notevoli a questo progetto saranno documentate in questo file.

Il formato è basato su [Keep a Changelog](https://keepachangelog.com/it/1.0.0/),
e questo progetto aderisce a [Semantic Versioning](https://semver.org/lang/it/).

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
