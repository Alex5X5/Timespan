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
	const string CurrentVersionString = "6.0";

	public static async Task<Version?> CheckForUpdateAsync() {

		if(!shouldCheck())
			return null;

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

		var currentVersion = Version.Parse(CurrentVersionString);
		var githubVersion = ParseVersion(tagName);
		if (githubVersion > currentVersion)
			return githubVersion;
		return null;
	}

	private static bool shouldCheck() {
		string path = PathService.FilesPath("last_update_check");
		string[] updateCheckInfo = ["1.1.2005"];
		if (File.Exists(path)) {
			updateCheckInfo = File.ReadAllLines(path);
		}
		DateTime lastCheckDate = DateTime.Parse(updateCheckInfo[0]);
		if(lastCheckDate > DateTime.UtcNow.AddDays(-1))
			return false;
		updateCheckInfo[0] = DateTime.Today.Date.ToString();
		File.WriteAllLines(path, updateCheckInfo);
		return true;
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
