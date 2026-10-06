using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

internal static class SyntheticReconnect
{
    internal static async Task Run(string resultPath)
    {
        IntPtr receiver = IntPtr.Zero;
        Process? sender = null;
        var report = new List<string>();
        Directory.CreateDirectory(Path.GetDirectoryName(resultPath)!);
        try
        {
            foreach (var mode in new[] { "720p50", "1080i50", "1080i60" })
            foreach (var quality in new[] { "Normal", "High", "Low", "High", "Low", "Normal" })
            {
                var start = new ProcessStartInfo(Path.Combine(AppContext.BaseDirectory, "sdi_omt.exe"))
                {
                    UseShellExecute = false, CreateNoWindow = true,
                    RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true
                };
                foreach (var argument in new[] { "--test-source", "SDI OMT Reconnect Test", mode, quality }) start.ArgumentList.Add(argument);
                sender = Process.Start(start)!;
                var line = await sender.StandardOutput.ReadLineAsync().WaitAsync(TimeSpan.FromSeconds(10));
                var address = Regex.Match(line ?? "", @" as (.+)\. Ctrl").Groups[1].Value;
                if (address.Length == 0) throw new Exception("Test sender did not start: " + line + await sender.StandardError.ReadToEndAsync());
                if (receiver == IntPtr.Zero)
                {
                    receiver = ReconnectCheck.omt_receive_create(address, 2 | 4, 0, 2);
                    if (receiver == IntPtr.Zero) throw new Exception("Receiver creation failed");
                    report.Add("One persistent receiver, source: " + address);
                }
                int video = 0, audio = 0;
                var watch = Stopwatch.StartNew();
                long firstVideo = -1;
                while (watch.ElapsedMilliseconds < 15000 && (video < 30 || audio < 30))
                {
                    var pointer = ReconnectCheck.omt_receive(receiver, 2, 0);
                    if (pointer != IntPtr.Zero)
                    {
                        var frame = Marshal.PtrToStructure<ReconnectCheck.Frame>(pointer);
                        bool interlaced = mode.StartsWith("1080i");
                        int fps = (mode.EndsWith("60") ? 60 : 50) / (interlaced ? 2 : 1);
                        if (frame.Width != (interlaced ? 1920 : 1280) || frame.Height != (interlaced ? 1080 : 720) ||
                            frame.RateD <= 0 || frame.RateN / (double)frame.RateD != fps ||
                            (frame.Flags & 1) != (interlaced ? 1 : 0) || frame.Data == IntPtr.Zero)
                            throw new Exception("Received stale or incorrect video format");
                        if (firstVideo < 0) firstVideo = watch.ElapsedMilliseconds;
                        video++;
                    }
                    pointer = ReconnectCheck.omt_receive(receiver, 4, 0);
                    if (pointer != IntPtr.Zero)
                    {
                        var frame = Marshal.PtrToStructure<ReconnectCheck.Frame>(pointer);
                        if (frame.Channels != 2 || frame.SampleRate != 48000 || frame.Samples <= 0 || frame.Data == IntPtr.Zero)
                            throw new Exception("Incorrect audio format");
                        audio++;
                    }
                    await Task.Delay(5);
                }
                if (video < 30 || audio < 30) throw new Exception($"{mode} / {quality}: reconnect failed, video={video}, audio={audio}");
                var addresses = ReconnectCheck.omt_discovery_getaddresses(out int count);
                bool discovered = false;
                for (int i = 0; i < count; i++)
                    if (Marshal.PtrToStringUTF8(Marshal.ReadIntPtr(addresses, i * IntPtr.Size)) == address) discovered = true;
                if (!discovered) throw new Exception("Source absent from discovery");
                report.Add($"PASS {mode} / {quality}: video={video}, audio={audio}, first video={firstVideo} ms, discovery=true");
                File.WriteAllLines(resultPath, report);
                await Stop(sender); sender.Dispose(); sender = null;
                var quiet = Stopwatch.StartNew(); var drain = Stopwatch.StartNew();
                while (quiet.ElapsedMilliseconds < 1000 && drain.ElapsedMilliseconds < 5000)
                {
                    if (ReconnectCheck.omt_receive(receiver, 2 | 4, 0) != IntPtr.Zero) quiet.Restart();
                    await Task.Delay(10);
                }
                if (quiet.ElapsedMilliseconds < 1000) throw new Exception("Receiver failed to become quiet after stop");
            }
            report.Add("PASS: 18 sender starts, 17 reconnects; one receiver retained; video/audio/discovery verified after every restart.");
        }
        catch (Exception ex) { report.Add("FAIL: " + ex); Environment.ExitCode = 1; }
        finally
        {
            if (sender != null) { await Stop(sender); sender.Dispose(); }
            if (receiver != IntPtr.Zero) ReconnectCheck.omt_receive_destroy(receiver);
            ReconnectCheck.omt_shutdown();
            File.WriteAllLines(resultPath, report);
        }
    }
    private static async Task Stop(Process sender)
    {
        if (sender.HasExited) return;
        await sender.StandardInput.WriteLineAsync("stop"); await sender.StandardInput.FlushAsync();
        try { await sender.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(8)); }
        catch (TimeoutException) { sender.Kill(true); await sender.WaitForExitAsync(); throw; }
    }
}
