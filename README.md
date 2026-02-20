<div align="center">

# AllKeyShop Price Monitor

**Estensione Playnite per il monitoraggio prezzi da AllKeyShop**

[![Version](https://img.shields.io/badge/version-1.0.0-blue.svg)]()
[![Platform](https://img.shields.io/badge/platform-Playnite%2010+-purple.svg)]()
[![Framework](https://img.shields.io/badge/.NET-Framework%204.8-green.svg)]()
[![License](https://img.shields.io/badge/license-MIT-lightgrey.svg)](LICENSE)

Monitora i prezzi dei tuoi giochi da AllKeyShop, ricevi notifiche Windows quando un prezzo scende sotto la soglia che hai impostato, e scopri i giochi gratis disponibili — tutto direttamente dalla sidebar di Playnite.

</div>

---

## Indice

- [Funzionalita](#funzionalita)
- [Screenshot](#screenshot)
- [Installazione](#installazione)
- [Guida Rapida](#guida-rapida)
- [Impostazioni](#impostazioni)
- [Struttura Progetto](#struttura-progetto)
- [Sviluppo](#sviluppo)
- [Roadmap](#roadmap)
- [Changelog](#changelog)
- [Licenza](#licenza)

---

## Funzionalita

### Monitoraggio Prezzi
| Funzione | Descrizione |
|----------|-------------|
| **Watchlist** | Aggiungi giochi alla tua lista personale con ricerca integrata AllKeyShop |
| **Prezzi Key + Account** | Ogni gioco mostra il miglior prezzo per chiave digitale e per account |
| **Prezzo con commissioni** | Il prezzo mostrato include le commissioni di pagamento (LowestFeePrice) |
| **Soglia prezzo** | Imposta una soglia per ogni gioco: ricevi un alert quando il prezzo scende |
| **Aggiornamento automatico** | I prezzi si aggiornano in background a intervalli configurabili |
| **Thumbnail gioco** | Ogni gioco mostra la copertina dalla ricerca AllKeyShop |

### Notifiche
| Funzione | Descrizione |
|----------|-------------|
| **Toast Windows** | Notifiche native Windows 10/11 (area notifiche + centro notifiche) |
| **Notifiche Playnite** | Alert anche nel sistema notifiche integrato di Playnite |
| **Alert prezzo** | Quando un prezzo scende sotto la soglia impostata |
| **Giochi gratis** | Quando vengono trovati nuovi giochi gratis su qualsiasi piattaforma |
| **Aggiornamento prezzo** | Notifica sulle variazioni di prezzo (solo ribassi per le toast) |

### Giochi Gratis
| Funzione | Descrizione |
|----------|-------------|
| **Widget AllKeyShop** | Scraping dal widget ufficiale AllKeyShop per massima copertura |
| **Multi-piattaforma** | Steam, Epic, GOG, Xbox Game Pass, PlayStation, Amazon Prime, e altri |
| **Tipo offerta** | Mostra il tipo (Free to Keep, Free with Prime, Gamepass, Free DLC, ecc.) |
| **Storico** | Database locale per evitare notifiche duplicate |

### Interfaccia
| Funzione | Descrizione |
|----------|-------------|
| **Sidebar integrata** | App completa nella barra laterale di Playnite |
| **Layout a card** | Ogni gioco e una card con thumbnail, prezzi, soglia e azioni |
| **Icone Segoe MDL2** | Icone native Windows in toolbar e pulsanti azione |
| **Tema adattivo** | Si adatta automaticamente a qualsiasi tema Playnite (scuro/chiaro) |
| **Ricerca avanzata** | Finestra di ricerca con anteprima immagine, prezzo e piattaforma |

---

## Screenshot

> La sidebar principale mostra i giochi monitorati come card con thumbnail, prezzi e azioni rapide.
> La sezione inferiore (espandibile) elenca i giochi gratis recenti.

---

## Installazione

### Metodo Rapido (file .pext)

1. Scarica `AllKeyShopExtension-1.0.0.pext` dalla pagina [Releases](https://github.com/sassoanarchico/allkeyshop-playnite/releases)
2. In Playnite: **Menu > Estensioni > Installa estensione da file**
3. Seleziona il file `.pext` scaricato
4. **Riavvia Playnite**
5. L icona AllKeyShop apparira nella barra laterale

### Installazione Manuale (sviluppatori)

```powershell
# Clona il repository
git clone https://github.com/sassoanarchico/allkeyshop-playnite.git
cd allkeyshop-playnite/AllKeyShopExtension

# Compila e pacchettizza
.\build.ps1
```

Oppure copia la cartella compilata in `%AppData%\Playnite\Extensions\AllKeyShopExtension\`.

---

## Guida Rapida

### Aggiungere un Gioco

1. Clicca l icona **AllKeyShop** nella sidebar di Playnite
2. Clicca il pulsante **+ Aggiungi** nella toolbar
3. Cerca il nome del gioco nella finestra di ricerca
4. Seleziona il risultato corretto (con anteprima immagine e prezzo)
5. (Opzionale) Inserisci una **soglia prezzo** in EUR
6. Clicca **Aggiungi** — il prezzo verra aggiornato immediatamente

### Azioni per Gioco

Ogni card nella lista ha 5 pulsanti azione:

| Icona | Azione | Descrizione |
|:-----:|--------|-------------|
| 🌐 | **Apri** | Apre la pagina AllKeyShop del gioco nel browser |
| 🛒 | **Compra** | Vai direttamente all offerta al miglior prezzo |
| ↻ | **Aggiorna** | Aggiorna il prezzo di questo specifico gioco |
| ✏ | **Modifica soglia** | Apre il dialog per modificare/rimuovere la soglia prezzo |
| 🗑 | **Rimuovi** | Rimuove il gioco dalla watchlist |

### Giochi Gratis

1. Clicca **Gratis** nella toolbar per controllare subito
2. L elenco appare nella sezione espandibile in basso
3. Clicca l icona link per aprire la pagina dell offerta

---

## Impostazioni

Accessibili da **Menu > Estensioni > Impostazioni estensioni > AllKeyShop Price Monitor**.

| Impostazione | Default | Descrizione |
|-------------|---------|-------------|
| Notifiche abilitate | `Si` | Abilita/disabilita tutte le notifiche |
| Alert prezzo | `Si` | Notifiche quando un prezzo scende sotto la soglia |
| Intervallo aggiornamento prezzi | `60 min` | Frequenza aggiornamento automatico dei prezzi |
| Intervallo controllo giochi gratis | `120 min` | Frequenza controllo nuovi giochi gratis |
| Piattaforme | Nessuna | Piattaforme da monitorare per i giochi gratis |

**Piattaforme supportate:** Steam, Epic Games Store, GOG, Xbox, PlayStation, Nintendo Switch, Origin, Uplay, Battle.net

---

## Struttura Progetto

```
AllKeyShopExtension/
+-- Plugin.cs                  # Entry point: GenericPlugin, timers, sidebar
+-- extension.yaml             # Manifest Playnite
|
+-- Models/
|   +-- WatchedGame.cs         # Gioco monitorato (prezzi, soglia, thumbnail)
|   +-- FreeGame.cs            # Gioco gratis trovato
|   +-- GamePrice.cs           # Risultato scraping prezzi + OfferInfo
|   +-- SearchResult.cs        # Risultato ricerca AllKeyShop
|   +-- ExtensionSettings.cs   # Modello impostazioni
|
+-- Services/
|   +-- AllKeyShopScraper.cs   # Scraping: search API + gamePageTrans JSON
|   +-- PriceService.cs        # Logica business prezzi e watchlist
|   +-- FreeGamesService.cs    # Logica giochi gratis (widget)
|   +-- NotificationService.cs # Notifiche Playnite + Toast Windows
|
+-- Data/
|   +-- Database.cs            # SQLite: CRUD + migration automatiche
|
+-- Views/
|   +-- AllKeyShopSidebarView  # Sidebar principale (XAML + code-behind)
|   +-- SearchGameWindow       # Dialog ricerca giochi
|   +-- SettingsView            # Pagina impostazioni
|   +-- PriceMonitorView/Window # Vista monitor prezzi (legacy)
|
+-- Utilities/
|   +-- HttpClientHelper.cs    # HTTP client wrapper
|
+-- build.ps1                  # Script di build + packaging .pext
+-- CHANGELOG.md               # Storico versioni
+-- TODO.md                    # Roadmap feature future
```

---

## Sviluppo

### Prerequisiti

- Visual Studio 2022+ o VS Code con C# extension
- .NET Framework 4.8 Developer Pack
- Playnite installato (per Playnite.SDK.dll)

### Compilazione

```powershell
cd AllKeyShopExtension
.\build.ps1
```

Lo script:
1. Trova automaticamente `Playnite.SDK.dll` in `%LOCALAPPDATA%\Playnite`
2. Compila in modalita Release
3. Copia le DLL necessarie (HtmlAgilityPack, Newtonsoft.Json, SQLite, Toast Notifications)
4. Crea il file `.pext` con la versione letta da `extension.yaml`

### Dipendenze NuGet

| Pacchetto | Versione | Uso |
|-----------|----------|-----|
| HtmlAgilityPack | 1.11.71 | Parsing HTML per scraping |
| Newtonsoft.Json | 13.0.3 | Serializzazione JSON (gamePageTrans, settings) |
| System.Data.SQLite.Core | 1.0.118 | Database locale |
| Microsoft.Toolkit.Uwp.Notifications | 7.1.3 | Toast Windows 10/11 |

### Come Funziona lo Scraping

1. **Ricerca**: `POST` all endpoint AJAX `quicksearch` di AllKeyShop → parsing HTML dei risultati
2. **Prezzi**: `GET` sulla pagina del gioco → estrazione del JSON `gamePageTrans` dallo script inline
3. **Calcolo prezzo**: Per ogni offerta, il prezzo finale e il minimo tra `priceCard` e `pricePaypal` (LowestFeePrice)
4. **Giochi gratis**: `GET` dal widget `widget.allkeyshop.com` → parsing degli slide HTML

---

## Roadmap

Vedi [TODO.md](TODO.md) per la roadmap completa.

**Prossimamente:**
- Storico prezzi con mini-grafico
- Import dalla libreria Playnite
- Multi-valuta
- Sync wishlist Steam
- Export CSV/JSON

---

## Changelog

Vedi [CHANGELOG.md](CHANGELOG.md) per la lista completa delle modifiche.

### Ultime versioni

- **v1.0.0** — UI professionale a card, thumbnail giochi, icone MDL2, fix timer, fix icone rotte
- **v0.2.4** — Modifica soglia prezzo, notifiche Toast Windows
- **v0.2.3** — Fix notifiche prezzo non funzionanti
- **v0.2.2** — Rework tema/colori, widget giochi gratis
- **v0.2.1** — Prezzi con commissioni (LowestFeePrice)
- **v0.2.0** — Nuovo scraping gamePageTrans, ricerca giochi, prezzi key/account

---

## Licenza

Distribuito con licenza [MIT](LICENSE).

---

## Autore

**Sassoanarchico** — [GitHub](https://github.com/sassoanarchico)

---

<div align="center">
<sub>Fatto con caffe e Python regex per Playnite</sub>
</div>