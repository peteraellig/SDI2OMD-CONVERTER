using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using SdiOmt;

// Hardware integration test: preserves one receiver across GUI-driven sender restarts.
internal static class ReconnectCheck
{
    [DllImport("libomt.dll", CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr omt_receive_create([MarshalAs(UnmanagedType.LPUTF8Str)] string address, int types, int format, int flags);
    [DllImport("libomt.dll", CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr omt_receive(IntPtr receiver, int types, int timeout);
    [DllImport("libomt.dll", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void omt_receive_destroy(IntPtr receiver);
    [DllImport("libomt.dll", CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr omt_discovery_getaddresses(out int count);
    [DllImport("libomt.dll", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void omt_shutdown();
    [StructLayout(LayoutKind.Sequential)]
    internal struct Frame
    {
        public int Type; public long Timestamp;
        public int Codec, Width, Height, Stride, Flags, RateN, RateD;
        public float Aspect; public int ColorSpace, SampleRate, Channels, Samples;
        public IntPtr Data; public int DataLength; public IntPtr CompressedData; public int CompressedLength;
        public IntPtr Metadata; public int MetadataLength;
    }

    [STAThread]
    private static void Main(string[] args)
    {
        var resultPath = args.Length > 0 ? Path.GetFullPath(args[0]) : Path.Combine(AppContext.BaseDirectory, "reconnect-results.txt");
        if (args.Contains("--synthetic")) { SyntheticReconnect.Run(resultPath).GetAwaiter().GetResult(); return; }
        bool audioOnly = args.Contains("--audio-only");
        var originalSettings = File.Exists(AppSettings.FilePath) ? File.ReadAllBytes(AppSettings.FilePath) : null;
        var report = new List<string>();
        ApplicationConfiguration.Initialize();
        using var form = new MainForm();
        form.Shown += async (_, _) =>
        {
            IntPtr receiver = IntPtr.Zero;
            var toggle = (Button)form.Controls.Find("startButton", true)[0];
            var test = (Button)form.Controls.Find("testButton", true)[0];
            var log = (TextBox)typeof(MainForm).GetField("logBox",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance)!.GetValue(form)!;
            var quality = (ComboBox)form.Controls.Find("qualityBox", true)[0];
            var mode = (ComboBox)form.Controls.Find("modeBox", true)[0];
            var device = (ComboBox)form.Controls.Find("deviceBox", true)[0];
            var status = (Label)form.Controls.Find("statusLabel", true)[0];
            try
            {
                await Until(() => test.Enabled, 10000, "Device enumeration");
                if (mode.Items.Count != 6) throw new Exception("Expected six video modes");
                if(args.Contains("--1080i50")) mode.SelectedItem="1080i50";
                // Use the user's input and source name. The SDI source must match the saved mode.
                if (device.SelectedIndex < 0) throw new Exception("No selected DeckLink input");
                if(args.Contains("--ui"))
                {
                    await Task.Delay(2200);
                    if(form.FormBorderStyle!=FormBorderStyle.FixedSingle || form.MaximizeBox)throw new Exception("Main window is not fixed");
                    var originalSize=form.Size;
                    var strips=new[]{(StatusStrip)form.Controls.Find("systemStrip",true)[0],(StatusStrip)form.Controls.Find("streamStrip",true)[0]};
                    foreach(var strip in strips){
                        if(strip.Items.Count!=6 || strip.Items.OfType<ToolStripStatusLabel>().Any(cell=>cell.IsOnOverflow))throw new Exception("Status cells missing or overflowing");
                        if(strip.Bottom>form.ClientSize.Height)throw new Exception("Status strip clipped");
                    }
                    ((Button)form.Controls.Find("logButton",true)[0]).PerformClick();await Task.Delay(100);
                    var diagnostics=form.OwnedForms.OfType<DiagnosticsForm>().Single();
                    if(diagnostics.Controls.Find("logBox",true).Length!=1 || form.Size!=originalSize)throw new Exception("Diagnostics window/layout");
                    using(var bitmap=new Bitmap(diagnostics.Width,diagnostics.Height)){diagnostics.DrawToBitmap(bitmap,new Rectangle(Point.Empty,diagnostics.Size));bitmap.Save(Path.Combine(Path.GetDirectoryName(resultPath)!,"diagnostics-window.png"));}
                    diagnostics.Close();await Task.Delay(100);
                    ((Button)form.Controls.Find("logButton",true)[0]).PerformClick();await Task.Delay(100);
                    form.OwnedForms.OfType<DiagnosticsForm>().Single().Close();
                    if(log.IsDisposed || form.Size!=originalSize)throw new Exception("Reopening diagnostics corrupted buffer/size");
                    using(var bitmap=new Bitmap(form.Width,form.Height)){form.DrawToBitmap(bitmap,new Rectangle(Point.Empty,form.Size));bitmap.Save(Path.Combine(Path.GetDirectoryName(resultPath)!,"fixed-interface.png"));}
                    report.Add("PASS: fixed main window, two aligned status strips, separate diagnostics open/close/reopen without resizing");return;
                }
                string? sourceAddress = null;
                foreach (var setting in new[] { "Normal", "High", "Low", "High", "Low", "Normal" })
                {
                    quality.SelectedItem = setting;
                    toggle.PerformClick();
                    await Until(() => Regex.IsMatch(log.Text, @"Sending .+ as (.+)\. Ctrl") || test.Enabled, 10000, "Sender startup");
                    var match = Regex.Match(log.Text, @"Sending .+ as (.+)\. Ctrl");
                    if (!match.Success) throw new Exception("Capture failed: " + log.Text);
                    var address = match.Groups[1].Value.Trim();
                    if (sourceAddress == null)
                    {
                        sourceAddress = address;
                        receiver = omt_receive_create(address, 2 | 4, 0, 2);
                        if (receiver == IntPtr.Zero) throw new Exception("Receiver creation failed");
                        report.Add("One persistent receiver, source: " + address);
                    }
                    var watch = Stopwatch.StartNew();
                    int videos = 0, audios = 0; long bytes = 0; long firstVideoMs = -1, firstAudioMs = -1;
                    var phaseLimit = TimeSpan.FromSeconds(15);
                    while (watch.Elapsed < phaseLimit && ((!audioOnly && videos < 30) || audios < 30))
                    {
                        var pointer = omt_receive(receiver, 2, 0);
                        if (pointer != IntPtr.Zero)
                        {
                            var frame = Marshal.PtrToStructure<Frame>(pointer);
                            bool interlaced = mode.Text.StartsWith("1080i");
                            int fps = (mode.Text.EndsWith("60") ? 60 : 50) / (interlaced ? 2 : 1);
                            if (frame.Width != (mode.Text.StartsWith("1080") ? 1920 : 1280) ||
                                frame.Height != (mode.Text.StartsWith("1080") ? 1080 : 720) ||
                                frame.RateD <= 0 || frame.RateN / (double)frame.RateD != fps ||
                                (frame.Flags & 1) != (interlaced ? 1 : 0) || frame.Data == IntPtr.Zero)
                                throw new Exception("Incorrect received video metadata");
                            if (firstVideoMs < 0) firstVideoMs = watch.ElapsedMilliseconds;
                            ++videos; bytes += frame.CompressedLength;
                        }
                        pointer = omt_receive(receiver, 4, 0);
                        if (pointer != IntPtr.Zero)
                        {
                            var frame = Marshal.PtrToStructure<Frame>(pointer);
                            if (frame.Channels != 2 || frame.SampleRate != 48000 || frame.Samples <= 0 || frame.Data == IntPtr.Zero)
                                throw new Exception("Incorrect received audio metadata");
                            if (firstAudioMs < 0) firstAudioMs = watch.ElapsedMilliseconds;
                            ++audios;
                        }
                        await Task.Delay(5);
                    }
                    if ((!audioOnly && videos < 30) || audios < 30) throw new Exception($"{setting}: receiver failed to reconnect; video={videos}, audio={audios}; {status.Text}\n{log.Text}");
                    if (!log.Text.Contains(" / " + setting + " / ")) throw new Exception("Sender quality argument mismatch");
                    if (args.Contains("--health"))
                    {
                        await Task.Delay(3200);
                        var system=(StatusStrip)form.Controls.Find("systemStrip",true)[0];
                        var stream=(StatusStrip)form.Controls.Find("streamStrip",true)[0];
                        if((system.Items[0].Text??"").EndsWith("—") || (system.Items[3].Text??"").EndsWith("—"))throw new Exception("Missing CPU metrics");
                        if((stream.Items[3].Text??"").EndsWith("—") || (stream.Items[4].Text??"").EndsWith("—"))throw new Exception("Missing queue metrics");
                        if(form.OwnedForms.OfType<DiagnosticsForm>().Any() || log.Text.Contains("Health=") || log.Text.Contains("Captured="))throw new Exception("Closed diagnostics produced detailed logs");
                        report.Add("HEALTH: "+status.Text+"; "+string.Join("; ",system.Items.Cast<ToolStripItem>().Concat(stream.Items.Cast<ToolStripItem>()).Select(cell=>(cell.Text??"").Replace('\n',' '))));
                        if(setting=="Normal"){
                            var originalSize=form.Size;
                            ((Button)form.Controls.Find("logButton",true)[0]).PerformClick();await Task.Delay(1500);
                            var diagnostics=form.OwnedForms.OfType<DiagnosticsForm>().Single();
                            if(!log.Text.Contains("Captured="))throw new Exception("Opening diagnostics did not enable logging");
                            diagnostics.Close();await Task.Delay(500);
                            int afterClose=Regex.Matches(log.Text,"Captured=").Count;await Task.Delay(1500);
                            if(Regex.Matches(log.Text,"Captured=").Count!=afterClose || form.Size!=originalSize)throw new Exception("Closing diagnostics did not stop logging / changed size");
                            if(!toggle.Enabled || !toggle.Text.StartsWith("Running"))throw new Exception("Closing diagnostics stopped capture");
                            report.Add("PASS: diagnostics open/close during capture; detailed logging stops, main size and transmission retained");
                        }
                    }
                    bool discovered = false;
                    var addresses = omt_discovery_getaddresses(out int count);
                    for (int i = 0; i < count; i++)
                        if (Marshal.PtrToStringUTF8(Marshal.ReadIntPtr(addresses, i * IntPtr.Size)) == address) discovered = true;
                    report.Add($"PASS {setting}: video={videos}, audio={audios}; first video={firstVideoMs} ms, audio={firstAudioMs} ms; compressed mean={bytes / Math.Max(1, videos)} bytes; discovery={discovered}; address={address}");
                    if (!discovered) throw new Exception("Source missing from discovery");
                    toggle.PerformClick();
                    await Until(() => test.Enabled, 10000, "Sender stop");
                    // Drain buffered packets before proving the next start supplies new media.
                    var quiet = Stopwatch.StartNew(); var drainLimit = Stopwatch.StartNew();
                    while (quiet.ElapsedMilliseconds < 1000 && drainLimit.ElapsedMilliseconds < 5000)
                    {
                        if (omt_receive(receiver, 2 | 4, 0) != IntPtr.Zero) quiet.Restart();
                        await Task.Delay(10);
                    }
                    if (quiet.ElapsedMilliseconds < 1000) throw new Exception("Receiver did not become quiet after stop");
                }
                report.Add(audioOnly ? "PASS AUDIO ONLY: six GUI starts, five reconnects, same receiver retained. No physical video validated." : "PASS: six starts, five reconnects, same receiver retained, source discovered each time.");
            }
            catch (Exception ex) { report.Add("FAIL: " + ex); Environment.ExitCode = 1; }
            finally
            {
                if (!test.Enabled && toggle.Enabled)
                {
                    toggle.PerformClick();
                    try { await Until(() => test.Enabled, 10000, "Final stop"); } catch { }
                }
                if (receiver != IntPtr.Zero) omt_receive_destroy(receiver);
                omt_shutdown();
                Directory.CreateDirectory(Path.GetDirectoryName(resultPath)!);
                File.WriteAllLines(resultPath, report);
                typeof(MainForm).GetField("exitConfirmed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.SetValue(form, true);
                form.Close();
            }
        };
        Application.Run(form);
        if (originalSettings != null) File.WriteAllBytes(AppSettings.FilePath, originalSettings);
    }
    private static async Task Until(Func<bool> condition, int timeout, string operation)
    {
        var watch = Stopwatch.StartNew();
        while (!condition())
        {
            if (watch.ElapsedMilliseconds >= timeout) throw new TimeoutException(operation);
            await Task.Delay(50);
        }
    }
}
