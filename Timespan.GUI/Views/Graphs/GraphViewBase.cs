using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;

using CommunityToolkit.Mvvm.Input;

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

using Timespan.GUI.Interfaces;
using Timespan.GUI.Types;
using Timespan.GUI.Types.Events;
using Timespan.GUI.ViewModels.Graphs;
using Timespan.Util.Services;

using SharedTypes = Timespan.Types.Models;

namespace Timespan.GUI.Views.Graphs;

public abstract partial class GraphViewBase : UserControl {

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
	protected double YAxisSegmentSize => GraphAreaHeight / (YAxisSegmentCount * 1.5) * YAxisSegmentCount;

	#endregion

	#region styled properties

	public static readonly StyledProperty<ObservableCollection<ObservableTask>> TasksProperty =
		AvaloniaProperty.Register<GraphViewBase, ObservableCollection<ObservableTask>>(nameof(Tasks), []);

	public static readonly StyledProperty<long> IntervalStartSecondsProperty =
		AvaloniaProperty.Register<GraphViewBase, long>(nameof(MarkedColumns), 0);

	public static readonly StyledProperty<long> IntervalStopSecondsProperty =
		AvaloniaProperty.Register<GraphViewBase, long>(nameof(BlockedColumns), 0);

	public static readonly StyledProperty<ObservableCollection<ObservableBool>> MarkedRowsProperty =
		AvaloniaProperty.Register<GraphViewBase, ObservableCollection<ObservableBool>>(nameof(MarkedRows), []);

	public static readonly StyledProperty<ObservableCollection<ObservableBool>> BlockedRowsProperty =
		AvaloniaProperty.Register<GraphViewBase, ObservableCollection<ObservableBool>>(nameof(BlockedRows), []);

	public static readonly StyledProperty<ObservableCollection<ObservableBool>> MarkedColumnsProperty =
		AvaloniaProperty.Register<GraphViewBase, ObservableCollection<ObservableBool>>(nameof(MarkedColumns), []);

	public static readonly StyledProperty<ObservableCollection<ObservableBool>> BlockedColumnsProperty =
		AvaloniaProperty.Register<GraphViewBase, ObservableCollection<ObservableBool>>(nameof(BlockedColumns), []);

	public static readonly StyledProperty<int> GraphsPaddingProperty =
		AvaloniaProperty.Register<GraphViewBase, int>(nameof(BlockedColumns), 0);

	public static readonly StyledProperty<int> XAxisSegmentDurationProperty =
		AvaloniaProperty.Register<GraphViewBase, int>(nameof(XAxisSegmentDuration), 0);

	public static readonly StyledProperty<int> XAxisSegmentCountProperty =
		AvaloniaProperty.Register<GraphViewBase, int>(nameof(XAxisSegmentCount), 0);

	public static readonly StyledProperty<int> YAxisSegmentCountProperty =
		AvaloniaProperty.Register<GraphViewBase, int>(nameof(YAxisSegmentCount), 0);

	public static readonly StyledProperty<IRelayCommand> LoadCommandProperty =
		AvaloniaProperty.Register<GraphViewBase, IRelayCommand>(nameof(LoadCommand), new RelayCommand(() => { }));

	public static readonly StyledProperty<IRelayCommand> ClickedCommandProperty =
		AvaloniaProperty.Register<GraphViewBase, IRelayCommand>(nameof(ClickedCommand), new RelayCommand(() => { }));

	public static readonly StyledProperty<IRelayCommand<TaskClickedEventArgs>> TaskClickedCommandProperty =
		AvaloniaProperty.Register<GraphViewBase, IRelayCommand<TaskClickedEventArgs>>(
			nameof(TaskClickedCommand),
			new RelayCommand<TaskClickedEventArgs>(args => { }));

	public static readonly StyledProperty<IRelayCommand<MousePressedEventArgs>> MousePressedCommandProperty =
		AvaloniaProperty.Register<GraphViewBase, IRelayCommand<MousePressedEventArgs>>(
			nameof(MousePressedCommand),
			new RelayCommand<MousePressedEventArgs>(args => { }));

	public static readonly StyledProperty<IRelayCommand<MouseReleasedEventArgs>> MouseReleasedCommandProperty =
		AvaloniaProperty.Register<GraphViewBase, IRelayCommand<MouseReleasedEventArgs>>(
			nameof(MouseReleasedCommand),
			new RelayCommand<MouseReleasedEventArgs>(args => { }));

	public static readonly StyledProperty<IRelayCommand<MouseDraggingEventArgs>> MouseDraggingCommandProperty =
		AvaloniaProperty.Register<GraphViewBase, IRelayCommand<MouseDraggingEventArgs>>(
			nameof(MouseDraggingCommand),
			new RelayCommand<MouseDraggingEventArgs>(args => { }));

	public static readonly StyledProperty<IRelayCommand<MissingContextClickedEventArgs>> MissingContextClickedCommandProperty =
		AvaloniaProperty.Register<GraphViewBase, IRelayCommand<MissingContextClickedEventArgs>>(
			nameof(MissingContextClickedCommand),
			new RelayCommand<MissingContextClickedEventArgs>(args => { }));

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

	public IRelayCommand LoadCommand {
		get => GetValue(LoadCommandProperty);
		set => SetValue(LoadCommandProperty, value);
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

	static GraphViewBase() {
		AffectsRender<GraphViewBase>(IntervalStartSecondsProperty);
		AffectsRender<GraphViewBase>(IntervalStopSecondsProperty);
		AffectsRender<GraphViewBase>(MarkedRowsProperty);
		AffectsRender<GraphViewBase>(BlockedRowsProperty);
		AffectsRender<GraphViewBase>(MarkedColumnsProperty);
		AffectsRender<GraphViewBase>(BlockedColumnsProperty);
		AffectsRender<GraphViewBase>(GraphsPaddingProperty);
	}

	public GraphViewBase() {
		TranslatorService.Singleton.TranslateAnnotatedMembers(this);
		//this.Bind(MarkedColumnsProperty,new Binding(nameof(MarkedColumns)));

		AddHandler(TappedEvent, OnClickBase);
		AddHandler(DoubleTappedEvent, OnDoubleClickBase);
		AddHandler(PointerMovedEvent, OnMouseMovedBase);
		AddHandler(PointerPressedEvent, OnMousePressedBase);
		AddHandler(PointerReleasedEvent, OnMouseReleasedBase);
		AddHandler(LoadedEvent, OnLoadBase);
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

	#region rendering

	public override void Render(DrawingContext context) {
		if (!IsVisible)
			return;
		DrawBackground(context);
		DrawTimeline(context);
		DrawColumnMarkers(context);
		DrawMouseRectangle(context);
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
	protected abstract void DrawColumnMarkers(DrawingContext context);
	protected abstract void DrawMouseRectangle(DrawingContext context);

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

	private void OnClickBase(object? sender, TappedEventArgs e) {
		Point mousePos = e.GetPosition(this);
		if (DataContext is IGraphViewModel model) {
			//int i = 0;
			SharedTypes.Task? clickedTask = null;
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
			if (clickedTask != null) {
				var clickArgs = new TaskClickedEventArgs(clickedTask);
				TaskClickedCommand.Execute(clickArgs);
			}
		}

	}

	private void OnDoubleClickBase(object? sender, TappedEventArgs args) {
		Console.WriteLine("Double Click!");

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
			var dragArgs = new MouseDraggingEventArgs(MarkerDragRectangle, GraphAreaWidth, PaddingX);
			MouseDraggingCommand.Execute(dragArgs);
		}
		InvalidateVisual();
	}

	private void OnMousePressedBase(object? sender, PointerPressedEventArgs args) {
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
		var pressedArgs = new MousePressedEventArgs(LeftMouseDown, RightMouseDown);
		MousePressedCommand.Execute(pressedArgs);
		InvalidateVisual();
	}

	private void OnMouseReleasedBase(object? sender, PointerReleasedEventArgs args) {
		Console.WriteLine($"mouse released!");
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
	}

	#endregion
}
