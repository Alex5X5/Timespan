namespace Timespan.GUI.Views.Graphs;

using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;

using CommunityToolkit.Mvvm.Input;

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

using Timespan.GUI.Types;
using Timespan.GUI.ViewModels.Graphs;
using Timespan.Util.Attributes;
using Timespan.Util.Services;

using Types = Timespan.Types.Models;

public partial class WeekView : UserControl {

	#region fields

	[TranslateMember("Views.Pages.Timer.Labels.Title", "Timer")]
	public string TitleLabelText { get; set; } = "";

	public const int MAX_TASK_DESCRIPTION_CHARS = 30;

	private bool RightMouseDown = false;
	private bool LeftMouseDown = false;

	private Rect MarkerDragRectangle;
	private Point DragOrigin;
	private Point MousePos = new(0.0, 0.0);

	private ContextMenu? _contextMenu;

	#endregion fields

	#region styledProperties

	private const int MAX_COLUMN_COUNT = 7;

	public static readonly StyledProperty<long> IntervalStartSecondsProperty =
		AvaloniaProperty.Register<DayView, long>(nameof(MarkedColumns), 0);

	public static readonly StyledProperty<long> IntervalStopSecondsProperty =
		AvaloniaProperty.Register<DayView, long>(nameof(BlockedColumns), 0);

	public static readonly StyledProperty<ObservableCollection<ObservableBool>> MarkedColumnsProperty =
		AvaloniaProperty.Register<DayView, ObservableCollection<ObservableBool>>(nameof(MarkedColumns), []);

	public static readonly StyledProperty<ObservableCollection<ObservableBool>> BlockedColumnsProperty =
		AvaloniaProperty.Register<DayView, ObservableCollection<ObservableBool>>(nameof(BlockedColumns), []);

	public static readonly StyledProperty<int> GraphsPaddingProperty =
		AvaloniaProperty.Register<DayView, int>(nameof(BlockedColumns), 0);

	public static readonly StyledProperty<int> XAxisSegmentDurationProperty =
		AvaloniaProperty.Register<DayView, int>(nameof(XAxisSegmentDuration), 0);

	public static readonly StyledProperty<int> XAxisSegmentCountProperty =
		AvaloniaProperty.Register<DayView, int>(nameof(XAxisSegmentCount), 0);

	public static readonly StyledProperty<int> YAxisSegmentCountProperty =
		AvaloniaProperty.Register<DayView, int>(nameof(YAxisSegmentCount), 0);

	public ObservableCollection<ObservableBool> MarkedColumns {
		get => GetValue(MarkedColumnsProperty);
		set => SetValue(MarkedColumnsProperty, value);
	}

	public ObservableCollection<ObservableBool> BlockedColumns {
		get => GetValue(BlockedColumnsProperty);
		set => SetValue(BlockedColumnsProperty, value);
	}

	public long IntervalStartSeconds {
		get => GetValue(XAxisSegmentCountProperty);
		set => SetValue(XAxisSegmentCountProperty, value);
	}

	public long IntervalStopSeconds {
		get => GetValue(YAxisSegmentCountProperty);
		set => SetValue(YAxisSegmentCountProperty, value);
	}

	public int XAxisSegmentDuration {
		get => GetValue(XAxisSegmentDurationProperty);
		set => SetValue(XAxisSegmentDurationProperty, value);
	}

	public int XAxisSegmentCount {
		get => GetValue(XAxisSegmentCountProperty);
		set => SetValue(XAxisSegmentCountProperty, value);
	}

	public int YAxisSegmentCount {
		get => GetValue(YAxisSegmentCountProperty);
		set => SetValue(YAxisSegmentCountProperty, value);
	}

	#endregion styledProperties

	#region sizeFields
	private const double GRAPH_AREA_X_WEIGHT = 28;
	private const double GRAPH_AREA_Y_WEIGHT = 28;
	private const double PADDING_X_WEIGHT = 1;
	private const double PADDING_Y_WEIGHT = 1;

	private double PaddingX => Bounds.Width * PADDING_X_WEIGHT / (GRAPH_AREA_X_WEIGHT + 2 * PADDING_X_WEIGHT);
	private double PaddingY => Bounds.Height * PADDING_Y_WEIGHT / (GRAPH_AREA_Y_WEIGHT + 2 * PADDING_Y_WEIGHT);

	private double GraphAreaWidth => Bounds.Width - 2 * PaddingX;
	private double GraphAreaHeight => Bounds.Height - 2 * PaddingY;

	private double XAxisSegmentSize => GraphAreaWidth / XAxisSegmentCount;
	private double YAxisSegmentSize => GraphAreaHeight / (YAxisSegmentCount * 1.5) * YAxisSegmentCount;
	#endregion

	static WeekView() {
		AffectsRender<DayView>(MarkedColumnsProperty);
		AffectsRender<DayView>(BlockedColumnsProperty);
		AffectsRender<DayView>(GraphsPaddingProperty);
	}


	public WeekView() {
		TranslatorService.Singleton.TranslateAnnotatedMembers(this);
		//this.Bind(MarkedColumnsProperty,new Binding(nameof(MarkedColumns)));

		//InitializeComponent();
		AddHandler(TappedEvent, OnClick);
		AddHandler(DoubleTappedEvent, OnDoubleClick);
		AddHandler(PointerMovedEvent, OnMouseMoved);
		AddHandler(PointerPressedEvent, OnMousePressed);
		AddHandler(PointerReleasedEvent, OnMouseReleased);
		AddHandler(LoadedEvent, OnLoad);
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
		//if (!IsVisible)
		//	return;
		DrawBackground(context);
		DrawTimeline(context);
		DrawColumnMarkers(context);
		DrawMouseRectangle(context);
	}

	private void DrawBackground(DrawingContext context) {
		var background = new SolidColorBrush(Color.FromArgb(255, 217, 217, 217));
		Pen pen = new(background, 0);
		RectangleGeometry rrect = new(Bounds) {
			RadiusX = 20,
			RadiusY = 20
		};
		context.DrawGeometry(background, pen, rrect);
	}

	private void DrawColumnMarkers(DrawingContext context) {
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

	private void DrawTimeline(DrawingContext context) {
		Pen timeLine = new(new SolidColorBrush(Colors.Black));
		Pen hintLine = new(new SolidColorBrush(Color.FromArgb(255, 170, 170, 170)));
		Brush textBrush = new SolidColorBrush(Colors.Gray);
		context.DrawLine(timeLine, new(PaddingX, Bounds.Height - PaddingY), new(Bounds.Width - PaddingX, Bounds.Height - PaddingY));
		double textSize = Math.Round(PaddingY * 0.7, 1);
		for (int i = 0; i < 25; i++) {
			double xPos = XAxisSegmentSize * i + PaddingX;
			context.DrawLine(hintLine, new Point(xPos, Bounds.Height - PaddingY), new Point(xPos, PaddingY));
			context.DrawLine(timeLine, new Point(xPos, Bounds.Height - PaddingY), new Point(xPos, Bounds.Height - PaddingY * 1.5));
			var formattedText = new FormattedText(
				Convert.ToString(i) + ":00",
				System.Globalization.CultureInfo.CurrentCulture,
				FlowDirection.LeftToRight,
				new Typeface("Arial"),
				textSize,
				textBrush
			);
			Point textPos = new(xPos - formattedText.Width / 2.0, Bounds.Height - (PaddingY * 0.85));
			context.DrawText(
				formattedText,
				textPos
			);
		}
	}

	private void DrawMouseRectangle(DrawingContext context) {
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
			(DataContext as DayViewModel)?.OnMissingContextMenuClicked(reason);
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

	protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change) {
		base.OnPropertyChanged(change);
		if (change.Property == MarkedColumnsProperty) {
			if (change.OldValue is ObservableCollection<ObservableBool> oldList) {
				foreach (ObservableBool item in oldList)
					item.PropertyChanged -= OnBoolValueChanged;
				oldList.CollectionChanged -= OnBoolListChanged;
			}
			if (change.NewValue is ObservableCollection<ObservableBool> newList) {
				foreach (ObservableBool item in newList)
					item.PropertyChanged += OnBoolValueChanged;
				newList.CollectionChanged += OnBoolListChanged;
			}
		} else if (change.Property == BlockedColumnsProperty) {
			if (change.OldValue is ObservableCollection<ObservableBool> oldList) {
				foreach (ObservableBool item in oldList)
					item.PropertyChanged -= OnBoolValueChanged;
				oldList.CollectionChanged -= OnBoolListChanged;
			}
			if (change.NewValue is ObservableCollection<ObservableBool> newList) {
				foreach (ObservableBool item in newList)
					item.PropertyChanged += OnBoolValueChanged;
				newList.CollectionChanged += OnBoolListChanged;
			}
		}
	}

	private void OnBoolListChanged(object? sender, NotifyCollectionChangedEventArgs e) {
		if (e.OldItems != null)
			foreach (ObservableBool item in e.OldItems)
				item.PropertyChanged -= OnBoolValueChanged;
		if (e.NewItems != null)
			foreach (ObservableBool item in e.NewItems)
				item.PropertyChanged += OnBoolValueChanged;
	}

	private void OnBoolValueChanged(object? sender, PropertyChangedEventArgs e) {
		InvalidateVisual();
	}

	public void OnClick(object? sender, TappedEventArgs e) {
		//Console.WriteLine("Click!");
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
		(DataContext as DayViewModel)?.OnClicked();
	}

	public void OnDoubleClick(object? sender, TappedEventArgs args) {
		Console.WriteLine("Double Click!");

	}

	public void OnMouseMoved(object? sender, PointerEventArgs args) {
		//Console.WriteLine("Mouse moved!");
		MousePos = args.GetCurrentPoint(this).Position;
		MarkerDragRectangle = new Rect(
			Math.Min(MousePos.X, DragOrigin.X),
			Math.Min(MousePos.Y, DragOrigin.Y),
			Math.Abs(MousePos.X - DragOrigin.X),
			Math.Abs(MousePos.Y - DragOrigin.Y)
		);
		if (RightMouseDown) {
			(DataContext as DayViewModel)?.OnMouseDragging(MarkerDragRectangle, GraphAreaWidth, PaddingX);
		}
		InvalidateVisual();
	}

	public void OnMousePressed(object? sender, PointerPressedEventArgs args) {
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
		(DataContext as DayViewModel)?.OnMousePressed(LeftMouseDown, RightMouseDown);
		InvalidateVisual();
	}

	public void OnMouseReleased(object? sender, PointerReleasedEventArgs args) {
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

	public void OnLoad(object? sender, RoutedEventArgs args) {
		(DataContext as DayViewModel)?.OnLoad();
	}
}