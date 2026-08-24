namespace Timespan.GUI.Views.Graphs;

using Avalonia.Input;
using Avalonia.Interactivity;

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

using Timespan.GUI.Generators.Attributes;
using Timespan.GUI.Helpers;
using Timespan.GUI.Types;
using Timespan.GUI.Types.Events;
using Timespan.Util.Services;

using SharedTypes = Timespan.Types.Models;

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
	private const double PADDING_X_WEIGHT = 2;
	private const double PADDING_Y_WEIGHT = 2;

	protected double PaddingX => Bounds.Width * PADDING_X_WEIGHT / (GRAPH_AREA_X_WEIGHT + 2 * PADDING_X_WEIGHT);
	protected double PaddingY => Bounds.Height * PADDING_Y_WEIGHT / (GRAPH_AREA_Y_WEIGHT + 2 * PADDING_Y_WEIGHT);

	protected double TaskDescriptionPadding => 5;

	protected double GraphAreaWidth => Bounds.Width - 2 * PaddingX;
	protected double GraphAreaHeight => Bounds.Height - 2 * PaddingY;

	protected double XAxisSegmentSize => GraphAreaWidth / XAxisSegmentCount;
	protected double YAxisSegmentSize => GraphAreaHeight / YAxisSegmentCount;

	#endregion

	#region styled properties

	public static readonly StyledProperty<ObservableCollection<ObservableTask>> TasksProperty =
		AvaloniaProperty.Register<GraphPanelViewBase, ObservableCollection<ObservableTask>>(nameof(Tasks), []);

	[BasicStyledProperty<GraphPanelViewBase>]
	private double extraClickSize;
	[BasicStyledProperty<GraphPanelViewBase>]
	private double minimalGraphWidth;
	[BasicStyledProperty<GraphPanelViewBase>]
	private long intervalStartSeconds = 0;
	[BasicStyledProperty<GraphPanelViewBase>]
	private long intervalStopSeconds = 0;

	public long IntervalDuration => IntervalStopSeconds - IntervalStartSeconds;


	[BasicStyledProperty<GraphPanelViewBase>]
	private int xAxisSegmentCount = 1;
	[BasicStyledProperty<GraphPanelViewBase>]
	private int yAxisSegmentCount = 1;
	[BasicStyledProperty<GraphPanelViewBase>]
	private int xAxisSegmentDuration = 1;

	public int YAxisSegmentDuration {
		get => XAxisSegmentDuration * XAxisSegmentCount;
	}

	[BasicStyledProperty<GraphPanelViewBase>]
	private int maxCellTasks;

	[BasicStyledProperty<GraphPanelViewBase>]
	private bool fillColumn = false;

	[BasicDirectProperty<GraphPanelViewBase>]
	private bool suspendRendering = false;

	[BasicStyledProperty<GraphPanelViewBase>]
	private IRelayCommand loadCommand;
	[BasicStyledProperty<GraphPanelViewBase>]
	private IRelayCommand unloadCommand;

	[BasicStyledProperty<GraphPanelViewBase>]
	private IRelayCommand clickedCommand;
	[BasicStyledProperty<GraphPanelViewBase>]
	private IRelayCommand<DoubleClickedEventArgs> doubleClickedCommand;
	[BasicStyledProperty<GraphPanelViewBase>]
	private IRelayCommand<TaskClickedEventArgs> taskClickedCommand;
	[BasicStyledProperty<GraphPanelViewBase>]
	private IRelayCommand<MousePressedEventArgs> mousePressedCommand;
	[BasicStyledProperty<GraphPanelViewBase>]
	private IRelayCommand<MouseReleasedEventArgs> mouseReleasedCommand;
	[BasicStyledProperty<GraphPanelViewBase>]
	private IRelayCommand<MouseDraggingEventArgs> mouseDraggingCommand;
	[BasicStyledProperty<GraphPanelViewBase>]
	private IRelayCommand<MissingContextClickedEventArgs> missingContextClickedCommand;

	#endregion

	#region styled property members

	public ObservableCollection<ObservableTask> Tasks {
		get => GetValue(TasksProperty);
		set => SetValue(TasksProperty, value);
	}

	[BasicStyledProperty<GraphPanelViewBase>]
	private ObservableBool[,] isMarked;

	[BasicStyledProperty<GraphPanelViewBase>]
	private ObservableBool[,] isBlocked;

	[BasicStyledProperty<GraphPanelViewBase>]
	private bool[,] isToday;

	[BasicStyledProperty<GraphPanelViewBase>]
	private DateTime selectedDate;

	#endregion

	static GraphPanelViewBase() {
		
		AffectsRender<GraphPanelViewBase>(BoundsProperty);
	}

	public GraphPanelViewBase() {
		TranslatorService.Singleton.TranslateAnnotatedMembers(this);

		AddHandler(TappedEvent, OnClickBase);
		AddHandler(DoubleTappedEvent, OnDoubleClickBase);
		AddHandler(PointerMovedEvent, OnMouseMovedBase);
		AddHandler(PointerPressedEvent, OnMousePressedBase);
		AddHandler(PointerReleasedEvent, OnMouseReleasedBase);
		AddHandler(LoadedEvent, OnLoadBase);
		AddHandler(UnloadedEvent, OnUnloadBase);
	}

	private bool IsOutsideGraphArea(Point p) {
		if (p.X < PaddingX)
			return true;
		if (p.X > Bounds.Width - PaddingX)
			return true;
		if (p.Y < PaddingY)
			return true;
		if (p.Y > Bounds.Height - PaddingY)
			return true;
		return false;
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
					Header = TranslatorService.Singleton["Absence.Sick"],
					Command = new RelayCommand(()=>Callback(SharedTypes.BlockedTimeIntervallType.Sick))
				},
				new() {
					Header = TranslatorService.Singleton["Absence.Holiday"],
					Command = new RelayCommand(()=>Callback(SharedTypes.BlockedTimeIntervallType.Holiday))
				},
				new() {
					Header = TranslatorService.Singleton["Absence.Vacant"],
					Command = new RelayCommand(()=>Callback(SharedTypes.BlockedTimeIntervallType.Vacant))
				},
				new() {
					Header = TranslatorService.Singleton["Absence.HomeWork"],
					Command = new RelayCommand(()=>Callback(SharedTypes.BlockedTimeIntervallType.HomeWork))
				},
				new() {
					Header = TranslatorService.Singleton["Absence.NoExcuse"],
					Command = new RelayCommand(()=>Callback(SharedTypes.BlockedTimeIntervallType.NoExcuse))
				},
				new() {
					Header = TranslatorService.Singleton["Absence.None"],
					Command = new RelayCommand(()=>Callback(SharedTypes.BlockedTimeIntervallType.None))
				}
			}
		};
		_contextMenu?.Open(this);
	}

	#region rendering

	protected virtual int GetTaskRow(ObservableTask task) {
		long offset = task.Start - IntervalStartSeconds;
		if (offset < 0)
			return 0;
		int res = (int)Math.Floor((double)offset / (double)YAxisSegmentDuration);
		return res;
	}

	protected virtual int GetTaskColummn(ObservableTask task) {
		long offset = task.Start - IntervalStartSeconds;
		if (offset < 0)
			return 0;
		int res = (int)Math.Floor(((double)offset % (double)YAxisSegmentDuration) / (double)XAxisSegmentDuration);
		return res;
	}

	protected virtual Rect GetTaskRectangle(ObservableTask task, int[,] cellTaskCount, double additionalWidth, double additionalHeight) {
		int row = GetTaskRow(task);
		int column = GetTaskColummn(task);
		if (row < 0)
			return new Rect();
		if (row > YAxisSegmentCount - 1)
			return new Rect();
		if (column < 0)
			return new Rect();
		if (column > XAxisSegmentCount - 1)
			return new Rect();
		double graphPosX = column * XAxisSegmentSize;
		double graphPosY = row * YAxisSegmentSize;
		double width = additionalWidth * 2;
		double height = additionalHeight * 2;
		graphPosX += PaddingX;
		graphPosX -= additionalWidth;
		graphPosY += PaddingY;
		graphPosY -= additionalHeight;
		graphPosY += YAxisSegmentSize * 0.05;
		graphPosY += cellTaskCount[row, column] * YAxisSegmentSize / MaxCellTasks * 1.5;
		height += YAxisSegmentSize / MaxCellTasks;
		if (FillColumn) {
			graphPosX += XAxisSegmentSize * 0.05;
			width += XAxisSegmentSize * 0.9;
		} else {
			double proportion = GraphAreaWidth / IntervalDuration;
			long startOffset = task.Start - IntervalStartSeconds;
			graphPosX += (double)startOffset * proportion;
			long duration = task.Running ? DateTimeService.ToSeconds(DateTime.Now) - task.Start : task.Finish - task.Start;
			double graphLength = duration * proportion;
			width += Math.Max(graphLength, MinimalGraphWidth);
		}
		Rect res = new(
			graphPosX,
			graphPosY,
			width,
			height
		);
		cellTaskCount[row, column]++;
		return res;
	}

	private static Color GetTaskDescriptionTextColor(ObservableTask task) {
		int average = (int)task.DisplayColorRed + (int)task.DisplayColorGreen + (int)task.DisplayColorBlue;
		average /= 3;
		if (average > 120) {
			return Color.FromArgb(255, 0, 0, 0);
		} else {
			return Color.FromArgb(255, 255, 255, 255);
		}
	}

	protected static Brush GetTaskGraphBrush(ObservableTask task) {
		if (task.Running) {
			Color gradientStartColor = Color.FromArgb(255, task.DisplayColorRed, task.DisplayColorGreen, task.DisplayColorBlue);
			Color gradientFinishColor = Color.FromArgb(20, task.DisplayColorRed, task.DisplayColorGreen, task.DisplayColorBlue);
			return new LinearGradientBrush() {
				StartPoint = new RelativePoint(0.0, 0.5, RelativeUnit.Relative),
				EndPoint = new RelativePoint(1.0, 0.5, RelativeUnit.Relative),
				GradientStops = {
					new GradientStop(gradientStartColor, 0.0),
					new GradientStop(gradientFinishColor, 1.0)
				}
			};
		} else {
			return new SolidColorBrush(task.DisplayColor);
		}
	}

	protected double ArialHeightToPt(double height, double x = 1) =>
		Math.Round(height * 0.7 * x, 1);

	public override void Render(DrawingContext context) {
		if (SuspendRendering)
			return;
		DrawBackground(context);
		DrawColumnMarkers(context);
		DrawTimeline(context);
		DrawTasks(context);
		DrawMouseRectangle(context);
	}

	private void DrawTasks(DrawingContext context) {
		int[,] cells = new int[YAxisSegmentCount, XAxisSegmentCount];
		for (int row = 0; row < cells.GetLength(0); row++)
			for (int column = 0; column < cells.GetLength(1); column++)
				cells[row, column] = 0;
		foreach (var task in Tasks ?? [])
			DrawTaskGraph(context, task, cells);
	}

	protected virtual void DrawTaskGraph(DrawingContext context, ObservableTask task, int[,] cellTaskCount) {
		Rect rect = GetTaskRectangle(task, cellTaskCount, 0, 0);
		Brush brush = GetTaskGraphBrush(task);
		double r = Math.Min(10, rect.Height / 4);
		r = Math.Min(r, rect.Width / 2);
		RectangleGeometry rrect = new(rect) {
			RadiusX = r,
			RadiusY = r
		};
		context.DrawGeometry(brush, null, rrect);
		DrawTaskDescriptionStub(context, task, rect);
	}

	protected virtual void DrawBackground(DrawingContext context) {
		var background = new SolidColorBrush(Color.FromArgb(255, 235, 235, 235));
		RectangleGeometry rrect = new(Bounds) {
			RadiusX = 20,
			RadiusY = 20
		};
		context.DrawGeometry(background, null, rrect);
	}

	private static Rect GetTextBoundsForFill(Rect graphBounds) {
		double padding = Math.Min(graphBounds.Width * 0.05, graphBounds.Height * 0.05);
		double x = graphBounds.X + padding;
		double y = graphBounds.Y + padding;
		double width = graphBounds.Width - 2 * padding;
		double height = graphBounds.Height - 2 * padding;
		return new Rect(x, y, width, height);
	}

	protected virtual void DrawTaskDescriptionStub(DrawingContext context, ObservableTask task, Rect taskRect) {
		var formattedText = new FormattedText(
			TaskHelper.GetTitleString(task.Description, true),
			System.Globalization.CultureInfo.CurrentCulture,
			FlowDirection.LeftToRight,
			new Typeface("Arial"),
			Math.Max(2.0, ArialHeightToPt(taskRect.Height, 0.9)),
			null
		);
		double xPos = 0;
		if (FillColumn) {
			Rect textBounds = GetTextBoundsForFill(taskRect);
			formattedText.SetForegroundBrush(new SolidColorBrush(GetTaskDescriptionTextColor(task)));
			using (context.PushClip(textBounds)) {
				double _xPos = textBounds.X;
				xPos += (textBounds.Width / 2.0);
				xPos -= (textBounds.Width / 2.0);
				double _yPos = taskRect.Y + ((taskRect.Height / 2.0) - (formattedText.Height / 2.0));
				context.DrawText(formattedText, new Point(_xPos, _yPos));
			}
			return;
		} else {
			if (formattedText.Width < taskRect.Width) {
				xPos = taskRect.X;
				xPos += (taskRect.Width / 2.0);
				xPos -= (formattedText.Width / 2.0);
				formattedText.SetForegroundBrush(new SolidColorBrush(GetTaskDescriptionTextColor(task)));
			} else if (taskRect.X - formattedText.Width > PaddingX) {
				xPos = taskRect.X - formattedText.Width;
				xPos -= TaskDescriptionPadding;
				formattedText.SetForegroundBrush(new SolidColorBrush(Colors.Black));
			} else {
				xPos = taskRect.X + taskRect.Width;
				xPos += TaskDescriptionPadding;
				formattedText.SetForegroundBrush(new SolidColorBrush(Colors.Black));
			}
		}
		double yPos = taskRect.Y + ((taskRect.Height / 2.0) - (formattedText.Height / 2.0));
		context.DrawText(formattedText, new Point(xPos, yPos));
		return;
	}

	protected abstract void DrawTimeline(DrawingContext context);

	private void DrawColumnMarkers(DrawingContext context) {
		Brush markedBrush = new SolidColorBrush(Color.FromArgb(120, 120, 120, 240));
		Brush blockedBrush = new SolidColorBrush(Color.FromArgb(255, 255, 80, 80));
		Brush todayBrush = new SolidColorBrush(Color.FromArgb(120, 130, 130, 130));
		double y = PaddingY + 2;
		double width = XAxisSegmentSize - 4;
		double height = YAxisSegmentSize - 4;
		for (int row = 0; row < YAxisSegmentCount; row++) {
			double x = PaddingX + 2;
			for (int column = 0; column < XAxisSegmentCount; column++) {
				if (IsToday[row, column])
					context.FillRectangle(todayBrush, new Rect(x, y, width, height));
				if (IsBlocked[row, column].Value)
					context.FillRectangle(blockedBrush, new Rect(x, y, width, height));
				if (IsMarked[row, column].Value)
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
		if (change.Property == IsMarkedProperty) {
			if (change.OldValue is ObservableBool[,] oldList) {
				foreach (ObservableBool item in oldList)
					item.PropertyChanged -= OnBoolValueChanged;
			}
			if (change.NewValue is ObservableBool[,] newList) {
				foreach (ObservableBool item in newList)
					item.PropertyChanged += OnBoolValueChanged;
			}
			InvalidateVisual();
		} else if (change.Property == IsBlockedProperty) {
			if (change.OldValue is ObservableBool[,] oldList) {
				foreach (ObservableBool item in oldList)
					item.PropertyChanged -= OnBoolValueChanged;
			}
			if (change.NewValue is ObservableBool[,] newList) {
				foreach (ObservableBool item in newList)
					item.PropertyChanged += OnBoolValueChanged;
			}
			InvalidateVisual();
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
		} else if (change.Property == XAxisSegmentCountProperty | change.Property == YAxisSegmentCountProperty) {
			//Console.WriteLine($"resizing isToday array to ({YAxisSegmentCount}, {XAxisSegmentCount})");
			IsToday = ArrayHelper.ResizeArray(IsToday, YAxisSegmentCount, XAxisSegmentCount, false);
		}
	}

	private void OnBoolListChanged(object? sender, NotifyCollectionChangedEventArgs e) {
		if (e.OldItems != null)
			foreach (ObservableBool item in e.OldItems)
				item.PropertyChanged -= OnBoolValueChanged;
		if (e.NewItems != null)
			foreach (ObservableBool item in e.NewItems)
				item.PropertyChanged += OnBoolValueChanged;
		InvalidateVisual();
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
		int[,] cells = new int[YAxisSegmentCount, XAxisSegmentCount];
		for (int row = 0; row < YAxisSegmentCount; row++)
			for (int column = 0; column < XAxisSegmentCount; column++)
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
		var point = args.GetPosition(this);
		int row = (int)Math.Floor((point.Y - PaddingY) / YAxisSegmentSize);
		int col = (int)Math.Floor((point.X - PaddingX) / XAxisSegmentSize);
		var args_ = new DoubleClickedEventArgs(row, col);
		if(DoubleClickedCommand.CanExecute(args_))
			DoubleClickedCommand.Execute(args_);
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
		if (RightMouseDown) {
			var dragArgs = new MouseDraggingEventArgs(MarkerDragRectangle, GraphAreaWidth, GraphAreaHeight, PaddingX, PaddingY);
			MouseDraggingCommand.Execute(dragArgs);
			double yPos = PaddingY;
			for (int row = 0; row < YAxisSegmentCount; row++) {
				double xPos = PaddingX;
				for (int column = 0; column < XAxisSegmentCount; column++) {
					var segment = new Rect(xPos, yPos, XAxisSegmentSize, YAxisSegmentSize);
					var intersects = MarkerDragRectangle.Intersects(segment);
					IsMarked[row, column].Value = intersects;
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
				if(!IsOutsideGraphArea(MousePos))
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
