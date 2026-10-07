using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Text.Json;
namespace SdiOmt;
public partial class MainForm : Form
{
    private Process? engine;
    // Runtime diagnostics buffer is owned here, so the form designer cannot remove it.
    private readonly TextBox logBox = new() {
        Name="logBox",Multiline=true,ReadOnly=true,Visible=false,Dock=DockStyle.Fill,
        Font=new Font("Consolas",10F),ScrollBars=ScrollBars.Vertical,BackColor=SystemColors.Window
    };
    private bool busy;
    private bool closing;
    private bool exitConfirmed;
    private bool settingsReady;
    private AppSettings settings = new();
    private readonly string enginePath = Path.Combine(AppContext.BaseDirectory, "sdi_omt.exe");
    private CardInfoForm? cardInfo;
    private DiagnosticsForm? diagnosticsForm;
    private bool DiagnosticsVisible => diagnosticsForm is {IsDisposed:false,Visible:true};
    private readonly LiveHealth health=new();
    private readonly SystemLoad systemLoad=new();
    private readonly System.Windows.Forms.Timer healthTimer=new() {Interval=1000};
    private bool capturing;
    private bool stopping;
    private record Device(int Index, string Name) { public override string ToString() => Name; }
    public MainForm()
    {
        InitializeComponent();
        if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime) return;
        healthTimer.Tick+=(_,_)=>{ if(!DesignMode) UpdateHealthDisplay(true); };
        healthTimer.Start();
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
        // Fixed client size follows WinForms DPI/font scaling; ignore old saved window bounds.
        nameBox.Leave += (_, _) => SaveSettings();
        deviceBox.SelectedIndexChanged += (_, _) => { if (!busy) SaveSettings(); };
    }
    private void UpdateHealthDisplay(bool readResources=false)
    {
        if(readResources){
            var resources=systemLoad.Read(capturing ? engine : null);
            SetCell(cpuCell,resources.Cpu);SetCell(ramCell,resources.Ram);SetCell(freeRamCell,resources.FreeRam);
            SetCell(encoderCpuCell,resources.EncoderCpu);SetCell(encoderRamCell,resources.EncoderRam);
        }
        var now=Environment.TickCount64;var recent=health.Window(now);
        SetCell(videoLossCell,recent.Video.ToString(),recent.Video>0 ? Color.DarkOrange : Color.FromArgb(35,125,75));
        SetCell(audioLossCell,$"{recent.AudioMs:0.0} ms",recent.AudioMs>0 ? Color.Firebrick : Color.FromArgb(35,125,75));
        SetCell(errorsCell,recent.Errors.ToString(),recent.Errors>0 ? Color.Firebrick : Color.FromArgb(35,125,75));
        var sample=capturing ? health.Latest : null;
        SetCell(connectionsCell,sample?.Connections.ToString()??"—");
        SetCell(videoQueueCell,sample is null ? "—" : $"{sample.VideoQueueMs} ms");
        SetCell(audioQueueCell,sample is null ? "—" : $"{sample.AudioQueueMs} ms");
        SetCell(sendCell,sample is null ? "—" : $"{sample.EncodeMs} ms");
        if(!capturing || stopping)return;
        if(sample is null){statusLabel.Text="Starting transmission …";return;}
        if(now-health.LastReceivedMs>2500){statusLabel.Text="Sender telemetry delayed — check CPU load";statusLabel.ForeColor=Color.Salmon;}
        else if(!sample.Signal||sample.VideoAgeMs<0||sample.VideoAgeMs>500){statusLabel.Text="No fresh SDI video — check input and selected format";statusLabel.ForeColor=Color.Salmon;}
        else if(sample.SendBusyMs>500||sample.SendAgeMs>500){statusLabel.Text="Encoder / send stalled — audio and video may be interrupted";statusLabel.ForeColor=Color.Salmon;}
        else if(sample.Connections==0){statusLabel.Text="SDI present · waiting for OMT receiver";statusLabel.ForeColor=Color.Khaki;}
        else if(recent.Errors>0||recent.AudioMs>0){statusLabel.Text="Recent capture / audio problem — see live counters";statusLabel.ForeColor=Color.Salmon;}
        else if(recent.Video>0){statusLabel.Text="Recent video loss · freshest picture retained";statusLabel.ForeColor=Color.Khaki;}
        else {statusLabel.Text="SDI present · OMT active";statusLabel.ForeColor=Color.PaleGreen;}
    }
    private void StatusStrip_SizeChanged(object? sender, EventArgs e)
    {
        if(sender is not StatusStrip strip || strip.Items.Count==0)return;
        int width=strip.ClientSize.Width/strip.Items.Count;
        for(int i=0;i<strip.Items.Count;i++)
            strip.Items[i].Width=i==strip.Items.Count-1 ? strip.ClientSize.Width-width*i : width;
    }
    private static void SetCell(ToolStripStatusLabel cell,string value,Color? color=null)
    {
        cell.Text=cell.Tag+"\n"+value;
        cell.ForeColor=color??Color.FromArgb(33,37,41);
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
        if(line.StartsWith("Health=")){
            try {
                var sample=JsonSerializer.Deserialize<HealthSample>(line[7..],new JsonSerializerOptions {PropertyNameCaseInsensitive=true});
                if(sample is not null){health.Add(sample,Environment.TickCount64);UpdateHealthDisplay();}
            }catch(JsonException){statusLabel.Text="Health sample could not be read";statusLabel.ForeColor=Color.Salmon;}
            return;
        }
        if (busy && !DiagnosticsVisible && line.StartsWith("Captured=")) return;
        if (logBox.TextLength > 60000) logBox.Text = logBox.Text[^30000..];
        logBox.AppendText(line + Environment.NewLine);
        if (line.StartsWith("Error:") && !closing && !exitConfirmed) OpenDiagnostics();
    }
    private Process NewEngine(params string[] args)
    {
        if (!File.Exists(enginePath)) throw new FileNotFoundException("Capture executable is missing from the application folder.", enginePath);
        var start = new ProcessStartInfo(enginePath) {
            UseShellExecute = false, CreateNoWindow = true,
            RedirectStandardOutput = true, RedirectStandardError = true, RedirectStandardInput = true,
            StandardOutputEncoding = Encoding.UTF8, StandardErrorEncoding = Encoding.UTF8,
            StandardInputEncoding = new UTF8Encoding(false),
            WorkingDirectory = AppContext.BaseDirectory
        };
        foreach (var arg in args) start.ArgumentList.Add(arg);
        return new Process { StartInfo = start };
    }
    private void SetBusy(bool value, bool canStop = false)
    {
        busy = value;
        statusLabel.Visible = true;
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
            AppendLog(text.Trim()); statusLabel.ForeColor=Color.Gainsboro; statusLabel.Text = $"Ready · {deviceBox.Items.Count} inputs found";
        } catch (Exception ex) { AppendLog("Error: " + ex.Message); statusLabel.Text = "Device detection failed"; }
        finally { SetBusy(false); if (exitConfirmed && !closing) Close(); }
    }
    private async Task RunEngineAsync(bool selftest)
    {
        if (busy) return;
        if (!selftest && (deviceBox.SelectedItem is not Device || string.IsNullOrWhiteSpace(nameBox.Text))) {
            MessageBox.Show(this, "Please select an input and enter an OMT sender name.", "SDI2OMD CONVERTER"); return;
        }
        SaveSettings(); SetBusy(true); logBox.Clear(); statusLabel.ForeColor=Color.Gainsboro; statusLabel.Text = selftest ? "OMT self-test is running …" : "Starting transmission …";
        health.Reset();capturing=!selftest;stopping=false;
        try {
            var device = deviceBox.SelectedItem as Device;
            using var process = selftest ? NewEngine("--selftest", modeBox.SelectedItem!.ToString()!, qualityBox.SelectedItem!.ToString()!, "250", audioBox.SelectedItem!.ToString()!) : NewEngine("--capture", device!.Index.ToString(), nameBox.Text.Trim(), "0", modeBox.SelectedItem!.ToString()!, qualityBox.SelectedItem!.ToString()!, audioBox.SelectedItem!.ToString()!, DiagnosticsVisible ? "--diagnostics" : "--quiet");
            engine = process;
            process.OutputDataReceived += (_, e) => { if (e.Data is not null) AppendLog(e.Data); };
            process.ErrorDataReceived += (_, e) => { if (e.Data is not null) AppendLog("Error: " + e.Data); };
            process.Start(); process.BeginOutputReadLine(); process.BeginErrorReadLine();
            SetBusy(true, !selftest);
            await process.WaitForExitAsync();
            statusLabel.ForeColor=process.ExitCode==0 ? Color.Gainsboro : Color.Salmon;
            statusLabel.Text = process.ExitCode == 0 ? (selftest ? "OMT self-test passed" : "Transmission stopped") : "Error — see log";
        } catch (Exception ex) { AppendLog("Error: " + ex.Message); statusLabel.Text = "Failed to start"; }
        finally { capturing=false;stopping=false;engine = null; SetBusy(false); UpdateHealthDisplay(true); }
    }
    private async Task StopEngineAsync()
    {
        var process = engine;
        if (process is null) return;
        stopping=true; startButton.Enabled = false; startButton.Text = "Stopping …"; statusLabel.Text = "Stopping transmission …";
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
    private void CardInfoButton_Click(object? sender, EventArgs e)
    {
        if (cardInfo is null || cardInfo.IsDisposed) { cardInfo = new CardInfoForm(enginePath); cardInfo.Show(this); }
        else cardInfo.Activate();
    }
    private void ExitButton_Click(object? sender, EventArgs e) => Close();
    private async Task SetEngineDiagnosticsAsync()
    {
        statusLabel.Visible = true;
        var process = engine;
        if (process is null) return;
        try
        {
            if (process.HasExited) return;
            await process.StandardInput.WriteLineAsync(DiagnosticsVisible ? "diagnostics on" : "diagnostics off");
            await process.StandardInput.FlushAsync();
        }
        catch (Exception ex) when (ex is InvalidOperationException or IOException or ObjectDisposedException) { }
    }
    private void OpenDiagnostics()
    {
        if(DiagnosticsVisible){diagnosticsForm!.Activate();return;}
        diagnosticsForm=new DiagnosticsForm(logBox);
        diagnosticsForm.FormClosed+=(_,_)=>{diagnosticsForm=null;_=SetEngineDiagnosticsAsync();};
        diagnosticsForm.Show(this);
        if(health.Latest is { } totals){
            AppendLog($"Session totals: video lost {totals.VideoLost}, audio discarded {totals.AudioLostSamples/48.0:0.0} ms, errors {totals.Errors}.");
            AppendLog($"Startup only: video lost {health.StartupVideoLost}, audio discarded {health.StartupAudioLostSamples/48.0:0.0} ms, errors {health.StartupErrors}. These do not latch the live warning.");
        }
        _=SetEngineDiagnosticsAsync();
    }
    private void LogButton_Click(object? sender, EventArgs e) => OpenDiagnostics();
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
            "BM card info shows signal detection, detected format and capture/output use only while its window is open.\nNumbers follow DeckLink device order; physical connector mapping is set in Desktop Video Setup.\nUnavailable means the driver did not supply that status; Not detected is the driver's current input-lock status.\n\n" +
            "Choose an input, format, quality, audio channels and sender name, then press Start transmission.\n" +
            "Press Running — press to stop to stop. The input must be free.\n" +
            "Physical SDI port mapping is configured in Blackmagic Desktop Video Setup.\n" +
            "Sender name, input, format, quality, audio selection are saved automatically. The main window has a fixed size.\n\n" +
            "NETWORK\n" +
            "OMT video/audio uses TCP ports 6400–6600; discovery uses UDP 5353.\n" +
            "Allow inbound connections on the sender PC for your intended receivers.\n" +
            "A new sender can use a different TCP port if another OMT source is already running.\n\n" +
            "SELF-TEST\n" +
            "OMT self-test checks local synthetic video/audio at the selected format, quality and audio routing. Each output sample is checked.\n" +
            "It does not capture SDI or test reception on another PC.\n\nDIAGNOSTICS\nLive health and CPU/RAM refresh once per second. The two bottom status strips show system and stream values in aligned cells. Detailed logging is off while the list window is closed.\nLost frames, discarded audio duration and errors show only the last five seconds; startup losses are recorded separately.\nShow list opens a separate window including session and startup totals. Close stops detailed logging. Missing signal and stalled sending are current-state warnings.\nVideo keeps only one waiting frame, discarding older pictures. Audio has a separate 120 ms buffer.\nAudio runs through short stalls where possible; long stalls discard the oldest audio to recover latency.\nA queued picture older than two frame periods is discarded. A running encoder call cannot be interrupted.\nLoss counters cover local capture/queue losses, not receiver or network losses.\nNo signal is the cumulative count of missing-input callbacks, not current signal loss.\nNo delivery counts zero-byte sends, including no matching receiver or silent audio.\nNetwork connections are TCP channels, not viewer counts. Errors reveal the list automatically.\nBM card info polls only while its window is open; Close stops all card-status polling.",
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
        cardInfo?.Close();
        diagnosticsForm?.Close();
        if (busy) {
            e.Cancel = true;
            if (engine is null) return;
            closing = true;
            await StopEngineAsync(); Close();
        }
    }
}




