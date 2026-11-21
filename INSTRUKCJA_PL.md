# Theocracy Encoder / Decoder - Wersja GUI

## 1. Weryfikacja działania dekodera

Oryginalny kod dekodera/kodera w katalogu `/src` (`TheocracyEncDec.cpp`) oraz binarka `TheocracyEncDec.exe` zostały szczegółowo przetestowane pod kątem poprawności:

### Jak działa algorytm?
1. **Nagłówek magiczny**: `RSA4096` (7 bajtów ASCII: `0x52, 0x53, 0x41, 0x34, 0x30, 0x39, 0x36`).
2. **Klucze szyfrowania (XOR)**:
   - `key1` = `"theocracy sux"` (długość: 13 bajtów / `0x0D`)
   - `key2` = `"mutant technology"` (długość: 17 bajtów / `0x11`)
3. **Transformacja bajtów**:
   Każdy bajt `i` przekształcany jest symetryczną operacją XOR:
   ```c
   buff[i] = buff[i] ^ key2[i % 17] ^ key1[i % 13];
   ```
4. **Wykrywanie stanu**:
   - Jeżeli plik rozpoczyna się od `RSA4096`, program traktuje go jako **zaszyfrowany**. Odszyfrowanie polega na pominięciu pierwszych 7 bajtów i wykonaniu operacji XOR na reszcie pliku.
   - Jeżeli plik **nie** rozpoczyna się od `RSA4096`, program traktuje go jako **odszyfrowany**. Szyfrowanie polega na wykonaniu operacji XOR na całym pliku i dodaniu na początku 7-bajtowego nagłówka `RSA4096`.

### Wyniki testów:
- **Plik `selap.txt`**:
  - Stan początkowy: Zaszyfrowany (rozpoczyna się od `RSA4096`, rozmiar: 21 063 bajty).
  - Po odszyfrowaniu: Rozmiar 21 056 bajtów, czytelny tekstowy plik konfiguracyjny INI (`[Building_sys]`, `RESOURCE_UNIT=1`, `[Buildings]`, itp.).
  - Ponowne zaszyfrowanie: Zwraca dokładnie 21 063 bajty o identycznej sumie kontrolnej (100% zgodności SHA-256).
- **Plik `save00.tsg`**:
  - Stan początkowy: Odszyfrowany stan zapisu gry Theocracy (brak `RSA4096`, na początku data w grze `1449/13/14` oraz sygnatura formatu `theosg42`, rozmiar: 664 268 bajtów).
  - Po zaszyfrowaniu: Dopisany nagłówek `RSA4096`, rozmiar 664 275 bajtów.
  - Po ponownym odszyfrowaniu: Zwraca identyczny plik wyjściowy (100% zgodności SHA-256).

---

## 2. Nowa wersja z graficznym interfejsem użytkownika (GUI)

W katalogu przygotowano nowoczesne aplikacje okienkowe:

### Dostępne pliki:
1. **`TheocracyEncDecGUI.exe`** (Główna aplikacja)
   - W pełni samodzielna wersja (Standalone, Windows x64).
   - Nie wymaga instalowania środowiska .NET ani żadnych zewnętrznych bibliotek (działa na każdym Windows 10/11 od razu po dwukliku).
2. **`TheocracyEncDecGUI_Lite.exe`**
   - Lekka wersja (ok. 197 KB) dla systemów z zainstalowanym .NET 9 Desktop Runtime.
3. **`TheocracyEncDecGUI.py`**
   - Wersja skryptowa w Pythonie (Tkinter), w pełni funkcjonalna i wieloplatformowa.
4. **`TheocracyEncDecGUI/`**
   - Kompletny kod źródłowy projektu C# (.NET 9 WinForms).

---

## 3. Funkcje aplikacji GUI

- **Wielojęzyczność (Bilingual PL / EN)**:
  - W prawym górnym rogu okna programu znajduje się przełącznik języka (`🇵🇱 Polski` / `🇬🇧 English`).
  - Cały interfejs (etykiety, przyciski, komunikaty, statusy) natychmiast przełącza się na wybrany język (zarówno w wersji C#, jak i Python).
- **Wizualny wskaźnik stanu pliku (Badge)**:
  - 🟢 `ZASZYFROWANY [RSA4096]` – plik gotowy do odszyfrowania.
  - 🔵 `ODSZYFROWANY [Jawny]` – plik gotowy do edycji lub zaszyfrowania dla gry.
- **Wykrywanie metadanych zapisu gry**:
  - Automatyczne rozpoznawanie daty w grze (np. `1449/13/14`) oraz sygnatury `theosg42`.
  - Wykrywanie plików konfiguracyjnych INI.
- **Drag & Drop (Przeciągnij i upuść)**:
  - Wystarczy przeciągnąć plik `.tsg` lub `.txt` bezpośrednio na okno programu.
  - Można również przeciągnąć plik na ikonę `.exe`.
- **Podgląd heksadecymalny i tekstowy (Hex / ASCII Dump)**:
  - Podgląd na żywo pierwszych 256 bajtów pliku przed i po operacji.
- **Ochrona przed utratą danych**:
  - Opcja automatycznego tworzenia kopii zapasowej (`.bak`) przed każdą modyfikacją pliku.
  - Możliwość użycia przycisku "Zapisz jako...", aby nie nadpisywać oryginalnego pliku.
- **Przetwarzanie wsadowe (Wiele plików naraz)**:
  - Druga zakładka w programie pozwala na wskazanie całego folderu z zapisami lub zaznaczenie wielu plików i ich jednoczesne odszyfrowanie/zaszyfrowanie z paskiem postępu.
- **Dziennik zdarzeń (Log)**:
  - Podgląd komunikatów o operacjach, rozmiarach plików i ewentualnych błędach.
