namespace Timespan.GUI.Helpers;

public static class TaskHelper {

	public const int MAX_TASK_DESCRIPTION_CHARS = 20;

	public static string GetTitleString(string description, bool addDots = false, int maxChars = MAX_TASK_DESCRIPTION_CHARS) {
		if (description.Length <= maxChars)
			return description;
		List<char> res = [];
		List<char> word = [];
		for (int i = 0; i < maxChars && i < description.Length; i++) {
			char current = description[i];
			if (current == ' ') {
				if (res.Count + 1 + word.Count <= maxChars) {
					res.AddRange(word);
					res.Add(current);
					word = [];
				}
				continue;
			}
			word.Add(current);
		}
		if (addDots) {
			if (res[^1] == ' ')
				res.RemoveAt(res.Count - 1);
			res.AddRange(['.', '.', '.']);
		}
		return new(res.ToArray());
	}
}
