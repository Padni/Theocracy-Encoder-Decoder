using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TheocracyEncDecGUI
{
    public partial class Form1 : Form
    {
        // Top banner
        private Panel pnlHeader = null!;
        private Label lblTitle = null!;
        private Label lblSubtitle = null!;

        // Tabs
        private TabControl tabControl = null!;
        private TabPage tabSingle = null!;
        private TabPage tabBatch = null!;

        // Tab 1: Single File controls
        private Panel pnlDropZone = null!;
        private Label lblDropHint = null!;
        private Button btnBrowse = null!;

        private GroupBox grpFileInfo = null!;
        private Label lblFilePath = null!;
        private TextBox txtFilePath = null!;
        private Label lblFileSize = null!;
        private Label lblFileModified = null!;
        private Label lblBadge = null!;
        private Label lblDetails = null!;

        private GroupBox grpActions = null!;
        private Button btnDecode = null!;
        private Button btnEncode = null!;
        private Button btnToggle = null!;
        private Button btnSaveAs = null!;
        private CheckBox chkBackup = null!;

        private GroupBox grpHex = null!;
        private TextBox txtHexDump = null!;

        // Tab 2: Batch Processing controls
        private Panel pnlBatchTop = null!;
        private Button btnBatchAddFiles = null!;
        private Button btnBatchAddFolder = null!;
        private Button btnBatchClear = null!;
        private CheckBox chkBatchBackup = null!;

        private ListView lvBatch = null!;
        private Panel pnlBatchBottom = null!;
        private Button btnBatchDecode = null!;
        private Button btnBatchEncode = null!;
        private Button btnBatchToggle = null!;
        private ProgressBar prgBatch = null!;
        private Label lblBatchProgress = null!;

        // Bottom log area
        private GroupBox grpLog = null!;
        private TextBox txtLog = null!;

        // Status bar
        private StatusStrip statusStrip = null!;
        private ToolStripStatusLabel statusLabel = null!;

        // State for single file
        private string? _currentFilePath;
        private byte[]? _currentFileData;
        private FileInspectionResult? _currentInspection;

        public Form1()
        {
            InitializeComponent();
            BuildCustomUi();
            SetupEvents();
            LogMessage("Program Theocracy Encoder / Decoder uruchomiony.");
            LogMessage("Wskazówka: Możesz przeciągnąć plik .tsg lub .txt bezpośrednio do okna programu.");
        }

        private void BuildCustomUi()
        {
            this.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            this.BackColor = Color.FromArgb(245, 247, 250);

            // 1. Top Header Banner
            pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 65,
                BackColor = Color.FromArgb(26, 36, 54),
                Padding = new Padding(20, 10, 20, 10)
            };

            lblTitle = new Label
            {
                Text = "THEOCRACY - Save & File Encoder / Decoder",
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = Color.FromArgb(240, 192, 64), // Aztec gold accent
                AutoSize = true,
                Location = new Point(18, 10)
            };

            lblSubtitle = new Label
            {
                Text = "Narzędzie do szyfrowania i odszyfrowywania save'ów (.tsg) oraz plików konfiguracyjnych gry Theocracy",
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular),
                ForeColor = Color.FromArgb(200, 210, 225),
                AutoSize = true,
                Location = new Point(20, 36)
            };

            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(lblSubtitle);

            // 2. Status Strip at bottom
            statusStrip = new StatusStrip();
            statusLabel = new ToolStripStatusLabel("Gotowy do pracy") { Spring = true, TextAlign = ContentAlignment.MiddleLeft };
            statusStrip.Items.Add(statusLabel);

            // 3. Bottom Log Box
            grpLog = new GroupBox
            {
                Text = "Dziennik zdarzeń (Log)",
                Dock = DockStyle.Bottom,
                Height = 110,
                Padding = new Padding(8)
            };

            txtLog = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                Dock = DockStyle.Fill,
                ScrollBars = ScrollBars.Vertical,
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.FromArgb(220, 220, 220),
                Font = new Font("Consolas", 8.5F)
            };
            grpLog.Controls.Add(txtLog);

            // 4. Tab Control
            tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Padding = new Point(12, 6)
            };

            tabSingle = new TabPage("  Pojedynczy plik  ") { BackColor = Color.FromArgb(245, 247, 250), Padding = new Padding(12) };
            tabBatch = new TabPage("  Przetwarzanie wsadowe (Wiele plików)  ") { BackColor = Color.FromArgb(245, 247, 250), Padding = new Padding(12) };

            tabControl.TabPages.Add(tabSingle);
            tabControl.TabPages.Add(tabBatch);

            BuildSingleFileTab();
            BuildBatchTab();

            // Assemble main controls
            this.Controls.Add(tabControl);
            this.Controls.Add(grpLog);
            this.Controls.Add(statusStrip);
            this.Controls.Add(pnlHeader);

            // Enable Drag & Drop on Form
            this.AllowDrop = true;
        }

        private void BuildSingleFileTab()
        {
            // Drop zone at top of tab 1
            pnlDropZone = new Panel
            {
                Dock = DockStyle.Top,
                Height = 65,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(238, 242, 248),
                AllowDrop = true
            };

            lblDropHint = new Label
            {
                Text = "Przeciągnij i upuść tutaj plik (.tsg, .txt) lub użyj przycisku obok:",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Regular),
                ForeColor = Color.FromArgb(60, 70, 85),
                AutoSize = true,
                Location = new Point(16, 22)
            };

            btnBrowse = new Button
            {
                Text = "📂 Przeglądaj...",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Size = new Size(130, 36),
                Location = new Point(480, 14),
                BackColor = Color.FromArgb(41, 128, 185),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnBrowse.FlatAppearance.BorderSize = 0;

            pnlDropZone.Controls.Add(lblDropHint);
            pnlDropZone.Controls.Add(btnBrowse);

            // File info group
            grpFileInfo = new GroupBox
            {
                Text = "Informacje o wybranym pliku",
                Dock = DockStyle.Top,
                Height = 135,
                Padding = new Padding(10)
            };

            lblFilePath = new Label { Text = "Plik:", Location = new Point(15, 25), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            txtFilePath = new TextBox { Location = new Point(60, 22), Width = 520, ReadOnly = true, BackColor = Color.White };

            lblFileSize = new Label { Text = "Rozmiar: Brak pliku", Location = new Point(15, 52), AutoSize = true };
            lblFileModified = new Label { Text = "Data modyfikacji: -", Location = new Point(250, 52), AutoSize = true };

            lblBadge = new Label
            {
                Text = "BRAK PLIKU",
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Location = new Point(600, 20),
                Size = new Size(240, 32),
                BackColor = Color.FromArgb(180, 185, 195),
                ForeColor = Color.White
            };

            lblDetails = new Label
            {
                Text = "Wybierz plik zapisu gry (.tsg) lub plik konfiguracyjny (.txt), aby zbadać jego strukturę.",
                Location = new Point(15, 78),
                Size = new Size(820, 48),
                ForeColor = Color.FromArgb(50, 60, 75)
            };

            grpFileInfo.Controls.Add(lblFilePath);
            grpFileInfo.Controls.Add(txtFilePath);
            grpFileInfo.Controls.Add(lblFileSize);
            grpFileInfo.Controls.Add(lblFileModified);
            grpFileInfo.Controls.Add(lblBadge);
            grpFileInfo.Controls.Add(lblDetails);

            // Actions group
            grpActions = new GroupBox
            {
                Text = "Operacje szyfrowania / deszyfrowania",
                Dock = DockStyle.Top,
                Height = 85,
                Padding = new Padding(10)
            };

            btnDecode = new Button
            {
                Text = "🔓 Odszyfruj (Decode)",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Size = new Size(180, 40),
                Location = new Point(15, 26),
                BackColor = Color.FromArgb(39, 174, 96), // Green
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Enabled = false,
                Cursor = Cursors.Hand
            };
            btnDecode.FlatAppearance.BorderSize = 0;

            btnEncode = new Button
            {
                Text = "🔒 Zaszyfruj (Encode)",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Size = new Size(180, 40),
                Location = new Point(205, 26),
                BackColor = Color.FromArgb(230, 126, 34), // Orange/Amber
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Enabled = false,
                Cursor = Cursors.Hand
            };
            btnEncode.FlatAppearance.BorderSize = 0;

            btnToggle = new Button
            {
                Text = "🔄 Przełącz (Auto)",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Size = new Size(150, 40),
                Location = new Point(395, 26),
                BackColor = Color.FromArgb(52, 73, 94),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Enabled = false,
                Cursor = Cursors.Hand
            };
            btnToggle.FlatAppearance.BorderSize = 0;

            btnSaveAs = new Button
            {
                Text = "💾 Zapisz jako...",
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                Size = new Size(130, 40),
                Location = new Point(555, 26),
                BackColor = Color.FromArgb(142, 68, 173),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Enabled = false,
                Cursor = Cursors.Hand
            };
            btnSaveAs.FlatAppearance.BorderSize = 0;

            chkBackup = new CheckBox
            {
                Text = "Twórz kopię zapasową (.bak) przed zapisem",
                Checked = true,
                AutoSize = true,
                Location = new Point(695, 36),
                Font = new Font("Segoe UI", 8.5F)
            };

            grpActions.Controls.Add(btnDecode);
            grpActions.Controls.Add(btnEncode);
            grpActions.Controls.Add(btnToggle);
            grpActions.Controls.Add(btnSaveAs);
            grpActions.Controls.Add(chkBackup);

            // Hex Dump group
            grpHex = new GroupBox
            {
                Text = "Podgląd heksadecymalny i tekstowy (pierwsze 256 bajtów)",
                Dock = DockStyle.Fill,
                Padding = new Padding(8)
            };

            txtHexDump = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                Dock = DockStyle.Fill,
                ScrollBars = ScrollBars.Vertical,
                BackColor = Color.FromArgb(24, 26, 32),
                ForeColor = Color.FromArgb(230, 230, 180),
                Font = new Font("Consolas", 9.5F, FontStyle.Regular),
                WordWrap = false
            };
            grpHex.Controls.Add(txtHexDump);

            // Docking order
            tabSingle.Controls.Add(grpHex);
            tabSingle.Controls.Add(grpActions);
            tabSingle.Controls.Add(grpFileInfo);
            tabSingle.Controls.Add(pnlDropZone);
        }

        private void BuildBatchTab()
        {
            pnlBatchTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 55,
                Padding = new Padding(5)
            };

            btnBatchAddFiles = new Button
            {
                Text = "📂 Dodaj pliki...",
                Size = new Size(125, 36),
                Location = new Point(5, 8),
                BackColor = Color.FromArgb(41, 128, 185),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnBatchAddFiles.FlatAppearance.BorderSize = 0;

            btnBatchAddFolder = new Button
            {
                Text = "📁 Dodaj folder...",
                Size = new Size(125, 36),
                Location = new Point(135, 8),
                BackColor = Color.FromArgb(41, 128, 185),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnBatchAddFolder.FlatAppearance.BorderSize = 0;

            btnBatchClear = new Button
            {
                Text = "🗑 Wyczyść listę",
                Size = new Size(115, 36),
                Location = new Point(265, 8),
                BackColor = Color.FromArgb(149, 165, 166),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnBatchClear.FlatAppearance.BorderSize = 0;

            chkBatchBackup = new CheckBox
            {
                Text = "Twórz kopie zapasowe (.bak)",
                Checked = true,
                AutoSize = true,
                Location = new Point(400, 16),
                Font = new Font("Segoe UI", 8.5F)
            };

            pnlBatchTop.Controls.Add(btnBatchAddFiles);
            pnlBatchTop.Controls.Add(btnBatchAddFolder);
            pnlBatchTop.Controls.Add(btnBatchClear);
            pnlBatchTop.Controls.Add(chkBatchBackup);

            lvBatch = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                CheckBoxes = true,
                Font = new Font("Segoe UI", 9F)
            };

            lvBatch.Columns.Add("Plik", 180);
            lvBatch.Columns.Add("Stan", 140);
            lvBatch.Columns.Add("Rozmiar", 100);
            lvBatch.Columns.Add("Ścieżka", 320);
            lvBatch.Columns.Add("Wynik operacji", 140);

            pnlBatchBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 65,
                Padding = new Padding(5)
            };

            btnBatchDecode = new Button
            {
                Text = "🔓 Odszyfruj zaznaczone",
                Size = new Size(175, 36),
                Location = new Point(5, 12),
                BackColor = Color.FromArgb(39, 174, 96),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnBatchDecode.FlatAppearance.BorderSize = 0;

            btnBatchEncode = new Button
            {
                Text = "🔒 Zaszyfruj zaznaczone",
                Size = new Size(175, 36),
                Location = new Point(185, 12),
                BackColor = Color.FromArgb(230, 126, 34),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnBatchEncode.FlatAppearance.BorderSize = 0;

            btnBatchToggle = new Button
            {
                Text = "🔄 Przełącz zaznaczone",
                Size = new Size(175, 36),
                Location = new Point(365, 12),
                BackColor = Color.FromArgb(52, 73, 94),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnBatchToggle.FlatAppearance.BorderSize = 0;

            prgBatch = new ProgressBar
            {
                Location = new Point(550, 12),
                Size = new Size(200, 20),
                Visible = false
            };

            lblBatchProgress = new Label
            {
                Location = new Point(550, 36),
                Size = new Size(250, 20),
                Text = "",
                Font = new Font("Segoe UI", 8F),
                Visible = false
            };

            pnlBatchBottom.Controls.Add(btnBatchDecode);
            pnlBatchBottom.Controls.Add(btnBatchEncode);
            pnlBatchBottom.Controls.Add(btnBatchToggle);
            pnlBatchBottom.Controls.Add(prgBatch);
            pnlBatchBottom.Controls.Add(lblBatchProgress);

            tabBatch.Controls.Add(lvBatch);
            tabBatch.Controls.Add(pnlBatchBottom);
            tabBatch.Controls.Add(pnlBatchTop);
        }

        private void SetupEvents()
        {
            // Browse button
            btnBrowse.Click += (s, e) => SelectSingleFile();

            // Single file actions
            btnDecode.Click += (s, e) => ExecuteSingleOperation(CryptoAction.Decode);
            btnEncode.Click += (s, e) => ExecuteSingleOperation(CryptoAction.Encode);
            btnToggle.Click += (s, e) => ExecuteSingleOperation(CryptoAction.Toggle);
            btnSaveAs.Click += (s, e) => ExecuteSingleSaveAs();

            // Batch events
            btnBatchAddFiles.Click += (s, e) => AddBatchFiles();
            btnBatchAddFolder.Click += (s, e) => AddBatchFolder();
            btnBatchClear.Click += (s, e) => lvBatch.Items.Clear();

            btnBatchDecode.Click += async (s, e) => await ExecuteBatchOperationAsync(CryptoAction.Decode);
            btnBatchEncode.Click += async (s, e) => await ExecuteBatchOperationAsync(CryptoAction.Encode);
            btnBatchToggle.Click += async (s, e) => await ExecuteBatchOperationAsync(CryptoAction.Toggle);

            // Drag and drop handling
            this.DragEnter += OnFormDragEnter;
            this.DragDrop += OnFormDragDrop;
            pnlDropZone.DragEnter += OnFormDragEnter;
            pnlDropZone.DragDrop += OnFormDragDrop;
            lvBatch.DragEnter += OnFormDragEnter;
            lvBatch.DragDrop += OnFormDragDrop;
        }

        private void OnFormDragEnter(object? sender, DragEventArgs e)
        {
            if (e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void OnFormDragDrop(object? sender, DragEventArgs e)
        {
            if (e.Data == null || !e.Data.GetDataPresent(DataFormats.FileDrop)) return;

            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop)!;
            if (files == null || files.Length == 0) return;

            if (tabControl.SelectedTab == tabBatch || files.Length > 1)
            {
                if (files.Length > 1 && tabControl.SelectedTab != tabBatch)
                {
                    tabControl.SelectedTab = tabBatch;
                }
                AddFilesToBatch(files);
            }
            else
            {
                LoadSingleFile(files[0]);
            }
        }

        private void SelectSingleFile()
        {
            using var ofd = new OpenFileDialog
            {
                Title = "Wybierz plik do dekodowania / kodowania",
                Filter = "Wszystkie obsługiwane (*.tsg;*.txt;*.*)|*.tsg;*.txt;*.*|Pliki zapisu Theocracy (*.tsg)|*.tsg|Pliki konfiguracyjne (*.txt)|*.txt|Wszystkie pliki (*.*)|*.*",
                InitialDirectory = AppDomain.CurrentDomain.BaseDirectory
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                LoadSingleFile(ofd.FileName);
            }
        }

        public void LoadSingleFile(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    MessageBox.Show($"Nie znaleziono pliku:\n{filePath}", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                _currentFilePath = filePath;
                _currentFileData = File.ReadAllBytes(filePath);
                _currentInspection = TheocracyCrypto.InspectData(_currentFileData);

                txtFilePath.Text = filePath;
                lblFileSize.Text = $"Rozmiar: {_currentFileData.Length:N0} B ({_currentFileData.Length / 1024.0:F1} KB)";
                lblFileModified.Text = $"Data modyfikacji: {File.GetLastWriteTime(filePath):yyyy-MM-dd HH:mm:ss}";

                if (_currentInspection.State == FileCryptoState.Encrypted)
                {
                    lblBadge.Text = "🔒 ZASZYFROWANY [RSA4096]";
                    lblBadge.BackColor = Color.FromArgb(46, 125, 50); // Dark Green
                    btnDecode.Enabled = true;
                    btnEncode.Enabled = false;
                }
                else
                {
                    lblBadge.Text = "🔓 ODSZYFROWANY [Jawny]";
                    lblBadge.BackColor = Color.FromArgb(21, 101, 192); // Blue
                    btnDecode.Enabled = false;
                    btnEncode.Enabled = true;
                }

                btnToggle.Enabled = true;
                btnSaveAs.Enabled = true;

                lblDetails.Text = _currentInspection.ExtraDetails;
                txtHexDump.Text = TheocracyCrypto.GenerateHexDump(_currentFileData, 256);

                statusLabel.Text = $"Wczytano: {Path.GetFileName(filePath)} ({lblBadge.Text})";
                LogMessage($"Wczytano plik: {Path.GetFileName(filePath)} | Rozmiar: {_currentFileData.Length:N0} B | Stan: {lblBadge.Text}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd podczas odczytu pliku:\n{ex.Message}", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogMessage($"[BŁĄD] Nie udało się wczytać pliku: {ex.Message}");
            }
        }

        private enum CryptoAction { Decode, Encode, Toggle }

        private void ExecuteSingleOperation(CryptoAction action)
        {
            if (string.IsNullOrEmpty(_currentFilePath) || _currentFileData == null || _currentInspection == null)
            {
                MessageBox.Show("Nie wybrano pliku do przetworzenia.", "Uwaga", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                byte[] outputData;
                string actionDesc;

                switch (action)
                {
                    case CryptoAction.Decode:
                        if (_currentInspection.State != FileCryptoState.Encrypted)
                        {
                            var dr = MessageBox.Show(
                                "Plik nie ma nagłówka RSA4096 i wygląda na już odszyfrowany.\nCzy na pewno chcesz wymusić operację odszyfrowania?",
                                "Potwierdzenie", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                            if (dr != DialogResult.Yes) return;
                        }
                        outputData = TheocracyCrypto.Decrypt(_currentFileData);
                        actionDesc = "Odszyfrowano";
                        break;

                    case CryptoAction.Encode:
                        if (_currentInspection.State == FileCryptoState.Encrypted)
                        {
                            var dr = MessageBox.Show(
                                "Plik ma już nagłówek RSA4096 i wygląda na zaszyfrowany.\nCzy na pewno chcesz zaszyfrować go ponownie?",
                                "Potwierdzenie", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                            if (dr != DialogResult.Yes) return;
                        }
                        outputData = TheocracyCrypto.Encrypt(_currentFileData);
                        actionDesc = "Zaszyfrowano";
                        break;

                    case CryptoAction.Toggle:
                    default:
                        var toggleRes = TheocracyCrypto.Toggle(_currentFileData);
                        outputData = toggleRes.result;
                        actionDesc = toggleRes.wasEncrypted ? "Odszyfrowano" : "Zaszyfrowano";
                        break;
                }

                // Backup if requested
                if (chkBackup.Checked)
                {
                    string backupPath = _currentFilePath + ".bak";
                    File.Copy(_currentFilePath, backupPath, true);
                    LogMessage($"Utworzono kopię zapasową: {Path.GetFileName(backupPath)}");
                }

                // Write file
                File.WriteAllBytes(_currentFilePath, outputData);

                LogMessage($"[SUKCES] {actionDesc} plik: {Path.GetFileName(_currentFilePath)} ({_currentFileData.Length:N0} B -> {outputData.Length:N0} B)");
                MessageBox.Show($"Pomyślnie wykonano operację: {actionDesc}!\nPlik został zaktualizowany.", "Sukces", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Reload
                LoadSingleFile(_currentFilePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Wystąpił błąd podczas operacji:\n{ex.Message}", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogMessage($"[BŁĄD] Operacja nie powiodła się: {ex.Message}");
            }
        }

        private void ExecuteSingleSaveAs()
        {
            if (string.IsNullOrEmpty(_currentFilePath) || _currentFileData == null || _currentInspection == null)
                return;

            string dir = Path.GetDirectoryName(_currentFilePath) ?? "";
            string nameNoExt = Path.GetFileNameWithoutExtension(_currentFilePath);
            string ext = Path.GetExtension(_currentFilePath);

            bool isCurrentlyEncrypted = (_currentInspection.State == FileCryptoState.Encrypted);
            string suggestedName = isCurrentlyEncrypted
                ? $"{nameNoExt}_decrypted{ext}"
                : $"{nameNoExt}_encrypted{ext}";

            using var sfd = new SaveFileDialog
            {
                Title = "Zapisz przetworzony plik jako...",
                Filter = "Ten sam typ pliku (*" + ext + ")|*" + ext + "|Wszystkie pliki (*.*)|*.*",
                InitialDirectory = dir,
                FileName = suggestedName
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    byte[] outputData;
                    if (isCurrentlyEncrypted)
                    {
                        outputData = TheocracyCrypto.Decrypt(_currentFileData);
                    }
                    else
                    {
                        outputData = TheocracyCrypto.Encrypt(_currentFileData);
                    }

                    File.WriteAllBytes(sfd.FileName, outputData);
                    LogMessage($"Zapisano jako nowy plik: {Path.GetFileName(sfd.FileName)} ({outputData.Length:N0} B)");
                    MessageBox.Show($"Plik został pomyślnie zapisany jako:\n{sfd.FileName}", "Zapisano", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Błąd podczas zapisu:\n{ex.Message}", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // Batch processing methods
        private void AddBatchFiles()
        {
            using var ofd = new OpenFileDialog
            {
                Title = "Wybierz pliki do przetwarzania wsadowego",
                Filter = "Wszystkie obsługiwane (*.tsg;*.txt;*.*)|*.tsg;*.txt;*.*|Pliki zapisu (*.tsg)|*.tsg|Pliki konfiguracyjne (*.txt)|*.txt|Wszystkie (*.*)|*.*",
                Multiselect = true,
                InitialDirectory = AppDomain.CurrentDomain.BaseDirectory
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                AddFilesToBatch(ofd.FileNames);
            }
        }

        private void AddBatchFolder()
        {
            using var fbd = new FolderBrowserDialog
            {
                Description = "Wybierz folder z plikami zapisu lub gry Theocracy",
                InitialDirectory = AppDomain.CurrentDomain.BaseDirectory
            };

            if (fbd.ShowDialog() == DialogResult.OK)
            {
                var files = new List<string>();
                files.AddRange(Directory.GetFiles(fbd.SelectedPath, "*.tsg", SearchOption.TopDirectoryOnly));
                files.AddRange(Directory.GetFiles(fbd.SelectedPath, "*.txt", SearchOption.TopDirectoryOnly));
                AddFilesToBatch(files.ToArray());
            }
        }

        private void AddFilesToBatch(string[] filePaths)
        {
            foreach (var path in filePaths)
            {
                if (Directory.Exists(path))
                {
                    var innerFiles = Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly);
                    AddFilesToBatch(innerFiles);
                    continue;
                }

                if (!File.Exists(path)) continue;

                // Check if already in list
                bool exists = false;
                foreach (ListViewItem existing in lvBatch.Items)
                {
                    if (string.Equals(existing.SubItems[3].Text, path, StringComparison.OrdinalIgnoreCase))
                    {
                        exists = true;
                        break;
                    }
                }
                if (exists) continue;

                try
                {
                    var fi = new FileInfo(path);
                    byte[] headerBytes = new byte[Math.Min(fi.Length, 7)];
                    using (var fs = fi.OpenRead())
                    {
                        fs.ReadExactly(headerBytes, 0, headerBytes.Length);
                    }

                    bool isEnc = TheocracyCrypto.StartsWithHeader(headerBytes);
                    string stateStr = isEnc ? "🔒 Zaszyfrowany" : "🔓 Odszyfrowany";

                    var lvi = new ListViewItem(fi.Name);
                    lvi.SubItems.Add(stateStr);
                    lvi.SubItems.Add($"{fi.Length:N0} B");
                    lvi.SubItems.Add(path);
                    lvi.SubItems.Add("Oczekuje");
                    lvi.Checked = true;

                    lvBatch.Items.Add(lvi);
                }
                catch { }
            }

            LogMessage($"Dodano pliki do listy wsadowej. Łącznie plików na liście: {lvBatch.Items.Count}");
        }

        private async Task ExecuteBatchOperationAsync(CryptoAction action)
        {
            var selectedItems = new List<ListViewItem>();
            foreach (ListViewItem item in lvBatch.Items)
            {
                if (item.Checked) selectedItems.Add(item);
            }

            if (selectedItems.Count == 0)
            {
                MessageBox.Show("Zaznacz co najmniej jeden plik do przetworzenia na liście (checkbox).", "Uwaga", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            prgBatch.Visible = true;
            lblBatchProgress.Visible = true;
            prgBatch.Minimum = 0;
            prgBatch.Maximum = selectedItems.Count;
            prgBatch.Value = 0;

            int successCount = 0;
            int errorCount = 0;

            for (int i = 0; i < selectedItems.Count; i++)
            {
                var item = selectedItems[i];
                string filePath = item.SubItems[3].Text;
                lblBatchProgress.Text = $"Przetwarzanie {i + 1}/{selectedItems.Count}: {Path.GetFileName(filePath)}";

                try
                {
                    byte[] data = await File.ReadAllBytesAsync(filePath);
                    byte[] result;
                    string resDesc;

                    bool isEnc = TheocracyCrypto.StartsWithHeader(data);

                    if (action == CryptoAction.Decode)
                    {
                        if (!isEnc)
                        {
                            item.SubItems[4].Text = "Pominięto (już odszyfrowany)";
                            continue;
                        }
                        result = TheocracyCrypto.Decrypt(data);
                        resDesc = "Odszyfrowano";
                    }
                    else if (action == CryptoAction.Encode)
                    {
                        if (isEnc)
                        {
                            item.SubItems[4].Text = "Pominięto (już zaszyfrowany)";
                            continue;
                        }
                        result = TheocracyCrypto.Encrypt(data);
                        resDesc = "Zaszyfrowano";
                    }
                    else // Toggle
                    {
                        var tRes = TheocracyCrypto.Toggle(data);
                        result = tRes.result;
                        resDesc = tRes.wasEncrypted ? "Odszyfrowano" : "Zaszyfrowano";
                    }

                    if (chkBatchBackup.Checked)
                    {
                        File.Copy(filePath, filePath + ".bak", true);
                    }

                    await File.WriteAllBytesAsync(filePath, result);

                    bool newEnc = TheocracyCrypto.StartsWithHeader(result);
                    item.SubItems[1].Text = newEnc ? "🔒 Zaszyfrowany" : "🔓 Odszyfrowany";
                    item.SubItems[2].Text = $"{result.Length:N0} B";
                    item.SubItems[4].Text = $"OK ({resDesc})";
                    successCount++;
                }
                catch (Exception ex)
                {
                    item.SubItems[4].Text = $"Błąd: {ex.Message}";
                    errorCount++;
                    LogMessage($"[BŁĄD WSADOWY] {Path.GetFileName(filePath)}: {ex.Message}");
                }

                prgBatch.Value = i + 1;
            }

            lblBatchProgress.Text = $"Zakończono! Sukces: {successCount}, Błędy: {errorCount}";
            LogMessage($"Przetwarzanie wsadowe zakończone. Przetworzono {successCount} plików, błędów: {errorCount}.");
            MessageBox.Show($"Przetwarzanie wsadowe zakończone.\nSukces: {successCount} plików\nBłędy: {errorCount}", "Wynik", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // If the single file view has one of the edited files, reload it
            if (!string.IsNullOrEmpty(_currentFilePath))
            {
                LoadSingleFile(_currentFilePath);
            }
        }

        private void LogMessage(string message)
        {
            string line = $"[{DateTime.Now:HH:mm:ss}] {message}";
            txtLog.AppendText(line + Environment.NewLine);
            statusLabel.Text = message;
        }
    }
}
