namespace Timespan.GUI.Views.Graphs;

using Avalonia.Media;

using Timespan.GUI.Generators.Attributes;
using Timespan.GUI.Types;
using Timespan.Util.Attributes;
using Timespan.Util.Services;

using static System.Net.Mime.MediaTypeNames;

public partial class MonthPanelView : GraphPanelViewBase {

	[BasicStyledProperty<MonthPanelView>]
	private int weekOffset = 0;

	#region fields

	[TranslateMember("Views.Pages.Timer.Labels.Title", "Timer")]
	public string TitleLabelText { get; set; } = "";

	public const int MAX_TASK_DESCRIPTION_CHARS = 30;

	#endregion fields

	public MonthPanelView() : base() {
	}

	protected override int GetTaskRow(ObservableTask task) {
		int res = DateTimeService.WeekOfMonth(task.StartDateTime);
		res = Math.Max(res, 0);
		res = Math.Min(res, YAxisSegmentCount - 1);
		return res;
	}

	protected override int GetTaskColummn(ObservableTask task) {
		int res = DateTimeService.DayOfWorkWeek(task.StartDateTime);
		res = Math.Max(res, 0);
		res = Math.Min(res, XAxisSegmentCount - 1);
		return res;
	}

	protected override void DrawTimeline(DrawingContext context) {
		Pen timeLine = new(new SolidColorBrush(Colors.Black));
		Pen hintLine = new(new SolidColorBrush(Color.FromArgb(255, 0, 0, 0)));
		Brush textBrush = new SolidColorBrush(Colors.Gray);
		string[] days = [
			TranslatorService.Singleton["Days.Monday"],
			TranslatorService.Singleton["Days.Tuesday"],
			TranslatorService.Singleton["Days.Wednesday"],
			TranslatorService.Singleton["Days.Thursday"],
			TranslatorService.Singleton["Days.Friday"]
		];
		for (int i = 0; i < XAxisSegmentCount + 1; i++) {
			double xPos = XAxisSegmentSize * i + PaddingX;
			context.DrawLine(hintLine, new Point(xPos, Bounds.Height - PaddingY), new Point(xPos, PaddingY));
		}
		for (int i = 1; i < YAxisSegmentCount; i++) {
			double yPos = YAxisSegmentSize * i + PaddingY;
			context.DrawLine(hintLine, new Point(PaddingX, yPos), new Point(Bounds.Width - PaddingX, yPos));
		}

		double textSize = ArialHeightToPt(PaddingY, 0.7);
		for (int i = 0; i < YAxisSegmentCount; i++) {
			double xPos = XAxisSegmentSize * i + PaddingX;
			context.DrawLine(hintLine, new Point(xPos, Bounds.Height - PaddingY), new Point(xPos, PaddingY));
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
		for (int i = 0; i < YAxisSegmentCount; i++) {
			var formattedText = new FormattedText(
				Convert.ToString(i+WeekOffset),
				System.Globalization.CultureInfo.CurrentCulture,
				FlowDirection.LeftToRight,
				new Typeface("Arial"),
				textSize,
				textBrush
			);
			double xPos = PaddingX / 2.0;
			xPos -= formattedText.Height / 2;
			double yPos = PaddingY + YAxisSegmentSize * (i + 1);
			yPos -= YAxisSegmentSize / 2.0;
			yPos += formattedText.Width / 2;
			var matrix = Matrix.CreateRotation(-Math.PI / 2) * Matrix.CreateTranslation(xPos, yPos);
			var p = new Point(0, 0).Transform(matrix);
			using (context.PushTransform(matrix)) {
				context.DrawText(
					formattedText,
					new(0, 0)
				);
			}
		}
	}
}