namespace Timespan.GUI.Helpers;

public static class TaskHelper {

	public const int MAX_TASK_DESCRIPTION_CHARS = 21;

	public static string GetTitleString(string description, bool addDots = false, int maxChars = MAX_TASK_DESCRIPTION_CHARS) {
		if (description.Length <= maxChars)
			return description;
		List<char> res = [];
		List<char> word = [];
		for (int i = 0; i < maxChars && i < description.Length; i++) {
			char current = description[i];
			word.Add(current);
			if (current == ' ') {
				res.AddRange(word);
				word = [];
				continue;
			}
			if (res.Count + 1 + word.Count >= maxChars) {
				if (res.Count == 0) {
					res.AddRange(word);
				}
				break;
			}
		}
		if (addDots) {
			if(res.Count > 1)
				if (res[^1] == ' ')
					res.RemoveAt(res.Count - 1);
			res.AddRange(['.', '.', '.']);
		}
		return new(res.ToArray());
	}
}
