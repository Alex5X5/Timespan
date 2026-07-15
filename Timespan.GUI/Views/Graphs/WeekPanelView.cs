namespace Timespan.GUI.Views.Graphs;

using Avalonia.Input;
using Avalonia.Media;

using Timespan.GUI.Types;
using Timespan.Util.Attributes;
using Timespan.Util.Services;

public partial class WeekPanelView : GraphPanelViewBase {

	#region fields

	[TranslateMember("Views.Pages.Timer.Labels.Title", "Timer")]
	public string TitleLabelText { get; set; } = "";

	public const int MAX_TASK_DESCRIPTION_CHARS = 30;

	#endregion fields

	#region styledProperties

	private const int MAX_COLUMN_COUNT = 24;

	#endregion styledProperties

	public WeekPanelView() : base() {
	}


	private static double ArialHeightToPt(double height, double x = 1) =>
		Math.Round(Math.Log(3 * height + 1) * 3 * x + height * 0.3 * x, 2);

	protected override void DrawTimeline(DrawingContext context) {
		Brush weekedDayBackground = new SolidColorBrush(Color.FromArgb(255, 200, 200, 200));
		Brush todayBackgroundColor = new SolidColorBrush(Color.FromArgb(255, 237, 166, 166));
		Pen timeLine = new(new SolidColorBrush(Colors.Black));
		Pen hintLine = new(new SolidColorBrush(Color.FromArgb(255, 170, 170, 170)));
		Brush textBrush = new SolidColorBrush(Colors.Gray);
		string[] days = [
			TranslatorService.Singleton["Days.Monday"],
			TranslatorService.Singleton["Days.Tuesday"],
			TranslatorService.Singleton["Days.Wednesday"],
			TranslatorService.Singleton["Days.Thursday"],
			TranslatorService.Singleton["Days.Friday"]
		];
		double textSize = Math.Round(PaddingY * 0.7, 1);
		for (int i = 0; i < 6; i++) {
			double xPos = XAxisSegmentSize * i + PaddingX;
			context.DrawLine(hintLine, new Point(xPos, Bounds.Height - PaddingY), new Point(xPos, PaddingY));
			//context.DrawLine(timeLine, new Point(xPos, Bounds.Height - PaddingY), new Point(xPos, Bounds.Height - PaddingY - TimelineMarkHeight));
			if (i < 5) {
				var formattedText = new FormattedText(
					days[i],
					System.Globalization.CultureInfo.CurrentCulture,
					FlowDirection.LeftToRight,
					new Typeface("Arial"),
					textSize,
					textBrush
				);
				Point textPos = new(xPos + XAxisSegmentSize / 2.0 - formattedText.Width / 2.0, Bounds.Height - (PaddingY * 0.85));
				context.DrawText(
					formattedText,
					textPos
				);
			}
		}
	}
}