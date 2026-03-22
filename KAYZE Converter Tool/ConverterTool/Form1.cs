using ImageMagick;
using Spire.Doc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Downloader;
using YoutubeExplode;
using YoutubeExplode.Converter;
using YoutubeExplode.Videos.Streams;

namespace KAYZEConverterTool
{
    public partial class Form1 : Form
    {
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        private int cpuCores = Environment.ProcessorCount;
        private int MaxFiles => cpuCores * 15;
        private int MaxDegree => Math.Max(1, cpuCores - 1);

        private string[] selectedFiles = Array.Empty<string>();
        private bool isImageMode = true;
        private bool isFFmpegReady = false;

        private Color darkBg = Color.FromArgb(22, 22, 22);
        private Color darkPanel = Color.FromArgb(34, 34, 34);
        private Color kayzeCyan = Color.FromArgb(0, 210, 220);
        private Color textGray = Color.FromArgb(120, 120, 120);
        private Color textColor = Color.FromArgb(220, 220, 220);

        private Panel pnlHome;
        private Panel pnlConverter;
        private Panel pnlYoutube;
        private Panel pnlStem;

        private Panel customProgressBg;
        private Panel customProgressFill;
        private Label lblProgressText;
        private Label lblBack;

        private TextBox txtYoutubeLink;
        private ComboBox cmbYoutubeFormat;
        private Button btnDownloadYoutube;
        private Panel ytProgressBg;
        private Panel ytProgressFill;
        private Label lblYtProgressText;

        private Button btnSelectStemAudio;
        private CheckedListBox chkStems;
        private Button btnStartStem;
        private Panel stemProgressBg;
        private Panel stemProgressFill;
        private Label lblStemProgressText;
        private string stemAudioPath = "";
        private ToolTip kayzeToolTip;

        private Button btnSetupAi;
        private Label lblAiStatus;
        private bool isAiReady = false;

        private System.Windows.Forms.Timer glowTimer;
        private double glowStep = 0;
        private Button activeGlowButton = null;

        public Form1()
        {
            InitializeComponent();
            this.Text = "KAYZE Converter Tool";

            this.AutoScaleMode = AutoScaleMode.None;

            this.MinimumSize = new Size(680, 550);
            this.MaximumSize = new Size(680, 550);
            this.Size = new Size(680, 550);
            this.ClientSize = new Size(680, 550);

            kayzeToolTip = new ToolTip();
            kayzeToolTip.InitialDelay = 500; 
            kayzeToolTip.ReshowDelay = 500;
            kayzeToolTip.AutoPopDelay = 10000;
            kayzeToolTip.OwnerDraw = true; 

            kayzeToolTip.Draw += (s, e) => {
                e.Graphics.FillRectangle(new SolidBrush(darkPanel), e.Bounds);
                e.Graphics.DrawRectangle(new Pen(kayzeCyan, 1), new Rectangle(0, 0, e.Bounds.Width - 1, e.Bounds.Height - 1));
                e.Graphics.DrawString(e.ToolTipText, new Font("Segoe UI", 9, FontStyle.Regular), new SolidBrush(textColor), new PointF(5, 5)); 
            };

            kayzeToolTip.Popup += (s, e) => {
                Size size = TextRenderer.MeasureText(kayzeToolTip.GetToolTip(e.AssociatedControl), new Font("Segoe UI", 9));
                e.ToolTipSize = new Size(size.Width + 10, size.Height + 10);
            };

            glowTimer = new System.Windows.Forms.Timer();
            glowTimer.Interval = 30;
            glowTimer.Tick += GlowTimer_Tick;

            ApplyModernDesign();
            SetupCustomUI();

            this.Load += async (s, e) => {
                CenterElements();
                await SetupFFmpeg();
            };
            this.Resize += (s, e) => CenterElements();
        }

        private async Task SetupFFmpeg()
        {
            try
            {
                string ffmpegPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FFmpeg");
                Directory.CreateDirectory(ffmpegPath);
                FFmpeg.SetExecutablesPath(ffmpegPath);

                if (!File.Exists(Path.Combine(ffmpegPath, "ffmpeg.exe")))
                {
                    await FFmpegDownloader.GetLatestVersion(FFmpegVersion.Official, ffmpegPath);
                    MessageBox.Show("Audio/Video Modules (FFmpeg) successfully installed!", "KAYZE Converter", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                isFFmpegReady = true;
            }
            catch (Exception ex)
            {
                isFFmpegReady = false;
                MessageBox.Show($"Error downloading video modules: {ex.Message}", "Error");
            }
        }

        private void ApplyModernDesign()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 15, 15));
            this.BackColor = darkBg;
            this.MinimumSize = new Size(680, 550);

            this.AllowDrop = true;
            this.DragEnter += (s, e) => {
                if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
            };
            this.DragDrop += (s, e) => {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > MaxFiles)
                {
                    MessageBox.Show($"Limite CPU: {cpuCores} Core.\nPuoi elaborare massimo {MaxFiles} file. I file in eccesso verranno ignorati.", "Ottimizzazione CPU");
                    selectedFiles = files.Take(MaxFiles).ToArray();
                }
                else { selectedFiles = files; }

                ProcessSelectedFiles();
            };

            Panel topBar = new Panel { Height = 40, Dock = DockStyle.Top, BackColor = darkBg };
            topBar.MouseDown += (s, e) => {
                if (e.Button == MouseButtons.Left)
                {
                    ReleaseCapture();
                    SendMessage(Handle, 0xA1, 0x2, 0);
                }
            };
            this.Controls.Add(topBar);

            Button btnClose = new Button { Width = 30, Height = 30, Text = "✕", ForeColor = Color.Gray, BackColor = darkBg, Cursor = Cursors.Hand, FlatStyle = FlatStyle.Flat };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            btnClose.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, btnClose.Width, btnClose.Height, 8, 8));
            btnClose.Click += (s, e) => Application.Exit();
            btnClose.MouseEnter += (s, e) => { btnClose.BackColor = Color.FromArgb(232, 17, 35); btnClose.ForeColor = Color.White; };
            btnClose.MouseLeave += (s, e) => { btnClose.BackColor = darkBg; btnClose.ForeColor = Color.Gray; };

            Button btnMin = new Button { Width = 30, Height = 30, Text = "—", ForeColor = Color.Gray, BackColor = darkBg, Cursor = Cursors.Hand, FlatStyle = FlatStyle.Flat };
            btnMin.FlatAppearance.BorderSize = 0;
            btnMin.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            btnMin.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, btnMin.Width, btnMin.Height, 8, 8));
            btnMin.Click += (s, e) => this.WindowState = FormWindowState.Minimized;
            btnMin.MouseEnter += (s, e) => { btnMin.BackColor = kayzeCyan; btnMin.ForeColor = darkBg; };
            btnMin.MouseLeave += (s, e) => { btnMin.BackColor = darkBg; btnMin.ForeColor = Color.Gray; };

            topBar.Resize += (s, e) => {
                btnClose.Left = topBar.Width - 40; btnClose.Top = 5;
                btnMin.Left = topBar.Width - 75; btnMin.Top = 5;
            };

            topBar.Controls.Add(btnClose);
            topBar.Controls.Add(btnMin);

            Label lblVersion = new Label { Text = "1.1.1 Stable   |", ForeColor = textGray, Font = new Font("Segoe UI", 9, FontStyle.Bold), AutoSize = true, Top = 12, Left = 15 };
            topBar.Controls.Add(lblVersion);

            Label lblGithub = new Label { Text = "GITHUB", ForeColor = Color.White, BackColor = darkPanel, Font = new Font("Segoe UI", 8, FontStyle.Bold), Height = 22, Width = 75, TextAlign = ContentAlignment.MiddleCenter, Cursor = Cursors.Hand };
            lblGithub.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, lblGithub.Width, lblGithub.Height, 10, 10));
            this.Load += (s, e) => { lblGithub.Left = lblVersion.Right + 10; lblGithub.Top = lblVersion.Top + ((lblVersion.Height - lblGithub.Height) / 2); };
            lblGithub.MouseEnter += (s, e) => lblGithub.ForeColor = kayzeCyan;
            lblGithub.MouseLeave += (s, e) => lblGithub.ForeColor = Color.White;
            lblGithub.Click += (s, e) => { Process.Start(new ProcessStartInfo { FileName = "https://github.com/kayzedevx0", UseShellExecute = true }); };
            topBar.Controls.Add(lblGithub);

            lblStatus.ForeColor = kayzeCyan;
            cmbOutputFormat.BackColor = darkPanel;
            cmbOutputFormat.ForeColor = textColor;
            cmbOutputFormat.FlatStyle = FlatStyle.Flat;
            cmbOutputFormat.DropDownStyle = ComboBoxStyle.DropDownList;
            progressBar.Visible = false;

            StyleModernButton(btnSelectFiles);
            StyleModernButton(btnConvert);
        }

        private void StyleModernButton(Button btn)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.BorderColor = Color.FromArgb(50, 50, 50);
            btn.BackColor = darkPanel;
            btn.ForeColor = textGray;
            btn.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            btn.Cursor = Cursors.Hand;
            btn.MouseEnter += (s, e) => { btn.BackColor = kayzeCyan; btn.ForeColor = darkBg; btn.FlatAppearance.BorderColor = kayzeCyan; };
            btn.MouseLeave += (s, e) => { btn.BackColor = darkPanel; btn.ForeColor = textGray; btn.FlatAppearance.BorderColor = Color.FromArgb(50, 50, 50); };
            btn.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, btn.Width, btn.Height, 10, 10));
        }

        private void SetupCustomUI()
        {
            pnlConverter = new Panel { Dock = DockStyle.Fill, BackColor = darkBg, Visible = false };
            this.Controls.Add(pnlConverter);
            pnlConverter.Controls.Add(btnSelectFiles);
            lblStatus.AutoSize = false; lblStatus.TextAlign = ContentAlignment.MiddleCenter;
            pnlConverter.Controls.Add(lblStatus);
            pnlConverter.Controls.Add(cmbOutputFormat);
            pnlConverter.Controls.Add(btnConvert);

            customProgressBg = new Panel { Height = 8, BackColor = darkPanel };
            customProgressBg.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, 300, 8, 4, 4));
            customProgressFill = new Panel { Height = 8, Width = 0, BackColor = kayzeCyan };
            customProgressBg.Controls.Add(customProgressFill);
            lblProgressText = new Label { ForeColor = kayzeCyan, Font = new Font("Segoe UI", 9, FontStyle.Bold), AutoSize = true, Text = "0%" };
            pnlConverter.Controls.Add(customProgressBg);
            pnlConverter.Controls.Add(lblProgressText);

            lblBack = CreateBackLabel();
            pnlConverter.Controls.Add(lblBack);

            pnlYoutube = new Panel { Dock = DockStyle.Fill, BackColor = darkBg, Visible = false };
            this.Controls.Add(pnlYoutube);

            Panel pnlTxtYtBg = new Panel { Name = "pnlTxtYtBg", Width = 300, Height = 42, BackColor = Color.FromArgb(40, 40, 40) };
            pnlTxtYtBg.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, 300, 42, 8, 8));

            txtYoutubeLink = new TextBox
            {
                Width = 280,
                Left = 10,
                Font = new Font("Segoe UI", 12),
                BackColor = Color.FromArgb(40, 40, 40),
                ForeColor = Color.FromArgb(150, 150, 150),
                BorderStyle = BorderStyle.None,
                Text = "Paste YouTube URL here..."
            };
            txtYoutubeLink.Top = (pnlTxtYtBg.Height - txtYoutubeLink.Height) / 2 + 1;

            txtYoutubeLink.GotFocus += (s, e) => {
                if (txtYoutubeLink.Text == "Paste YouTube URL here...")
                {
                    txtYoutubeLink.Text = "";
                    txtYoutubeLink.ForeColor = textColor;
                }
            };
            txtYoutubeLink.LostFocus += (s, e) => {
                if (string.IsNullOrWhiteSpace(txtYoutubeLink.Text))
                {
                    txtYoutubeLink.Text = "Paste YouTube URL here...";
                    txtYoutubeLink.ForeColor = Color.FromArgb(150, 150, 150);
                }
            };

            pnlTxtYtBg.Controls.Add(txtYoutubeLink);

            cmbYoutubeFormat = new ComboBox
            {
                Width = 300,
                Font = new Font("Segoe UI", 12),
                BackColor = Color.FromArgb(40, 40, 40),
                ForeColor = textColor,
                FlatStyle = FlatStyle.Flat,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbYoutubeFormat.Items.AddRange(new string[] { "Video MP4 (Max Quality)", "Audio Only (MP3)" });
            cmbYoutubeFormat.SelectedIndex = 0;

            Panel ytSpacer = new Panel { Name = "ytSpacer", Width = 300, Height = 25, BackColor = Color.Transparent };

            btnDownloadYoutube = new Button { Width = 300, Height = 50, Text = "DOWNLOAD YOUTUBE" };
            StyleModernButton(btnDownloadYoutube);
            btnDownloadYoutube.Click += BtnDownloadYoutube_Click;

            ytProgressBg = new Panel { Height = 8, Width = 300, BackColor = darkPanel };
            ytProgressBg.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, 300, 8, 4, 4));

            ytProgressFill = new Panel { Height = 8, Width = 0, BackColor = kayzeCyan };
            ytProgressBg.Controls.Add(ytProgressFill);

            lblYtProgressText = new Label { ForeColor = kayzeCyan, Font = new Font("Segoe UI", 9, FontStyle.Bold), AutoSize = true, Text = "0%" };

            Label lblYtBack = CreateBackLabel();

            pnlYoutube.Controls.Add(pnlTxtYtBg);
            pnlYoutube.Controls.Add(cmbYoutubeFormat);
            pnlYoutube.Controls.Add(ytSpacer);
            pnlYoutube.Controls.Add(btnDownloadYoutube);
            pnlYoutube.Controls.Add(ytProgressBg);
            pnlYoutube.Controls.Add(lblYtProgressText);
            pnlYoutube.Controls.Add(lblYtBack);



            pnlStem = new Panel { Dock = DockStyle.Fill, BackColor = darkBg, Visible = false };
            this.Controls.Add(pnlStem);

            btnSetupAi = new Button { Width = 300, Height = 55, Text = "INSTALL AI MODULES", Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            StyleModernButton(btnSetupAi);
            btnSetupAi.Click += BtnSetupAi_Click;

            lblAiStatus = new Label { Width = 300, Height = 40, ForeColor = textGray, Font = new Font("Segoe UI", 9), TextAlign = ContentAlignment.MiddleCenter };
            lblAiStatus.Text = "Python && Demucs are required for AI extraction.";

            btnSelectStemAudio = new Button { Width = 300, Height = 50, Text = "Select Audio File", Enabled = false };
            StyleModernButton(btnSelectStemAudio);
            btnSelectStemAudio.Click += BtnSelectStemAudio_Click;

            chkStems = new CheckedListBox { Width = 300, Height = 130, BackColor = darkPanel, ForeColor = textColor, BorderStyle = BorderStyle.None, Font = new Font("Segoe UI", 10) };
            chkStems.Items.AddRange(new string[] { "Vocals", "Drums", "Bass", "Other (Melody)" });
            for (int i = 0; i < chkStems.Items.Count; i++) chkStems.SetItemChecked(i, true);
            chkStems.Enabled = false;

            btnStartStem = new Button { Width = 300, Height = 55, Text = "SEPARATE STEMS", Enabled = false, Font = new Font("Segoe UI", 11, FontStyle.Bold) };
            StyleModernButton(btnStartStem);
            btnStartStem.Click += BtnStartStem_Click;

            stemProgressBg = new Panel { Height = 6, Width = 300, BackColor = Color.FromArgb(40, 40, 40) };
            stemProgressBg.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, 300, 6, 3, 3));
            stemProgressFill = new Panel { Height = 6, Width = 0, BackColor = kayzeCyan };
            stemProgressFill.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, 1, 6, 3, 3));
            stemProgressBg.Controls.Add(stemProgressFill);

            lblStemProgressText = new Label { ForeColor = kayzeCyan, Font = new Font("Segoe UI", 9, FontStyle.Bold), AutoSize = true, Text = "0%" };
            Label lblStemBack = CreateBackLabel();

            pnlStem.Controls.Add(btnSetupAi);
            pnlStem.Controls.Add(lblAiStatus);
            pnlStem.Controls.Add(btnSelectStemAudio);
            pnlStem.Controls.Add(chkStems);
            pnlStem.Controls.Add(btnStartStem);
            pnlStem.Controls.Add(stemProgressBg);
            pnlStem.Controls.Add(lblStemProgressText);
            pnlStem.Controls.Add(lblStemBack);

            kayzeToolTip.SetToolTip(btnSelectStemAudio, "Please install AI modules first using the button above.");


            pnlHome = new Panel { Dock = DockStyle.Fill, BackColor = darkBg };
            this.Controls.Add(pnlHome);

            Label lblKayze = new Label { Name = "lblKayze", Text = "KAYZE", ForeColor = kayzeCyan, Font = new Font("Segoe UI", 36, FontStyle.Bold), AutoSize = true };
            Label lblSub = new Label { Name = "lblSub", Text = "CONVERTER TOOL", ForeColor = textGray, Font = new Font("Segoe UI", 12, FontStyle.Regular), AutoSize = true };

            Button btnImgMode = new Button { Name = "btnImgMode", Text = "Image Converter", Width = 280, Height = 60 };
            StyleModernButton(btnImgMode);
            btnImgMode.Click += (s, e) => OpenConverter(true);
            kayzeToolTip.SetToolTip(btnImgMode, "Convert single or multiple images into various formats (PNG, JPG, WEBP).");

            Button btnFileMode = new Button { Name = "btnFileMode", Text = "Generic Files && Audio", Width = 280, Height = 60 };
            StyleModernButton(btnFileMode);
            btnFileMode.Click += (s, e) => OpenConverter(false);
            kayzeToolTip.SetToolTip(btnFileMode, "Convert documents, videos, and local audio files into your preferred format.");

            Button btnYoutubeMode = new Button { Name = "btnYoutubeMode", Text = "YouTube Downloader", Width = 280, Height = 60 };
            StyleModernButton(btnYoutubeMode);
            btnYoutubeMode.Click += (s, e) => OpenPanel(pnlYoutube);
            kayzeToolTip.SetToolTip(btnYoutubeMode, "Download YouTube videos at maximum quality mixing audio and video.");

            Button btnStemMode = new Button { Name = "btnStemMode", Text = "Stem Separation (AI)", Width = 280, Height = 60 };
            StyleModernButton(btnStemMode);
            btnStemMode.Click += async (s, e) => {
                OpenPanel(pnlStem);
                await CheckAiEnvironmentAsync();
            };
            kayzeToolTip.SetToolTip(btnStemMode, "Separate an audio file into individual stems: Vocals, Drums, Bass, Melody.");


            pnlHome.Controls.Add(lblKayze);
            pnlHome.Controls.Add(lblSub);
            pnlHome.Controls.Add(btnImgMode);
            pnlHome.Controls.Add(btnFileMode);
            pnlHome.Controls.Add(btnYoutubeMode);
            pnlHome.Controls.Add(btnStemMode);

            Label lblDev = new Label { Name = "lblDev", Text = "dev by ", ForeColor = textGray, Font = new Font("Segoe UI", 9), AutoSize = true };
            Label lblDevName = new Label { Name = "lblDevName", Text = "KAYZE", ForeColor = kayzeCyan, Font = new Font("Segoe UI", 9, FontStyle.Bold), AutoSize = true };
            pnlHome.Controls.Add(lblDev);
            pnlHome.Controls.Add(lblDevName);

            ShowHome();
        }

        private Label CreateBackLabel()
        {
            Label lbl = new Label { Text = "◄ BACK", AutoSize = true, ForeColor = textGray, Font = new Font("Segoe UI", 10, FontStyle.Bold), Cursor = Cursors.Hand };
            lbl.MouseEnter += (s, e) => lbl.ForeColor = kayzeCyan;
            lbl.MouseLeave += (s, e) => lbl.ForeColor = textGray;
            lbl.Click += (s, e) => ShowHome();
            return lbl;
        }

        private void CenterElements()
        {
            if (pnlHome != null && pnlHome.Visible)
            {
                Control kayze = pnlHome.Controls["lblKayze"];
                Control sub = pnlHome.Controls["lblSub"];
                Control btnImg = pnlHome.Controls["btnImgMode"];
                Control btnFile = pnlHome.Controls["btnFileMode"];
                Control btnYt = pnlHome.Controls["btnYoutubeMode"];
                Control btnStem = pnlHome.Controls["btnStemMode"];
                Control dev = pnlHome.Controls["lblDev"];
                Control devName = pnlHome.Controls["lblDevName"];

                if (kayze != null && sub != null)
                {
                    kayze.Left = (this.ClientSize.Width - kayze.Width) / 2;
                    kayze.Top = 70;
                    sub.Left = (this.ClientSize.Width - sub.Width) / 2;
                    sub.Top = kayze.Bottom - 5;
                }

                if (btnImg != null && btnFile != null && btnYt != null && btnStem != null)
                {
                    int gapY = 15;
                    int totalButtonsHeight = (btnImg.Height * 4) + (gapY * 3);
                    int availableHeight = this.ClientSize.Height - (sub != null ? sub.Bottom : 0) - 30;
                    int startY = (sub != null ? sub.Bottom : 0) + (availableHeight - totalButtonsHeight) / 2;
                    if (sub != null && startY < sub.Bottom + 20) startY = sub.Bottom + 20;

                    btnImg.Left = (this.ClientSize.Width - btnImg.Width) / 2;
                    btnFile.Left = (this.ClientSize.Width - btnFile.Width) / 2;
                    btnYt.Left = (this.ClientSize.Width - btnYt.Width) / 2;
                    btnStem.Left = (this.ClientSize.Width - btnStem.Width) / 2;

                    btnImg.Top = startY;
                    btnFile.Top = btnImg.Bottom + gapY;
                    btnYt.Top = btnFile.Bottom + gapY;
                    btnStem.Top = btnYt.Bottom + gapY;
                }

                if (dev != null && devName != null)
                {
                    int totWidth = dev.Width + devName.Width;
                    dev.Left = (this.ClientSize.Width - totWidth) / 2;
                    devName.Left = dev.Right - 5;
                    dev.Top = this.ClientSize.Height - dev.Height - 15;
                    devName.Top = dev.Top;
                }
            }

            CenterPanelControls(pnlConverter, new Control[] {
        btnSelectFiles, cmbOutputFormat, lblStatus, btnConvert, customProgressBg
    }, pnlConverter?.Controls.OfType<Label>().FirstOrDefault(l => l.Text != null && l.Text.Contains("BACK")));

            if (customProgressBg != null && lblProgressText != null)
            {
                lblProgressText.Left = customProgressBg.Right + 10;
                lblProgressText.Top = customProgressBg.Top + (customProgressBg.Height - lblProgressText.Height) / 2;
            }

            CenterPanelControls(pnlYoutube, new Control[] {
        pnlYoutube?.Controls["pnlTxtYtBg"],
        cmbYoutubeFormat,
        pnlYoutube?.Controls["ytSpacer"],
        btnDownloadYoutube,
        ytProgressBg
    }, pnlYoutube?.Controls.OfType<Label>().FirstOrDefault(l => l.Text != null && l.Text.Contains("BACK")));

            if (ytProgressBg != null && lblYtProgressText != null)
            {
                lblYtProgressText.Left = ytProgressBg.Right + 10;
                lblYtProgressText.Top = ytProgressBg.Top + (ytProgressBg.Height - lblYtProgressText.Height) / 2;
            }

            CenterPanelControls(pnlStem, new Control[] {
        btnSetupAi, lblAiStatus, btnSelectStemAudio, chkStems, btnStartStem, stemProgressBg
    }, pnlStem?.Controls.OfType<Label>().FirstOrDefault(l => l.Text != null && l.Text.Contains("BACK")));

            if (stemProgressBg != null && lblStemProgressText != null)
            {
                lblStemProgressText.Left = stemProgressBg.Right + 12;
                lblStemProgressText.Top = stemProgressBg.Top - (lblStemProgressText.Height / 2) + (stemProgressBg.Height / 2);
            }
        }

        private void CenterPanelControls(Panel parent, Control[] controls, Control backBtn)
        {
            if (parent == null || !parent.Visible) return;

            int colWidth = 300;
            int startX = (this.ClientSize.Width - colWidth) / 2;

            int totalHeight = controls.Sum(c => (c != null && c.Visible) ? c.Height + 15 : 0);

            int startY = (this.ClientSize.Height - totalHeight) / 2;
            if (startY < 60) startY = 60;

            int currentY = startY;

            foreach (var ctrl in controls)
            {
                if (ctrl == null || !ctrl.Visible) continue;

                ctrl.Width = colWidth;
                ctrl.Left = startX;

                if (ctrl == lblStatus)
                {
                    ctrl.Width = colWidth + 100;
                    ctrl.Left = (this.ClientSize.Width - ctrl.Width) / 2;
                }

                ctrl.Top = currentY;
                currentY += ctrl.Height + 15;
            }

            if (backBtn != null)
            {
                backBtn.Top = currentY + 20;
                backBtn.Left = (this.ClientSize.Width - backBtn.Width) / 2;
            }
        }

        private void ShowHome()
        {
            if (pnlConverter != null) pnlConverter.Visible = false;
            if (pnlYoutube != null) pnlYoutube.Visible = false;
            if (pnlStem != null) pnlStem.Visible = false;
            if (pnlHome != null) pnlHome.Visible = true;
        }

        private void OpenPanel(Panel target)
        {
            ShowHome();
            pnlHome.Visible = false;
            target.Visible = true;
            CenterElements();
        }

        private void OpenConverter(bool imageMode)
        {
            isImageMode = imageMode;
            cmbOutputFormat.Items.Clear();

            btnSelectFiles.Text = isImageMode ? $"Select Images (Max {MaxFiles})" : $"Select Files (Max {MaxFiles})";
            cmbOutputFormat.Items.AddRange(new string[] { "Choose a file first..." });
            cmbOutputFormat.SelectedIndex = 0;
            cmbOutputFormat.Enabled = false;
            btnConvert.Enabled = false;

            lblStatus.Text = "";
            lblStatus.Visible = false;
            selectedFiles = Array.Empty<string>();
            UpdateProgress(customProgressFill, lblProgressText, 0, 100);

            OpenPanel(pnlConverter);
        }

        private void ProcessSelectedFiles()
        {
            if (selectedFiles.Length == 0) return;
            string firstFileExt = Path.GetExtension(selectedFiles[0]).ToLower().Replace(".", "");
            bool isImageExt = new[] { "jpg", "jpeg", "png", "webp", "bmp", "gif", "tiff" }.Contains(firstFileExt);

            if (!isImageMode && isImageExt)
            {
                MessageBox.Show("You selected image files.\nSwitching to Image Module!", "Auto Switch", MessageBoxButtons.OK, MessageBoxIcon.Information);
                isImageMode = true;
            }

            lblStatus.Visible = true;
            lblStatus.Text = $"{selectedFiles.Length} file{(selectedFiles.Length == 1 ? "" : "s")} ready";
            btnSelectFiles.Text = $"{selectedFiles.Length} files selected";

            cmbOutputFormat.Items.Clear();
            cmbOutputFormat.Enabled = true;
            btnConvert.Enabled = true;

            if (isImageMode)
            {
                cmbOutputFormat.Items.AddRange(new string[] { "webp", "jpg", "png", "bmp", "gif", "tiff" });
            }
            else
            {
                if (firstFileExt == "mp4" || firstFileExt == "avi" || firstFileExt == "mkv" || firstFileExt == "mov")
                    cmbOutputFormat.Items.AddRange(new string[] { "mp4", "avi", "mkv", "mp3", "wav" });
                else if (firstFileExt == "mp3" || firstFileExt == "wav" || firstFileExt == "flac" || firstFileExt == "aac")
                    cmbOutputFormat.Items.AddRange(new string[] { "mp3", "wav" });
                else if (firstFileExt == "pdf")
                    cmbOutputFormat.Items.AddRange(new string[] { "docx", "txt" });
                else if (firstFileExt == "docx" || firstFileExt == "doc" || firstFileExt == "txt")
                    cmbOutputFormat.Items.AddRange(new string[] { "pdf", "txt", "docx" });
                else
                {
                    cmbOutputFormat.Items.Add("Unsupported format");
                    btnConvert.Enabled = false;
                }
            }

            if (cmbOutputFormat.Items.Count > 0) cmbOutputFormat.SelectedIndex = 0;
        }

        private void btnSelectFiles_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog { Multiselect = true, Filter = isImageMode ? "Images|*.jpg;*.jpeg;*.png;*.webp;*.bmp;*.gif;*.tiff" : "All files|*.*" })
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    if (ofd.FileNames.Length > MaxFiles)
                    {
                        MessageBox.Show($"Massimo {MaxFiles} file supportati dalla tua CPU ({cpuCores} core).", "Limit Exceeded");
                        selectedFiles = ofd.FileNames.Take(MaxFiles).ToArray();
                    }
                    else { selectedFiles = ofd.FileNames; }
                    ProcessSelectedFiles();
                }
            }
        }

        private async void btnConvert_Click(object sender, EventArgs e)
        {
            if (activeGlowButton == btnConvert) return;
            if (selectedFiles.Length == 0) return;

            string targetFormat = cmbOutputFormat.Text.Replace(".", "").Trim().ToLower();
            if (string.IsNullOrEmpty(targetFormat) || targetFormat.Contains("first") || targetFormat.Contains("unsupported")) return;

            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    activeGlowButton = btnConvert;
                    btnConvert.Text = "CONVERTING...";

                    int totalFiles = selectedFiles.Length;
                    int completedFiles = 0;
                    UpdateProgress(customProgressFill, lblProgressText, 0, 100);

                    var options = new ParallelOptions { MaxDegreeOfParallelism = MaxDegree };

                    await Task.Run(async () =>
                    {
                        await Parallel.ForEachAsync(selectedFiles, options, async (file, token) =>
                        {
                            try
                            {
                                string fileName = Path.GetFileNameWithoutExtension(file);
                                string destPath = Path.Combine(fbd.SelectedPath, $"{fileName}.{targetFormat}");

                                if (isImageMode)
                                {
                                    using (MagickImage image = new MagickImage(file))
                                    {
                                        image.Write(destPath);
                                    }
                                }
                                else if (new[] { "mp3", "wav", "mp4", "avi", "mkv" }.Contains(targetFormat))
                                {
                                    if (!isFFmpegReady) throw new Exception("FFmpeg Modules non scaricati.");
                                    var conversion = await FFmpeg.Conversions.FromSnippet.Convert(file, destPath);

                                    conversion.OnProgress += (s, args) =>
                                    {
                                        if (totalFiles == 1)
                                        {
                                            this.Invoke((MethodInvoker)delegate {
                                                UpdateProgress(customProgressFill, lblProgressText, args.Percent, 100);
                                            });
                                        }
                                    };
                                    await conversion.Start(token);
                                }
                                else if (targetFormat == "pdf")
                                {
                                    using (Document doc = new Document())
                                    {
                                        doc.LoadFromFile(file);
                                        doc.SaveToFile(destPath, FileFormat.PDF);
                                    }
                                }
                                else if (targetFormat == "docx")
                                {
                                    using (Document doc = new Document())
                                    {
                                        doc.LoadFromFile(file);
                                        doc.SaveToFile(destPath, FileFormat.Docx);
                                    }
                                }
                                else if (targetFormat == "txt")
                                {
                                    using (Document doc = new Document())
                                    {
                                        doc.LoadFromFile(file);
                                        doc.SaveToFile(destPath, FileFormat.Txt);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                this.Invoke((MethodInvoker)delegate {
                                    MessageBox.Show($"Errore in {Path.GetFileName(file)}:\n{ex.Message}");
                                });
                            }
                            finally
                            {
                                Interlocked.Increment(ref completedFiles);
                                if (totalFiles > 1)
                                {
                                    int perc = (int)((double)completedFiles / totalFiles * 100);
                                    this.Invoke((MethodInvoker)delegate {
                                        UpdateProgress(customProgressFill, lblProgressText, perc, 100);
                                    });
                                }
                            }
                        });
                    });

                    lblStatus.Text = "Conversion completed!";
                    btnConvert.Text = "CONVERT AND SAVE";
                    UpdateProgress(customProgressFill, lblProgressText, 100, 100);
                }
            }
        }

        private async void BtnDownloadYoutube_Click(object sender, EventArgs e)
        {
            if (activeGlowButton == btnDownloadYoutube) return;

            string url = txtYoutubeLink.Text;
            if (string.IsNullOrWhiteSpace(url) || !url.Contains("youtu"))
            {
                MessageBox.Show("Inserisci un link YouTube valido.");
                return;
            }

            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    activeGlowButton = btnDownloadYoutube;
                    btnDownloadYoutube.Text = "DOWNLOADING...";
                    UpdateProgress(ytProgressFill, lblYtProgressText, 0, 100);

                    try
                    {
                        var youtube = new YoutubeClient();
                        var video = await youtube.Videos.GetAsync(url);
                        string safeTitle = string.Join("_", video.Title.Split(Path.GetInvalidFileNameChars()));

                        bool isAudioOnly = cmbYoutubeFormat.SelectedIndex == 1;
                        string ext = isAudioOnly ? "mp3" : "mp4";
                        string destPath = Path.Combine(fbd.SelectedPath, $"{safeTitle}.{ext}");

                        DateTime lastUpdate = DateTime.Now;
                        var progress = new Progress<double>(p =>
                        {
                            if ((DateTime.Now - lastUpdate).TotalMilliseconds > 200)
                            {
                                lastUpdate = DateTime.Now;
                                this.Invoke((MethodInvoker)delegate {
                                    UpdateProgress(ytProgressFill, lblYtProgressText, (int)(p * 100), 100);
                                });
                            }
                        });

                        if (isAudioOnly)
                        {
                            var streamManifest = await youtube.Videos.Streams.GetManifestAsync(url);
                            var streamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();
                            await youtube.Videos.Streams.DownloadAsync(streamInfo, destPath, progress);
                        }
                        else
                        {
                            if (!isFFmpegReady) throw new Exception("FFmpeg modules non sono pronti.");
                            string ffmpegPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FFmpeg", "ffmpeg.exe");

                            var builder = new ConversionRequestBuilder(destPath).SetFFmpegPath(ffmpegPath).Build();

                            var streamManifest = await youtube.Videos.Streams.GetManifestAsync(url);
                            var audioStreamInfo = streamManifest.GetAudioStreams().GetWithHighestBitrate();
                            var videoStreamInfo = streamManifest.GetVideoStreams().Where(s => s.Container.Name == "mp4").GetWithHighestVideoQuality();
                            var streamInfos = new IStreamInfo[] { audioStreamInfo, videoStreamInfo };

                            await youtube.Videos.DownloadAsync(streamInfos, builder, progress);
                        }

                        this.Invoke((MethodInvoker)delegate {
                            UpdateProgress(ytProgressFill, lblYtProgressText, 100, 100);
                            btnDownloadYoutube.Text = "DOWNLOAD COMPLETED!";
                        });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Errore download YouTube: {ex.Message}");
                        btnDownloadYoutube.Text = "DOWNLOAD YOUTUBE";
                        if (activeGlowButton != null)
                        {
                            glowTimer.Stop();
                            activeGlowButton.ForeColor = textGray;
                            activeGlowButton = null;
                        }
                    }
                }
            }
        }

        private void BtnSelectStemAudio_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog { Filter = "Audio Files|*.mp3;*.wav;*.flac;*.m4a" })
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    stemAudioPath = ofd.FileName;
                    btnSelectStemAudio.Text = Path.GetFileName(stemAudioPath);
                    btnStartStem.Enabled = true;
                }
            }
        }

        private async void BtnStartStem_Click(object sender, EventArgs e)
        {
            if (activeGlowButton == btnStartStem) return;
            if (string.IsNullOrEmpty(stemAudioPath)) return;

            bool keepVocals = chkStems.GetItemChecked(0);
            bool keepDrums = chkStems.GetItemChecked(1);
            bool keepBass = chkStems.GetItemChecked(2);
            bool keepOther = chkStems.GetItemChecked(3);

            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    activeGlowButton = btnStartStem;
                    btnStartStem.Text = "SEPARATING STEMS... (AI)";
                    UpdateProgress(stemProgressFill, lblStemProgressText, 0, 100);

                    string outputDir = fbd.SelectedPath;

                    await Task.Run(async () =>
                    {
                        bool isProcessing = true;

                        try
                        {
                            string errorMessage = "";
                            string ffmpegFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FFmpeg");
                            string currentPath = Environment.GetEnvironmentVariable("PATH") ?? "";

                            if (!currentPath.Contains(ffmpegFolder))
                            {
                                Environment.SetEnvironmentVariable("PATH", ffmpegFolder + ";" + currentPath);
                            }

                            double targetSeconds = 10.0;
                            try
                            {
                                var mediaInfo = await Xabe.FFmpeg.FFmpeg.GetMediaInfo(stemAudioPath);
                                if (mediaInfo.Duration.TotalSeconds > 0 && mediaInfo.Duration.TotalSeconds < 10)
                                {
                                    targetSeconds = mediaInfo.Duration.TotalSeconds;
                                }
                            }
                            catch { }

                            int delayMs = (int)((targetSeconds * 1000) / 10);

                            Task fakeProgress = Task.Run(async () =>
                            {
                                for (int i = 1; i <= 10; i++)
                                {
                                    if (!isProcessing) return;
                                    this.Invoke((MethodInvoker)delegate { UpdateProgress(stemProgressFill, lblStemProgressText, i, 100); });
                                    await Task.Delay(delayMs);
                                }

                                int percent = 11;
                                while (isProcessing && percent <= 85)
                                {
                                    await Task.Delay(3000);
                                    if (!isProcessing) return;
                                    this.Invoke((MethodInvoker)delegate { UpdateProgress(stemProgressFill, lblStemProgressText, percent, 100); });
                                    percent++;
                                }
                            });

                            ProcessStartInfo startInfo = new ProcessStartInfo
                            {
                                FileName = "cmd.exe",
                                Arguments = $"/c py -m demucs \"{stemAudioPath}\" -o \"{outputDir}\"",
                                RedirectStandardOutput = true,
                                RedirectStandardError = true,
                                UseShellExecute = false,
                                CreateNoWindow = true
                            };

                            using (Process process = new Process { StartInfo = startInfo })
                            {
                                process.ErrorDataReceived += (s, ev) =>
                                {
                                    if (!string.IsNullOrEmpty(ev.Data) && (ev.Data.Contains("Error") || ev.Data.Contains("Exception") || ev.Data.Contains("Traceback")))
                                    {
                                        errorMessage += ev.Data + "\n";
                                    }
                                };

                                process.Start();
                                process.BeginErrorReadLine();
                                process.WaitForExit();

                                isProcessing = false;

                                if (process.ExitCode != 0)
                                {
                                    string finalError = string.IsNullOrEmpty(errorMessage) ? "Unknown crash." : errorMessage;
                                    throw new Exception($"Process failed.\n\nDetails:\n{finalError}");
                                }
                            }

                            string songName = Path.GetFileNameWithoutExtension(stemAudioPath);
                            string originalModelFolder = Path.Combine(outputDir, "htdemucs");
                            string originalStemsFolder = Path.Combine(originalModelFolder, songName);

                            string kayzeRootFolder = Path.Combine(outputDir, "KAYZE Stems");
                            string finalStemsFolder = Path.Combine(kayzeRootFolder, songName);

                            if (Directory.Exists(originalStemsFolder))
                            {
                                Directory.CreateDirectory(kayzeRootFolder);

                                if (Directory.Exists(finalStemsFolder))
                                {
                                    Directory.Delete(finalStemsFolder, true);
                                }

                                Directory.Move(originalStemsFolder, finalStemsFolder);

                                try
                                {
                                    if (Directory.GetFileSystemEntries(originalModelFolder).Length == 0)
                                    {
                                        Directory.Delete(originalModelFolder);
                                    }
                                }
                                catch { }

                                if (!keepVocals && File.Exists(Path.Combine(finalStemsFolder, "vocals.wav"))) File.Delete(Path.Combine(finalStemsFolder, "vocals.wav"));
                                if (!keepDrums && File.Exists(Path.Combine(finalStemsFolder, "drums.wav"))) File.Delete(Path.Combine(finalStemsFolder, "drums.wav"));
                                if (!keepBass && File.Exists(Path.Combine(finalStemsFolder, "bass.wav"))) File.Delete(Path.Combine(finalStemsFolder, "bass.wav"));
                                if (!keepOther && File.Exists(Path.Combine(finalStemsFolder, "other.wav"))) File.Delete(Path.Combine(finalStemsFolder, "other.wav"));
                            }

                            this.Invoke((MethodInvoker)delegate {
                                UpdateProgress(stemProgressFill, lblStemProgressText, 100, 100);
                                MessageBox.Show("Stem Separation completed successfully!", "Completed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            });
                        }
                        catch (Exception ex)
                        {
                            isProcessing = false;
                            this.Invoke((MethodInvoker)delegate {
                                UpdateProgress(stemProgressFill, lblStemProgressText, 0, 100);
                                DialogResult dr = MessageBox.Show(
                                    ex.Message + "\n\nIt seems your AI modules are missing or corrupted (possibly from an interrupted installation).\n\n" +
                                    "Would you like to FORCE REPAIR? (This will cleanly reinstall the modules).",
                                    "AI Extraction Failed", MessageBoxButtons.YesNo, MessageBoxIcon.Error);

                                if (dr == DialogResult.Yes)
                                {
                                    lblAiStatus.Text = "Uninstalling corrupted modules... Please wait.";
                                    lblAiStatus.ForeColor = Color.Orange;
                                    btnSetupAi.Visible = true;

                                    Task.Run(() => {
                                        ProcessStartInfo purge = new ProcessStartInfo
                                        {
                                            FileName = "cmd.exe",
                                            Arguments = "/c py -m pip uninstall -y torch torchvision torchaudio torchcodec demucs soundfile numpy",
                                            UseShellExecute = false,
                                            CreateNoWindow = true
                                        };
                                        Process.Start(purge)?.WaitForExit();

                                        this.Invoke((MethodInvoker)delegate {
                                            isAiReady = false;
                                            BtnSetupAi_Click(null, null);
                                        });
                                    });
                                }

                                btnStartStem.Text = "SEPARATE STEMS";
                                if (activeGlowButton != null)
                                {
                                    glowTimer.Stop();
                                    activeGlowButton.ForeColor = textGray;
                                    activeGlowButton = null;
                                }
                            });
                        }
                    });

                    btnStartStem.Text = "SEPARATE STEMS";
                }
            }
        }

        private async void BtnSetupAi_Click(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show(
                "The AI Setup will download approximately 3GB of data (PyTorch & Demucs).\n\n" +
                "During the installation your computer may slow down and the progress bar might seem stuck at some percentages. " +
                "DO NOT close this window or kill background processes, otherwise the files will corrupt.\n\n" +
                "Do you want to proceed?",
                "Important Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (dr == DialogResult.No) return;

            btnSetupAi.Enabled = false;
            activeGlowButton = btnSetupAi;
            stemProgressBg.Visible = true;
            lblStemProgressText.Visible = true;
            UpdateProgress(stemProgressFill, lblStemProgressText, 0, 100);

            await Task.Run(() =>
            {
                try
                {
                    this.Invoke((MethodInvoker)delegate { lblAiStatus.Text = "Checking Python installation..."; });
                    bool pythonExists = CheckIfPythonInstalled();
                    int simulatedProgress = 0; 

                    if (!pythonExists)
                    {
                        this.Invoke((MethodInvoker)delegate { lblAiStatus.Text = "Downloading Python (Step 0/3)..."; UpdateProgress(stemProgressFill, lblStemProgressText, 5, 100); });
                        string installerPath = Path.Combine(Path.GetTempPath(), "python-installer.exe");

                        using (var client = new System.Net.WebClient())
                        {
                            client.DownloadFile("https://www.python.org/ftp/python/3.10.11/python-3.10.11-amd64.exe", installerPath);
                        }

                        this.Invoke((MethodInvoker)delegate { lblAiStatus.Text = "Installing Python... This may take a minute."; UpdateProgress(stemProgressFill, lblStemProgressText, 10, 100); });

                        ProcessStartInfo psiPy = new ProcessStartInfo
                        {
                            FileName = installerPath,
                            Arguments = "/quiet InstallAllUsers=1 PrependPath=1",
                            UseShellExecute = true,
                            Verb = "runas"
                        };
                        using (Process pyProcess = Process.Start(psiPy))
                        {
                            pyProcess.WaitForExit();
                        }

                        Environment.SetEnvironmentVariable("PATH", Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine) + ";" + Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User));
                    }

                    simulatedProgress = 15;
                    this.Invoke((MethodInvoker)delegate { UpdateProgress(stemProgressFill, lblStemProgressText, simulatedProgress, 100); });

                    this.Invoke((MethodInvoker)delegate { lblAiStatus.Text = "Downloading AI Core (Step 1/3)..."; });
                    ProcessStartInfo psiTorch = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = "/c py -m pip install torch torchvision torchaudio --index-url https://download.pytorch.org/whl/cu118",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using (Process torchProcess = new Process { StartInfo = psiTorch })
                    {
                        torchProcess.OutputDataReceived += (s, ev) =>
                        {
                            if (!string.IsNullOrEmpty(ev.Data) && (ev.Data.Contains("Downloading") || ev.Data.Contains("Collecting") || ev.Data.Contains("Installing")))
                            {
                                string log = ev.Data.Length > 60 ? ev.Data.Substring(0, 57) + "..." : ev.Data;

                                if (simulatedProgress < 60) simulatedProgress += 1;

                                this.Invoke((MethodInvoker)delegate {
                                    lblAiStatus.Text = $"Step 1/3: {log}";
                                    UpdateProgress(stemProgressFill, lblStemProgressText, simulatedProgress, 100);
                                });
                            }
                        };

                        torchProcess.Start();
                        torchProcess.BeginOutputReadLine();
                        torchProcess.WaitForExit();
                    }

                    simulatedProgress = 65;
                    this.Invoke((MethodInvoker)delegate { UpdateProgress(stemProgressFill, lblStemProgressText, simulatedProgress, 100); });

                    this.Invoke((MethodInvoker)delegate { lblAiStatus.Text = "Downloading Codecs (Step 2/3)..."; });
                    ProcessStartInfo psiPip = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = "/c py -m pip install -U demucs torchcodec soundfile numpy",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using (Process pipProcess = new Process { StartInfo = psiPip })
                    {
                        pipProcess.OutputDataReceived += (s, ev) =>
                        {
                            if (!string.IsNullOrEmpty(ev.Data) && (ev.Data.Contains("Downloading") || ev.Data.Contains("Collecting")))
                            {
                                string log = ev.Data.Length > 60 ? ev.Data.Substring(0, 57) + "..." : ev.Data;

                                if (simulatedProgress < 85) simulatedProgress += 2;

                                this.Invoke((MethodInvoker)delegate {
                                    lblAiStatus.Text = $"Step 2/3: {log}";
                                    UpdateProgress(stemProgressFill, lblStemProgressText, simulatedProgress, 100);
                                });
                            }
                        };

                        pipProcess.Start();
                        pipProcess.BeginOutputReadLine();
                        pipProcess.WaitForExit();

                        if (pipProcess.ExitCode != 0)
                        {
                            throw new Exception("Installation failed during Step 2.");
                        }
                    }

                    simulatedProgress = 90;
                    this.Invoke((MethodInvoker)delegate { UpdateProgress(stemProgressFill, lblStemProgressText, simulatedProgress, 100); });

                    this.Invoke((MethodInvoker)delegate { lblAiStatus.Text = "Downloading Neural Weights... (Step 3/3)"; });
                    ProcessStartInfo psiWeights = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = "/c python -c \"import demucs.api; demucs.api.Separator(model='htdemucs')\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using (Process weightProcess = new Process { StartInfo = psiWeights })
                    {
                        weightProcess.OutputDataReceived += (s, ev) =>
                        {
                            if (!string.IsNullOrEmpty(ev.Data) && ev.Data.Contains("%"))
                            {
                                if (simulatedProgress < 99) simulatedProgress += 1;
                                this.Invoke((MethodInvoker)delegate {
                                    UpdateProgress(stemProgressFill, lblStemProgressText, simulatedProgress, 100);
                                });
                            }
                        };

                        weightProcess.Start();
                        weightProcess.BeginOutputReadLine();
                        weightProcess.WaitForExit();
                    }

                    this.Invoke((MethodInvoker)delegate {
                        UpdateProgress(stemProgressFill, lblStemProgressText, 100, 100);
                        lblAiStatus.Text = "AI Environment Ready!";
                        lblAiStatus.ForeColor = kayzeCyan;
                        btnSetupAi.Visible = false;
                        isAiReady = true;
                        btnSelectStemAudio.Enabled = true;
                        chkStems.Enabled = true;
                        kayzeToolTip.SetToolTip(btnSelectStemAudio, "");

                        CenterElements();

                        Task.Delay(1500).ContinueWith(_ => {
                            this.Invoke((MethodInvoker)delegate {
                                UpdateProgress(stemProgressFill, lblStemProgressText, 0, 100);
                            });
                        });
                    });

                }
                catch (Exception ex)
                {
                    this.Invoke((MethodInvoker)delegate {
                        UpdateProgress(stemProgressFill, lblStemProgressText, 0, 100);
                        lblAiStatus.Text = "Setup failed. Check internet connection.";
                        MessageBox.Show(ex.Message, "AI Setup Error");
                        btnSetupAi.Enabled = true;
                    });
                }
            });
        }

        private bool CheckIfPythonInstalled()
        {
            try
            {
                ProcessStartInfo pyCheck = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "/c python --version",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process process = Process.Start(pyCheck))
                {
                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();
                    return output.Contains("Python 3") || process.ExitCode == 0;
                }
            }
            catch { return false; }
        }

        private async Task CheckAiEnvironmentAsync()
        {
            if (isAiReady) return;

            this.Invoke((MethodInvoker)delegate {
                lblAiStatus.Text = "Checking AI Requirements...";
                btnSetupAi.Enabled = false;
            });

            bool isReady = await Task.Run(() =>
            {
                try
                {
                    ProcessStartInfo pipList = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = "/c py -m pip list | findstr /I \"torch torchaudio torchcodec demucs soundfile\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using (Process pList = new Process { StartInfo = pipList })
                    {
                        pList.Start();
                        string output = pList.StandardOutput.ReadToEnd();
                        pList.WaitForExit();

                        if (output.Contains("torch") && output.Contains("torchaudio") &&
                            output.Contains("torchcodec") && output.Contains("demucs") &&
                            output.Contains("soundfile"))
                        {
                            return true;
                        }
                        return false;
                    }
                }
                catch { return false; }
            });

            this.Invoke((MethodInvoker)delegate {
                if (isReady)
                {
                    isAiReady = true;
                    lblAiStatus.Text = "AI Environment Ready!";
                    lblAiStatus.ForeColor = kayzeCyan;
                    btnSetupAi.Visible = false;
                    btnSelectStemAudio.Enabled = true;
                    chkStems.Enabled = true;
                    kayzeToolTip.SetToolTip(btnSelectStemAudio, "");
                    CenterElements(); 
                }
                else
                {
                    lblAiStatus.Text = "Missing Core Modules. Please install them.";
                    lblAiStatus.ForeColor = Color.Orange;
                    btnSetupAi.Visible = true;
                    btnSetupAi.Enabled = true;
                    CenterElements();
                }
            });
        }

        private void UpdateProgress(Panel fillPanel, Label textLabel, int current, int total)
        {
            if (total == 0) total = 1;
            int newWidth = (int)((double)current / total * 300);
            if (newWidth < 1) newWidth = 1;
            if (newWidth > 300) newWidth = 300;

            fillPanel.Width = newWidth;
            fillPanel.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, newWidth, fillPanel.Height, 4, 4));

            if (textLabel != null) textLabel.Text = $"{current}%";

            if (current >= 100)
            {
                glowTimer.Stop();
                glowStep = 0;
                fillPanel.BackColor = kayzeCyan;

                if (activeGlowButton != null)
                {
                    activeGlowButton.ForeColor = textGray;
                    activeGlowButton = null;
                }
            }
            else if (current >= 0)
            {
                if (!glowTimer.Enabled && activeGlowButton != null)
                {
                    glowStep = 0;
                    glowTimer.Start();
                }
            }
        }

        private void GlowTimer_Tick(object sender, EventArgs e)
        {
            glowStep += 0.15;
            if (glowStep > Math.PI * 2) glowStep = 0;

            double intensity = (Math.Sin(glowStep) + 1.0) / 2.0;

            int r = (int)(kayzeCyan.R + ((255 - kayzeCyan.R) * intensity * 0.7));
            int g = (int)(kayzeCyan.G + ((255 - kayzeCyan.G) * intensity * 0.5));
            int b = (int)(kayzeCyan.B + ((255 - kayzeCyan.B) * intensity * 0.5));

            Color glowingCyan = Color.FromArgb(255, r, g, b);

            if (activeGlowButton != null)
            {
                activeGlowButton.ForeColor = glowingCyan;
            }

            if (customProgressFill != null && customProgressFill.Width > 0 && pnlConverter.Visible)
                customProgressFill.BackColor = glowingCyan;

            if (ytProgressFill != null && ytProgressFill.Width > 0 && pnlYoutube.Visible)
                ytProgressFill.BackColor = glowingCyan;

            if (stemProgressFill != null && stemProgressFill.Width > 0 && pnlStem.Visible)
                stemProgressFill.BackColor = glowingCyan;
        }

    }
}
