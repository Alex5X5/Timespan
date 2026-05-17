namespace Timespan.Util.Services;

using System.Collections.Generic;
using System.IO;
using System.Linq;

public partial class AppStateService {

    private const string FILE_NAME = "apptate.yml";
    
    public const string USER_NAME_KEY = "name";
    public const string JOB_NAME_KEY = "job";
    public const string START_DATE_KEY = "date";

    private Dictionary<string, string> Settings;

    public bool HasUnsavedChanges { get; private set; } = false;

    private Dictionary<string, string> LoadSettings() {
        Dictionary<string, string> res = [];
        using FileStream fileHandle = File.Open(PathService.FilesPath(FILE_NAME), FileMode.OpenOrCreate);
        using StreamReader streamReader = new(fileHandle);
        string[] lines = streamReader.ReadToEnd().Split('\n');
        foreach (string line in lines) {
            string[] keyvaluePair = line.Split(':');
            if (keyvaluePair.Length == 2)
                res[keyvaluePair[0]] = keyvaluePair[1];
            if (keyvaluePair.Length > 2)
                res[keyvaluePair[0]] = string.Join(':', keyvaluePair.Skip(1));
        }
        return res;
    }

    public AppStateService() {
        Settings = LoadSettings();
    }

    public void SaveSettings() {
        if (!HasUnsavedChanges)
            return;
        string[] lines = new string[Settings.Count];
        int i = 0;
        foreach (string key in Settings.Keys) {
            lines[i] = key + ":" + Settings[key];
            i++;
        }
        string res = string.Join("\n", lines);
        string path = PathService.FilesPath(FILE_NAME);
        if (!File.Exists(path))
            File.Create(path);
        using FileStream fileHandle = File.Open(path, FileMode.Truncate);
        using StreamWriter streamWriter = new(fileHandle);
        streamWriter.Write(res);
        HasUnsavedChanges = false;
    }

    public string GetSetting(string key) {
        Settings.TryGetValue(key, out string? setting);
        return setting ?? "";
    }

    public string? TryGetSetting(string key) {
        Settings.TryGetValue(key, out string? setting);
        return setting;
    }

    public void SetSetting(string key, string newValue) {
        Settings[key] = newValue;
        HasUnsavedChanges = true;
    }

    public void UpdateSettings() {
        SaveSettings();
        Settings = LoadSettings();
    }
}
