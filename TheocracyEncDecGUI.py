#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Theocracy Save & File Encoder / Decoder - GUI
Narzędzie do szyfrowania i odszyfrowywania plików zapisu (.tsg) oraz plików konfiguracyjnych gry Theocracy.
"""

import os
import sys
import shutil
import datetime
import tkinter as tk
from tkinter import ttk, filedialog, messagebox

KEY1 = b"theocracy sux"       # 13 bajtów (0x0D)
KEY2 = b"mutant technology"  # 17 bajtów (0x11)
HEADER = b"RSA4096"          # 7 bajtów

def xor_bytes(data: bytes) -> bytes:
    """Szyfrowanie/deszyfrowanie XOR identycznym algorytmem z TheocracyEncDec.cpp"""
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
        raise ValueError("Plik nie ma nagłówka RSA4096 (prawdopodobnie jest już odszyfrowany).")
    payload = data[len(HEADER):]
    return xor_bytes(payload)

def encrypt_data(data: bytes) -> bytes:
    if is_encrypted(data):
        raise ValueError("Plik ma już nagłówek RSA4096 (jest już zaszyfrowany).")
    return HEADER + xor_bytes(data)

def toggle_data(data: bytes) -> tuple[bytes, bool]:
    if is_encrypted(data):
        return decrypt_data(data), True
    else:
        return encrypt_data(data), False

def generate_hex_dump(data: bytes, max_bytes: int = 256) -> str:
    if not data:
        return "(Pusty plik)"
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
        lines.append(f"... [pominięto pozostałe {len(data) - max_bytes:,} bajtów]")
    return "\n".join(lines)


class TheocracyApp(tk.Tk):
    def __init__(self, initial_file=None):
        super().__init__()
        self.title("Theocracy - Save & File Encoder / Decoder")
        self.geometry("900x700")
        self.minsize(820, 600)

        self.current_path = None
        self.current_data = None

        self._build_ui()
        self.log("Program Theocracy Encoder / Decoder gotowy do pracy.")

        if initial_file and os.path.exists(initial_file):
            self.load_file(initial_file)

    def _build_ui(self):
        # Header banner
        header = tk.Frame(self, bg="#1a2436", height=65)
        header.pack(fill=tk.X, side=tk.TOP)
        header.pack_propagate(False)

        title = tk.Label(header, text="THEOCRACY - Save & File Encoder / Decoder",
                         font=("Segoe UI", 13, "bold"), fg="#f0c040", bg="#1a2436")
        title.pack(anchor="w", padx=20, pady=(10, 0))

        sub = tk.Label(header, text="Narzędzie do szyfrowania i odszyfrowywania save'ów (.tsg) oraz plików gry Theocracy",
                       font=("Segoe UI", 8), fg="#c8d2e1", bg="#1a2436")
        sub.pack(anchor="w", padx=20, pady=(2, 5))

        # Status Bar at bottom
        self.status_var = tk.StringVar(value="Gotowy")
        status_bar = tk.Label(self, textvariable=self.status_var, bd=1, relief=tk.SUNKEN, anchor="w",
                              font=("Segoe UI", 8), bg="#e1e6eb", fg="#333333")
        status_bar.pack(side=tk.BOTTOM, fill=tk.X)

        # Log box at bottom
        log_frame = ttk.LabelFrame(self, text="Dziennik zdarzeń (Log)")
        log_frame.pack(side=tk.BOTTOM, fill=tk.X, padx=12, pady=(0, 6))

        self.txt_log = tk.Text(log_frame, height=5, bg="#1e1e1e", fg="#dedede",
                               font=("Consolas", 8), wrap="none")
        scroll_log = ttk.Scrollbar(log_frame, orient=tk.VERTICAL, command=self.txt_log.yview)
        self.txt_log.configure(yscrollcommand=scroll_log.set)
        self.txt_log.pack(side=tk.LEFT, fill=tk.BOTH, expand=True)
        scroll_log.pack(side=tk.RIGHT, fill=tk.Y)

        # Notebook (Tabs)
        self.tabs = ttk.Notebook(self)
        self.tabs.pack(fill=tk.BOTH, expand=True, padx=10, pady=6)

        tab_single = ttk.Frame(self.tabs, padding=8)
        tab_batch = ttk.Frame(self.tabs, padding=8)

        self.tabs.add(tab_single, text="  Pojedynczy plik  ")
        self.tabs.add(tab_batch, text="  Przetwarzanie wsadowe  ")

        self._build_single_tab(tab_single)
        self._build_batch_tab(tab_batch)

    def _build_single_tab(self, parent):
        # File selector frame
        sel_frame = ttk.LabelFrame(parent, text="Wybór pliku", padding=8)
        sel_frame.pack(fill=tk.X, pady=(0, 6))

        self.lbl_path = ttk.Label(sel_frame, text="Ścieżka do pliku:")
        self.lbl_path.grid(row=0, column=0, sticky="w", padx=(0, 6))

        self.entry_path = ttk.Entry(sel_frame, width=70)
        self.entry_path.grid(row=0, column=1, sticky="ew", padx=(0, 6))
        sel_frame.columnconfigure(1, weight=1)

        btn_browse = ttk.Button(sel_frame, text="📂 Przeglądaj...", command=self.browse_single_file)
        btn_browse.grid(row=0, column=2, padx=2)

        # File info & badge frame
        info_frame = ttk.LabelFrame(parent, text="Informacje o pliku i status", padding=8)
        info_frame.pack(fill=tk.X, pady=(0, 6))

        self.lbl_size = ttk.Label(info_frame, text="Rozmiar: Brak pliku", font=("Segoe UI", 9))
        self.lbl_size.grid(row=0, column=0, sticky="w", padx=4, pady=2)

        self.lbl_mtime = ttk.Label(info_frame, text="Data modyfikacji: -", font=("Segoe UI", 9))
        self.lbl_mtime.grid(row=0, column=1, sticky="w", padx=20, pady=2)

        self.lbl_badge = tk.Label(info_frame, text="BRAK PLIKU", font=("Segoe UI", 9, "bold"),
                                  bg="#b0b5be", fg="white", width=26, height=1, relief=tk.RAISED)
        self.lbl_badge.grid(row=0, column=2, sticky="e", padx=4, pady=2)
        info_frame.columnconfigure(2, weight=1)

        self.lbl_details = ttk.Label(info_frame, text="Wybierz plik .tsg lub .txt, aby wyświetlić szczegóły.",
                                     font=("Segoe UI", 8, "italic"), foreground="#445566")
        self.lbl_details.grid(row=1, column=0, columnspan=3, sticky="w", padx=4, pady=(4, 0))

        # Actions frame
        act_frame = ttk.LabelFrame(parent, text="Operacje szyfrowania / odszyfrowywania", padding=8)
        act_frame.pack(fill=tk.X, pady=(0, 6))

        self.btn_decode = tk.Button(act_frame, text="🔓 Odszyfruj (Decode)", font=("Segoe UI", 9, "bold"),
                                    bg="#27ae60", fg="white", activebackground="#219150", activeforeground="white",
                                    relief=tk.FLAT, padx=12, pady=6, state=tk.DISABLED, command=lambda: self.execute_single("decode"))
        self.btn_decode.grid(row=0, column=0, padx=4, pady=4)

        self.btn_encode = tk.Button(act_frame, text="🔒 Zaszyfruj (Encode)", font=("Segoe UI", 9, "bold"),
                                    bg="#e67e22", fg="white", activebackground="#cf6d17", activeforeground="white",
                                    relief=tk.FLAT, padx=12, pady=6, state=tk.DISABLED, command=lambda: self.execute_single("encode"))
        self.btn_encode.grid(row=0, column=1, padx=4, pady=4)

        self.btn_toggle = tk.Button(act_frame, text="🔄 Przełącz (Auto)", font=("Segoe UI", 9, "bold"),
                                    bg="#34495e", fg="white", activebackground="#2c3e50", activeforeground="white",
                                    relief=tk.FLAT, padx=12, pady=6, state=tk.DISABLED, command=lambda: self.execute_single("toggle"))
        self.btn_toggle.grid(row=0, column=2, padx=4, pady=4)

        self.btn_save_as = tk.Button(act_frame, text="💾 Zapisz jako...", font=("Segoe UI", 9),
                                     bg="#8e44ad", fg="white", activebackground="#763a91", activeforeground="white",
                                     relief=tk.FLAT, padx=12, pady=6, state=tk.DISABLED, command=self.save_single_as)
        self.btn_save_as.grid(row=0, column=3, padx=4, pady=4)

        self.var_backup = tk.BooleanVar(value=True)
        chk_bak = ttk.Checkbutton(act_frame, text="Twórz kopię zapasową (.bak) przed zapisem", variable=self.var_backup)
        chk_bak.grid(row=0, column=4, padx=16, pady=4, sticky="e")
        act_frame.columnconfigure(4, weight=1)

        # Hex Dump frame
        hex_frame = ttk.LabelFrame(parent, text="Podgląd heksadecymalny i tekstowy (pierwsze 256 bajtów)", padding=8)
        hex_frame.pack(fill=tk.BOTH, expand=True)

        self.txt_hex = tk.Text(hex_frame, bg="#181a20", fg="#ffffb0", font=("Consolas", 9), wrap="none")
        scroll_hex_y = ttk.Scrollbar(hex_frame, orient=tk.VERTICAL, command=self.txt_hex.yview)
        scroll_hex_x = ttk.Scrollbar(hex_frame, orient=tk.HORIZONTAL, command=self.txt_hex.xview)
        self.txt_hex.configure(yscrollcommand=scroll_hex_y.set, xscrollcommand=scroll_hex_x.set)

        self.txt_hex.pack(side=tk.LEFT, fill=tk.BOTH, expand=True)
        scroll_hex_y.pack(side=tk.RIGHT, fill=tk.Y)
        scroll_hex_x.pack(side=tk.BOTTOM, fill=tk.X)

    def _build_batch_tab(self, parent):
        top_bar = ttk.Frame(parent)
        top_bar.pack(fill=tk.X, pady=(0, 6))

        btn_add_files = ttk.Button(top_bar, text="📂 Dodaj pliki...", command=self.batch_add_files)
        btn_add_files.pack(side=tk.LEFT, padx=3)

        btn_add_folder = ttk.Button(top_bar, text="📁 Dodaj folder...", command=self.batch_add_folder)
        btn_add_folder.pack(side=tk.LEFT, padx=3)

        btn_clear = ttk.Button(top_bar, text="🗑 Wyczyść listę", command=self.batch_clear)
        btn_clear.pack(side=tk.LEFT, padx=3)

        self.var_batch_bak = tk.BooleanVar(value=True)
        chk_b_bak = ttk.Checkbutton(top_bar, text="Twórz kopie zapasowe (.bak)", variable=self.var_batch_bak)
        chk_b_bak.pack(side=tk.LEFT, padx=16)

        # Treeview / List
        cols = ("file", "state", "size", "path", "result")
        self.tree_batch = ttk.Treeview(parent, columns=cols, show="headings", selectmode="extended")
        self.tree_batch.heading("file", text="Nazwa pliku")
        self.tree_batch.heading("state", text="Stan obecny")
        self.tree_batch.heading("size", text="Rozmiar")
        self.tree_batch.heading("path", text="Pełna ścieżka")
        self.tree_batch.heading("result", text="Wynik operacji")

        self.tree_batch.column("file", width=160)
        self.tree_batch.column("state", width=130)
        self.tree_batch.column("size", width=100)
        self.tree_batch.column("path", width=300)
        self.tree_batch.column("result", width=140)

        tree_scroll = ttk.Scrollbar(parent, orient=tk.VERTICAL, command=self.tree_batch.yview)
        self.tree_batch.configure(yscrollcommand=tree_scroll.set)

        self.tree_batch.pack(side=tk.TOP, fill=tk.BOTH, expand=True)
        tree_scroll.pack(side=tk.RIGHT, fill=tk.Y)

        # Bottom batch actions
        bottom_bar = ttk.Frame(parent)
        bottom_bar.pack(fill=tk.X, pady=(8, 0))

        btn_b_dec = tk.Button(bottom_bar, text="🔓 Odszyfruj wszystkie z listy", font=("Segoe UI", 9, "bold"),
                              bg="#27ae60", fg="white", relief=tk.FLAT, padx=10, pady=5,
                              command=lambda: self.execute_batch("decode"))
        btn_b_dec.pack(side=tk.LEFT, padx=3)

        btn_b_enc = tk.Button(bottom_bar, text="🔒 Zaszyfruj wszystkie z listy", font=("Segoe UI", 9, "bold"),
                              bg="#e67e22", fg="white", relief=tk.FLAT, padx=10, pady=5,
                              command=lambda: self.execute_batch("encode"))
        btn_b_enc.pack(side=tk.LEFT, padx=3)

        btn_b_tog = tk.Button(bottom_bar, text="🔄 Przełącz wszystkie (Auto)", font=("Segoe UI", 9, "bold"),
                              bg="#34495e", fg="white", relief=tk.FLAT, padx=10, pady=5,
                              command=lambda: self.execute_batch("toggle"))
        btn_b_tog.pack(side=tk.LEFT, padx=3)

        self.prg_batch = ttk.Progressbar(bottom_bar, orient=tk.HORIZONTAL, mode="determinate", length=180)
        self.prg_batch.pack(side=tk.RIGHT, padx=6)

    def log(self, message: str):
        now = datetime.datetime.now().strftime("%H:%M:%S")
        line = f"[{now}] {message}\n"
        self.txt_log.insert(tk.END, line)
        self.txt_log.see(tk.END)
        self.status_var.set(message)

    def browse_single_file(self):
        f = filedialog.askopenfilename(
            title="Wybierz plik do dekodowania/kodowania",
            filetypes=[
                ("Wszystkie obsługiwane", "*.tsg;*.txt"),
                ("Pliki zapisu Theocracy (*.tsg)", "*.tsg"),
                ("Pliki konfiguracyjne (*.txt)", "*.txt"),
                ("Wszystkie pliki", "*.*")
            ]
        )
        if f:
            self.load_file(f)

    def load_file(self, path: str):
        try:
            if not os.path.exists(path):
                messagebox.showerror("Błąd", f"Nie znaleziono pliku:\n{path}")
                return

            with open(path, "rb") as fp:
                data = fp.read()

            self.current_path = path
            self.current_data = data

            self.entry_path.delete(0, tk.END)
            self.entry_path.insert(0, path)

            size_str = f"Rozmiar: {len(data):,} B ({len(data)/1024:.1f} KB)"
            mtime = datetime.datetime.fromtimestamp(os.path.getmtime(path)).strftime("%Y-%m-%d %H:%M:%S")
            self.lbl_size.config(text=size_str)
            self.lbl_mtime.config(text=f"Data modyfikacji: {mtime}")

            is_enc = is_encrypted(data)
            if is_enc:
                self.lbl_badge.config(text="🔒 ZASZYFROWANY [RSA4096]", bg="#2e7d32")
                self.btn_decode.config(state=tk.NORMAL)
                self.btn_encode.config(state=tk.DISABLED)

                # Inspect payload preview
                peek = xor_bytes(data[7:135])
                extra = f"Zaszyfrowany plik gry Theocracy (nagłówek RSA4096). Rozmiar danych: {len(data)-7:,} B."
                if b"theosg" in peek:
                    extra += " [Wykryto sygnaturę zapisu gry Theocracy (theosg)]"
                elif b"[" in peek:
                    extra += " [Wykryto plik tekstowy / konfigurację]"
                self.lbl_details.config(text=extra)
            else:
                self.lbl_badge.config(text="🔓 ODSZYFROWANY [Jawny]", bg="#1565c0")
                self.btn_decode.config(state=tk.DISABLED)
                self.btn_encode.config(state=tk.NORMAL)

                extra = "Odszyfrowany plik (brak nagłówka RSA4096)."
                if b"theosg" in data[:128]:
                    date_str = ""
                    try:
                        ascii_start = data[:10].decode("ascii")
                        if "/" in ascii_start:
                            date_str = f"Data w grze: {ascii_start}"
                    except Exception:
                        pass
                    extra += f" Wykryto zapis gry Theocracy (theosg42). {date_str}"
                elif data.startswith(b"["):
                    extra += " Wykryto tekstowy plik INI / konfiguracji."
                self.lbl_details.config(text=extra)

            self.btn_toggle.config(state=tk.NORMAL)
            self.btn_save_as.config(state=tk.NORMAL)

            # Hex dump
            self.txt_hex.delete("1.0", tk.END)
            self.txt_hex.insert("1.0", generate_hex_dump(data, 256))

            self.log(f"Wczytano: {os.path.basename(path)} ({len(data):,} B, {self.lbl_badge['text']})")
        except Exception as e:
            messagebox.showerror("Błąd odczytu", str(e))
            self.log(f"[BŁĄD] Nie udało się wczytać pliku: {e}")

    def execute_single(self, action: str):
        if not self.current_path or self.current_data is None:
            messagebox.showwarning("Uwaga", "Najpierw wybierz plik.")
            return

        try:
            is_enc = is_encrypted(self.current_data)

            if action == "decode":
                if not is_enc:
                    if not messagebox.askyesno("Potwierdzenie", "Plik wygląda na już odszyfrowany (brak nagłówka RSA4096).\nCzy na pewno wymusić odszyfrowanie?"):
                        return
                output = decrypt_data(self.current_data)
                desc = "Odszyfrowano"
            elif action == "encode":
                if is_enc:
                    if not messagebox.askyesno("Potwierdzenie", "Plik ma już nagłówek RSA4096.\nCzy na pewno chcesz zaszyfrować go ponownie?"):
                        return
                output = encrypt_data(self.current_data)
                desc = "Zaszyfrowano"
            else: # toggle
                output, was_enc = toggle_data(self.current_data)
                desc = "Odszyfrowano" if was_enc else "Zaszyfrowano"

            if self.var_backup.get():
                bak_path = self.current_path + ".bak"
                shutil.copy2(self.current_path, bak_path)
                self.log(f"Utworzono kopię zapasową: {os.path.basename(bak_path)}")

            with open(self.current_path, "wb") as fp:
                fp.write(output)

            self.log(f"[SUKCES] {desc} plik: {os.path.basename(self.current_path)} ({len(self.current_data):,} B -> {len(output):,} B)")
            messagebox.showinfo("Sukces", f"Pomyślnie wykonano: {desc}!\nPlik został zaktualizowany.")
            self.load_file(self.current_path)
        except Exception as e:
            messagebox.showerror("Błąd operacji", str(e))
            self.log(f"[BŁĄD] {e}")

    def save_single_as(self):
        if not self.current_path or self.current_data is None:
            return

        dir_name, base_name = os.path.split(self.current_path)
        name, ext = os.path.splitext(base_name)
        is_enc = is_encrypted(self.current_data)
        sugg = f"{name}_decrypted{ext}" if is_enc else f"{name}_encrypted{ext}"

        out_path = filedialog.asksaveasfilename(
            title="Zapisz przetworzony plik jako...",
            initialdir=dir_name,
            initialfile=sugg,
            filetypes=[(f"Pliki {ext}", f"*{ext}"), ("Wszystkie pliki", "*.*")]
        )
        if not out_path:
            return

        try:
            if is_enc:
                out_data = decrypt_data(self.current_data)
            else:
                out_data = encrypt_data(self.current_data)

            with open(out_path, "wb") as fp:
                fp.write(out_data)

            self.log(f"Zapisano nowy plik: {os.path.basename(out_path)} ({len(out_data):,} B)")
            messagebox.showinfo("Zapisano", f"Plik został pomyślnie zapisany jako:\n{out_path}")
        except Exception as e:
            messagebox.showerror("Błąd zapisu", str(e))

    def batch_add_files(self):
        files = filedialog.askopenfilenames(
            title="Wybierz pliki",
            filetypes=[
                ("Pliki Theocracy", "*.tsg;*.txt"),
                ("Pliki zapisu (*.tsg)", "*.tsg"),
                ("Pliki konfiguracyjne (*.txt)", "*.txt"),
                ("Wszystkie", "*.*")
            ]
        )
        if files:
            for f in files:
                self._add_to_batch_tree(f)

    def batch_add_folder(self):
        d = filedialog.askdirectory(title="Wybierz folder z plikami Theocracy")
        if d:
            for entry in os.listdir(d):
                if entry.lower().endswith((".tsg", ".txt")):
                    full = os.path.join(d, entry)
                    if os.path.isfile(full):
                        self._add_to_batch_tree(full)

    def _add_to_batch_tree(self, path):
        # avoid duplicates
        for item in self.tree_batch.get_children():
            if self.tree_batch.item(item, "values")[3] == path:
                return

        try:
            size = os.path.getsize(path)
            with open(path, "rb") as fp:
                head = fp.read(7)
            state = "🔒 Zaszyfrowany" if head == HEADER else "🔓 Odszyfrowany"
            self.tree_batch.insert("", tk.END, values=(os.path.basename(path), state, f"{size:,} B", path, "Oczekuje"))
        except Exception:
            pass

    def batch_clear(self):
        for item in self.tree_batch.get_children():
            self.tree_batch.delete(item)

    def execute_batch(self, action: str):
        items = self.tree_batch.get_children()
        if not items:
            messagebox.showwarning("Uwaga", "Lista plików jest pusta.")
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
                        vals[4] = "Pominięto (jawny)"
                        self.tree_batch.item(item, values=vals)
                        continue
                    out = decrypt_data(data)
                    desc = "Odszyfrowano"
                elif action == "encode":
                    if is_enc:
                        vals[4] = "Pominięto (zaszyfrowany)"
                        self.tree_batch.item(item, values=vals)
                        continue
                    out = encrypt_data(data)
                    desc = "Zaszyfrowano"
                else: # toggle
                    out, was_enc = toggle_data(data)
                    desc = "Odszyfrowano" if was_enc else "Zaszyfrowano"

                if self.var_batch_bak.get():
                    shutil.copy2(path, path + ".bak")

                with open(path, "wb") as fp:
                    fp.write(out)

                new_enc = is_encrypted(out)
                vals[1] = "🔒 Zaszyfrowany" if new_enc else "🔓 Odszyfrowany"
                vals[2] = f"{len(out):,} B"
                vals[4] = f"OK ({desc})"
                self.tree_batch.item(item, values=vals)
                succ += 1
            except Exception as e:
                vals[4] = f"Błąd: {e}"
                self.tree_batch.item(item, values=vals)
                err += 1
                self.log(f"[BŁĄD WSADOWY] {os.path.basename(path)}: {e}")

            self.prg_batch["value"] = i + 1
            self.update_idletasks()

        self.log(f"Zakończono przetwarzanie wsadowe. Sukces: {succ}, Błędy: {err}.")
        messagebox.showinfo("Wynik wsadowy", f"Przetwarzanie zakończone!\nSukces: {succ}\nBłędy: {err}")

        # Refresh single file view if open
        if self.current_path and os.path.exists(self.current_path):
            self.load_file(self.current_path)


if __name__ == "__main__":
    initial = sys.argv[1] if len(sys.argv) > 1 else None
    app = TheocracyApp(initial)
    app.mainloop()
