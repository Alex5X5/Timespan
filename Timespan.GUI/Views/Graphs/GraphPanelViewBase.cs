using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;

using CommunityToolkit.Mvvm.Input;

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

using Timespan.GUI.Types;
using Timespan.GUI.Types.Events;
using Timespan.Util.Services;

using SharedTypes = Timespan.Types.Models;

namespace Timespan.GUI.Views.Graphs;

public abstract partial class GraphPanelViewBase : UserControl {

	#region fields

	protected bool RightMouseDown = false;
	protected bool LeftMouseDown = false;

	protected Rect MarkerDragRectangle;
	protected Point DragOrigin;
	protected Point MousePos = new(0.0, 0.0);

	private ContextMenu? _contextMenu;

	#endregion

	#region size fields

	private const double GRAPH_AREA_X_WEIGHT = 28;
	private const double GRAPH_AREA_Y_WEIGHT = 28;
	private const double PADDING_X_WEIGHT = 1;
	private const double PADDING_Y_WEIGHT = 1;

	protected double PaddingX => Bounds.Width * PADDING_X_WEIGHT / (GRAPH_AREA_X_WEIGHT + 2 * PADDING_X_WEIGHT);
	protected double PaddingY => Bounds.Height * PADDING_Y_WEIGHT / (GRAPH_AREA_Y_WEIGHT + 2 * PADDING_Y_WEIGHT);

	protected double GraphAreaWidth => Bounds.Width - 2 * PaddingX;
	protected double GraphAreaHeight => Bounds.Height - 2 * PaddingY;

	protected double XAxisSegmentSize => GraphAreaWidth / XAxisSegmentCount;
	protected double YAxisSegmentSize => GraphAreaHeight / YAxisSegmentCount;

	#endregion

	#region styled properties

	public static readonly StyledProperty<ObservableCollection<ObservableTask>> TasksProperty =
		AvaloniaProperty.Register<GraphPanelViewBase, ObservableCollection<ObservableTask>>(nameof(Tasks), []);

	public static readonly StyledProperty<ObservableCollection<ObservableBool>> MarkedRowsProperty =
		AvaloniaProperty.Register<GraphPanelViewBase, ObservableCollection<ObservableBool>>(nameof(MarkedRows), []);

	public static readonly StyledProperty<ObservableCollection<ObservableBool>> BlockedRowsProperty =
		AvaloniaProperty.Register<GraphPanelViewBase, ObservableCollection<ObservableBool>>(nameof(BlockedRows), []);

	public static readonly StyledProperty<ObservableCollection<ObservableBool>> MarkedColumnsProperty =
		AvaloniaProperty.Register<GraphPanelViewBase, ObservableCollection<ObservableBool>>(nameof(MarkedColumns), []);

	public static readonly StyledProperty<ObservableCollection<ObservableBool>> BlockedColumnsProperty =
		AvaloniaProperty.Register<GraphPanelViewBase, ObservableCollection<ObservableBool>>(nameof(BlockedColumns), []);

	public static readonly StyledProperty<bool[,]> IsTodayProperty =
		AvaloniaProperty.Register<GraphPanelViewBase, bool[,]>(nameof(IsToday), new bool[0,0]);

	public static readonly StyledProperty<double> ExtraClickSizeProperty =
		AvaloniaProperty.Register<GraphPanelViewBase, double>(nameof(ExtraClickSize), 0);

	public static readonly StyledProperty<double> MinimalWidthProperty =
		AvaloniaProperty.Register<GraphPanelViewBase, double>(nameof(MinimalGraphWidth), 0);

	public static readonly StyledProperty<int> MaxTasksProperty =
		AvaloniaProperty.Register<GraphPanelViewBase, int>(nameof(MaxTasks), 0);


	public static readonly StyledProperty<long> IntervalStartSecondsProperty =
		AvaloniaProperty.Register<GraphPanelViewBase, long>(nameof(IntervalStartSeconds), 0);

	public static readonly StyledProperty<long> IntervalStopSecondsProperty =
		AvaloniaProperty.Register<GraphPanelViewBase, long>(nameof(IntervalStopSeconds), 0);

	public static readonly StyledProperty<int> XAxisSegmentDurationProperty =
		AvaloniaProperty.Register<GraphPanelViewBase, int>(nameof(XAxisSegmentDuration), 0);

	public static readonly StyledProperty<int> XAxisSegmentCountProperty =
		AvaloniaProperty.Register<GraphPanelViewBase, int>(nameof(XAxisSegmentCount), 0);

	public static readonly StyledProperty<int> YAxisSegmentCountProperty =
		AvaloniaProperty.Register<GraphPanelViewBase, int>(nameof(YAxisSegmentCount), 0);

	public static readonly StyledProperty<int> TaskGridRowCountProperty =
		AvaloniaProperty.Register<GraphPanelViewBase, int>(nameof(TaskGridRowCount), 0);

	public static readonly StyledProperty<int> TaskGridColumnCountProperty =
		AvaloniaProperty.Register<GraphPanelViewBase, int>(nameof(TaskGridColumnCount), 0);

	public static readonly StyledProperty<int> MaxCellTasksProperty =
		AvaloniaProperty.Register<GraphPanelViewBase, int>(nameof(MaxCellTasks), 0);


	public static readonly StyledProperty<IRelayCommand> LoadCommandProperty =
		AvaloniaProperty.Register<GraphPanelViewBase, IRelayCommand>(nameof(LoadCommand), new RelayCommand(() => { }));

	public static readonly StyledProperty<IRelayCommand> UnloadCommandProperty =
		AvaloniaProperty.Register<GraphPanelViewBase, IRelayCommand>(nameof(UnloadCommand), new RelayCommand(() => { }));

	public static readonly StyledProperty<IRelayCommand> ClickedCommandProperty =
		AvaloniaProperty.Register<GraphPanelViewBase, IRelayCommand>(nameof(ClickedCommand), new RelayCommand(() => { }));

	public static readonly StyledProperty<IRelayCommand<TaskClickedEventArgs>> TaskClickedCommandProperty =
		AvaloniaProperty.Register<GraphPanelViewBase, IRelayCommand<TaskClickedEventArgs>>(
			nameof(TaskClickedCommand),
			new RelayCommand<TaskClickedEventArgs>(args => { }));

	public static readonly StyledProperty<IRelayCommand<MousePressedEventArgs>> MousePressedCommandProperty =
		AvaloniaProperty.Register<GraphPanelViewBase, IRelayCommand<MousePressedEventArgs>>(
			nameof(MousePressedCommand),
			new RelayCommand<MousePressedEventArgs>(args => { }));

	public static readonly StyledProperty<IRelayCommand<MouseReleasedEventArgs>> MouseReleasedCommandProperty =
		AvaloniaProperty.Register<GraphPanelViewBase, IRelayCommand<MouseReleasedEventArgs>>(
			nameof(MouseReleasedCommand),
			new RelayCommand<MouseReleasedEventArgs>(args => { }));

	public static readonly StyledProperty<IRelayCommand<MouseDraggingEventArgs>> MouseDraggingCommandProperty =
		AvaloniaProperty.Register<GraphPanelViewBase, IRelayCommand<MouseDraggingEventArgs>>(
			nameof(MouseDraggingCommand),
			new RelayCommand<MouseDraggingEventArgs>(args => { }));

	public static readonly StyledProperty<IRelayCommand<MissingContextClickedEventArgs>> MissingContextClickedCommandProperty =
		AvaloniaProperty.Register<GraphPanelViewBase, IRelayCommand<MissingContextClickedEventArgs>>(
			nameof(MissingContextClickedCommand),
			new RelayCommand<MissingContextClickedEventArgs>(args => { }));

	#endregion

	#region styled property members

	public ObservableCollection<ObservableTask> Tasks {
		get => GetValue(TasksProperty);
		set => SetValue(TasksProperty, value);
	}

	public ObservableCollection<ObservableBool> MarkedRows {
		get => GetValue(MarkedRowsProperty);
		set => SetValue(MarkedRowsProperty, value);
	}

	public ObservableCollection<ObservableBool> BlockedRows {
		get => GetValue(BlockedRowsProperty);
		set => SetValue(BlockedRowsProperty, value);
	}

	public ObservableCollection<ObservableBool> MarkedColumns {
		get => GetValue(MarkedColumnsProperty);
		set => SetValue(MarkedColumnsProperty, value);
	}

	public ObservableCollection<ObservableBool> BlockedColumns {
		get => GetValue(BlockedColumnsProperty);
		set => SetValue(BlockedColumnsProperty, value);
	}

	public bool[,] IsToday {
		get => GetValue(IsTodayProperty);
		set => SetValue(IsTodayProperty, value);
	}

	public double ExtraClickSize {
		get => GetValue(ExtraClickSizeProperty);
		set => SetValue(ExtraClickSizeProperty, value);
	}

	public double MinimalGraphWidth {
		get => GetValue(MinimalWidthProperty);
		set => SetValue(MinimalWidthProperty, value);
	}

	public int MaxTasks {
		get => GetValue(MaxTasksProperty);
		set => SetValue(MaxTasksProperty, value);
	}

	public long IntervalStartSeconds {
		get => GetValue(IntervalStartSecondsProperty);
		set => SetValue(IntervalStartSecondsProperty, value);
	}

	public long IntervalStopSeconds {
		get => GetValue(IntervalStopSecondsProperty);
		set => SetValue(IntervalStopSecondsProperty, value);
	}

	public long IntervalDuration => IntervalStopSeconds - IntervalStartSeconds;

	public int XAxisSegmentDuration {
		get => GetValue(XAxisSegmentDurationProperty);
		set => SetValue(XAxisSegmentDurationProperty, value);
	}

	public int YAxisSegmentDuration {
		get => XAxisSegmentDuration * XAxisSegmentCount;
	}

	public int XAxisSegmentCount {
		get => GetValue(XAxisSegmentCountProperty);
		set => SetValue(XAxisSegmentCountProperty, value);
	}

	public int YAxisSegmentCount {
		get => GetValue(YAxisSegmentCountProperty);
		set => SetValue(YAxisSegmentCountProperty, value);
	}

	public int TaskGridRowCount {
		get => GetValue(TaskGridRowCountProperty);
		set => SetValue(TaskGridRowCountProperty, value);
	}

	public int TaskGridColumnCount {
		get => GetValue(TaskGridColumnCountProperty);
		set => SetValue(TaskGridColumnCountProperty, value);
	}
	
	public long TaskGridColumnDuration {
		get => IntervalDuration / (long)TaskGridColumnCount;
	}

	public int MaxCellTasks {
		get => GetValue(MaxCellTasksProperty);
		set => SetValue(MaxCellTasksProperty, value);
	}

	public IRelayCommand LoadCommand {
		get => GetValue(LoadCommandProperty);
		set => SetValue(LoadCommandProperty, value);
	}

	public IRelayCommand UnloadCommand {
		get => GetValue(UnloadCommandProperty);
		set => SetValue(UnloadCommandProperty, value);
	}

	public IRelayCommand ClickedCommand {
		get => GetValue(ClickedCommandProperty);
		set => SetValue(ClickedCommandProperty, value);
	}

	public IRelayCommand<TaskClickedEventArgs> TaskClickedCommand {
		get => GetValue(TaskClickedCommandProperty);
		set => SetValue(TaskClickedCommandProperty, value);
	}

	public IRelayCommand<MousePressedEventArgs> MousePressedCommand {
		get => GetValue(MousePressedCommandProperty);
		set => SetValue(MousePressedCommandProperty, value);
	}

	public IRelayCommand<MouseReleasedEventArgs> MouseReleasedCommand {
		get => GetValue(MouseReleasedCommandProperty);
		set => SetValue(MouseReleasedCommandProperty, value);
	}

	public IRelayCommand<MouseDraggingEventArgs> MouseDraggingCommand {
		get => GetValue(MouseDraggingCommandProperty);
		set => SetValue(MouseDraggingCommandProperty, value);
	}

	public IRelayCommand<MissingContextClickedEventArgs> MissingContextClickedCommand {
		get => GetValue(MissingContextClickedCommandProperty);
		set => SetValue(MissingContextClickedCommandProperty, value);
	}

	#endregion

	static GraphPanelViewBase() {
		
		AffectsRender<GraphPanelViewBase>(BoundsProperty);
		AffectsRender<GraphPanelViewBase>(TasksProperty);
		//AffectsRender<GraphPanelViewBase>(MarkedRowsProperty);
		//AffectsRender<GraphPanelViewBase>(BlockedRowsProperty);
		//AffectsRender<GraphPanelViewBase>(MarkedColumnsProperty);
		//AffectsRender<GraphPanelViewBase>(BlockedColumnsProperty);
	}

	public GraphPanelViewBase() {
		TranslatorService.Singleton.TranslateAnnotatedMembers(this);
		//this.Bind(MarkedColumnsProperty,new Binding(nameof(MarkedColumns)));

		AddHandler(TappedEvent, OnClickBase);
		AddHandler(DoubleTappedEvent, OnDoubleClickBase);
		AddHandler(PointerMovedEvent, OnMouseMovedBase);
		AddHandler(PointerPressedEvent, OnMousePressedBase);
		AddHandler(PointerReleasedEvent, OnMouseReleasedBase);
		AddHandler(LoadedEvent, OnLoadBase);
		AddHandler(UnloadedEvent, OnUnloadBase);
	}

	protected void ShowReasonContextMenu() {

		void Callback(SharedTypes.BlockedTimeIntervallType reason) {
			var clickedArgs = new MissingContextClickedEventArgs(reason);
			MissingContextClickedCommand.Execute(clickedArgs);
			InvalidateVisual();
		}

		_contextMenu = new() {
			ItemsSource = new List<MenuItem>() {
				new() {
					Header = "Krank",
					Command = new RelayCommand(()=>Callback(SharedTypes.BlockedTimeIntervallType.Sick))
				},
				new() {
					Header = "Feiertag",
					Command = new RelayCommand(()=>Callback(SharedTypes.BlockedTimeIntervallType.Holiday))
				},
				new() {
					Header = "Urlaub",
					Command = new RelayCommand(()=>Callback(SharedTypes.BlockedTimeIntervallType.Vacant))
				},
				new() {
					Header = "Heimarbeitstag",
					Command = new RelayCommand(()=>Callback(SharedTypes.BlockedTimeIntervallType.HomeWork))
				},
				new() {
					Header = "Unentschuldigt",
					Command = new RelayCommand(()=>Callback(SharedTypes.BlockedTimeIntervallType.NoExcuse))
				},
				new() {
					Header = "Anwesend",
					Command = new RelayCommand(()=>Callback(SharedTypes.BlockedTimeIntervallType.None))
				}
			}
		};
		_contextMenu?.Open(this);
	}

	protected virtual int GetTaskRow(ObservableTask task) {
		long offset = task.Start - IntervalStartSeconds;
		if (offset < 0)
			return 0;
		return (int)Math.Floor((double)offset / (double)YAxisSegmentDuration);
	}

	protected virtual int GetTaskColummn(ObservableTask task) {
		long offset = task.Start - IntervalStartSeconds;
		if (offset < 0)
			return 0;
		return (int)Math.Floor(((double)offset % (double)YAxisSegmentDuration) / (double)XAxisSegmentDuration);
	}

	protected virtual Rect GetTaskRectangle(ObservableTask task, int[,] cellTaskCount, double additionalWidth, double additionalHeight) {
		int row = GetTaskRow(task);
		int column = GetTaskColummn(task);
		double proportion = GraphAreaWidth / IntervalDuration;
		double graphPosX = column * XAxisSegmentSize;
		graphPosX += PaddingX;
		graphPosX -= additionalWidth;
		long startOffset = task.Start - IntervalStartSeconds;
		graphPosX += ((double)startOffset % (double)TaskGridColumnDuration) * proportion;
		double graphPosY = row * YAxisSegmentSize;
		graphPosY += PaddingY;
		graphPosY += cellTaskCount[row, column] * YAxisSegmentSize / MaxCellTasks * 1.5;
		graphPosY -= additionalHeight;
		long duration = task.Running ? DateTimeService.ToSeconds(DateTime.Now) - task.Start : task.Finish - task.Start;
		double graphLength = duration * proportion;
		double width = Math.Max(graphLength, MinimalGraphWidth) + additionalWidth * 2;
		double height = YAxisSegmentSize / MaxCellTasks + additionalHeight * 2;
		Rect res = new(
			graphPosX,
			graphPosY,
			width,
			height
		);
		cellTaskCount[row, column] ++;
		return res;
	}

	#region rendering

	public override void Render(DrawingContext context) {
		DrawBackground(context);
		DrawTimeline(context);
		DrawTodayMarker(context);
		DrawTasks(context);
		DrawColumnMarkers(context);
		DrawMouseRectangle(context);
	}

	private void DrawTasks(DrawingContext context) {
		int[,] cells = new int[TaskGridRowCount, TaskGridColumnCount];
		for (int row = 0; row < TaskGridRowCount; row++)
			for (int column = 0; column < TaskGridColumnCount; column++)
				cells[row, column] = 0;
		foreach (var task in Tasks ?? [])
			DrawTaskGraph(context, task, cells);
	}

	private void DrawTaskGraph(DrawingContext context, ObservableTask task, int[,] cellTaskCount) {
		Rect rect = GetTaskRectangle(task, cellTaskCount, 0, 0);

		Brush brush;
		if (task.Running) {
			Color gradientStartColor = Color.FromArgb(255, task.DisplayColorRed, task.DisplayColorGreen, task.DisplayColorBlue);
			Color gradientFinishColor = Color.FromArgb(20, task.DisplayColorRed, task.DisplayColorGreen, task.DisplayColorBlue);
			brush = new LinearGradientBrush() {
				StartPoint = new RelativePoint(0.0, 0.5, RelativeUnit.Relative),
				EndPoint = new RelativePoint(1.0, 0.5, RelativeUnit.Relative),
				GradientStops = {
					new GradientStop(gradientStartColor, 0.0),
					new GradientStop(gradientFinishColor, 1.0)
				}
			};
		} else {
			brush = new SolidColorBrush(task.DisplayColor);
		}

		double r = Math.Min(10, rect.Height / 4);
		r = Math.Min(r, rect.Width / 2);
		RectangleGeometry rrect = new(rect) {
			RadiusX = r,
			RadiusY = r
		};
		context.DrawGeometry(brush, null, rrect);
		//DrawTaskDescriptionStub(context, task, rect);
	}

	protected virtual void DrawBackground(DrawingContext context) {
		var background = new SolidColorBrush(Color.FromArgb(255, 235, 235, 235));
		Pen pen = new(background, 0);
		RectangleGeometry rrect = new(Bounds) {
			RadiusX = 20,
			RadiusY = 20
		};
		context.DrawGeometry(background, null, rrect);
	}

	protected abstract void DrawTimeline(DrawingContext context);

	private void DrawTodayMarker(DrawingContext context) {
		for (int row = 0; row < YAxisSegmentCount; row++) {
			for (int column = 0; column < XAxisSegmentCount; column++) {
				if (IsToday[row, column]) {
					Brush brush = new SolidColorBrush(Color.FromArgb(255, 170, 170, 170));
					var rect = new Rect(
						PaddingX + column * XAxisSegmentSize + 2,
						PaddingY + row * YAxisSegmentSize + 2,
						XAxisSegmentSize - 4,
						YAxisSegmentSize - 4);
					context.FillRectangle(brush, rect);
				}
			}
		}
	}

	private void DrawColumnMarkers(DrawingContext context) {
		Brush markedBrush = new SolidColorBrush(Color.FromArgb(120, 120, 120, 240));
		Brush blockedBrush = new SolidColorBrush(Color.FromArgb(255, 255, 80, 80));
		double y = PaddingY + 2;
		double width = XAxisSegmentSize - 4;
		double height = YAxisSegmentSize - 4;
		for (int row = 0; row < YAxisSegmentCount; row++) {
			double x = PaddingX + 2;
			for (int column = 0; column < XAxisSegmentCount; column++) {
				if (BlockedRows[row].Value)
					if (BlockedColumns[column].Value)
						context.FillRectangle(blockedBrush, new Rect(x, y, width, height));
				if (MarkedRows[row].Value)
					if (MarkedColumns[column].Value)
						context.FillRectangle(markedBrush, new Rect(x, y, width, height));
				x += XAxisSegmentSize;
			}
			y += YAxisSegmentSize;
		}
	}

	private void DrawMouseRectangle(DrawingContext context) {
		if (RightMouseDown) {
			Brush borderBrush = new SolidColorBrush(Color.FromArgb(200, 100, 100, 100));
			Brush areaBrush = new SolidColorBrush(Color.FromArgb(150, 150, 220, 255));
			Pen borderPen = new Pen(borderBrush, 2);
			context.DrawRectangle(areaBrush, borderPen, MarkerDragRectangle);
		}
	}

	#endregion

	#region events

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
		} else if (change.Property == TasksProperty) {
			if (change.OldValue is ObservableCollection<ObservableTask> oldList) {
				foreach (ObservableTask item in oldList)
					item.PropertyChanged -= OnTaskValueChanged;
				oldList.CollectionChanged -= OnTaskListChanged;
			}
			if (change.NewValue is ObservableCollection<ObservableTask> newList) {
				foreach (ObservableTask item in newList)
					item.PropertyChanged += OnTaskValueChanged;
				newList.CollectionChanged += OnTaskListChanged;
			}
			InvalidateVisual();
		}
	}

	private static T[,] ResizeArray<T>(T[,] original, int rows, int cols) {
		var newArray = new T[rows, cols];
		int minRows = Math.Min(rows, original.GetLength(0));
		int minCols = Math.Min(cols, original.GetLength(1));
		for(int i = 0; i<minRows; i++)
			for(int j = 0; j<minCols; j++)
			   newArray[i, j] = original[i, j];
		return newArray;
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

	private void OnTaskListChanged(object? sender, NotifyCollectionChangedEventArgs e) {
		if (e.OldItems != null)
			foreach (ObservableTask item in e.OldItems)
				item.PropertyChanged -=OnTaskValueChanged;
		if (e.NewItems != null)
			foreach (ObservableTask item in e.NewItems)
				item.PropertyChanged += OnTaskValueChanged;
	}

	private void OnTaskValueChanged(object? sender, PropertyChangedEventArgs e) {
		InvalidateVisual();
	}

	private void OnClickBase(object? sender, TappedEventArgs e) {
		ObservableTask? clickedTask = null;
		Point mousePos = e.GetPosition(this);
		int[,] cells = new int[TaskGridRowCount, TaskGridColumnCount];
		for (int row = 0; row < TaskGridRowCount; row++)
			for (int column = 0; column < TaskGridColumnCount; column++)
				cells[row, column] = 0;
		int i = 0;
		foreach (var task in Tasks) {
			Rect rect = GetTaskRectangle(task, cells, 10, 10);
			i++;
			if (rect.Contains(mousePos)) {
				clickedTask = task;
				break;
			} else {
				continue;
			}
		}
		ClickedCommand.Execute(EventArgs.Empty);
		if (clickedTask != null) {
			var clickArgs = new TaskClickedEventArgs(clickedTask.Value);
			TaskClickedCommand.Execute(clickArgs);
		}
	}

	private void OnDoubleClickBase(object? sender, TappedEventArgs args) {
		//Console.WriteLine("Double Click!");
	}

	private void OnMouseMovedBase(object? sender, PointerEventArgs args) {
		//Console.WriteLine("Mouse moved!");
		MousePos = args.GetCurrentPoint(this).Position;
		MarkerDragRectangle = new Rect(
			Math.Min(MousePos.X, DragOrigin.X),
			Math.Min(MousePos.Y, DragOrigin.Y),
			Math.Abs(MousePos.X - DragOrigin.X),
			Math.Abs(MousePos.Y - DragOrigin.Y)
		);
		foreach (var flag in MarkedRows)
			flag.Value = false;
		foreach (var flag in MarkedColumns)
			flag.Value = false;
		if (RightMouseDown) {
			var dragArgs = new MouseDraggingEventArgs(MarkerDragRectangle, GraphAreaWidth, GraphAreaHeight, PaddingX, PaddingY);
			MouseDraggingCommand.Execute(dragArgs);
			double yPos = PaddingY;
			for (int row = 0; row < YAxisSegmentCount; row++) {
				double xPos = PaddingX;
				for (int column = 0; column < XAxisSegmentCount; column++) {
					var segment = new Rect(xPos, yPos, XAxisSegmentSize, YAxisSegmentSize);
					var intersects = MarkerDragRectangle.Intersects(segment);
					MarkedRows[row].Value |= intersects;
					MarkedColumns[column].Value |= intersects;
					xPos += XAxisSegmentSize;
				}
				yPos += YAxisSegmentSize;
			}
		}
		InvalidateVisual();
	}

	private void OnMousePressedBase(object? sender, PointerPressedEventArgs args) {
		//Console.WriteLine("Mouse pressed!");
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
		var pressedArgs = new MousePressedEventArgs(LeftMouseDown, RightMouseDown);
		MousePressedCommand.Execute(pressedArgs);
		InvalidateVisual();
	}

	private void OnMouseReleasedBase(object? sender, PointerReleasedEventArgs args) {
		//Console.WriteLine($"mouse released!");
		if (!args.GetCurrentPoint(sender as Control).Properties.IsRightButtonPressed) {
			if (RightMouseDown) {
				ShowReasonContextMenu();
			}
			RightMouseDown = false;
		}
		if (!args.GetCurrentPoint(sender as Control).Properties.IsLeftButtonPressed)
			LeftMouseDown = false;

		var releasedArgs = new MouseReleasedEventArgs(LeftMouseDown, RightMouseDown);
		MouseReleasedCommand.Execute(releasedArgs);
		InvalidateVisual();
	}

	private void OnLoadBase(object? sender, RoutedEventArgs args) {
		LoadCommand.Execute(EventArgs.Empty);
		InvalidateVisual();
	}

	private void OnUnloadBase(object? sender, RoutedEventArgs args) {
		UnloadCommand.Execute(EventArgs.Empty);
	}

	#endregion
}
