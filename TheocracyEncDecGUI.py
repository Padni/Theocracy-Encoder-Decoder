#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Theocracy Save & File Encoder / Decoder - GUI
Tool to encrypt and decrypt save files (.tsg) and configuration files for Theocracy.
Bilingual version (PL / EN).
"""

import os
import sys
import shutil
import datetime
import tkinter as tk
from tkinter import ttk, filedialog, messagebox

KEY1 = b"theocracy sux"       # 13 bytes (0x0D)
KEY2 = b"mutant technology"  # 17 bytes (0x11)
HEADER = b"RSA4096"          # 7 bytes

TEXTS = {
    "pl": {
        "title": "THEOCRACY - Save & File Encoder / Decoder",
        "subtitle": "Narzędzie do szyfrowania i odszyfrowywania save'ów (.tsg) oraz plików gry Theocracy",
        "lang_lbl": "🌐 Język / Language:",
        "tab_single": "  Pojedynczy plik  ",
        "tab_batch": "  Przetwarzanie wsadowe  ",
        "sel_file": "Wybór pliku",
        "path_lbl": "Ścieżka do pliku:",
        "browse": "📂 Przeglądaj...",
        "info_frame": "Informacje o pliku i status",
        "size_no_file": "Rozmiar: Brak pliku",
        "size_fmt": "Rozmiar: {size:,} B ({kb:.1f} KB)",
        "mtime_no_file": "Data modyfikacji: -",
        "mtime_fmt": "Data modyfikacji: {mtime}",
        "badge_none": "BRAK PLIKU",
        "badge_enc": "🔒 ZASZYFROWANY [RSA4096]",
        "badge_dec": "🔓 ODSZYFROWANY [Jawny]",
        "details_hint": "Wybierz plik .tsg lub .txt, aby wyświetlić szczegóły.",
        "details_enc": "Zaszyfrowany plik gry Theocracy (nagłówek RSA4096). Rozmiar danych: {size:,} B.",
        "details_dec": "Odszyfrowany plik (brak nagłówka RSA4096).",
        "save_sig": " [Wykryto sygnaturę zapisu gry: {sig}]",
        "save_date": " Data w grze: {date}.",
        "config_sig": " [Wykryto plik tekstowy / konfigurację]",
        "act_frame": "Operacje szyfrowania / odszyfrowywania",
        "decode": "🔓 Odszyfruj (Decode)",
        "encode": "🔒 Zaszyfruj (Encode)",
        "toggle": "🔄 Przełącz (Auto)",
        "save_as": "💾 Zapisz jako...",
        "backup": "Twórz kopię zapasową (.bak) przed zapisem",
        "hex_frame": "Podgląd heksadecymalny i tekstowy (pierwsze 256 bajtów)",
        "batch_add_files": "📂 Dodaj pliki...",
        "batch_add_folder": "📁 Dodaj folder...",
        "batch_clear": "🗑 Wyczyść listę",
        "batch_backup": "Twórz kopie zapasowe (.bak)",
        "col_file": "Nazwa pliku",
        "col_state": "Stan obecny",
        "col_size": "Rozmiar",
        "col_path": "Pełna ścieżka",
        "col_result": "Wynik operacji",
        "batch_decode": "🔓 Odszyfruj wszystkie z listy",
        "batch_encode": "🔒 Zaszyfruj wszystkie z listy",
        "batch_toggle": "🔄 Przełącz wszystkie (Auto)",
        "log_frame": "Dziennik zdarzeń (Log)",
        "ready": "Gotowy do pracy",
        "switched_lang": "Przełączono język interfejsu na: Polski",
        "start_msg": "Program Theocracy Encoder / Decoder gotowy do pracy.",
        "confirm_title": "Potwierdzenie",
        "msg_already_dec": "Plik wygląda na już odszyfrowany (brak nagłówka RSA4096).\nCzy na pewno wymusić odszyfrowanie?",
        "msg_already_enc": "Plik ma już nagłówek RSA4096.\nCzy na pewno chcesz zaszyfrować go ponownie?",
        "success_title": "Sukces",
        "success_msg": "Pomyślnie wykonano: {desc}!\nPlik został zaktualizowany.",
        "save_as_title": "Zapisano",
        "save_as_msg": "Plik został pomyślnie zapisany jako:\n{path}",
        "batch_done_title": "Wynik wsadowy",
        "batch_done_msg": "Przetwarzanie zakończone!\nSukces: {succ}\nBłędy: {err}",
        "desc_dec": "Odszyfrowano",
        "desc_enc": "Zaszyfrowano",
        "state_enc": "🔒 Zaszyfrowany",
        "state_dec": "🔓 Odszyfrowany",
        "pending": "Oczekuje"
    },
    "en": {
        "title": "THEOCRACY - Save & File Encoder / Decoder",
        "subtitle": "Utility to encrypt and decrypt save files (.tsg) and configuration files for Theocracy",
        "lang_lbl": "🌐 Language / Język:",
        "tab_single": "  Single File  ",
        "tab_batch": "  Batch Processing  ",
        "sel_file": "File Selection",
        "path_lbl": "File path:",
        "browse": "📂 Browse...",
        "info_frame": "File Information & Status",
        "size_no_file": "Size: No file",
        "size_fmt": "Size: {size:,} B ({kb:.1f} KB)",
        "mtime_no_file": "Modified: -",
        "mtime_fmt": "Modified: {mtime}",
        "badge_none": "NO FILE",
        "badge_enc": "🔒 ENCRYPTED [RSA4096]",
        "badge_dec": "🔓 DECRYPTED [Plaintext]",
        "details_hint": "Select a .tsg or .txt file to view its structure.",
        "details_enc": "Encrypted Theocracy file (RSA4096 header). Data size: {size:,} B.",
        "details_dec": "Decrypted file (no RSA4096 header).",
        "save_sig": " [Save signature detected: {sig}]",
        "save_date": " In-game date: {date}.",
        "config_sig": " [Configuration file detected]",
        "act_frame": "Encryption / Decryption Actions",
        "decode": "🔓 Decode (Decrypt)",
        "encode": "🔒 Encode (Encrypt)",
        "toggle": "🔄 Toggle (Auto)",
        "save_as": "💾 Save As...",
        "backup": "Create backup (.bak) before saving",
        "hex_frame": "Hexadecimal & Text Preview (first 256 bytes)",
        "batch_add_files": "📂 Add Files...",
        "batch_add_folder": "📁 Add Folder...",
        "batch_clear": "🗑 Clear List",
        "batch_backup": "Create backups (.bak)",
        "col_file": "File Name",
        "col_state": "Current State",
        "col_size": "Size",
        "col_path": "Full Path",
        "col_result": "Operation Result",
        "batch_decode": "🔓 Decode All Listed",
        "batch_encode": "🔒 Encode All Listed",
        "batch_toggle": "🔄 Toggle All Listed",
        "log_frame": "Event Log",
        "ready": "Ready",
        "switched_lang": "Interface language switched to: English",
        "start_msg": "Theocracy Encoder / Decoder is ready.",
        "confirm_title": "Confirmation",
        "msg_already_dec": "The file appears to be already decrypted (no RSA4096 header).\nDo you really want to force decryption?",
        "msg_already_enc": "The file already has an RSA4096 header.\nDo you really want to force encryption?",
        "success_title": "Success",
        "success_msg": "Successfully completed: {desc}!\nThe file has been updated.",
        "save_as_title": "Saved",
        "save_as_msg": "File successfully saved as:\n{path}",
        "batch_done_title": "Batch Result",
        "batch_done_msg": "Batch processing completed!\nSuccess: {succ}\nErrors: {err}",
        "desc_dec": "Decrypted",
        "desc_enc": "Encrypted",
        "state_enc": "🔒 Encrypted",
        "state_dec": "🔓 Decrypted",
        "pending": "Pending"
    }
}

def xor_bytes(data: bytes) -> bytes:
    k1_len = len(KEY1)
    k2_len = len(KEY2)
    res = bytearray(len(data))
    for i in range(len(data)):
        res[i] = data[i] ^ KEY2[i % k2_len] ^ KEY1[i % k1_len]
    return bytes(res)

def is_encrypted(data: bytes) -> bool:
    return data.startswith(HEADER)

def decrypt_data(data: bytes) -> bytes:
    if not is_encrypted(data):
        raise ValueError("Plik nie ma nagłówka RSA4096 / File does not have RSA4096 header.")
    payload = data[len(HEADER):]
    return xor_bytes(payload)

def encrypt_data(data: bytes) -> bytes:
    if is_encrypted(data):
        raise ValueError("Plik ma już nagłówek RSA4096 / File already has RSA4096 header.")
    return HEADER + xor_bytes(data)

def toggle_data(data: bytes) -> tuple[bytes, bool]:
    if is_encrypted(data):
        return decrypt_data(data), True
    else:
        return encrypt_data(data), False

def generate_hex_dump(data: bytes, max_bytes: int = 256) -> str:
    if not data:
        return "(Pusty plik / Empty file)"
    lines = []
    chunk = data[:max_bytes]
    for i in range(0, len(chunk), 16):
        sub = chunk[i:i+16]
        hex_part = " ".join(f"{b:02X}" for b in sub)
        if len(sub) < 16:
            hex_part = hex_part.ljust(16 * 3 - 1)
        ascii_part = "".join(chr(b) if 32 <= b <= 126 else "." for b in sub)
        lines.append(f"{i:04X}:  {hex_part}  |{ascii_part}|")
    if len(data) > max_bytes:
        lines.append(f"... [pominięto pozostałe / skipped remaining {len(data) - max_bytes:,} B]")
    return "\n".join(lines)


class TheocracyApp(tk.Tk):
    def __init__(self, initial_file=None):
        super().__init__()
        self.lang = "pl"
        self.title("Theocracy - Save & File Encoder / Decoder")
        self.geometry("920x720")
        self.minsize(840, 620)

        self.current_path = None
        self.current_data = None

        self._build_ui()
        self.apply_language()
        self.log(self.t("start_msg"))

        if initial_file and os.path.exists(initial_file):
            self.load_file(initial_file)

    def t(self, key: str) -> str:
        return TEXTS[self.lang].get(key, key)

    def _build_ui(self):
        # Header banner
        header = tk.Frame(self, bg="#1a2436", height=68)
        header.pack(fill=tk.X, side=tk.TOP)
        header.pack_propagate(False)

        self.lbl_title = tk.Label(header, text="", font=("Segoe UI", 13, "bold"), fg="#f0c040", bg="#1a2436")
        self.lbl_title.pack(anchor="w", padx=20, pady=(8, 0))

        self.lbl_sub = tk.Label(header, text="", font=("Segoe UI", 8), fg="#c8d2e1", bg="#1a2436")
        self.lbl_sub.pack(anchor="w", padx=20, pady=(2, 5))

        # Language dropdown in top right of header
        lang_frame = tk.Frame(header, bg="#1a2436")
        lang_frame.place(relx=1.0, rely=0.5, anchor="e", x=-20)

        self.lbl_lang_head = tk.Label(lang_frame, text="🌐 Język / Language:", font=("Segoe UI", 8), fg="#b0c0d8", bg="#1a2436")
        self.lbl_lang_head.pack(side=tk.LEFT, padx=6)

        self.combo_lang = ttk.Combobox(lang_frame, values=["🇵🇱 Polski", "🇬🇧 English"], state="readonly", width=12)
        self.combo_lang.current(0)
        self.combo_lang.bind("<<ComboboxSelected>>", self.on_lang_changed)
        self.combo_lang.pack(side=tk.LEFT)

        # Status Bar at bottom
        self.status_var = tk.StringVar(value="")
        status_bar = tk.Label(self, textvariable=self.status_var, bd=1, relief=tk.SUNKEN, anchor="w",
                              font=("Segoe UI", 8), bg="#e1e6eb", fg="#333333")
        status_bar.pack(side=tk.BOTTOM, fill=tk.X)

        # Log box at bottom
        self.log_frame = ttk.LabelFrame(self, text="", padding=6)
        self.log_frame.pack(side=tk.BOTTOM, fill=tk.X, padx=10, pady=(0, 6))

        self.txt_log = tk.Text(self.log_frame, height=5, bg="#1e1e1e", fg="#dedede",
                               font=("Consolas", 8), wrap="none")
        scroll_log = ttk.Scrollbar(self.log_frame, orient=tk.VERTICAL, command=self.txt_log.yview)
        self.txt_log.configure(yscrollcommand=scroll_log.set)
        self.txt_log.pack(side=tk.LEFT, fill=tk.BOTH, expand=True)
        scroll_log.pack(side=tk.RIGHT, fill=tk.Y)

        # Tabs
        self.tabs = ttk.Notebook(self)
        self.tabs.pack(fill=tk.BOTH, expand=True, padx=10, pady=6)

        self.tab_single = ttk.Frame(self.tabs, padding=8)
        self.tab_batch = ttk.Frame(self.tabs, padding=8)

        self.tabs.add(self.tab_single, text="")
        self.tabs.add(self.tab_batch, text="")

        self._build_single_tab(self.tab_single)
        self._build_batch_tab(self.tab_batch)

    def _build_single_tab(self, parent):
        self.sel_frame = ttk.LabelFrame(parent, text="", padding=8)
        self.sel_frame.pack(fill=tk.X, pady=(0, 6))

        self.lbl_path = ttk.Label(self.sel_frame, text="")
        self.lbl_path.grid(row=0, column=0, sticky="w", padx=(0, 6))

        self.entry_path = ttk.Entry(self.sel_frame, width=65)
        self.entry_path.grid(row=0, column=1, sticky="ew", padx=(0, 6))
        self.sel_frame.columnconfigure(1, weight=1)

        self.btn_browse = ttk.Button(self.sel_frame, text="", command=self.browse_single_file)
        self.btn_browse.grid(row=0, column=2, padx=2)

        # Info frame
        self.info_frame = ttk.LabelFrame(parent, text="", padding=8)
        self.info_frame.pack(fill=tk.X, pady=(0, 6))

        self.lbl_size = ttk.Label(self.info_frame, text="", font=("Segoe UI", 9))
        self.lbl_size.grid(row=0, column=0, sticky="w", padx=4, pady=2)

        self.lbl_mtime = ttk.Label(self.info_frame, text="", font=("Segoe UI", 9))
        self.lbl_mtime.grid(row=0, column=1, sticky="w", padx=20, pady=2)

        self.lbl_badge = tk.Label(self.info_frame, text="", font=("Segoe UI", 9, "bold"),
                                  bg="#b0b5be", fg="white", width=26, height=1, relief=tk.RAISED)
        self.lbl_badge.grid(row=0, column=2, sticky="e", padx=4, pady=2)
        self.info_frame.columnconfigure(2, weight=1)

        self.lbl_details = ttk.Label(self.info_frame, text="", font=("Segoe UI", 8, "italic"), foreground="#445566")
        self.lbl_details.grid(row=1, column=0, columnspan=3, sticky="w", padx=4, pady=(4, 0))

        # Actions frame
        self.act_frame = ttk.LabelFrame(parent, text="", padding=8)
        self.act_frame.pack(fill=tk.X, pady=(0, 6))

        self.btn_decode = tk.Button(self.act_frame, text="", font=("Segoe UI", 9, "bold"),
                                    bg="#27ae60", fg="white", activebackground="#219150", activeforeground="white",
                                    relief=tk.FLAT, padx=12, pady=6, state=tk.DISABLED, command=lambda: self.execute_single("decode"))
        self.btn_decode.grid(row=0, column=0, padx=4, pady=4)

        self.btn_encode = tk.Button(self.act_frame, text="", font=("Segoe UI", 9, "bold"),
                                    bg="#e67e22", fg="white", activebackground="#cf6d17", activeforeground="white",
                                    relief=tk.FLAT, padx=12, pady=6, state=tk.DISABLED, command=lambda: self.execute_single("encode"))
        self.btn_encode.grid(row=0, column=1, padx=4, pady=4)

        self.btn_toggle = tk.Button(self.act_frame, text="", font=("Segoe UI", 9, "bold"),
                                    bg="#34495e", fg="white", activebackground="#2c3e50", activeforeground="white",
                                    relief=tk.FLAT, padx=12, pady=6, state=tk.DISABLED, command=lambda: self.execute_single("toggle"))
        self.btn_toggle.grid(row=0, column=2, padx=4, pady=4)

        self.btn_save_as = tk.Button(self.act_frame, text="", font=("Segoe UI", 9),
                                     bg="#8e44ad", fg="white", activebackground="#763a91", activeforeground="white",
                                     relief=tk.FLAT, padx=12, pady=6, state=tk.DISABLED, command=self.save_single_as)
        self.btn_save_as.grid(row=0, column=3, padx=4, pady=4)

        self.var_backup = tk.BooleanVar(value=True)
        self.chk_bak = ttk.Checkbutton(self.act_frame, text="", variable=self.var_backup)
        self.chk_bak.grid(row=0, column=4, padx=16, pady=4, sticky="e")
        self.act_frame.columnconfigure(4, weight=1)

        # Hex Dump frame
        self.hex_frame = ttk.LabelFrame(parent, text="", padding=8)
        self.hex_frame.pack(fill=tk.BOTH, expand=True)

        self.txt_hex = tk.Text(self.hex_frame, bg="#181a20", fg="#ffffb0", font=("Consolas", 9), wrap="none")
        scroll_hex_y = ttk.Scrollbar(self.hex_frame, orient=tk.VERTICAL, command=self.txt_hex.yview)
        scroll_hex_x = ttk.Scrollbar(self.hex_frame, orient=tk.HORIZONTAL, command=self.txt_hex.xview)
        self.txt_hex.configure(yscrollcommand=scroll_hex_y.set, xscrollcommand=scroll_hex_x.set)

        self.txt_hex.pack(side=tk.LEFT, fill=tk.BOTH, expand=True)
        scroll_hex_y.pack(side=tk.RIGHT, fill=tk.Y)
        scroll_hex_x.pack(side=tk.BOTTOM, fill=tk.X)

    def _build_batch_tab(self, parent):
        top_bar = ttk.Frame(parent)
        top_bar.pack(fill=tk.X, pady=(0, 6))

        self.btn_batch_add_files = ttk.Button(top_bar, text="", command=self.batch_add_files)
        self.btn_batch_add_files.pack(side=tk.LEFT, padx=3)

        self.btn_batch_add_folder = ttk.Button(top_bar, text="", command=self.batch_add_folder)
        self.btn_batch_add_folder.pack(side=tk.LEFT, padx=3)

        self.btn_batch_clear = ttk.Button(top_bar, text="", command=self.batch_clear)
        self.btn_batch_clear.pack(side=tk.LEFT, padx=3)

        self.var_batch_bak = tk.BooleanVar(value=True)
        self.chk_b_bak = ttk.Checkbutton(top_bar, text="", variable=self.var_batch_bak)
        self.chk_b_bak.pack(side=tk.LEFT, padx=16)

        cols = ("file", "state", "size", "path", "result")
        self.tree_batch = ttk.Treeview(parent, columns=cols, show="headings", selectmode="extended")
        self.tree_batch.column("file", width=160)
        self.tree_batch.column("state", width=130)
        self.tree_batch.column("size", width=100)
        self.tree_batch.column("path", width=300)
        self.tree_batch.column("result", width=140)

        tree_scroll = ttk.Scrollbar(parent, orient=tk.VERTICAL, command=self.tree_batch.yview)
        self.tree_batch.configure(yscrollcommand=tree_scroll.set)

        self.tree_batch.pack(side=tk.TOP, fill=tk.BOTH, expand=True)
        tree_scroll.pack(side=tk.RIGHT, fill=tk.Y)

        bottom_bar = ttk.Frame(parent)
        bottom_bar.pack(fill=tk.X, pady=(8, 0))

        self.btn_b_dec = tk.Button(bottom_bar, text="", font=("Segoe UI", 9, "bold"),
                                   bg="#27ae60", fg="white", relief=tk.FLAT, padx=10, pady=5,
                                   command=lambda: self.execute_batch("decode"))
        self.btn_b_dec.pack(side=tk.LEFT, padx=3)

        self.btn_b_enc = tk.Button(bottom_bar, text="", font=("Segoe UI", 9, "bold"),
                                   bg="#e67e22", fg="white", relief=tk.FLAT, padx=10, pady=5,
                                   command=lambda: self.execute_batch("encode"))
        self.btn_b_enc.pack(side=tk.LEFT, padx=3)

        self.btn_b_tog = tk.Button(bottom_bar, text="", font=("Segoe UI", 9, "bold"),
                                   bg="#34495e", fg="white", relief=tk.FLAT, padx=10, pady=5,
                                   command=lambda: self.execute_batch("toggle"))
        self.btn_b_tog.pack(side=tk.LEFT, padx=3)

        self.prg_batch = ttk.Progressbar(bottom_bar, orient=tk.HORIZONTAL, mode="determinate", length=180)
        self.prg_batch.pack(side=tk.RIGHT, padx=6)

    def on_lang_changed(self, event=None):
        idx = self.combo_lang.current()
        self.lang = "en" if idx == 1 else "pl"
        self.apply_language()
        self.log(self.t("switched_lang"))

    def apply_language(self):
        self.lbl_title.config(text=self.t("title"))
        self.lbl_sub.config(text=self.t("subtitle"))
        self.lbl_lang_head.config(text=self.t("lang_lbl"))

        self.tabs.tab(self.tab_single, text=self.t("tab_single"))
        self.tabs.tab(self.tab_batch, text=self.t("tab_batch"))

        self.sel_frame.config(text=self.t("sel_file"))
        self.lbl_path.config(text=self.t("path_lbl"))
        self.btn_browse.config(text=self.t("browse"))

        self.info_frame.config(text=self.t("info_frame"))
        self.act_frame.config(text=self.t("act_frame"))

        self.btn_decode.config(text=self.t("decode"))
        self.btn_encode.config(text=self.t("encode"))
        self.btn_toggle.config(text=self.t("toggle"))
        self.btn_save_as.config(text=self.t("save_as"))
        self.chk_bak.config(text=self.t("backup"))

        self.hex_frame.config(text=self.t("hex_frame"))

        self.btn_batch_add_files.config(text=self.t("batch_add_files"))
        self.btn_batch_add_folder.config(text=self.t("batch_add_folder"))
        self.btn_batch_clear.config(text=self.t("batch_clear"))
        self.chk_b_bak.config(text=self.t("batch_backup"))

        self.tree_batch.heading("file", text=self.t("col_file"))
        self.tree_batch.heading("state", text=self.t("col_state"))
        self.tree_batch.heading("size", text=self.t("col_size"))
        self.tree_batch.heading("path", text=self.t("col_path"))
        self.tree_batch.heading("result", text=self.t("col_result"))

        self.btn_b_dec.config(text=self.t("batch_decode"))
        self.btn_b_enc.config(text=self.t("batch_encode"))
        self.btn_b_tog.config(text=self.t("batch_toggle"))

        self.log_frame.config(text=self.t("log_frame"))

        # Update currently loaded file view
        if self.current_data is not None and self.current_path:
            self.lbl_size.config(text=self.t("size_fmt").format(size=len(self.current_data), kb=len(self.current_data)/1024))
            mtime = datetime.datetime.fromtimestamp(os.path.getmtime(self.current_path)).strftime("%Y-%m-%d %H:%M:%S")
            self.lbl_mtime.config(text=self.t("mtime_fmt").format(mtime=mtime))

            is_enc = is_encrypted(self.current_data)
            if is_enc:
                self.lbl_badge.config(text=self.t("badge_enc"), bg="#2e7d32")
                extra = self.t("details_enc").format(size=len(self.current_data)-7)
                peek = xor_bytes(self.current_data[7:135])
                if b"theosg" in peek:
                    idx = peek.find(b"theosg")
                    sig = peek[idx:idx+8].decode("ascii", errors="ignore")
                    extra += self.t("save_sig").format(sig=sig)
                elif b"[" in peek:
                    extra += self.t("config_sig")
                self.lbl_details.config(text=extra)
            else:
                self.lbl_badge.config(text=self.t("badge_dec"), bg="#1565c0")
                extra = self.t("details_dec")
                if b"theosg" in self.current_data[:128]:
                    idx = self.current_data.find(b"theosg")
                    sig = self.current_data[idx:idx+8].decode("ascii", errors="ignore")
                    extra += self.t("save_sig").format(sig=sig)
                    try:
                        date_str = self.current_data[:10].decode("ascii")
                        if "/" in date_str:
                            extra += self.t("save_date").format(date=date_str)
                    except Exception:
                        pass
                elif self.current_data.startswith(b"["):
                    extra += self.t("config_sig")
                self.lbl_details.config(text=extra)
        else:
            self.lbl_size.config(text=self.t("size_no_file"))
            self.lbl_mtime.config(text=self.t("mtime_no_file"))
            self.lbl_badge.config(text=self.t("badge_none"))
            self.lbl_details.config(text=self.t("details_hint"))
            self.status_var.set(self.t("ready"))

    def log(self, message: str):
        now = datetime.datetime.now().strftime("%H:%M:%S")
        line = f"[{now}] {message}\n"
        self.txt_log.insert(tk.END, line)
        self.txt_log.see(tk.END)
        self.status_var.set(message)

    def browse_single_file(self):
        f = filedialog.askopenfilename(
            title=self.t("sel_file"),
            filetypes=[
                ("Supported files (*.tsg; *.txt)", "*.tsg;*.txt"),
                ("Theocracy Saves (*.tsg)", "*.tsg"),
                ("Config files (*.txt)", "*.txt"),
                ("All files (*.*)", "*.*")
            ]
        )
        if f:
            self.load_file(f)

    def load_file(self, path: str):
        try:
            if not os.path.exists(path):
                messagebox.showerror("Error", f"File not found:\n{path}")
                return

            with open(path, "rb") as fp:
                data = fp.read()

            self.current_path = path
            self.current_data = data

            self.entry_path.delete(0, tk.END)
            self.entry_path.insert(0, path)

            is_enc = is_encrypted(data)
            if is_enc:
                self.btn_decode.config(state=tk.NORMAL)
                self.btn_encode.config(state=tk.DISABLED)
            else:
                self.btn_decode.config(state=tk.DISABLED)
                self.btn_encode.config(state=tk.NORMAL)

            self.btn_toggle.config(state=tk.NORMAL)
            self.btn_save_as.config(state=tk.NORMAL)

            self.txt_hex.delete("1.0", tk.END)
            self.txt_hex.insert("1.0", generate_hex_dump(data, 256))

            self.apply_language()
            self.log(f"{self.t('ready')}: {os.path.basename(path)} ({len(data):,} B)")
        except Exception as e:
            messagebox.showerror("Error", str(e))
            self.log(f"[ERROR] {e}")

    def execute_single(self, action: str):
        if not self.current_path or self.current_data is None:
            return

        try:
            is_enc = is_encrypted(self.current_data)

            if action == "decode":
                if not is_enc:
                    if not messagebox.askyesno(self.t("confirm_title"), self.t("msg_already_dec")):
                        return
                output = decrypt_data(self.current_data)
                desc = self.t("desc_dec")
            elif action == "encode":
                if is_enc:
                    if not messagebox.askyesno(self.t("confirm_title"), self.t("msg_already_enc")):
                        return
                output = encrypt_data(self.current_data)
                desc = self.t("desc_enc")
            else:
                output, was_enc = toggle_data(self.current_data)
                desc = self.t("desc_dec") if was_enc else self.t("desc_enc")

            if self.var_backup.get():
                bak_path = self.current_path + ".bak"
                shutil.copy2(self.current_path, bak_path)

            with open(self.current_path, "wb") as fp:
                fp.write(output)

            self.log(f"[SUCCESS] {desc}: {os.path.basename(self.current_path)} ({len(self.current_data):,} B -> {len(output):,} B)")
            messagebox.showinfo(self.t("success_title"), self.t("success_msg").format(desc=desc))
            self.load_file(self.current_path)
        except Exception as e:
            messagebox.showerror("Error", str(e))
            self.log(f"[ERROR] {e}")

    def save_single_as(self):
        if not self.current_path or self.current_data is None:
            return

        dir_name, base_name = os.path.split(self.current_path)
        name, ext = os.path.splitext(base_name)
        is_enc = is_encrypted(self.current_data)
        sugg = f"{name}_decrypted{ext}" if is_enc else f"{name}_encrypted{ext}"

        out_path = filedialog.asksaveasfilename(
            title=self.t("save_as"),
            initialdir=dir_name,
            initialfile=sugg,
            filetypes=[(f"Files (*{ext})", f"*{ext}"), ("All files", "*.*")]
        )
        if not out_path:
            return

        try:
            out_data = decrypt_data(self.current_data) if is_enc else encrypt_data(self.current_data)
            with open(out_path, "wb") as fp:
                fp.write(out_data)

            self.log(f"Saved new file: {os.path.basename(out_path)} ({len(out_data):,} B)")
            messagebox.showinfo(self.t("save_as_title"), self.t("save_as_msg").format(path=out_path))
        except Exception as e:
            messagebox.showerror("Error", str(e))

    def batch_add_files(self):
        files = filedialog.askopenfilenames(
            title=self.t("batch_add_files"),
            filetypes=[
                ("Theocracy Files (*.tsg; *.txt)", "*.tsg;*.txt"),
                ("All files", "*.*")
            ]
        )
        if files:
            for f in files:
                self._add_to_batch_tree(f)

    def batch_add_folder(self):
        d = filedialog.askdirectory(title=self.t("batch_add_folder"))
        if d:
            for entry in os.listdir(d):
                if entry.lower().endswith((".tsg", ".txt")):
                    full = os.path.join(d, entry)
                    if os.path.isfile(full):
                        self._add_to_batch_tree(full)

    def _add_to_batch_tree(self, path):
        for item in self.tree_batch.get_children():
            if self.tree_batch.item(item, "values")[3] == path:
                return

        try:
            size = os.path.getsize(path)
            with open(path, "rb") as fp:
                head = fp.read(7)
            state = self.t("state_enc") if head == HEADER else self.t("state_dec")
            self.tree_batch.insert("", tk.END, values=(os.path.basename(path), state, f"{size:,} B", path, self.t("pending")))
        except Exception:
            pass

    def batch_clear(self):
        for item in self.tree_batch.get_children():
            self.tree_batch.delete(item)

    def execute_batch(self, action: str):
        items = self.tree_batch.get_children()
        if not items:
            return

        self.prg_batch["maximum"] = len(items)
        self.prg_batch["value"] = 0

        succ = 0
        err = 0

        for i, item in enumerate(items):
            vals = list(self.tree_batch.item(item, "values"))
            path = vals[3]

            try:
                with open(path, "rb") as fp:
                    data = fp.read()

                is_enc = is_encrypted(data)
                if action == "decode":
                    if not is_enc:
                        vals[4] = "Skipped"
                        self.tree_batch.item(item, values=vals)
                        continue
                    out = decrypt_data(data)
                    desc = self.t("desc_dec")
                elif action == "encode":
                    if is_enc:
                        vals[4] = "Skipped"
                        self.tree_batch.item(item, values=vals)
                        continue
                    out = encrypt_data(data)
                    desc = self.t("desc_enc")
                else:
                    out, was_enc = toggle_data(data)
                    desc = self.t("desc_dec") if was_enc else self.t("desc_enc")

                if self.var_batch_bak.get():
                    shutil.copy2(path, path + ".bak")

                with open(path, "wb") as fp:
                    fp.write(out)

                new_enc = is_encrypted(out)
                vals[1] = self.t("state_enc") if new_enc else self.t("state_dec")
                vals[2] = f"{len(out):,} B"
                vals[4] = f"OK ({desc})"
                self.tree_batch.item(item, values=vals)
                succ += 1
            except Exception as e:
                vals[4] = f"Error: {e}"
                self.tree_batch.item(item, values=vals)
                err += 1
                self.log(f"[BATCH ERROR] {os.path.basename(path)}: {e}")

            self.prg_batch["value"] = i + 1
            self.update_idletasks()

        self.log(f"Batch completed. Success: {succ}, Errors: {err}.")
        messagebox.showinfo(self.t("batch_done_title"), self.t("batch_done_msg").format(succ=succ, err=err))

        if self.current_path and os.path.exists(self.current_path):
            self.load_file(self.current_path)


if __name__ == "__main__":
    initial = sys.argv[1] if len(sys.argv) > 1 else None
    app = TheocracyApp(initial)
    app.mainloop()
