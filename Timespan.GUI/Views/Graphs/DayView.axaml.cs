namespace Timespan.GUI.Views.Graphs;

using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;

using CommunityToolkit.Mvvm.Input;

using Timespan.GUI.ViewModels.Graphs;
using Timespan.Util.Attributes;
using Timespan.Util.Services;

using Types = Timespan.Types.Models;

public partial class DayView : UserControl {

	#region fields

	[TranslateMember("Views.Pages.Timer.Labels.Title", "Timer")]
	public string TitleLabelText { get; set; } = "";


	public const int MAX_TASK_DESCRIPTION_CHARS = 30;

	private bool RightMouseDown = false;
	private bool LeftMouseDown = false;

	private Rect MarkerDragRectangle;
	private Point DragOrigin;
	private Point MousePos = new(0.0, 0.0);

	private DayViewModel Model => (DataContext as DayViewModel) ?? new DayViewModel();
	private ContextMenu? _contextMenu;
	
	#endregion fields


	public DayView() {
		TranslatorService.Singleton.TranslateAnnotatedMembers(this);
		InitializeComponent();
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

	public override void Render(DrawingContext context) {
		if (!IsVisible)
			return;
		IBrush? brush = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
		context.FillRectangle(brush, new Rect(Bounds.X , Bounds.Y , Bounds.Width , Bounds.Height ));
		var background = new SolidColorBrush(Color.FromArgb(255, 217, 217, 217));
		Pen pen = new Pen(background, 0);
		RectangleGeometry rrect = new(Bounds) {
			RadiusX = 20,
			RadiusY = 20
		};
		context.DrawGeometry(background, pen, rrect);
		//DrawTimeline(context);
		//DrawColumnMarkers(context);

		DrawMouseRectangle(context);
	}

	//private void DrawTimeline(DrawingContext context) {
	//	Pen timeLine = new(new SolidColorBrush(Colors.Black));
	//	Pen hintLine = new(new SolidColorBrush(Color.FromArgb(255, 170, 170, 170)));
	//	Brush textBrush = new SolidColorBrush(Colors.Gray);
	//	context.DrawLine(timeLine, new(PADDING_X, Bounds.Height - PADDING_Y), new(Bounds.Width - PADDING_X, Bounds.Height - PADDING_Y));
	//	double textSize = Math.Round(PADDING_Y * 0.7, 1);
	//	for (int i = 0; i < 25; i++) {
	//		double xPos = X_AXIS_SEGMENT_SIZE * i + PADDING_X;
	//		context.DrawLine(hintLine, new Point(xPos, Bounds.Height - PADDING_Y), new Point(xPos, PADDING_Y));
	//		context.DrawLine(timeLine, new Point(xPos, Bounds.Height - PADDING_Y), new Point(xPos, Bounds.Height - PADDING_Y - TIMELINE_MARK_HEIGHT));
	//		var formattedText = new FormattedText(
	//			Convert.ToString(i) + ":00",
	//			System.Globalization.CultureInfo.CurrentCulture,
	//			FlowDirection.LeftToRight,
	//			new Typeface("Arial"),
	//			textSize,
	//			textBrush
	//		);
	//		Point textPos = new(xPos - formattedText.Width / 2.0, Bounds.Height - (PADDING_Y * 0.85));
	//		context.DrawText(
	//			formattedText,
	//			textPos
	//		);
	//	}
	//}

	//private void DrawColumnMarkers(DrawingContext context) {
		
	//}

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

	private void DrawMouseRectangle(DrawingContext context) {
		if (RightMouseDown) {
			Brush borderBrush = new SolidColorBrush(Color.FromArgb(200, 100, 100, 100));
			Brush areaBrush = new SolidColorBrush(Color.FromArgb(150, 150, 220, 255));
			Pen pen = new Pen(borderBrush, 2);
			context.FillRectangle(areaBrush, MarkerDragRectangle);
			context.DrawRectangle(pen, MarkerDragRectangle);
			//Console.WriteLine($"filling marker rect from {MarkerDragRectangle.TopLeft} to {MarkerDragRectangle.BottomRight}");
		}
	}

	//public void OnMouseDragging(Avalonia.Rect dragRect, double width, double paddingX) {
	//	double leftRectBound = dragRect.X - paddingX;
	//	double rightRectBound = leftRectBound + dragRect.Width;
	//	for (int i = 0; i < X_AXIS_SEGMENT_COUNT; i++) {
	//		double leftSegmentBound = width * i / X_AXIS_SEGMENT_COUNT;
	//		double rightSegmentBound = width * (i + 1) / X_AXIS_SEGMENT_COUNT;
	//		MarkedColumns[i] = false;
	//		if (rightRectBound < leftSegmentBound)
	//			continue;
	//		if (leftRectBound > rightSegmentBound)
	//			continue;
	//		MarkedColumns[i] = true;
	//	}
	//}

	//public async Task SetTimeIntervallBlocked(BlockedTimeIntervallType reason) {
	//	if (reason == BlockedTimeIntervallType.None) {
	//		await SetTimeIntervallUnblocked();
	//		return;
	//	}
	//	long start = TIME_INTERVALL_START_SECONDS;
	//	long finish = start + X_AXIS_SEGMENT_DURATION;
	//	List<Database.Types.Task> tasks = dbService.QueryBlockingTasksInIntervallAsync(TIME_INTERVALL_START_SECONDS, TIME_INTERVALL_FINISH_SECONDS).Result;
	//	for (int i = 0; i < X_AXIS_SEGMENT_COUNT; i++) {
	//		if (MarkedColumns[i]) {
	//			IEnumerable<Database.Types.Task> tasks_ = tasks
	//				.Where(x => x.start >= start && x.start <= finish)
	//					.Where(x => x.finish >= start && x.finish <= finish);
	//			if (!tasks_.Any()) {
	//				await dbService.CreateIntervallBlockingTaskAsync(reason, new DateTime(start * TimeSpan.TicksPerSecond), X_AXIS_SEGMENT_DURATION);
	//			}
	//		}
	//		start += X_AXIS_SEGMENT_DURATION;
	//		finish += X_AXIS_SEGMENT_DURATION;
	//	}
	//}

	//public async Task SetTimeIntervallUnblocked() {
	//	long start = TIME_INTERVALL_START_SECONDS;
	//	long finish = start + X_AXIS_SEGMENT_DURATION;
	//	List<Database.Types.Task> tasks = dbService.QueryBlockingTasksInIntervallAsync(TIME_INTERVALL_START_SECONDS, TIME_INTERVALL_FINISH_SECONDS).Result;
	//	for (int i = 0; i < X_AXIS_SEGMENT_COUNT; i++) {
	//		if (MarkedColumns[i]) {
	//			IEnumerable<Database.Types.Task> tasks_ = tasks
	//				.Where(x => x.start >= start && x.start <= finish)
	//					.Where(x => x.finish >= start && x.finish <= finish);
	//			foreach (var task in tasks_)
	//				await dbService.DeleteTaskAsync(task);
	//		}
	//		start += X_AXIS_SEGMENT_DURATION;
	//		finish += X_AXIS_SEGMENT_DURATION;
	//	}
	//}

	private void ShowReasonContextMenu() {

		void Callback(Types.BlockedTimeIntervallType reason) {
			Model?.OnMissingContextMenuClicked(reason);
			InvalidateVisual();
		}

		_contextMenu = new() {
			ItemsSource = new List<MenuItem>() {
				new() {
					Header = "Krank",
					Command = new RelayCommand(()=>Callback(Types.BlockedTimeIntervallType.Sick))
				},
				new() {
					Header = "Feiertag",
					Command = new RelayCommand(()=>Callback(Types.BlockedTimeIntervallType.Holiday))
				},
				new() {
					Header = "Urlaub",
					Command = new RelayCommand(()=>Callback(Types.BlockedTimeIntervallType.Vacant))
				},
				new() {
					Header = "Heimarbeitstag",
					Command = new RelayCommand(()=>Callback(Types.BlockedTimeIntervallType.HomeWork))
				},
				new() {
					Header = "Unentschuldigt",
					Command = new RelayCommand(()=>Callback(Types.BlockedTimeIntervallType.NoExcuse))
				},
				new() {
					Header = "Anwesend",
					Command = new RelayCommand(()=>Callback(Types.BlockedTimeIntervallType.None))
				}
			}
		};
		_contextMenu?.Open(this);
	}

	public void OnClick(object? sender, TappedEventArgs e) {
		Console.WriteLine("Click!");
		Point mousePos = e.GetPosition(this);
		//if (IsOutsideGraphArea(mousePos))
		//	return;
		if (DataContext is DayViewModel model) {
			int i = 0;
			Timespan.Types.Models.Task? clickedTask = null;
			//foreach (var task in await model.GetTasksAsync()) {
			//	Rect rect = GetTaskRectanlge(task, GRAPH_CLICK_ADDITIONAL_WIDTH, GRAPH_CLICK_ADDITIONAL_HEIGHT, i);
			//	i++;
			//	if (rect.Contains(mousePos)) {
			//		clickedTask = task;
			//		break;
			//	} else {
			//		continue;
			//	}
			//}
			if (clickedTask != null)
				model.OnTaskClicked(clickedTask);
		}
	}

	public void OnDoubleClick(object? sender, TappedEventArgs args) {
		Console.WriteLine("Double Click!");

	}

	public void OnMouseMoved(object sender, PointerEventArgs args) {
		//Console.WriteLine("Mouse moved!");
		MousePos = args.GetCurrentPoint(this).Position;
		MarkerDragRectangle = new Rect(
			Math.Min(MousePos.X, DragOrigin.X),
			Math.Min(MousePos.Y, DragOrigin.Y),
			Math.Abs(MousePos.X - DragOrigin.X),
			Math.Abs(MousePos.Y - DragOrigin.Y)
		);
		InvalidateVisual();
	}

	public void OnMousePressed(object sender, PointerPressedEventArgs args) {
		Console.WriteLine("Mouse pressed!");
		PointerPoint mousePoint = args.GetCurrentPoint(sender as Control);
		MousePos = mousePoint.Position;
		DragOrigin = mousePoint.Position;
		if (mousePoint.Properties.IsRightButtonPressed) {
			if (!RightMouseDown)
				DragOrigin = MousePos;
			RightMouseDown = true;
			MarkerDragRectangle = new Rect(
				Math.Min(MousePos.X, DragOrigin.X),
				Math.Min(MousePos.Y, DragOrigin.Y),
				Math.Abs(MousePos.X - DragOrigin.X),
				Math.Abs(MousePos.Y - DragOrigin.Y)
			);
		}
		if (mousePoint.Properties.IsLeftButtonPressed)
			LeftMouseDown = true;
		InvalidateVisual();
	}

	public void OnMouseReleased(object sender, PointerReleasedEventArgs args) {
		Console.WriteLine($"mouse released!");
		if (!args.GetCurrentPoint(sender as Control).Properties.IsRightButtonPressed) {
			if (RightMouseDown) {
				ShowReasonContextMenu();
			}
			RightMouseDown = false;
		}
		if (!args.GetCurrentPoint(sender as Control).Properties.IsLeftButtonPressed)
			LeftMouseDown = false;
		InvalidateVisual();
	}
}