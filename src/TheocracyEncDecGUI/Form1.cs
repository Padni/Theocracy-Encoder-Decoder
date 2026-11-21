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
        private Label lblLang = null!;
        private ComboBox cmbLang = null!;

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
            ApplyLanguage();
            LogMessage(Strings.CurrentLanguage == AppLanguage.Polish
                ? "Program Theocracy Encoder / Decoder uruchomiony. Wybierz plik lub przeciągnij go do okna."
                : "Theocracy Encoder / Decoder started. Select a file or drag & drop it into the window.");
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
                Text = Strings.AppTitle,
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = Color.FromArgb(240, 192, 64),
                AutoSize = true,
                Location = new Point(18, 10)
            };

            lblSubtitle = new Label
            {
                Text = Strings.Subtitle,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular),
                ForeColor = Color.FromArgb(200, 210, 225),
                AutoSize = true,
                Location = new Point(20, 36)
            };

            lblLang = new Label
            {
                Text = "🌐 Language / Język:",
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = Color.FromArgb(180, 195, 215),
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(650, 22)
            };

            cmbLang = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 135,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(780, 18),
                BackColor = Color.White,
                Cursor = Cursors.Hand
            };
            cmbLang.Items.Add("🇵🇱 Polski");
            cmbLang.Items.Add("🇬🇧 English");
            cmbLang.SelectedIndex = 0;

            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(lblSubtitle);
            pnlHeader.Controls.Add(lblLang);
            pnlHeader.Controls.Add(cmbLang);

            // 2. Status Strip at bottom
            statusStrip = new StatusStrip();
            statusLabel = new ToolStripStatusLabel(Strings.StatusReady) { Spring = true, TextAlign = ContentAlignment.MiddleLeft };
            statusStrip.Items.Add(statusLabel);

            // 3. Bottom Log Box
            grpLog = new GroupBox
            {
                Text = Strings.GrpLog,
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

            tabSingle = new TabPage(Strings.TabSingle) { BackColor = Color.FromArgb(245, 247, 250), Padding = new Padding(12) };
            tabBatch = new TabPage(Strings.TabBatch) { BackColor = Color.FromArgb(245, 247, 250), Padding = new Padding(12) };

            tabControl.TabPages.Add(tabSingle);
            tabControl.TabPages.Add(tabBatch);

            BuildSingleFileTab();
            BuildBatchTab();

            // Assemble main controls
            this.Controls.Add(tabControl);
            this.Controls.Add(grpLog);
            this.Controls.Add(statusStrip);
            this.Controls.Add(pnlHeader);

            this.AllowDrop = true;
        }

        private void BuildSingleFileTab()
        {
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
                Text = Strings.DropHint,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Regular),
                ForeColor = Color.FromArgb(60, 70, 85),
                AutoSize = true,
                Location = new Point(16, 22)
            };

            btnBrowse = new Button
            {
                Text = Strings.BtnBrowse,
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
                Text = Strings.GrpFileInfo,
                Dock = DockStyle.Top,
                Height = 135,
                Padding = new Padding(10)
            };

            lblFilePath = new Label { Text = Strings.LblFile, Location = new Point(15, 25), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            txtFilePath = new TextBox { Location = new Point(60, 22), Width = 520, ReadOnly = true, BackColor = Color.White };

            lblFileSize = new Label { Text = Strings.LblSizeNoFile, Location = new Point(15, 52), AutoSize = true };
            lblFileModified = new Label { Text = Strings.LblModifiedNoFile, Location = new Point(250, 52), AutoSize = true };

            lblBadge = new Label
            {
                Text = Strings.BadgeNoFile,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Location = new Point(600, 20),
                Size = new Size(240, 32),
                BackColor = Color.FromArgb(180, 185, 195),
                ForeColor = Color.White
            };

            lblDetails = new Label
            {
                Text = Strings.DetailsDefaultHint,
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
                Text = Strings.GrpActions,
                Dock = DockStyle.Top,
                Height = 85,
                Padding = new Padding(10)
            };

            btnDecode = new Button
            {
                Text = Strings.BtnDecode,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Size = new Size(180, 40),
                Location = new Point(15, 26),
                BackColor = Color.FromArgb(39, 174, 96),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Enabled = false,
                Cursor = Cursors.Hand
            };
            btnDecode.FlatAppearance.BorderSize = 0;

            btnEncode = new Button
            {
                Text = Strings.BtnEncode,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Size = new Size(180, 40),
                Location = new Point(205, 26),
                BackColor = Color.FromArgb(230, 126, 34),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Enabled = false,
                Cursor = Cursors.Hand
            };
            btnEncode.FlatAppearance.BorderSize = 0;

            btnToggle = new Button
            {
                Text = Strings.BtnToggle,
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
                Text = Strings.BtnSaveAs,
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
                Text = Strings.ChkBackup,
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
                Text = Strings.GrpHex,
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
                Text = Strings.BtnBatchAddFiles,
                Size = new Size(135, 36),
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
                Text = Strings.BtnBatchAddFolder,
                Size = new Size(135, 36),
                Location = new Point(145, 8),
                BackColor = Color.FromArgb(41, 128, 185),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnBatchAddFolder.FlatAppearance.BorderSize = 0;

            btnBatchClear = new Button
            {
                Text = Strings.BtnBatchClear,
                Size = new Size(125, 36),
                Location = new Point(285, 8),
                BackColor = Color.FromArgb(149, 165, 166),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnBatchClear.FlatAppearance.BorderSize = 0;

            chkBatchBackup = new CheckBox
            {
                Text = Strings.ChkBatchBackup,
                Checked = true,
                AutoSize = true,
                Location = new Point(420, 16),
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

            lvBatch.Columns.Add(Strings.ColFile, 180);
            lvBatch.Columns.Add(Strings.ColState, 140);
            lvBatch.Columns.Add(Strings.ColSize, 100);
            lvBatch.Columns.Add(Strings.ColPath, 320);
            lvBatch.Columns.Add(Strings.ColResult, 140);

            pnlBatchBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 65,
                Padding = new Padding(5)
            };

            btnBatchDecode = new Button
            {
                Text = Strings.BtnBatchDecode,
                Size = new Size(185, 36),
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
                Text = Strings.BtnBatchEncode,
                Size = new Size(185, 36),
                Location = new Point(195, 12),
                BackColor = Color.FromArgb(230, 126, 34),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnBatchEncode.FlatAppearance.BorderSize = 0;

            btnBatchToggle = new Button
            {
                Text = Strings.BtnBatchToggle,
                Size = new Size(185, 36),
                Location = new Point(385, 12),
                BackColor = Color.FromArgb(52, 73, 94),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnBatchToggle.FlatAppearance.BorderSize = 0;

            prgBatch = new ProgressBar
            {
                Location = new Point(580, 12),
                Size = new Size(200, 20),
                Visible = false
            };

            lblBatchProgress = new Label
            {
                Location = new Point(580, 36),
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
            // Language selector
            cmbLang.SelectedIndexChanged += (s, e) =>
            {
                Strings.CurrentLanguage = cmbLang.SelectedIndex == 1 ? AppLanguage.English : AppLanguage.Polish;
                ApplyLanguage();
                LogMessage(Strings.CurrentLanguage == AppLanguage.Polish
                    ? "Przełączono język interfejsu na: Polski"
                    : "Interface language switched to: English");
            };

            btnBrowse.Click += (s, e) => SelectSingleFile();

            btnDecode.Click += (s, e) => ExecuteSingleOperation(CryptoAction.Decode);
            btnEncode.Click += (s, e) => ExecuteSingleOperation(CryptoAction.Encode);
            btnToggle.Click += (s, e) => ExecuteSingleOperation(CryptoAction.Toggle);
            btnSaveAs.Click += (s, e) => ExecuteSingleSaveAs();

            btnBatchAddFiles.Click += (s, e) => AddBatchFiles();
            btnBatchAddFolder.Click += (s, e) => AddBatchFolder();
            btnBatchClear.Click += (s, e) => lvBatch.Items.Clear();

            btnBatchDecode.Click += async (s, e) => await ExecuteBatchOperationAsync(CryptoAction.Decode);
            btnBatchEncode.Click += async (s, e) => await ExecuteBatchOperationAsync(CryptoAction.Encode);
            btnBatchToggle.Click += async (s, e) => await ExecuteBatchOperationAsync(CryptoAction.Toggle);

            this.DragEnter += OnFormDragEnter;
            this.DragDrop += OnFormDragDrop;
            pnlDropZone.DragEnter += OnFormDragEnter;
            pnlDropZone.DragDrop += OnFormDragDrop;
            lvBatch.DragEnter += OnFormDragEnter;
            lvBatch.DragDrop += OnFormDragDrop;
        }

        public void ApplyLanguage()
        {
            lblSubtitle.Text = Strings.Subtitle;
            tabSingle.Text = Strings.TabSingle;
            tabBatch.Text = Strings.TabBatch;

            lblDropHint.Text = Strings.DropHint;
            btnBrowse.Text = Strings.BtnBrowse;

            grpFileInfo.Text = Strings.GrpFileInfo;
            lblFilePath.Text = Strings.LblFile;
            grpActions.Text = Strings.GrpActions;

            btnDecode.Text = Strings.BtnDecode;
            btnEncode.Text = Strings.BtnEncode;
            btnToggle.Text = Strings.BtnToggle;
            btnSaveAs.Text = Strings.BtnSaveAs;
            chkBackup.Text = Strings.ChkBackup;

            grpHex.Text = Strings.GrpHex;

            btnBatchAddFiles.Text = Strings.BtnBatchAddFiles;
            btnBatchAddFolder.Text = Strings.BtnBatchAddFolder;
            btnBatchClear.Text = Strings.BtnBatchClear;
            chkBatchBackup.Text = Strings.ChkBatchBackup;

            if (lvBatch.Columns.Count >= 5)
            {
                lvBatch.Columns[0].Text = Strings.ColFile;
                lvBatch.Columns[1].Text = Strings.ColState;
                lvBatch.Columns[2].Text = Strings.ColSize;
                lvBatch.Columns[3].Text = Strings.ColPath;
                lvBatch.Columns[4].Text = Strings.ColResult;
            }

            btnBatchDecode.Text = Strings.BtnBatchDecode;
            btnBatchEncode.Text = Strings.BtnBatchEncode;
            btnBatchToggle.Text = Strings.BtnBatchToggle;

            grpLog.Text = Strings.GrpLog;

            // Refresh file display if a file is loaded
            if (_currentFileData != null && _currentInspection != null && !string.IsNullOrEmpty(_currentFilePath))
            {
                lblFileSize.Text = Strings.LblSize(_currentFileData.Length);
                lblFileModified.Text = Strings.LblModified(File.GetLastWriteTime(_currentFilePath));

                if (_currentInspection.State == FileCryptoState.Encrypted)
                {
                    lblBadge.Text = Strings.BadgeEncrypted;
                    lblDetails.Text = Strings.DetailsEncrypted(
                        _currentFileData.Length - 7,
                        _currentInspection.IsTheocracySave,
                        _currentInspection.SaveDate,
                        _currentInspection.IsTextConfig
                    );
                }
                else
                {
                    lblBadge.Text = Strings.BadgeDecrypted;
                    lblDetails.Text = Strings.DetailsDecrypted(
                        _currentInspection.IsTheocracySave,
                        _currentInspection.SaveDate,
                        _currentInspection.SaveSignature,
                        _currentInspection.IsTextConfig
                    );
                }
            }
            else
            {
                lblFileSize.Text = Strings.LblSizeNoFile;
                lblFileModified.Text = Strings.LblModifiedNoFile;
                lblBadge.Text = Strings.BadgeNoFile;
                lblDetails.Text = Strings.DetailsDefaultHint;
                statusLabel.Text = Strings.StatusReady;
            }
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
                Title = Strings.CurrentLanguage == AppLanguage.Polish ? "Wybierz plik do dekodowania / kodowania" : "Select file to decode / encode",
                Filter = Strings.CurrentLanguage == AppLanguage.Polish
                    ? "Wszystkie obsługiwane (*.tsg;*.txt;*.*)|*.tsg;*.txt;*.*|Pliki zapisu Theocracy (*.tsg)|*.tsg|Pliki konfiguracyjne (*.txt)|*.txt|Wszystkie pliki (*.*)|*.*"
                    : "All supported (*.tsg;*.txt;*.*)|*.tsg;*.txt;*.*|Theocracy Save Files (*.tsg)|*.tsg|Config Files (*.txt)|*.txt|All Files (*.*)|*.*",
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
                    MessageBox.Show(
                        Strings.CurrentLanguage == AppLanguage.Polish ? $"Nie znaleziono pliku:\n{filePath}" : $"File not found:\n{filePath}",
                        Strings.CurrentLanguage == AppLanguage.Polish ? "Błąd" : "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                _currentFilePath = filePath;
                _currentFileData = File.ReadAllBytes(filePath);
                _currentInspection = TheocracyCrypto.InspectData(_currentFileData);

                txtFilePath.Text = filePath;
                lblFileSize.Text = Strings.LblSize(_currentFileData.Length);
                lblFileModified.Text = Strings.LblModified(File.GetLastWriteTime(filePath));

                if (_currentInspection.State == FileCryptoState.Encrypted)
                {
                    lblBadge.Text = Strings.BadgeEncrypted;
                    lblBadge.BackColor = Color.FromArgb(46, 125, 50);
                    btnDecode.Enabled = true;
                    btnEncode.Enabled = false;

                    lblDetails.Text = Strings.DetailsEncrypted(
                        _currentFileData.Length - 7,
                        _currentInspection.IsTheocracySave,
                        _currentInspection.SaveDate,
                        _currentInspection.IsTextConfig
                    );
                }
                else
                {
                    lblBadge.Text = Strings.BadgeDecrypted;
                    lblBadge.BackColor = Color.FromArgb(21, 101, 192);
                    btnDecode.Enabled = false;
                    btnEncode.Enabled = true;

                    lblDetails.Text = Strings.DetailsDecrypted(
                        _currentInspection.IsTheocracySave,
                        _currentInspection.SaveDate,
                        _currentInspection.SaveSignature,
                        _currentInspection.IsTextConfig
                    );
                }

                btnToggle.Enabled = true;
                btnSaveAs.Enabled = true;

                txtHexDump.Text = TheocracyCrypto.GenerateHexDump(_currentFileData, 256);

                statusLabel.Text = Strings.StatusLoaded(Path.GetFileName(filePath), lblBadge.Text);
                LogMessage(Strings.StatusLoaded(Path.GetFileName(filePath), lblBadge.Text) + $" | {lblFileSize.Text}");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    (Strings.CurrentLanguage == AppLanguage.Polish ? "Błąd podczas odczytu pliku:\n" : "Error reading file:\n") + ex.Message,
                    Strings.CurrentLanguage == AppLanguage.Polish ? "Błąd" : "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogMessage($"[ERROR] {ex.Message}");
            }
        }

        private enum CryptoAction { Decode, Encode, Toggle }

        private void ExecuteSingleOperation(CryptoAction action)
        {
            if (string.IsNullOrEmpty(_currentFilePath) || _currentFileData == null || _currentInspection == null)
            {
                MessageBox.Show(
                    Strings.CurrentLanguage == AppLanguage.Polish ? "Nie wybrano pliku do przetworzenia." : "No file selected for processing.",
                    Strings.CurrentLanguage == AppLanguage.Polish ? "Uwaga" : "Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                                Strings.MsgDecodeAlreadyPlain,
                                Strings.CurrentLanguage == AppLanguage.Polish ? "Potwierdzenie" : "Confirmation",
                                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                            if (dr != DialogResult.Yes) return;
                        }
                        outputData = TheocracyCrypto.Decrypt(_currentFileData);
                        actionDesc = Strings.ActionDecoded;
                        break;

                    case CryptoAction.Encode:
                        if (_currentInspection.State == FileCryptoState.Encrypted)
                        {
                            var dr = MessageBox.Show(
                                Strings.MsgEncodeAlreadyEnc,
                                Strings.CurrentLanguage == AppLanguage.Polish ? "Potwierdzenie" : "Confirmation",
                                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                            if (dr != DialogResult.Yes) return;
                        }
                        outputData = TheocracyCrypto.Encrypt(_currentFileData);
                        actionDesc = Strings.ActionEncoded;
                        break;

                    case CryptoAction.Toggle:
                    default:
                        var toggleRes = TheocracyCrypto.Toggle(_currentFileData);
                        outputData = toggleRes.result;
                        actionDesc = toggleRes.wasEncrypted ? Strings.ActionDecoded : Strings.ActionEncoded;
                        break;
                }

                if (chkBackup.Checked)
                {
                    string backupPath = _currentFilePath + ".bak";
                    File.Copy(_currentFilePath, backupPath, true);
                    LogMessage(Strings.CurrentLanguage == AppLanguage.Polish
                        ? $"Utworzono kopię zapasową: {Path.GetFileName(backupPath)}"
                        : $"Backup created: {Path.GetFileName(backupPath)}");
                }

                File.WriteAllBytes(_currentFilePath, outputData);

                LogMessage($"[SUCCESS] {actionDesc}: {Path.GetFileName(_currentFilePath)} ({_currentFileData.Length:N0} B -> {outputData.Length:N0} B)");
                MessageBox.Show(
                    Strings.MsgSuccess(actionDesc),
                    Strings.CurrentLanguage == AppLanguage.Polish ? "Sukces" : "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                LoadSingleFile(_currentFilePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    (Strings.CurrentLanguage == AppLanguage.Polish ? "Wystąpił błąd podczas operacji:\n" : "An error occurred during operation:\n") + ex.Message,
                    Strings.CurrentLanguage == AppLanguage.Polish ? "Błąd" : "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogMessage($"[ERROR] {ex.Message}");
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
                Title = Strings.CurrentLanguage == AppLanguage.Polish ? "Zapisz przetworzony plik jako..." : "Save processed file as...",
                Filter = Strings.CurrentLanguage == AppLanguage.Polish
                    ? $"Ten sam typ pliku (*{ext})|*{ext}|Wszystkie pliki (*.*)|*.*"
                    : $"Same file type (*{ext})|*{ext}|All files (*.*)|*.*",
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
                    LogMessage(Strings.CurrentLanguage == AppLanguage.Polish
                        ? $"Zapisano jako nowy plik: {Path.GetFileName(sfd.FileName)} ({outputData.Length:N0} B)"
                        : $"Saved as new file: {Path.GetFileName(sfd.FileName)} ({outputData.Length:N0} B)");

                    MessageBox.Show(
                        Strings.MsgSaveAsSaved(sfd.FileName),
                        Strings.CurrentLanguage == AppLanguage.Polish ? "Zapisano" : "Saved",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        (Strings.CurrentLanguage == AppLanguage.Polish ? "Błąd podczas zapisu:\n" : "Error saving file:\n") + ex.Message,
                        Strings.CurrentLanguage == AppLanguage.Polish ? "Błąd" : "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void AddBatchFiles()
        {
            using var ofd = new OpenFileDialog
            {
                Title = Strings.CurrentLanguage == AppLanguage.Polish ? "Wybierz pliki do przetwarzania wsadowego" : "Select files for batch processing",
                Filter = Strings.CurrentLanguage == AppLanguage.Polish
                    ? "Wszystkie obsługiwane (*.tsg;*.txt;*.*)|*.tsg;*.txt;*.*|Pliki zapisu (*.tsg)|*.tsg|Pliki konfiguracyjne (*.txt)|*.txt|Wszystkie (*.*)|*.*"
                    : "All supported (*.tsg;*.txt;*.*)|*.tsg;*.txt;*.*|Save files (*.tsg)|*.tsg|Config files (*.txt)|*.txt|All files (*.*)|*.*",
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
                Description = Strings.CurrentLanguage == AppLanguage.Polish ? "Wybierz folder z plikami Theocracy" : "Select folder with Theocracy files",
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
                    string stateStr = isEnc
                        ? (Strings.CurrentLanguage == AppLanguage.Polish ? "🔒 Zaszyfrowany" : "🔒 Encrypted")
                        : (Strings.CurrentLanguage == AppLanguage.Polish ? "🔓 Odszyfrowany" : "🔓 Decrypted");

                    var lvi = new ListViewItem(fi.Name);
                    lvi.SubItems.Add(stateStr);
                    lvi.SubItems.Add($"{fi.Length:N0} B");
                    lvi.SubItems.Add(path);
                    lvi.SubItems.Add(Strings.CurrentLanguage == AppLanguage.Polish ? "Oczekuje" : "Pending");
                    lvi.Checked = true;

                    lvBatch.Items.Add(lvi);
                }
                catch { }
            }

            LogMessage(Strings.CurrentLanguage == AppLanguage.Polish
                ? $"Dodano pliki do listy wsadowej. Łącznie plików na liście: {lvBatch.Items.Count}"
                : $"Files added to batch list. Total files on list: {lvBatch.Items.Count}");
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
                MessageBox.Show(
                    Strings.CurrentLanguage == AppLanguage.Polish ? "Zaznacz co najmniej jeden plik do przetworzenia na liście." : "Select at least one file on the list.",
                    Strings.CurrentLanguage == AppLanguage.Polish ? "Uwaga" : "Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                lblBatchProgress.Text = $"{i + 1}/{selectedItems.Count}: {Path.GetFileName(filePath)}";

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
                            item.SubItems[4].Text = Strings.CurrentLanguage == AppLanguage.Polish ? "Pominięto (jawny)" : "Skipped (plain)";
                            continue;
                        }
                        result = TheocracyCrypto.Decrypt(data);
                        resDesc = Strings.ActionDecoded;
                    }
                    else if (action == CryptoAction.Encode)
                    {
                        if (isEnc)
                        {
                            item.SubItems[4].Text = Strings.CurrentLanguage == AppLanguage.Polish ? "Pominięto (zaszyfrowany)" : "Skipped (encrypted)";
                            continue;
                        }
                        result = TheocracyCrypto.Encrypt(data);
                        resDesc = Strings.ActionEncoded;
                    }
                    else
                    {
                        var tRes = TheocracyCrypto.Toggle(data);
                        result = tRes.result;
                        resDesc = tRes.wasEncrypted ? Strings.ActionDecoded : Strings.ActionEncoded;
                    }

                    if (chkBatchBackup.Checked)
                    {
                        File.Copy(filePath, filePath + ".bak", true);
                    }

                    await File.WriteAllBytesAsync(filePath, result);

                    bool newEnc = TheocracyCrypto.StartsWithHeader(result);
                    item.SubItems[1].Text = newEnc
                        ? (Strings.CurrentLanguage == AppLanguage.Polish ? "🔒 Zaszyfrowany" : "🔒 Encrypted")
                        : (Strings.CurrentLanguage == AppLanguage.Polish ? "🔓 Odszyfrowany" : "🔓 Decrypted");
                    item.SubItems[2].Text = $"{result.Length:N0} B";
                    item.SubItems[4].Text = $"OK ({resDesc})";
                    successCount++;
                }
                catch (Exception ex)
                {
                    item.SubItems[4].Text = $"Error: {ex.Message}";
                    errorCount++;
                    LogMessage($"[BATCH ERROR] {Path.GetFileName(filePath)}: {ex.Message}");
                }

                prgBatch.Value = i + 1;
            }

            lblBatchProgress.Text = Strings.CurrentLanguage == AppLanguage.Polish
                ? $"Zakończono! Sukces: {successCount}, Błędy: {errorCount}"
                : $"Done! Success: {successCount}, Errors: {errorCount}";

            LogMessage(Strings.CurrentLanguage == AppLanguage.Polish
                ? $"Przetwarzanie wsadowe zakończone. Sukces: {successCount}, błędy: {errorCount}."
                : $"Batch processing completed. Success: {successCount}, errors: {errorCount}.");

            MessageBox.Show(
                Strings.BatchCompleted(successCount, errorCount),
                Strings.CurrentLanguage == AppLanguage.Polish ? "Wynik" : "Result",
                MessageBoxButtons.OK, MessageBoxIcon.Information);

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
