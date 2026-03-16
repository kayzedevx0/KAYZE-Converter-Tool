using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using ImageMagick;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Downloader;
using Spire.Doc;

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
        private Label lblBack;

        private Panel customProgressBg;
        private Panel customProgressFill;

        public Form1()
        {
            InitializeComponent();
            this.Text = "KAYZE Converter Tool";
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
                    MessageBox.Show("Audio/Video Modules (FFmpeg) successfully installed and ready for use!", "KAYZE Converter", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            this.MinimumSize = new Size(450, 520);

            Panel topBar = new Panel { Height = 40, Dock = DockStyle.Top, BackColor = darkBg };
            topBar.MouseDown += (s, e) => {
                if (e.Button == MouseButtons.Left)
                {
                    ReleaseCapture();
                    SendMessage(Handle, 0xA1, 0x2, 0);
                }
            };
            this.Controls.Add(topBar);

            System.Windows.Forms.Button btnClose = new System.Windows.Forms.Button { Width = 30, Height = 30, Text = "✕", ForeColor = Color.Gray, BackColor = darkBg, Cursor = Cursors.Hand };
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            btnClose.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, btnClose.Width, btnClose.Height, 8, 8));
            btnClose.Click += (s, e) => Application.Exit();

            btnClose.MouseEnter += (s, e) => {
                btnClose.BackColor = Color.FromArgb(232, 17, 35);
                btnClose.ForeColor = Color.White;
            };
            btnClose.MouseLeave += (s, e) => {
                btnClose.BackColor = darkBg;
                btnClose.ForeColor = Color.Gray;
            };

            System.Windows.Forms.Button btnMin = new System.Windows.Forms.Button { Width = 30, Height = 30, Text = "—", ForeColor = Color.Gray, BackColor = darkBg, Cursor = Cursors.Hand };
            btnMin.FlatStyle = FlatStyle.Flat;
            btnMin.FlatAppearance.BorderSize = 0;
            btnMin.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            btnMin.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, btnMin.Width, btnMin.Height, 8, 8));
            btnMin.Click += (s, e) => this.WindowState = FormWindowState.Minimized;

            btnMin.MouseEnter += (s, e) => {
                btnMin.BackColor = kayzeCyan;
                btnMin.ForeColor = darkBg;
            };
            btnMin.MouseLeave += (s, e) => {
                btnMin.BackColor = darkBg;
                btnMin.ForeColor = Color.Gray;
            };

            topBar.Resize += (s, e) => {
                btnClose.Left = topBar.Width - 40;
                btnClose.Top = 5;

                btnMin.Left = topBar.Width - 75;
                btnMin.Top = 5;
            };

            topBar.Controls.Add(btnClose);
            topBar.Controls.Add(btnMin);

            Label lblVersion = new Label { Text = "1.0.0 Stable   |", ForeColor = textGray, Font = new Font("Segoe UI", 9, FontStyle.Bold), AutoSize = true, Top = 12, Left = 15 };
            topBar.Controls.Add(lblVersion);

            Label lblGithub = new Label
            {
                Text = "GITHUB",
                ForeColor = Color.White,
                BackColor = darkPanel,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                Height = 22,
                Width = 75,
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand,
                AutoSize = false
            };
            lblGithub.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, lblGithub.Width, lblGithub.Height, 10, 10));

            this.Load += (s, e) => {
                lblGithub.Left = lblVersion.Right + 10;
                lblGithub.Top = lblVersion.Top + ((lblVersion.Height - lblGithub.Height) / 2);
            };

            lblGithub.MouseEnter += (s, e) => lblGithub.ForeColor = kayzeCyan;
            lblGithub.MouseLeave += (s, e) => lblGithub.ForeColor = Color.White;
            lblGithub.Click += (s, e) => { Process.Start(new ProcessStartInfo { FileName = "https://github.com/kayzedevx0", UseShellExecute = true }); };

            topBar.Controls.Add(lblGithub);
            lblGithub.BringToFront();
            topBar.BringToFront();

            lblStatus.ForeColor = kayzeCyan;
            cmbOutputFormat.BackColor = darkPanel;
            cmbOutputFormat.ForeColor = textColor;
            cmbOutputFormat.FlatStyle = FlatStyle.Flat;
            cmbOutputFormat.DropDownStyle = ComboBoxStyle.DropDownList;
            progressBar.Visible = false;

            StyleModernButton(btnSelectFiles);
            StyleModernButton(btnConvert);
        }

        private void StyleModernButton(System.Windows.Forms.Button btn)
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
        }

        private void SetupCustomUI()
        {
            pnlConverter = new Panel { Dock = DockStyle.Fill, BackColor = darkBg, Visible = false };
            this.Controls.Add(pnlConverter);

            pnlConverter.Controls.Add(btnSelectFiles);

            lblStatus.AutoSize = false;
            lblStatus.TextAlign = ContentAlignment.MiddleCenter;
            pnlConverter.Controls.Add(lblStatus);

            pnlConverter.Controls.Add(cmbOutputFormat);
            pnlConverter.Controls.Add(btnConvert);

            customProgressBg = new Panel { Height = 8, BackColor = darkPanel };
            customProgressBg.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, 300, 8, 4, 4));
            customProgressFill = new Panel { Height = 8, Width = 0, BackColor = kayzeCyan };
            customProgressFill.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, 1, 8, 4, 4));
            customProgressBg.Controls.Add(customProgressFill);
            pnlConverter.Controls.Add(customProgressBg);

            lblBack = new Label
            {
                Text = "◄ BACK",
                AutoSize = true,
                ForeColor = textGray,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            lblBack.MouseEnter += (s, e) => lblBack.ForeColor = kayzeCyan;
            lblBack.MouseLeave += (s, e) => lblBack.ForeColor = textGray;
            lblBack.Click += (s, e) => ShowHome();
            pnlConverter.Controls.Add(lblBack);

            pnlHome = new Panel { Dock = DockStyle.Fill, BackColor = darkBg };
            this.Controls.Add(pnlHome);

            Label lblKayze = new Label { Name = "lblKayze", Text = "KAYZE", ForeColor = kayzeCyan, Font = new Font("Segoe UI", 36, FontStyle.Bold), AutoSize = true };
            pnlHome.Controls.Add(lblKayze);

            Label lblSub = new Label { Name = "lblSub", Text = "UNIVERSAL CONVERTER", ForeColor = textGray, Font = new Font("Segoe UI", 12, FontStyle.Regular), AutoSize = true };
            pnlHome.Controls.Add(lblSub);

            System.Windows.Forms.Button btnImgMode = new System.Windows.Forms.Button { Name = "btnImgMode", Text = "Image Converter Module", Width = 300, Height = 60 };
            StyleModernButton(btnImgMode);
            btnImgMode.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, btnImgMode.Width, btnImgMode.Height, 15, 15));
            btnImgMode.Click += (s, e) => OpenConverter(true);

            System.Windows.Forms.Button btnFileMode = new System.Windows.Forms.Button { Name = "btnFileMode", Text = "Generic Files Module", Width = 300, Height = 60 };
            StyleModernButton(btnFileMode);
            btnFileMode.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, btnFileMode.Width, btnFileMode.Height, 15, 15));
            btnFileMode.Click += (s, e) => OpenConverter(false);

            pnlHome.Controls.Add(btnImgMode);
            pnlHome.Controls.Add(btnFileMode);

            Label lblDev = new Label { Name = "lblDev", Text = "dev by ", ForeColor = textGray, Font = new Font("Segoe UI", 9), AutoSize = true };
            Label lblDevName = new Label { Name = "lblDevName", Text = "KAYZE", ForeColor = kayzeCyan, Font = new Font("Segoe UI", 9, FontStyle.Bold), AutoSize = true };
            pnlHome.Controls.Add(lblDev);
            pnlHome.Controls.Add(lblDevName);

            ShowHome();
        }

        private void CenterElements()
        {
            if (pnlHome != null && pnlHome.Visible)
            {
                Control kayze = pnlHome.Controls["lblKayze"];
                Control sub = pnlHome.Controls["lblSub"];
                Control btnImg = pnlHome.Controls["btnImgMode"];
                Control btnFile = pnlHome.Controls["btnFileMode"];
                Control dev = pnlHome.Controls["lblDev"];
                Control devName = pnlHome.Controls["lblDevName"];

                if (kayze != null && sub != null)
                {
                    kayze.Left = (this.ClientSize.Width - kayze.Width) / 2;
                    kayze.Top = 75;
                    sub.Left = (this.ClientSize.Width - sub.Width) / 2;
                    sub.Top = kayze.Bottom - 5;
                }

                if (btnImg != null && btnFile != null && sub != null)
                {
                    btnImg.Left = (this.ClientSize.Width - btnImg.Width) / 2;
                    btnFile.Left = (this.ClientSize.Width - btnFile.Width) / 2;
                    int availableHeight = (this.ClientSize.Height - 30) - sub.Bottom;
                    int totalButtonsHeight = btnImg.Height + btnFile.Height + 15;
                    int startY = sub.Bottom + ((availableHeight - totalButtonsHeight) / 2);
                    btnImg.Top = startY;
                    btnFile.Top = btnImg.Bottom + 15;
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

            if (pnlConverter != null && pnlConverter.Visible)
            {
                int colWidth = 300;
                int startX = (this.ClientSize.Width - colWidth) / 2;

                int btnSelectHeight = 50;
                int cmbHeight = 35;
                int lblStatusHeight = 25;
                int btnConvertHeight = 50;
                int progressHeight = 8;

                int smallGap = 8;
                int bigGap = 20;

                int totalBlockHeight = btnSelectHeight + smallGap + cmbHeight + bigGap + lblStatusHeight + bigGap + btnConvertHeight + bigGap + progressHeight + 50;

                int startY = (this.ClientSize.Height - totalBlockHeight) / 2;
                if (startY < 60) startY = 60;

                btnSelectFiles.Width = colWidth;
                btnSelectFiles.Height = btnSelectHeight;
                btnSelectFiles.Left = startX;
                btnSelectFiles.Top = startY;
                btnSelectFiles.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, btnSelectFiles.Width, btnSelectFiles.Height, 10, 10));

                cmbOutputFormat.Width = colWidth;
                cmbOutputFormat.Height = cmbHeight;
                cmbOutputFormat.Left = startX;
                cmbOutputFormat.Top = btnSelectFiles.Bottom + smallGap;

                lblStatus.AutoSize = false;
                lblStatus.Width = colWidth + 100;
                lblStatus.Height = lblStatusHeight;
                lblStatus.Left = (this.ClientSize.Width - lblStatus.Width) / 2;
                lblStatus.TextAlign = ContentAlignment.MiddleCenter;
                lblStatus.Top = cmbOutputFormat.Bottom + bigGap;

                btnConvert.Width = colWidth;
                btnConvert.Height = btnConvertHeight;
                btnConvert.Left = startX;
                btnConvert.Top = lblStatus.Bottom + bigGap;
                btnConvert.Text = "START CONVERSION";
                btnConvert.Font = new Font("Segoe UI", 11, FontStyle.Bold);
                btnConvert.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, btnConvert.Width, btnConvert.Height, 10, 10));

                if (customProgressBg != null)
                {
                    customProgressBg.Width = colWidth;
                    customProgressBg.Height = progressHeight;
                    customProgressBg.Left = startX;
                    customProgressBg.Top = btnConvert.Bottom + bigGap;
                    customProgressBg.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, customProgressBg.Width, customProgressBg.Height, 4, 4));
                }

                if (lblBack != null)
                {
                    lblBack.Top = customProgressBg.Bottom + 35;
                    lblBack.Left = (this.ClientSize.Width - lblBack.Width) / 2;
                }
            }
        }

        private void ShowHome()
        {
            if (pnlConverter != null) pnlConverter.Visible = false;
            if (pnlHome != null) pnlHome.Visible = true;
        }

        private void OpenConverter(bool imageMode)
        {
            isImageMode = imageMode;
            cmbOutputFormat.Items.Clear();

            if (isImageMode)
            {
                btnSelectFiles.Text = "Select Images (Max 100)";
                cmbOutputFormat.Items.AddRange(new string[] { "Choose an image first..." });
            }
            else
            {
                btnSelectFiles.Text = "Select Files (Max 100)";
                cmbOutputFormat.Items.AddRange(new string[] { "Choose a file first..." });
            }

            cmbOutputFormat.SelectedIndex = 0;
            cmbOutputFormat.Enabled = false;
            btnConvert.Enabled = false;

            lblStatus.Text = "";
            lblStatus.Visible = false;

            selectedFiles = Array.Empty<string>();
            UpdateCustomProgress(0, 1);

            if (pnlHome != null) pnlHome.Visible = false;
            if (pnlConverter != null) pnlConverter.Visible = true;

            CenterElements();
        }

        private void SwitchToImageMode(string[] files)
        {
            MessageBox.Show("You selected image files.\nSwitching to Image Converter Module!", "Auto Module Switch", MessageBoxButtons.OK, MessageBoxIcon.Information);

            isImageMode = true;
            btnSelectFiles.Text = "Select Images (Max 100)";
            selectedFiles = files;

            lblStatus.Visible = true;
            if (selectedFiles.Length == 1)
                lblStatus.Text = "1 image selected and ready";
            else
                lblStatus.Text = $"{selectedFiles.Length} images selected and ready";

            cmbOutputFormat.Items.Clear();
            cmbOutputFormat.Enabled = true;
            btnConvert.Enabled = true;
            cmbOutputFormat.Items.AddRange(new string[] { "webp", "jpg", "png", "bmp", "gif", "tiff" });
            cmbOutputFormat.SelectedIndex = 0;
        }

        private void btnSelectFiles_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Multiselect = true;
                ofd.Filter = isImageMode ? "Images|*.jpg;*.jpeg;*.png;*.webp;*.bmp;*.gif;*.tiff" : "All files|*.*";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    if (ofd.FileNames.Length > 100)
                    {
                        MessageBox.Show("You can select a maximum of 100 files.", "Limit Exceeded");
                        return;
                    }

                    selectedFiles = ofd.FileNames;

                    string firstFileExt = Path.GetExtension(selectedFiles[0]).ToLower().Replace(".", "");

                    bool isImageExt = new[] { "jpg", "jpeg", "png", "webp", "bmp", "gif", "tiff" }.Contains(firstFileExt);

                    if (!isImageMode && isImageExt)
                    {
                        SwitchToImageMode(selectedFiles);
                        return;
                    }

                    lblStatus.Visible = true;
                    string fileType = isImageMode ? "image" : "file";
                    string pluralS = selectedFiles.Length == 1 ? "" : "s";

                    lblStatus.Text = $"{selectedFiles.Length} {fileType}{pluralS} ready for conversion";

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
                        {
                            cmbOutputFormat.Items.AddRange(new string[] { "mp4", "avi", "mkv", "mp3", "wav" });
                        }
                        else if (firstFileExt == "mp3" || firstFileExt == "wav" || firstFileExt == "flac" || firstFileExt == "aac")
                        {
                            cmbOutputFormat.Items.AddRange(new string[] { "mp3", "wav" });
                        }
                        else if (firstFileExt == "pdf")
                        {
                            cmbOutputFormat.Items.AddRange(new string[] { "docx", "txt" });
                        }
                        else if (firstFileExt == "docx" || firstFileExt == "doc" || firstFileExt == "txt")
                        {
                            cmbOutputFormat.Items.AddRange(new string[] { "pdf", "txt", "docx" });
                        }
                        else if (firstFileExt == "zip" || firstFileExt == "rar")
                        {
                            cmbOutputFormat.Items.AddRange(new string[] { "Extracting files is not supported yet in this UI" });
                            btnConvert.Enabled = false;
                        }
                        else
                        {
                            cmbOutputFormat.Items.Add("Unsupported format");
                            btnConvert.Enabled = false;
                        }
                    }

                    if (cmbOutputFormat.Items.Count > 0)
                        cmbOutputFormat.SelectedIndex = 0;
                }
            }
        }

        private async void btnConvert_Click(object sender, EventArgs e)
        {
            if (selectedFiles.Length == 0) return;

            string targetFormat = cmbOutputFormat.Text.Replace(".", "").Trim().ToLower();
            if (string.IsNullOrEmpty(targetFormat) || targetFormat.Contains("first")) return;

            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    btnConvert.Enabled = false;
                    int total = selectedFiles.Length;
                    UpdateCustomProgress(0, total);

                    int current = 0;
                    foreach (string file in selectedFiles)
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
                            else
                            {
                                switch (targetFormat)
                                {
                                    case "mp3":
                                    case "wav":
                                    case "mp4":
                                    case "avi":
                                    case "mkv":
                                        if (!isFFmpegReady) throw new Exception("FFmpeg Modules are not ready or still downloading.");
                                        var conversion = await FFmpeg.Conversions.FromSnippet.Convert(file, destPath);
                                        await conversion.Start();
                                        break;

                                    case "pdf":
                                        Document docToPdf = new Document();
                                        docToPdf.LoadFromFile(file);
                                        docToPdf.SaveToFile(destPath, FileFormat.PDF);
                                        docToPdf.Dispose();
                                        break;

                                    case "docx":
                                        Document docToWord = new Document();
                                        docToWord.LoadFromFile(file);
                                        docToWord.SaveToFile(destPath, FileFormat.Docx);
                                        docToWord.Dispose();
                                        break;

                                    case "txt":
                                        Document docToTxt = new Document();
                                        docToTxt.LoadFromFile(file);
                                        docToTxt.SaveToFile(destPath, FileFormat.Txt);
                                        docToTxt.Dispose();
                                        break;

                                    default:
                                        File.Copy(file, destPath, true);
                                        break;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Conversion error for {Path.GetFileName(file)}: {ex.Message}", "Error");
                        }

                        current++;
                        UpdateCustomProgress(current, total);
                    }
                    lblStatus.Text = "Conversion completed!";
                    btnConvert.Enabled = true;
                }
            }
        }

        private void UpdateCustomProgress(int current, int total)
        {
            if (total == 0) total = 1;
            int newWidth = (int)((double)current / total * customProgressBg.Width);
            if (newWidth < 1) newWidth = 1;

            customProgressFill.Width = newWidth;
            customProgressFill.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, newWidth, customProgressFill.Height, 4, 4));
        }
    }
}
