using System.Text.Json;
namespace SdiOmt;
public sealed class AppSettings
{
    public string SenderName { get; set; } = "OMT-SDI";
    public string VideoMode { get; set; } = "720p50";
    public string Quality { get; set; } = "Normal";
    public string AudioSelection { get; set; } = "Stereo 1&2";
    public string? DeviceName { get; set; }
    public int WindowWidth { get; set; } = 800;
    public int WindowHeight { get; set; } = 540;
    public bool Maximized { get; set; }
    public static string FilePath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SdiOmt", "settings.json");
    public static AppSettings Load()
    {
        if (!File.Exists(FilePath)) return new();
        try { return JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(FilePath)) ?? new(); }
        catch (JsonException) { return new(); }
    }
    public void Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
        var temporary = FilePath + ".tmp";
        File.WriteAllText(temporary, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
        File.Move(temporary, FilePath, true);
    }
}
