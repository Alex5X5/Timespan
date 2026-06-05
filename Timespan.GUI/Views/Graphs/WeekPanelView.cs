namespace Timespan.GUI.Views.Graphs;

using Avalonia.Media;

using global::Timespan.GUI.ViewModels.Graphs;
using global::Timespan.Util.Services;

using Timespan.Util.Attributes;

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

	//public Rect GetTaskRectanlge(Types.Task task, double additionalWidth, double additionalHeight, int i) {
	//	double proportion = GRAPH_AREA_WIDTH / TIME_INTERVALL_DURATION;
	//	double graphPosX = (task.start - TIME_INTERVALL_START_SECONDS) * proportion + PADDING_X;
	//	long duration = task.running ? DateTimeService.ToSeconds(DateTime.Now) - task.start : task.finish - task.start;
	//	double graphLength = duration * proportion;
	//	double width = Math.Max(graphLength, GRAPH_MINIMAL_WIDTH) + additionalWidth * 2;
	//	Rect res = new(
	//		graphPosX - additionalWidth,
	//		Y_AXIS_SEGMENT_SIZE * (i % (MAX_TASKS / TASK_GRAPH_COLUMN_COUNT)) * 1.5 - additionalHeight + PADDING_Y,
	//		width,
	//		Y_AXIS_SEGMENT_SIZE + additionalHeight * 2
	//	);
	//	return res;
	//}

	protected override void DrawColumnMarkers(DrawingContext context) {
		Brush markedBrush = new SolidColorBrush(Color.FromArgb(120, 120, 120, 240));
		Brush blockedBrush = new SolidColorBrush(Color.FromArgb(255, 255, 80, 80));
		double x = PaddingX + 2;
		double y = PaddingY + 2;
		double width = XAxisSegmentSize - 4;
		double height = GraphAreaHeight - 5;
		for (int i = 0; i < XAxisSegmentCount; i++) {
			if (BlockedColumns[i].Value)
				context.FillRectangle(blockedBrush, new Rect(x, y, width, height));
			if (MarkedColumns[i].Value)
				context.FillRectangle(markedBrush, new Rect(x, y, width, height));
			x += XAxisSegmentSize;
		}
	}

	protected override void DrawTimeline(DrawingContext context) {
		Brush weekedDayBackground = new SolidColorBrush(Color.FromArgb(255, 200, 200, 200));
		Brush todayBackgroundColor = new SolidColorBrush(Color.FromArgb(255, 237, 166, 166));
		Pen timeLine = new(new SolidColorBrush(Colors.Black));
		Pen hintLine = new(new SolidColorBrush(Color.FromArgb(255, 170, 170, 170)));
		Brush textBrush = new SolidColorBrush(Colors.Gray);
		string[] days = [
			TranslatorService.Singleton["Days.Monday"] ?? "Monday",
			TranslatorService.Singleton["Days.Tuesday"] ?? "Tuesday",
			TranslatorService.Singleton["Days.Wednesday"] ?? "Wendsday",
			TranslatorService.Singleton["Days.Thursday"] ?? "Thursday",
			TranslatorService.Singleton["Days.Friday"] ?? "Friday"
		];
		for (int i = 0; i < XAxisSegmentCount; i++) {
			double xPos = XAxisSegmentSize * i + PaddingX;
			//if (i % 7 == 5 | i % 7 == 6)
			//context.FillRectangle(weekedDayBackground, new(xPos + 1, PaddingY, XAxisSegmentSize - 2, Bounds.Height - (2 * PaddingY)));
			if (i + 1 == (int)DateTime.Today.DayOfWeek)
				if (DateTimeService.FloorWeek((DataContext as WeekPanelViewModel)!.CacheService.SelectedDay) == DateTimeService.FloorWeek(DateTime.Now))
					context.FillRectangle(todayBackgroundColor, new(xPos + 1, PaddingY, XAxisSegmentSize - 2, Bounds.Height - (2 * PaddingY)));
		}
		//context.DrawLine(timeLine, new(PaddingX, Bounds.Height - PaddingY), new(Bounds.Width - PaddingX, Bounds.Height - PaddingY));
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

	protected override void DrawMouseRectangle(DrawingContext context) {
		if (RightMouseDown) {
			Brush borderBrush = new SolidColorBrush(Color.FromArgb(200, 100, 100, 100));
			Brush areaBrush = new SolidColorBrush(Color.FromArgb(150, 150, 220, 255));
			Pen borderPen = new Pen(borderBrush, 2);
			context.DrawRectangle(areaBrush, borderPen, MarkerDragRectangle);
			//Console.WriteLine($"filling marker rect from {MarkerDragRectangle.TopLeft} to {MarkerDragRectangle.BottomRight}");
		}
	}

	//private void DrawTaskGraph(DrawingContext context, Timespan.Types.Models.Task task, int i) {
	//	Rect rect = GetTaskRectanlge(task, 0, 0, i);
	//	r = Math.Min(r, rect.Width / 2);
	//	RectangleGeometry rrect = new(rect) {
	//		RadiusX = r,
	//		RadiusY = r
	//	};
	//	context.DrawGeometry(brush, null, rrect);
	//	DrawTaskDescriptionStub(context, task, rect);
	//}

	//private void DrawTaskDescriptionStub(DrawingContext context, Types.Task task, Rect taskRect) {
	//	var formattedText = new FormattedText(
	//		task.description.Length <= MAX_TASK_DESCRIPTION_CHARS ? task.description : task.description[..MAX_TASK_DESCRIPTION_CHARS] + "...",
	//		System.Globalization.CultureInfo.CurrentCulture,
	//		FlowDirection.LeftToRight,
	//		new Typeface("Arial"),
	//		Math.Max(2.0, ArialHeightToPt(Y_AXIS_SEGMENT_SIZE)),
	//		new SolidColorBrush(Colors.Black)
	//	);
	//	Point p = new(taskRect.X - formattedText.Width - TASK_DESCRIPTION_GRAPH_SPAGE, taskRect.Y + taskRect.Height / 2 - formattedText.Height / 2);
	//	context.DrawText(formattedText, p);
	//}
}