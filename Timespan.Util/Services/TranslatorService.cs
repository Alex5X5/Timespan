namespace Timespan.Util.Services;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using Timespan.Util.Attributes;

public class TranslatorService {

	public static readonly TranslatorService Singleton;

	private readonly YmlReader ymlReader;
	private readonly Dictionary<string, string> Languages;

	private string _currentLanguageName = "";
	public string CurrentLanguageName {
		set => ChangeLanguage(value);
		get => _currentLanguageName;
	}

	public string this[string index] => ymlReader[index];

	public string[] AvailableTranslations => Languages.Keys.ToArray();

	static TranslatorService() {
		Singleton = new TranslatorService();
	}

	private TranslatorService() {
		ymlReader = new();
		Languages = [];
		foreach (string path in Directory.GetFiles(PathService.LANGUAGES_DIRECTORY))
			Languages[Path.GetFileNameWithoutExtension(path)] = path;
		CurrentLanguageName = new SettingsService().TryGetSetting(SettingsService.LANGUAGE_KEY) ?? Languages.Keys.ToList()[0];
	}

	private void ChangeLanguage(string value) {
		if(Languages.TryGetValue(value, out var filePath)) {
			Console.WriteLine("Changing to Language:"+value);
			Console.WriteLine("Path to Language file is:"+filePath);
            _currentLanguageName = value;
			ymlReader.ReadFromFile(filePath);
		}
	}

	public void TranslateAnnotatedMembers(object obj) {
		Type objectType = obj.GetType();
		foreach (PropertyInfo property in objectType.GetProperties()) {
			Attribute? propertyAttribute = property.GetCustomAttributes()
				.FirstOrDefault(x=> x.GetType() == typeof(TranslateMember));
			if (propertyAttribute is TranslateMember translateAttribute) {
				if (this[translateAttribute.TranslationKey] is string translatedValue) {
					property.SetValue(obj, translatedValue);
				} else {
					property.SetValue(obj, translateAttribute.FallbackValue);
                }
            }
		}
	}

	public string TranslateMonth(int month) {
		return month switch {
			1 => this["Months.January"],
			2 => this["Months.February"],
			3 => this["Months.March"],
			4 => this["Months.April"],
			5 => this["Months.May"],
			6 => this["Months.June"],
			7 => this["Months.July"],
			8 => this["Months.August"],
			9 => this["Months.September"],
			10 => this["Months.October"],
			11 => this["Months.November"],
			12 => this["Months.December"],
			_ => ""
		};
	}

	public string TranslateMonthShort(int month) {
		return month switch {
			1 => this["Months.Short.January"],
			2 => this["Months.Short.February"],
			3 => this["Months.Short.March"],
			4 => this["Months.Short.April"],
			5 => this["Months.Short.May"],
			6 => this["Months.Short.June"],
			7 => this["Months.Short.July"],
			8 => this["Months.Short.August"],
			9 => this["Months.Short.September"],
			10 => this["Months.Short.October"],
			11 => this["Months.Short.November"],
			12 => this["Months.Short.December"],
			_ => ""
		};
	}

	public string TranslateDay(DayOfWeek day) {
		return day switch {
			DayOfWeek.Monday => this["Days.Monday"],
			DayOfWeek.Tuesday => this["Days.Tuesday"],
			DayOfWeek.Wednesday => this["Days.Wednesday"],
			DayOfWeek.Thursday => this["Days.Thursday"],
			DayOfWeek.Friday => this["Days.Friday"],
			DayOfWeek.Saturday => this["Days.Saturday"],
			DayOfWeek.Sunday => this["Days.Sunday"],
			_ => ""
		};
	}

	public string TranslateDayShort(DayOfWeek day) =>
		day switch {
			DayOfWeek.Monday => this["Days.Short.Monday"],
			DayOfWeek.Tuesday => this["Days.Short.Tuesday"],
			DayOfWeek.Wednesday => this["Days.Short.Wednesday"],
			DayOfWeek.Thursday => this["Days.Short.Thursday"],
			DayOfWeek.Friday => this["Days.Short.Friday"],
			DayOfWeek.Saturday => this["Days.Short.Saturday"],
			DayOfWeek.Sunday => this["Days.Short.Sunday"],
			_ => ""
		};
}
