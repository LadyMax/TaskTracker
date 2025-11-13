# Övning: TaskTracker
## Scenario
Ni arbetar med att vidareutveckla ett task management system. Systemet har redan ett fungerande WebAPI för grundläggande CRUD-operationer, men nu behöver ni implementera mer avancerad funktionalitet för att hantera filtrering och sortering av tasks. Ni ska implementera egna algoritmer för att:

- Filtrera tasks baserat på olika kriterier
- Sortera tasks med olika algoritmer
- Beräkna och sortera tasks baserat på urgency

### Viktiga begränsningar 
> Ni får **inte** använda externa bibliotek för sortering eller filtrering.
>
> **Vad innebär detta?**
> - Ni måste implementera egen logik för att hantera sortering och filtrering
> - Ni får inte använda färdiga implementationer av:
>   - LINQ-metoder för sortering och filtrering
>   - Array.Sort eller liknande
> - All funktionalitet ska vara väl testad med enhetstester

### Tekniska krav
* Använd TDD genom hela utvecklingsprocessen
* Dokumentera algoritmer med kommentarer och komplexitetsanalys
* Implementera robust felhantering
* Skriv tydliga och omfattande enhetstester

### Grundstruktur
Projektet innehåller följande huvudkomponenter:

- TaskTracker.Api - WebAPI projektet med endpoints
- TaskTracker.Core - Core-biblioteket med modeller och interfaces
- TaskTracker.Tests - Testprojektet

## Uppgifter
Tips: För varje steg:
1. Skriv ett failing test
2. Implementera minimal kod för att klara testet
3. Refaktorera vid behov
4. Fortsätt till nästa test

### 1 Filter
Vi ska kunna filtrera tasks baserat på olika kriterier. För att göra detta har vi en FilterCriteria klass som innehåller:
- Status
- DueBefore

Med hjälp av FilterCriteria klassen kan vi filtrera tasks genom att använda metoden `GetFilteredTasks` i TaskService klassen.

Implementera en filtreringsalgoritm genom TDD med följande steg:

1. **Skriv första testet - Filtrera på Status**
   * Skapa ett eller flera test för att filtrera tasks med en specifik status
   * Verifiera att endast tasks med rätt status returneras
   * Implementera minimal kod för att klara testet
   * Refaktorera koden för att göra den mer lättläst

2. **Lägg till tester och upprepa för:**
   * DueBefore, verifiera att endast tasks med DueDate före angivet datum returneras
   * Null-hantering, koden för att hantera null korrekt
   * När både Status och DueBefore används
   * Edge cases, skriv test för t.ex. tom task-lista

### 1 Sortering
Vi ska implementera sortering med hjälp av insertion sort algoritmen. Metoden `OrderTasks` i TaskService klassen tar emot en `OrderCriteria` som specificerar vilket fält som ska sorteras (`OrderByField`) och om sorteringen ska vara stigande eller fallande (`Descending`).

Implementera [insertion sort](https://en.wikipedia.org/wiki/Insertion_sort) genom TDD:
Finns också bra förklaring [här](https://www.geeksforgeeks.org/insertion-sort/)

Insertion Sort algoritmen ska:
- Iterera genom arrayen från vänster till höger
- För varje element, jämföra med tidigare element och flytta dem åt höger tills rätt position hittas
- Sätta in elementet på rätt position
- Hantera olika typer av fält (datum, text, enum) korrekt


1. **Första sorteringstestet - Grundläggande insertion sort**
   * Skriv test för enkel sortering på DueDate ascending
   * Implementera grundläggande insertion sort algoritm
   * Verifiera att algoritmen sorterar korrekt
   * Refaktorera för läsbarhet

2. **Utöka med descending ordning**
   * Lägg till test för descending sortering
   * Modifiera insertion sort för att hantera båda riktningarna
   * Verifiera att Descending = true ger omvänd ordning

3. **Implementera sortering för olika fält**
   * Lägg till tester för varje OrderByField
   * Utöka insertion sort för att jämföra olika fälttyper
   * Verifiera korrekt sortering för alla fält
   * Refaktorera för att undvika duplicerad kod

4. **Edge cases för insertion sort**
   * Skriv test för null-värden i fält som sorteras
   * Bestäm och implementera hur null ska hanteras (först/sist)
   * Testa med tom lista och lista med ett element
   * Testa med identiska värden

5. **(Bonus) Använd CountingSort för att sortera tasks baserat på status**
Eftersom vi har ett begränsat antal statusvärden kan vi använda CountingSort för att effektivt sortera tasks baserat på status. Detta är en stabil sorteringsalgoritm som kan vara snabbast för detta specifika fall, dock är den inte den lättaste att förstå.
   * Implementera [CountingSort](https://www.geeksforgeeks.org/counting-sort/) algoritmen
   * Verifiera att algoritmen sorterar korrekt
   * Refaktorera för att undvika duplicerad kod
