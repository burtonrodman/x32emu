using X32Emulator.Osc;

namespace X32Emulator.Services;

public class SceneService
{
    private const string ScenesDir = "/data/scenes";
    private readonly X32StateService _stateService;

    public SceneService(X32StateService stateService)
    {
        _stateService = stateService;
    }

    public IEnumerable<string> ListScenes()
    {
        try
        {
            if (!Directory.Exists(ScenesDir)) return Enumerable.Empty<string>();
            return Directory.GetFiles(ScenesDir).Select(Path.GetFileName).Where(f => f != null).Select(f => f!).OrderBy(f => f);
        }
        catch { return Enumerable.Empty<string>(); }
    }

    public void LoadScene(string name)
    {
        try
        {
            // Sanitize: reject paths with directory separators or traversal sequences
            var safeName = Path.GetFileName(name);
            if (string.IsNullOrEmpty(safeName) || safeName != name) return;
            var path = Path.GetFullPath(Path.Combine(ScenesDir, safeName));
            if (!path.StartsWith(Path.GetFullPath(ScenesDir) + Path.DirectorySeparatorChar, StringComparison.Ordinal) &&
                !path.Equals(Path.GetFullPath(ScenesDir), StringComparison.Ordinal)) return;
            if (!File.Exists(path)) return;
            foreach (var line in File.ReadLines(path))
            {
                var trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith('#')) continue;
                var spaceIdx = trimmed.IndexOf(' ');
                if (spaceIdx < 0) continue;
                var oscPath = trimmed[..spaceIdx];
                var valueStr = trimmed[(spaceIdx + 1)..].Trim();

                List<OscArg> args;
                if (float.TryParse(valueStr, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float fv))
                    args = new List<OscArg> { new OscArg(fv) };
                else if (int.TryParse(valueStr, out int iv))
                    args = new List<OscArg> { new OscArg(iv) };
                else if (valueStr.StartsWith("float:") && float.TryParse(valueStr[6..], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float fv2))
                    args = new List<OscArg> { new OscArg(fv2) };
                else if (valueStr.StartsWith("int:") && int.TryParse(valueStr[4..], out int iv2))
                    args = new List<OscArg> { new OscArg(iv2) };
                else
                    args = new List<OscArg> { new OscArg(valueStr) };

                _stateService.SetValue(oscPath, args);
            }
        }
        catch { }
    }

    public void GoToScene(int n)
    {
        var scenes = ListScenes().ToList();
        if (n >= 0 && n < scenes.Count)
            LoadScene(scenes[n]);
    }
}
