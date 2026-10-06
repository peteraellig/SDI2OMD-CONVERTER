using System.Diagnostics;
using System.Text.Json;

namespace SdiOmt;

public sealed class CardInfoForm : Form
{
    private readonly string enginePath;
    private readonly System.Windows.Forms.Timer timer = new() { Interval = 2000 };
    private readonly CancellationTokenSource lifetime = new();
    private readonly ListView ports = new() { Name = "portList", Dock = DockStyle.Fill, View = View.Details,
        FullRowSelect = true, MultiSelect = false, HeaderStyle = ColumnHeaderStyle.Nonclickable };
    private readonly Label summary = new() { AutoSize = true, Text = "Reading DeckLink status …" };
    private bool polling;
    internal int RefreshCount { get; private set; }
    private record PortStatus(int Index, string Name, bool? Signal, string? Mode, int? Busy);

    public CardInfoForm(string captureEnginePath)
    {
        enginePath = captureEnginePath;
        Text = "BM card info"; Font = new Font("Segoe UI", 9F);
        BackColor = Color.FromArgb(240, 242, 245);
        ClientSize = new Size(680, 280); MinimumSize = new Size(620, 320);
        StartPosition = FormStartPosition.CenterParent; ShowInTaskbar = false;
        MaximizeBox = false; MinimizeBox = false;
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(12), ColumnCount = 1, RowCount = 4 };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle()); layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle()); layout.RowStyles.Add(new RowStyle());
        summary.Margin = new Padding(0, 0, 0, 8);
        layout.Controls.Add(summary, 0, 0); layout.Controls.Add(ports, 0, 1);
        ports.Columns.Add("#", 32); ports.Columns.Add("Device", 200);
        ports.Columns.Add("Signal", 110); ports.Columns.Add("Format", 105); ports.Columns.Add("Use", 140);
        ports.Resize += (_, _) => ports.Columns[1].Width = Math.Max(150, ports.ClientSize.Width - 395);
        var note = new Label { AutoSize = true, Dock = DockStyle.Fill, Margin = new Padding(0, 8, 0, 8),
            Text = "Numbers follow DeckLink device order. Physical connector mapping: Desktop Video Setup.\nNot detected / Unavailable reflects the driver's input status; no input is opened for this check." };
        layout.Controls.Add(note, 0, 2);
        var close = new Button { Name = "closeButton", Text = "Close", Size = new Size(100, 32),
            Anchor = AnchorStyles.Right, BackColor = Color.White, FlatStyle = FlatStyle.Flat };
        close.Click += (_, _) => Close(); layout.Controls.Add(close, 0, 3); CancelButton = close;
        Controls.Add(layout);
        timer.Tick += async (_, _) => await RefreshAsync();
        Shown += async (_, _) => { await RefreshAsync(); if (!IsDisposed && !lifetime.IsCancellationRequested) timer.Start(); };
    }

    private async Task RefreshAsync()
    {
        if (polling || IsDisposed || lifetime.IsCancellationRequested) return;
        polling = true;
        try
        {
            var start = new ProcessStartInfo(enginePath) { UseShellExecute = false, CreateNoWindow = true,
                RedirectStandardOutput = true, RedirectStandardError = true,
                StandardOutputEncoding = System.Text.Encoding.UTF8, StandardErrorEncoding = System.Text.Encoding.UTF8 };
            start.ArgumentList.Add("--status");
            using var process = Process.Start(start) ?? throw new InvalidOperationException("Status process did not start");
            var output = process.StandardOutput.ReadToEndAsync(); var error = process.StandardError.ReadToEndAsync();
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(lifetime.Token);
            timeout.CancelAfter(TimeSpan.FromSeconds(4));
            try { await process.WaitForExitAsync(timeout.Token); }
            catch (OperationCanceledException)
            {
                if (!process.HasExited) { process.Kill(); await process.WaitForExitAsync(); }
                if (lifetime.IsCancellationRequested) return;
                throw new TimeoutException("Status query timed out");
            }
            var text = await output; var errors = await error;
            if (process.ExitCode != 0) throw new InvalidOperationException(errors);
            var devices = JsonSerializer.Deserialize<PortStatus[]>(text, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? throw new JsonException("No status data");
            if (IsDisposed || lifetime.IsCancellationRequested) return;
            ports.BeginUpdate();
            try
            {
                while (ports.Items.Count > devices.Length) ports.Items.RemoveAt(ports.Items.Count - 1);
                for (int i = 0; i < devices.Length; i++)
                {
                    var device = devices[i];
                    var busy = device.Busy;
                    string use = busy is null ? "Unavailable" : (busy.Value & 3) == 3 ? "Capture + output" :
                        (busy.Value & 1) != 0 ? "Capture in use" : (busy.Value & 2) != 0 ? "Output in use" : busy.Value != 0 ? "Other use" : "Free";
                    string[] cells = { (device.Index + 1).ToString(), device.Name,
                        device.Signal == true ? "Detected" : device.Signal == false ? "Not detected" : "Unavailable",
                        device.Signal == true ? device.Mode ?? "Unknown" : "—", use };
                    if (i >= ports.Items.Count) ports.Items.Add(new ListViewItem(cells));
                    else for (int column = 0; column < cells.Length; column++) ports.Items[i].SubItems[column].Text = cells[column];
                    ports.Items[i].ForeColor = device.Signal == true ? Color.FromArgb(35, 125, 75) : Color.DimGray;
                }
                summary.Text = devices.Length == 0 ? "No DeckLink devices found" : "DeckLink device status · updates every 2 seconds while open";
                RefreshCount++;
            }
            finally { ports.EndUpdate(); }
        }
        catch (Exception ex) when (ex is IOException or InvalidOperationException or JsonException or TimeoutException or System.ComponentModel.Win32Exception)
        {
            if (!IsDisposed && !lifetime.IsCancellationRequested) { summary.Text = "DeckLink status unavailable"; ports.Items.Clear(); }
        }
        finally { polling = false; }
    }
    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        timer.Stop(); lifetime.Cancel(); base.OnFormClosed(e);
    }
    protected override void Dispose(bool disposing)
    {
        if (disposing) { timer.Dispose(); lifetime.Cancel(); }
        base.Dispose(disposing);
    }
}
