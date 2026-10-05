using System.Reflection;
namespace SdiOmt;
internal static class WindowTitle
{
    // Like Fussball/FensterTitel: window name | assembly title + version | author.
    public static void Apply(Form form)
    {
        var assembly = typeof(WindowTitle).Assembly;
        var title = assembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title ?? "SDI2OMD CONVERTER";
        var company = assembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company ?? "Peter Aellig";
        var version = assembly.GetName().Version?.ToString() ?? "1.0.0.3";
        form.Text = $"{title} {version}  | {company}";
        using var stream = assembly.GetManifestResourceStream("SdiOmt.Branding.SDI.ico");
        if (stream is not null) {
            using var icon = new Icon(stream);
            form.Icon = (Icon)icon.Clone();
        }
    }
}
