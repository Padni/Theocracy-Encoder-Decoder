using System;

namespace TheocracyEncDecGUI
{
    public enum AppLanguage
    {
        Polish,
        English
    }

    public static class Strings
    {
        public static AppLanguage CurrentLanguage { get; set; } = AppLanguage.Polish;

        public static string AppTitle => "THEOCRACY - Save & File Encoder / Decoder";

        public static string Subtitle => CurrentLanguage == AppLanguage.Polish
            ? "Narzędzie do szyfrowania i odszyfrowywania save'ów (.tsg) oraz plików konfiguracyjnych gry Theocracy"
            : "Utility to encode and decode save files (.tsg) and configuration files for Theocracy";

        public static string TabSingle => CurrentLanguage == AppLanguage.Polish
            ? "  Pojedynczy plik  "
            : "  Single File  ";

        public static string TabBatch => CurrentLanguage == AppLanguage.Polish
            ? "  Przetwarzanie wsadowe (Wiele plików)  "
            : "  Batch Processing (Multiple Files)  ";

        public static string DropHint => CurrentLanguage == AppLanguage.Polish
            ? "Przeciągnij i upuść tutaj plik (.tsg, .txt) lub użyj przycisku obok:"
            : "Drag & drop a file here (.tsg, .txt) or use the browse button:";

        public static string BtnBrowse => CurrentLanguage == AppLanguage.Polish
            ? "Przeglądaj..."
            : "Browse...";

        public static string GrpFileInfo => CurrentLanguage == AppLanguage.Polish
            ? "Informacje o wybranym pliku"
            : "Selected File Information";

        public static string LblFile => CurrentLanguage == AppLanguage.Polish ? "Plik:" : "File:";

        public static string LblSizeNoFile => CurrentLanguage == AppLanguage.Polish
            ? "Rozmiar: Brak pliku"
            : "Size: No file";

        public static string LblSize(long bytes) => CurrentLanguage == AppLanguage.Polish
            ? $"Rozmiar: {bytes:N0} B ({bytes / 1024.0:F1} KB)"
            : $"Size: {bytes:N0} B ({bytes / 1024.0:F1} KB)";

        public static string LblModifiedNoFile => CurrentLanguage == AppLanguage.Polish
            ? "Data modyfikacji: -"
            : "Modified: -";

        public static string LblModified(DateTime dt) => CurrentLanguage == AppLanguage.Polish
            ? $"Data modyfikacji: {dt:yyyy-MM-dd HH:mm:ss}"
            : $"Modified: {dt:yyyy-MM-dd HH:mm:ss}";

        public static string BadgeNoFile => CurrentLanguage == AppLanguage.Polish
            ? "BRAK PLIKU"
            : "NO FILE";

        public static string BadgeEncrypted => CurrentLanguage == AppLanguage.Polish
            ? "ZASZYFROWANY [RSA4096]"
            : "ENCRYPTED [RSA4096]";

        public static string BadgeDecrypted => CurrentLanguage == AppLanguage.Polish
            ? "ODSZYFROWANY [Jawny]"
            : "DECRYPTED [Plaintext]";

        public static string DetailsDefaultHint => CurrentLanguage == AppLanguage.Polish
            ? "Wybierz plik zapisu gry (.tsg) lub plik konfiguracyjny (.txt), aby zbadać jego strukturę."
            : "Select a save file (.tsg) or configuration file (.txt) to inspect its structure.";

        public static string DetailsEncrypted(long payloadSize, bool isSave, string saveDate, bool isText)
        {
            if (CurrentLanguage == AppLanguage.Polish)
            {
                string s = $"Zaszyfrowany plik Theocracy (nagłówek RSA4096). Rozmiar danych: {payloadSize:N0} B.";
                if (isSave) s += $" [Zapis gry Theocracy - Data: {saveDate}]";
                else if (isText) s += " [Tekstowy plik konfiguracyjny]";
                return s;
            }
            else
            {
                string s = $"Encrypted Theocracy file (RSA4096 header). Payload size: {payloadSize:N0} B.";
                if (isSave) s += $" [Theocracy Save Game - Date: {saveDate}]";
                else if (isText) s += " [Configuration text file]";
                return s;
            }
        }

        public static string DetailsDecrypted(bool isSave, string saveDate, string sig, bool isText)
        {
            if (CurrentLanguage == AppLanguage.Polish)
            {
                string s = "Odszyfrowany plik (brak nagłówka RSA4096).";
                if (isSave) s += $" Wykryto zapis gry: data w grze: {saveDate}, sygnatura: {sig}.";
                else if (isText) s += " Wykryto tekstowy plik INI / konfiguracji.";
                return s;
            }
            else
            {
                string s = "Decrypted file (no RSA4096 header).";
                if (isSave) s += $" Save game detected: in-game date: {saveDate}, signature: {sig}.";
                else if (isText) s += " INI configuration file detected.";
                return s;
            }
        }

        public static string GrpActions => CurrentLanguage == AppLanguage.Polish
            ? "Operacje szyfrowania / deszyfrowania"
            : "Encryption / Decryption Actions";

        public static string BtnDecode => CurrentLanguage == AppLanguage.Polish
            ? "Odszyfruj (Decode)"
            : "Decode (Decrypt)";

        public static string BtnEncode => CurrentLanguage == AppLanguage.Polish
            ? "Zaszyfruj (Encode)"
            : "Encode (Encrypt)";

        public static string BtnToggle => CurrentLanguage == AppLanguage.Polish
            ? "Przełącz (Auto)"
            : "Toggle (Auto)";

        public static string BtnSaveAs => CurrentLanguage == AppLanguage.Polish
            ? "Zapisz jako..."
            : "Save As...";

        public static string ChkBackup => CurrentLanguage == AppLanguage.Polish
            ? "Twórz kopię zapasową (.bak) przed zapisem"
            : "Create backup (.bak) before saving";

        public static string GrpHex => CurrentLanguage == AppLanguage.Polish
            ? "Podgląd heksadecymalny i tekstowy (pierwsze 256 bajtów)"
            : "Hexadecimal & Text Preview (first 256 bytes)";

        public static string BtnBatchAddFiles => CurrentLanguage == AppLanguage.Polish
            ? "Dodaj pliki..."
            : "Add Files...";

        public static string BtnBatchAddFolder => CurrentLanguage == AppLanguage.Polish
            ? "Dodaj folder..."
            : "Add Folder...";

        public static string BtnBatchClear => CurrentLanguage == AppLanguage.Polish
            ? "Wyczyść listę"
            : "Clear List";

        public static string ChkBatchBackup => CurrentLanguage == AppLanguage.Polish
            ? "Twórz kopie zapasowe (.bak)"
            : "Create backups (.bak)";

        public static string ColFile => CurrentLanguage == AppLanguage.Polish ? "Plik" : "File";
        public static string ColState => CurrentLanguage == AppLanguage.Polish ? "Stan obecny" : "Current State";
        public static string ColSize => CurrentLanguage == AppLanguage.Polish ? "Rozmiar" : "Size";
        public static string ColPath => CurrentLanguage == AppLanguage.Polish ? "Ścieżka" : "Path";
        public static string ColResult => CurrentLanguage == AppLanguage.Polish ? "Wynik operacji" : "Operation Result";

        public static string BtnBatchDecode => CurrentLanguage == AppLanguage.Polish
            ? "Odszyfruj zaznaczone"
            : "Decode Selected";

        public static string BtnBatchEncode => CurrentLanguage == AppLanguage.Polish
            ? "Zaszyfruj zaznaczone"
            : "Encode Selected";

        public static string BtnBatchToggle => CurrentLanguage == AppLanguage.Polish
            ? "Przełącz zaznaczone"
            : "Toggle Selected";

        public static string GrpLog => CurrentLanguage == AppLanguage.Polish
            ? "Dziennik zdarzeń (Log)"
            : "Event Log";

        public static string StatusReady => CurrentLanguage == AppLanguage.Polish
            ? "Gotowy do pracy"
            : "Ready";

        public static string StatusLoaded(string filename, string badge) => CurrentLanguage == AppLanguage.Polish
            ? $"Wczytano: {filename} ({badge})"
            : $"Loaded: {filename} ({badge})";

        public static string MsgDecodeAlreadyPlain => CurrentLanguage == AppLanguage.Polish
            ? "Plik nie ma nagłówka RSA4096 i wygląda na już odszyfrowany.\nCzy na pewno chcesz wymusić operację odszyfrowania?"
            : "The file has no RSA4096 header and appears to be already decrypted.\nDo you really want to force decryption?";

        public static string MsgEncodeAlreadyEnc => CurrentLanguage == AppLanguage.Polish
            ? "Plik ma już nagłówek RSA4096 i wygląda na zaszyfrowany.\nCzy na pewno chcesz zaszyfrować go ponownie?"
            : "The file already has an RSA4096 header and appears to be encrypted.\nDo you really want to force encryption?";

        public static string MsgSuccess(string actionDesc) => CurrentLanguage == AppLanguage.Polish
            ? $"Pomyślnie wykonano operację: {actionDesc}!\nPlik został zaktualizowany."
            : $"Successfully completed: {actionDesc}!\nThe file has been updated.";

        public static string MsgSaveAsSaved(string path) => CurrentLanguage == AppLanguage.Polish
            ? $"Plik został pomyślnie zapisany jako:\n{path}"
            : $"File successfully saved as:\n{path}";

        public static string ActionDecoded => CurrentLanguage == AppLanguage.Polish ? "Odszyfrowano" : "Decoded";
        public static string ActionEncoded => CurrentLanguage == AppLanguage.Polish ? "Zaszyfrowano" : "Encoded";

        public static string BatchCompleted(int succ, int err) => CurrentLanguage == AppLanguage.Polish
            ? $"Przetwarzanie wsadowe zakończone.\nSukces: {succ} plików\nBłędy: {err}"
            : $"Batch processing completed.\nSuccess: {succ} files\nErrors: {err}";
    }
}
