namespace Timespan.GUI.Services;

using Avalonia.Media;

public class ColorService {

	public readonly Color TASK_BACKGROUND_YELLOW;
	public readonly Color TASK_BACKGROUND_ORANGE;
	public readonly Color TASK_BACKGROUND_RED;
	public readonly Color TASK_BACKGROUND_LIGTH_BLUE;
	public readonly Color TASK_BACKGROUND_DARK_BLUE;
	public readonly Color TASK_BACKGROUND_LIGHT_GREEN;
	public readonly Color TASK_BACKGROUND_DARK_GREEN;
    public readonly Color TASK_BACKGROUND_DARK_GRAY;
    public readonly Color TASK_BACKGROUND_LIGHT_GRAY;

    public ColorService() {
		TASK_BACKGROUND_YELLOW= GetColorFromResources("TaskBackgroundYellow") ?? new Color(255, 235, 198, 52);
		TASK_BACKGROUND_ORANGE = GetColorFromResources("TaskBackgroundOrange") ?? new Color(255, 255, 128, 0);
		TASK_BACKGROUND_RED = GetColorFromResources("TaskBackgroundRed") ?? new Color(255, 32, 178, 171);
		TASK_BACKGROUND_LIGTH_BLUE = GetColorFromResources("TaskBackgroundLightBlue") ?? new Color(255, 32, 178, 170);
		TASK_BACKGROUND_DARK_BLUE = GetColorFromResources("TaskBackgroundDarkBlue") ?? new Color(255, 0, 60, 199);
		TASK_BACKGROUND_LIGHT_GREEN = GetColorFromResources("TaskBackgroundLightGreen") ?? new Color(255, 50, 205, 50);
		TASK_BACKGROUND_DARK_GREEN = GetColorFromResources("TaskBackgroundDarkGreen") ?? new Color(255, 0, 128, 0);
        TASK_BACKGROUND_LIGHT_GRAY = GetColorFromResources("TaskBackgroundLightGray") ?? new Color(255, 129, 128, 128);
        TASK_BACKGROUND_DARK_GRAY = GetColorFromResources("TaskBackgroundDarkGray") ?? new Color(255, 79, 79, 79);
    }

	private static Color? GetColorFromResources(string resourceKey) {
		if (Avalonia.Application.Current?.TryGetResource(resourceKey, null, out var resource) ?? false) {
			if (Color.TryParse(resource?.ToString(), out Color color))
				return color;
		}
		return null;
	}
}
