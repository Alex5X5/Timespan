namespace Timespan.Util.Services;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

public partial class SettingsService {

    public event Action OnSettingsReload = () => { };
    public event Action OnPreSettingsSave = () => { };
    public event Action OnAfterSettingsSave = () => { };

    private const string FILE_NAME = "appsettings.yml";
    
    public const string USER_NAME_KEY = "name";
    public const string JOB_NAME_KEY = "job";
    public const string START_DATE_KEY = "date";
    public const string LANGUAGE_KEY = "language";
    public const string THEME_KEY = "theme";

    private Dictionary<string, string> Settings;
    private ReadOnlyDictionary<string, string> UnchangedSettings;

	public bool HasUnsavedChanges { get; private set; } = false;

    public bool RequiresRestart { get; private set; } = false;

    public SettingsService() {
        Settings = LoadSettings();
        UnchangedSettings = BackupSettings();
	}

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

    private ReadOnlyDictionary<string, string> BackupSettings() {
        var dict = new Dictionary<string, string>();
        foreach (var key in Settings.Keys)
            dict[key] = Settings[key];
        return new(dict);
    }

	public void SaveSettings() {
        if (!HasUnsavedChanges)
            return;
        OnPreSettingsSave.Invoke();
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

    public void CancelEdit() {
		foreach (var key in UnchangedSettings.Keys)
			Settings[key] = UnchangedSettings[key];
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
        OnSettingsReload.Invoke();
    }

    public string Language {
        set {
            SetSetting(LANGUAGE_KEY, value);
            RequiresRestart = true;
            OnLanguageChanged?.Invoke(Language);
        }
        get => GetSetting(LANGUAGE_KEY);
    }

    public event Action<string>? OnLanguageChanged =
        l => TranslatorService.Singleton.CurrentLanguageName = l;

    public string Theme {
        set {
            SetSetting(THEME_KEY, value);
			RequiresRestart = true;
			OnThemeChanged?.Invoke(Theme);
        }
        get => GetSetting(THEME_KEY);
    }

    public event Action<string>? OnThemeChanged;
}
