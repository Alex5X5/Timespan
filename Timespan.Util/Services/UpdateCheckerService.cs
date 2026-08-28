namespace Timespan.Util.Services;

using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

public static class UpdateCheckerService {

	const string Owner = "Alex5X5";
	const string Repo = "Timespan";
	private static readonly Version CurrentVersion = new(5, 0);

	public static async Task<Version?> CheckForUpdateAsync() {
		ReadLastCheckInfo(out DateTime lastCheckDate, out Version cachedVerstion);
		if (!shouldCheck(lastCheckDate, cachedVerstion)) {
			if (cachedVerstion > CurrentVersion) {
				return cachedVerstion;
			} else {
				return null;
			}
		}
		var githubVersion = await GetLatestVersionFromGithub();
		SaveLastCheckInfo(DateTime.Today, githubVersion ?? CurrentVersion);
		if (githubVersion > CurrentVersion)
			return githubVersion;
		return null;
	}

	private static bool shouldCheck(DateTime lastCheckDate, Version chached) {
		return lastCheckDate < DateTime.Now.AddDays(-1);
	}

	private static void SaveLastCheckInfo(DateTime lastCheckDate, Version version) {
		string path = PathService.FilesPath("last_update_check");
		string[] updateCheckInfo = [
			lastCheckDate.Date.ToString(),
			version.ToString()
		];
		File.WriteAllLines(path, updateCheckInfo);
	}

	private static void ReadLastCheckInfo(out DateTime lastCheckDate, out Version version) {
		string path = PathService.FilesPath("last_update_check");
		string[] updateCheckInfo = ["1.1.2005", "1.0"];
		if (File.Exists(path)) {
			updateCheckInfo = File.ReadAllLines(path);
		}
		if (updateCheckInfo.Length >= 1 && DateTime.TryParse(updateCheckInfo[0], out DateTime savedLastCheckDate)) {
			lastCheckDate = savedLastCheckDate;
		} else {
			lastCheckDate = new DateTime(2005, 1, 1);
		}
		if (updateCheckInfo.Length >= 2 && Version.TryParse(updateCheckInfo[1], out Version? savedVersion)) {
			version = savedVersion!;
		} else {
			version = new Version(1, 0);
		}
	}

	private static async Task<Version?> GetLatestVersionFromGithub() {
		using var client = new HttpClient();
		// GitHub API requires a User-Agent header.
		client.DefaultRequestHeaders.UserAgent.Add(
			new ProductInfoHeaderValue("ReleaseChecker", "1.0"));
		client.DefaultRequestHeaders.Accept.Add(
			new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));

		string url = $"https://api.github.com/repos/{Owner}/{Repo}/releases/latest";

		HttpResponseMessage response = await client.GetAsync(url);
		if (!response.IsSuccessStatusCode) {
			Console.Error.WriteLine(
				$"GitHub API request failed: {(int)response.StatusCode} {response.ReasonPhrase}");
			return null;
		}

		string json = await response.Content.ReadAsStringAsync();
		using JsonDocument doc = JsonDocument.Parse(json);

		if (!doc.RootElement.TryGetProperty("name", out JsonElement tagElement)) {
			return null;
		}

		string? tagName = tagElement.GetString();
		if (string.IsNullOrWhiteSpace(tagName)) {
			return null;
		}

		return ParseVersion(tagName);
	}

	private static Version ParseVersion(string raw) {
		string cleaned = raw.Trim();

		cleaned = cleaned.Replace("Release ", "");
		cleaned = cleaned.Replace("Beta ", "");

		// Strip a leading 'v' or 'V' (common release tag convention, e.g. "v1.2.3").
		if (cleaned.Length > 0 && (cleaned[0] == 'v' || cleaned[0] == 'V')) {
			cleaned = cleaned[1..];
		}

		// Strip anything after a '-' or '+' (pre-release/build metadata, e.g. "1.2.3-beta.1").
		int cutIndex = cleaned.IndexOfAny(new[] { '-', '+' });
		if (cutIndex >= 0) {
			cleaned = cleaned[..cutIndex];
		}

		// Ensure at least major.minor for System.Version parsing.
		int dotCount = cleaned.Split('.').Length;
		if (dotCount == 1) {
			cleaned += ".0";
		}
		return Version.Parse(cleaned);
	}
}
