# TODO — AllKeyShop Price Monitor

Roadmap e idee per le prossime versioni.

## v1.1.0 — Miglioramenti UX

- [ ] **Storico prezzi**: Registrare l'andamento del prezzo nel tempo e mostrare un mini-grafico (sparkline) nella card del gioco
- [ ] **Ordinamento lista**: Permettere di ordinare i giochi per nome, prezzo, data aggiornamento, soglia
- [ ] **Filtro di ricerca**: Aggiungere una barra di ricerca/filtro nella lista dei giochi monitorati
- [ ] **Import da Playnite Library**: Bottone per aggiungere rapidamente alla watchlist i giochi già presenti nella libreria Playnite
- [ ] **Soglie prezzo visive**: Evidenziare in verde i giochi sotto-soglia e in rosso quelli sopra

## v1.2.0 — Funzionalità Avanzate

- [ ] **Multi-valuta**: Supporto per valute diverse da EUR (USD, GBP, ecc.) con selezione nelle impostazioni
- [ ] **Wishlist Steam sync**: Importare automaticamente la wishlist di Steam per monitorare i prezzi
- [ ] **Confronto edizioni**: Mostrare nella card le diverse edizioni disponibili (Standard, Deluxe, GOTY, ecc.)
- [ ] **Esportazione CSV/JSON**: Esportare la lista dei giochi monitorati con i prezzi correnti
- [ ] **Click su toast notification**: Aprire direttamente il browser alla pagina del gioco quando si clicca la notifica Windows

## v1.3.0 — Integrazione e Sharing

- [ ] **Condivisione lista**: Generare un link condivisibile della propria watchlist
- [ ] **Webhook Discord/Telegram**: Invio notifiche a bot esterni quando un prezzo scende sotto la soglia
- [ ] **Plugin API**: Esporre un'API interna per altri plugin Playnite

## Bug Noti / Miglioramenti Tecnici

- [ ] Gestire gracefully il caso in cui AllKeyShop cambia struttura HTML (auto-detection failure + notifica utente)
- [ ] Aggiungere retry con backoff esponenziale per richieste HTTP fallite
- [ ] Cache più intelligente: invalidare solo i giochi che non hanno ricevuto aggiornamento, non tutta la cache
- [ ] Migliorare il parsing delle date nel database (usare DateTimeOffset per gestire fusi orari)
- [ ] Unit test per i servizi principali (PriceService, AllKeyShopScraper, Database)
