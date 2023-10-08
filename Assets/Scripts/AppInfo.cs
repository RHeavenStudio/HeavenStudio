using System;

public static class AppInfo {
    public const string Version = "0.0.1002";
    public static readonly DateTime Date = new DateTime(2023, 09, 27, 22, 20, 38, 655, DateTimeKind.Utc);
}


#if UNITY_EDITOR
/// <summary>
/// Increase Build Number Automatically
/// </summary>
public class BuildNumberUpdater : UnityEditor.Build.IPreprocessBuild
{
    private static readonly char[] LineDelimiter = {'\n', '\r'};

    /// <summary> File name where info is stored </summary>
    private const string AppInfoFileName = "AppInfo.cs";

    public int callbackOrder {
        get { return 1; }
    }

    void UnityEditor.Build.IPreprocessBuild.OnPreprocessBuild(UnityEditor.BuildTarget target, string path) {
        var scriptPath = GetScriptPath(AppInfoFileName);
        var version = IncVersion();
        var time = DateTime.UtcNow;
        string date = "new DateTime(" + time.ToString("yyyy, MM, dd, HH, mm, ss, fff") + ", DateTimeKind.Utc)";

        UnityEngine.Debug.LogFormat(
            "OnPreprocessBuild. Modify AppInfo in file={0}, set version={1} and current DateTime",
            scriptPath, version);

        var text = System.IO.File.ReadAllText(scriptPath);
        text = ReplaceText(text, " Version = ", "\"" + version + "\";");
        text = ReplaceText(text, " Date = ", date + ";");
        System.IO.File.WriteAllText(scriptPath, text);
    }

    private static string ReplaceText(string text, string field, string newValue) {
        int v1 = text.IndexOf(field, StringComparison.Ordinal);
        int v2 = text.IndexOfAny(LineDelimiter, v1);
        if (v1 < 0 || v2 < 0)
            throw new Exception("Undefined field=" + field);
        return text.Substring(0, v1 + field.Length) + newValue + text.Substring(v2);
    }

    private static string IncVersion() {
        var bundleVersionSplit = UnityEditor.PlayerSettings.bundleVersion.Split('.');
        int major = 0;
        int minor = 0;
        int subVersion = 0;
        if (bundleVersionSplit.Length >= 1)
            int.TryParse(bundleVersionSplit[0], out major);
        if (bundleVersionSplit.Length >= 2)
            int.TryParse(bundleVersionSplit[1], out minor);
        if (bundleVersionSplit.Length >= 3)
            int.TryParse(bundleVersionSplit[2], out subVersion);
        ++subVersion;
        string version = string.Format("{0}.{1}.{2}", major, minor, subVersion);
        var bundleVersionCode = (major * 100000) + (minor * 1000) + subVersion;

        UnityEditor.PlayerSettings.bundleVersion = version;
        UnityEditor.PlayerSettings.Android.bundleVersionCode = bundleVersionCode;
        UnityEditor.PlayerSettings.macOS.buildNumber = bundleVersionCode.ToString();
        return version;
    }


    private static string GetScriptPath(string fileName) {
        var assets = UnityEditor.AssetDatabase.FindAssets(System.IO.Path.GetFileNameWithoutExtension(fileName));
        string scriptPath = null;
        foreach (var asset in assets) {
            var path = UnityEditor.AssetDatabase.GUIDToAssetPath(asset);
            if (path.EndsWith(".cs")) {
                scriptPath = path;
                break;
            }
        }
        if (string.IsNullOrEmpty(scriptPath))
            throw new Exception("No asset file with name '" + AppInfoFileName + "' found");

        return scriptPath;
    }
}
#endif