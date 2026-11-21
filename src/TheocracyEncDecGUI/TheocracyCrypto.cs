using System;
using System.IO;
using System.Text;

namespace TheocracyEncDecGUI
{
    public enum FileCryptoState
    {
        Unknown,
        Encrypted,
        Decrypted
    }

    public class FileInspectionResult
    {
        public FileCryptoState State { get; set; } = FileCryptoState.Unknown;
        public long FileSize { get; set; }
        public string HeaderString { get; set; } = "";
        public bool IsTheocracySave { get; set; }
        public string SaveDate { get; set; } = "";
        public string SaveSignature { get; set; } = "";
        public bool IsTextConfig { get; set; }
        public string ExtraDetails { get; set; } = "";
        public byte[] PreviewBytes { get; set; } = Array.Empty<byte>();
    }

    public static class TheocracyCrypto
    {
        public static readonly byte[] Key1 = Encoding.ASCII.GetBytes("theocracy sux"); // 13 bytes (0x0D)
        public static readonly byte[] Key2 = Encoding.ASCII.GetBytes("mutant technology"); // 17 bytes (0x11)
        public static readonly byte[] Header = Encoding.ASCII.GetBytes("RSA4096"); // 7 bytes

        public static bool StartsWithHeader(byte[] data)
        {
            if (data == null || data.Length < Header.Length)
                return false;

            for (int i = 0; i < Header.Length; i++)
            {
                if (data[i] != Header[i])
                    return false;
            }
            return true;
        }

        public static void XorBuffer(byte[] buffer, int offset, int length)
        {
            int k1Len = Key1.Length;
            int k2Len = Key2.Length;

            for (int i = 0; i < length; i++)
            {
                buffer[offset + i] = (byte)(buffer[offset + i] ^ Key2[i % k2Len] ^ Key1[i % k1Len]);
            }
        }

        public static byte[] Decrypt(byte[] data)
        {
            if (!StartsWithHeader(data))
                throw new InvalidOperationException("Plik nie posiada nagłówka RSA4096 - prawdopodobnie jest już odszyfrowany.");

            int payloadSize = data.Length - Header.Length;
            byte[] decrypted = new byte[payloadSize];
            Array.Copy(data, Header.Length, decrypted, 0, payloadSize);
            XorBuffer(decrypted, 0, payloadSize);
            return decrypted;
        }

        public static byte[] Encrypt(byte[] data)
        {
            if (StartsWithHeader(data))
                throw new InvalidOperationException("Plik posiada już nagłówek RSA4096 - prawdopodobnie jest już zaszyfrowany.");

            byte[] encrypted = new byte[Header.Length + data.Length];
            Array.Copy(Header, 0, encrypted, 0, Header.Length);
            Array.Copy(data, 0, encrypted, Header.Length, data.Length);
            XorBuffer(encrypted, Header.Length, data.Length);
            return encrypted;
        }

        public static (byte[] result, bool wasEncrypted) Toggle(byte[] data)
        {
            if (StartsWithHeader(data))
            {
                return (Decrypt(data), true);
            }
            else
            {
                return (Encrypt(data), false);
            }
        }

        public static FileInspectionResult InspectData(byte[] data)
        {
            var result = new FileInspectionResult();
            if (data == null || data.Length == 0)
            {
                result.ExtraDetails = "Pusty plik (0 bajtów)";
                return result;
            }

            result.FileSize = data.Length;

            // Preview first 256 bytes
            int previewLen = Math.Min(data.Length, 256);
            result.PreviewBytes = new byte[previewLen];
            Array.Copy(data, 0, result.PreviewBytes, 0, previewLen);

            if (StartsWithHeader(data))
            {
                result.State = FileCryptoState.Encrypted;
                result.HeaderString = "RSA4096";

                // Peek decrypted payload to inspect what it is
                int peekLen = Math.Min(data.Length - Header.Length, 128);
                if (peekLen > 0)
                {
                    byte[] peek = new byte[peekLen];
                    Array.Copy(data, Header.Length, peek, 0, peekLen);
                    XorBuffer(peek, 0, peekLen);
                    InspectPayloadDetails(peek, result);
                }

                result.ExtraDetails = $"Zaszyfrowany plik Theocracy (nagłówek RSA4096). Rozmiar danych: {data.Length - 7:N0} B.";
                if (result.IsTheocracySave)
                {
                    result.ExtraDetails += $" [Zapis gry Theocracy - Data: {result.SaveDate}]";
                }
                else if (result.IsTextConfig)
                {
                    result.ExtraDetails += " [Tekstowy plik konfiguracyjny]";
                }
            }
            else
            {
                result.State = FileCryptoState.Decrypted;
                result.HeaderString = GetPrintableHeader(data, Math.Min(data.Length, 16));
                InspectPayloadDetails(data, result);

                result.ExtraDetails = "Odszyfrowany plik (brak nagłówka RSA4096).";
                if (result.IsTheocracySave)
                {
                    result.ExtraDetails += $" Wykryto zapis gry: data w grze: {result.SaveDate}, sygnatura: {result.SaveSignature}.";
                }
                else if (result.IsTextConfig)
                {
                    result.ExtraDetails += " Wykryto tekstowy plik INI / konfiguracji.";
                }
            }

            return result;
        }

        private static void InspectPayloadDetails(byte[] payload, FileInspectionResult result)
        {
            if (payload == null || payload.Length == 0) return;

            // Check for save game signature "theosg42" or date "YYYY/MM/DD"
            string ascii = Encoding.ASCII.GetString(payload, 0, Math.Min(payload.Length, 128));

            if (ascii.Contains("theosg"))
            {
                result.IsTheocracySave = true;
                int idx = ascii.IndexOf("theosg");
                if (idx + 8 <= ascii.Length)
                {
                    result.SaveSignature = ascii.Substring(idx, 8);
                }

                // Check date format at start (e.g. 1449/13/14)
                if (ascii.Length >= 10 && ascii[4] == '/' && ascii[7] == '/')
                {
                    result.SaveDate = ascii.Substring(0, 10);
                }
            }
            else if (ascii.TrimStart().StartsWith("[") || ascii.Contains("RESOURCE_") || ascii.Contains("BARRACK"))
            {
                result.IsTextConfig = true;
            }
        }

        private static string GetPrintableHeader(byte[] data, int max)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < max && i < data.Length; i++)
            {
                char c = (char)data[i];
                if (c >= 32 && c <= 126)
                    sb.Append(c);
                else
                    sb.Append('.');
            }
            return sb.ToString();
        }

        public static string GenerateHexDump(byte[] data, int maxBytes = 256)
        {
            if (data == null || data.Length == 0) return "(Pusty plik)";

            var sb = new StringBuilder();
            int count = Math.Min(data.Length, maxBytes);

            for (int i = 0; i < count; i += 16)
            {
                sb.AppendFormat("{0:X4}: ", i);

                int lineBytes = Math.Min(16, count - i);
                for (int j = 0; j < 16; j++)
                {
                    if (j < lineBytes)
                        sb.AppendFormat("{0:X2} ", data[i + j]);
                    else
                        sb.Append("   ");

                    if (j == 7) sb.Append(" ");
                }

                sb.Append(" |");
                for (int j = 0; j < lineBytes; j++)
                {
                    byte b = data[i + j];
                    char c = (b >= 32 && b <= 126) ? (char)b : '.';
                    sb.Append(c);
                }
                sb.Append("|");
                sb.AppendLine();
            }

            if (data.Length > maxBytes)
            {
                sb.AppendLine($"... [pominięto pozostałe {data.Length - maxBytes:N0} bajtów]");
            }

            return sb.ToString();
        }
    }
}
