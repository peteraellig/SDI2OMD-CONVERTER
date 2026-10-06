using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
namespace SdiOmt;
public partial class MainForm : Form
{
    private Process? engine;
    private bool busy;
    private bool closing;
    private bool exitConfirmed;
    private bool settingsReady;
    private AppSettings settings = new();
    private readonly string enginePath = Path.Combine(AppContext.BaseDirectory, "sdi_omt.exe");
    private record Device(int Index, string Name) { public override string ToString() => Name; }
    public MainForm()
    {
        InitializeComponent();
        WindowTitle.Apply(this);
        try { settings = AppSettings.Load(); }
        catch (IOException ex) { AppendLog("Cannot read settings: " + ex.Message); }
        catch (UnauthorizedAccessException ex) { AppendLog("Cannot read settings: " + ex.Message); }
        nameBox.Text = settings.SenderName;
        modeBox.SelectedItem = modeBox.Items.Contains(settings.VideoMode) ? settings.VideoMode : "720p50";
        qualityBox.SelectedItem = qualityBox.Items.Contains(settings.Quality) ? settings.Quality : "Normal";
        audioBox.SelectedItem = audioBox.Items.Contains(settings.AudioSelection) ? settings.AudioSelection : "Stereo 1&2";
        UpdateFormatSummary();
        modeBox.SelectedIndexChanged += (_, _) => { UpdateFormatSummary(); SaveSettings(); };
        audioBox.SelectedIndexChanged += (_, _) => SaveSettings();
        qualityBox.SelectedIndexChanged += (_, _) => { UpdateFormatSummary(); SaveSettings(); };
        Size = new Size(Math.Clamp(settings.WindowWidth, MinimumSize.Width, 2000), Math.Clamp(settings.WindowHeight, MinimumSize.Height, 1400));
        if (settings.Maximized) WindowState = FormWindowState.Maximized;
        nameBox.Leave += (_, _) => SaveSettings();
        deviceBox.SelectedIndexChanged += (_, _) => { if (!busy) SaveSettings(); };
    }
    private void UpdateFormatSummary() => headerInfo.Text = $"{modeBox.SelectedItem} · {qualityBox.SelectedItem}";
    private void SaveSettings()
    {
        if (!settingsReady) return;
        settings.SenderName = nameBox.Text;
        settings.VideoMode = modeBox.SelectedItem?.ToString() ?? "720p50";
        settings.Quality = qualityBox.SelectedItem?.ToString() ?? "Normal";
        settings.AudioSelection = audioBox.SelectedItem?.ToString() ?? "Stereo 1&2";
        if (deviceBox.SelectedItem is Device device) settings.DeviceName = device.Name;
        var bounds = WindowState == FormWindowState.Normal ? Bounds : RestoreBounds;
        settings.WindowWidth = bounds.Width; settings.WindowHeight = bounds.Height;
        settings.Maximized = WindowState == FormWindowState.Maximized;
        try { settings.Save(); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { AppendLog("Cannot save settings: " + ex.Message); }
    }
    private void AppendLog(string line)
    {
        if (IsDisposed || Disposing) return;
        if (InvokeRequired) { BeginInvoke(new Action(() => AppendLog(line))); return; }
        if (logBox.TextLength > 60000) logBox.Text = logBox.Text[^30000..];
        logBox.AppendText(line + Environment.NewLine);
        if (line.StartsWith("Captured=")) {
            var signal = Regex.Match(line, @"\bsignal=(\d+) connections=");
            var connections = Regex.Match(line, @"connections=(\d+)");
            if (signal.Success && connections.Success)
                statusLabel.Text = signal.Groups[1].Value == "0" ? "No SDI signal — check input and selected format" :
                    connections.Groups[1].Value == "0" ? "SDI signal present — waiting for OMT receiver" :
                    "SDI signal present · OMT active · " + connections.Groups[1].Value + " network connections";
        }
        if (line.StartsWith("Error:")) { logBox.Visible = true; logButton.Text = "Hide list"; }
    }
    private Process NewEngine(params string[] args)
    {
        if (!File.Exists(enginePath)) throw new FileNotFoundException("Capture executable is missing from the application folder.", enginePath);
        var start = new ProcessStartInfo(enginePath) {
            UseShellExecute = false, CreateNoWindow = true,
            RedirectStandardOutput = true, RedirectStandardError = true, RedirectStandardInput = true,
            StandardOutputEncoding = Encoding.UTF8, StandardErrorEncoding = Encoding.UTF8,
            StandardInputEncoding = Encoding.UTF8,
            WorkingDirectory = AppContext.BaseDirectory
        };
        foreach (var arg in args) start.ArgumentList.Add(arg);
        return new Process { StartInfo = start };
    }
    private void SetBusy(bool value, bool canStop = false)
    {
        busy = value;
        startButton.Enabled = canStop || (!value && deviceBox.SelectedItem is Device);
        startButton.Text = canStop ? "Running — press to stop" : value ? "Please wait …" : "Start transmission";
        startButton.BackColor = canStop ? Color.FromArgb(190, 65, 55) : Color.FromArgb(35, 125, 75);
        startButton.ForeColor = Color.White;
        testButton.Enabled = refreshButton.Enabled = deviceBox.Enabled = nameBox.Enabled = modeBox.Enabled = qualityBox.Enabled = audioBox.Enabled = !value;
    }
    private async Task RefreshDevicesAsync()
    {
        SetBusy(true); statusLabel.Text = "Searching for devices …";
        try {
            using var process = NewEngine("--list"); process.Start();
            var output = process.StandardOutput.ReadToEndAsync();
            var error = process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();
            var text = await output; var errors = await error;
            if (process.ExitCode != 0) throw new InvalidOperationException(errors);
            deviceBox.Items.Clear();
            foreach (var line in text.Split('\n')) {
                var match = Regex.Match(line.Trim(), @"^(\d+): (.+) \(capture interface: yes\)$");
                if (match.Success) deviceBox.Items.Add(new Device(int.Parse(match.Groups[1].Value), match.Groups[2].Value));
            }
                        if (deviceBox.Items.Count > 0) {
                var selected = deviceBox.Items.Cast<Device>().FirstOrDefault(d => d.Name == settings.DeviceName);
                deviceBox.SelectedItem = selected ?? (settings.DeviceName is null ? deviceBox.Items[0] : null);
                if (selected is null && settings.DeviceName is not null) AppendLog("Saved input is missing. Please select an input: " + settings.DeviceName);
            }
            settingsReady = true;
            AppendLog(text.Trim()); statusLabel.Text = $"Ready · {deviceBox.Items.Count} inputs found";
        } catch (Exception ex) { AppendLog("Error: " + ex.Message); statusLabel.Text = "Device detection failed"; }
        finally { SetBusy(false); if (exitConfirmed && !closing) Close(); }
    }
    private async Task RunEngineAsync(bool selftest)
    {
        if (busy) return;
        if (!selftest && (deviceBox.SelectedItem is not Device || string.IsNullOrWhiteSpace(nameBox.Text))) {
            MessageBox.Show(this, "Please select an input and enter an OMT sender name.", "SDI2OMD CONVERTER"); return;
        }
        SaveSettings(); SetBusy(true); logBox.Clear(); statusLabel.Text = selftest ? "OMT self-test is running …" : "Starting transmission …";
        try {
            var device = deviceBox.SelectedItem as Device;
            using var process = selftest ? NewEngine("--selftest", modeBox.SelectedItem!.ToString()!, qualityBox.SelectedItem!.ToString()!, "250", audioBox.SelectedItem!.ToString()!) : NewEngine("--capture", device!.Index.ToString(), nameBox.Text.Trim(), "0", modeBox.SelectedItem!.ToString()!, qualityBox.SelectedItem!.ToString()!, audioBox.SelectedItem!.ToString()!);
            engine = process;
            process.OutputDataReceived += (_, e) => { if (e.Data is not null) AppendLog(e.Data); };
            process.ErrorDataReceived += (_, e) => { if (e.Data is not null) AppendLog("Error: " + e.Data); };
            process.Start(); process.BeginOutputReadLine(); process.BeginErrorReadLine();
            SetBusy(true, !selftest);
            await process.WaitForExitAsync();
            statusLabel.Text = process.ExitCode == 0 ? (selftest ? "OMT self-test passed" : "Transmission stopped") : "Error — see log";
        } catch (Exception ex) { AppendLog("Error: " + ex.Message); statusLabel.Text = "Failed to start"; }
        finally { engine = null; SetBusy(false); }
    }
    private async Task StopEngineAsync()
    {
        var process = engine;
        if (process is null) return;
        startButton.Enabled = false; startButton.Text = "Stopping …"; statusLabel.Text = "Stopping transmission …";
        try {
            if (process.HasExited) return;
            await process.StandardInput.WriteLineAsync("stop"); await process.StandardInput.FlushAsync();
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            try { await process.WaitForExitAsync(timeout.Token); }
            catch (OperationCanceledException) { if (!process.HasExited) { process.Kill(entireProcessTree: true); await process.WaitForExitAsync(); } }
        } catch (InvalidOperationException) { }
        catch (IOException) { }

    }
    private async void MainForm_Shown(object? sender, EventArgs e)
    {
        await RefreshDevicesAsync();
        if (!closing && Environment.GetCommandLineArgs().Contains("--start")) await RunEngineAsync(false);
    }
    private async void RefreshButton_Click(object? sender, EventArgs e) => await RefreshDevicesAsync();
    private async void StartButton_Click(object? sender, EventArgs e)
    {
        if (busy) await StopEngineAsync();
        else await RunEngineAsync(false);
    }
    private async void TestButton_Click(object? sender, EventArgs e) => await RunEngineAsync(true);
    private void ExitButton_Click(object? sender, EventArgs e) => Close();
    private void LogButton_Click(object? sender, EventArgs e)
    {
        logBox.Visible = !logBox.Visible;
        logButton.Text = logBox.Visible ? "Hide list" : "Show list";
    }
    private void HelpButton_Click(object? sender, EventArgs e)
    {
        MessageBox.Show(this,
            "SUPPORTED INPUT\n" +
            "SDI video: 720p50, 720p60, 1080p50, 1080p60, 1080i50 or 1080i60, selected manually.\n" +
            "Video capture: 8-bit YUV 4:2:2. Audio: eight embedded SDI input channels, 48 kHz.\nOutput is always two-channel stereo: select Stereo 1&2, 3&4, 5&6 or 7&8.\nMono 1–8 duplicates the selected channel to both left and right (no mixing).\n\n" +
            "FORMAT LIMITATIONS\n" +
            "The input must match the selected format. The application does not auto-sense the SDI format.\n" +
            "1080i50 = 50 fields / 25 frames per second; 1080i60 = 60 fields / 30 frames per second. Interlacing is preserved.\n59.94 fps/fields and other formats are not supported. 60 means exactly 60.\n" +
            "No format conversion, 10-bit/HDR or eight-channel OMT output.\nOMT quality: High, Normal (OMT Medium) or Low. Higher quality uses more bandwidth.\n\n" +
            "SETUP\n" +
            "Choose an input, format, quality, audio channels and sender name, then press Start transmission.\n" +
            "Press Running — press to stop to stop. The input must be free.\n" +
            "Physical SDI port mapping is configured in Blackmagic Desktop Video Setup.\n" +
            "Sender name, input, format, quality, audio selection and window settings are saved automatically.\n\n" +
            "NETWORK\n" +
            "OMT video/audio uses TCP ports 6400–6600; discovery uses UDP 5353.\n" +
            "Allow inbound connections on the sender PC for your intended receivers.\n" +
            "A new sender can use a different TCP port if another OMT source is already running.\n\n" +
            "SELF-TEST\n" +
            "OMT self-test checks local synthetic video/audio at the selected format, quality and audio routing. Each output sample is checked.\n" +
            "It does not capture SDI or test reception on another PC.\n\nDIAGNOSTICS\nShow list reveals the detailed log. Counters are totals since capture started.\nNo signal counts missing-input frames, including startup. It does not mean current signal loss.\nNo delivery counts send calls with zero bytes delivered, including no matching receiver or silent audio. It is not an error count.\nNetwork connections are TCP connections, not the number of viewers.\nCurrent signal state is shown above the list; errors reveal the list automatically.",
            "SDI2OMD CONVERTER — Help", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
    private async void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
    {
        if (closing) return;
        if (!exitConfirmed) {
            if (MessageBox.Show(this, "Are you sure you want to exit?", "SDI2OMD CONVERTER",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes) {
                e.Cancel = true; return;
            }
            exitConfirmed = true;
        }
        SaveSettings();
        if (busy) {
            e.Cancel = true;
            if (engine is null) return;
            closing = true;
            await StopEngineAsync(); Close();
        }
    }
}




