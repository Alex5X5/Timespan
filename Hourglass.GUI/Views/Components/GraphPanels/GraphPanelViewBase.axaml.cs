namespace Hourglass.GUI.Views.Components.GraphPanels;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

using CommunityToolkit.Mvvm.Input;

using Hourglass.Database;
using Hourglass.Database.Models;
using Hourglass.GUI.ViewModels.Components.GraphPanels;
using Hourglass.Util.Services;


public abstract partial class GraphPanelViewBase : ViewBase {

	#region fields

	private GraphPanelViewModelBase Model => (DataContext as GraphPanelViewModelBase) ?? new DayGraphPanelViewModel();

	protected int MAX_TASKS => Model.MAX_TASKS;

	protected int GRAPH_CLICK_ADDITIONAL_WIDTH => Model.GRAPH_CLICK_ADDITIONAL_WIDTH;
	protected int GRAPH_CLICK_ADDITIONAL_HEIGHT => Model.GRAPH_CLICK_ADDITIONAL_HEIGHT;

    public int GRAPH_MINIMAL_WIDTH => Model.GRAPH_MINIMAL_WIDTH;
	protected int GRAPH_CORNER_RADIUS => Model.GRAPH_CORNER_RADIUS;

    public long TIME_INTERVALL_START_SECONDS => Model.TIME_INTERVALL_START_SECONDS;
    public long TIME_INTERVALL_FINISH_SECONDS => Model.TIME_INTERVALL_FINISH_SECONDS;
    public long TIME_INTERVALL_DURATION => Model.TIME_INTERVALL_DURATION;
    public long X_AXIS_SEGMENT_DURATION => Model.X_AXIS_SEGMENT_DURATION;

	protected int X_AXIS_SEGMENT_COUNT => Model.X_AXIS_SEGMENT_COUNT;
	protected int Y_AXIS_SEGMENT_COUNT => Model.Y_AXIS_SEGMENT_COUNT;

	protected double TASK_DESCRIPTION_GRAPH_SPAGE => Model.TASK_DESCRIPTION_GRAPH_SPACE;
	protected double TASK_DESCRIPTION_FONT_SIZE => Model.TASK_DESCRIPTION_FONT_SIZE;

	protected int TASK_GRAPH_COLUMN_COUNT => Model.TASK_GRAPH_COLUMN_COUNT;

    public double X_AXIS_SEGMENT_SIZE => GRAPH_AREA_WIDTH / X_AXIS_SEGMENT_COUNT;
	public double Y_AXIS_SEGMENT_SIZE => GRAPH_AREA_HEIGTH / (Y_AXIS_SEGMENT_COUNT * 1.5) * Model.TASK_GRAPH_COLUMN_COUNT;

    public double PADDING_X => Bounds.Width * GraphPanelViewModelBase.PADDING_X_WEIGHT / (GraphPanelViewModelBase.GRAPH_AREA_X_WEIGHT + 2 * GraphPanelViewModelBase.PADDING_X_WEIGHT);
    public double PADDING_Y => Bounds.Height * GraphPanelViewModelBase.PADDING_Y_WEIGHT / (GraphPanelViewModelBase.GRAPH_AREA_Y_WEIGHT + 2 * GraphPanelViewModelBase.PADDING_Y_WEIGHT);

    public double GRAPH_AREA_WIDTH => Bounds.Width - 2 * PADDING_X;
    public double GRAPH_AREA_HEIGTH => Bounds.Height - 2 * PADDING_Y;
	
	public const double TIMELINE_MARK_HEIGHT = 7;
	public const int MAX_TASK_DESCRIPTION_CHARS = 30;

	private bool RightMouseDown = false;
	private bool LeftMouseDown = false;

	private Rect MarkerDragRectangle;
	private Point DragOrigin;
	private Point MousePos = new(0.0,0.0);

	private ContextMenu? _contextMenu;

    #endregion fields

    public GraphPanelViewBase() : base() {
		//InitializeComponent();
    }

	protected static double ArialHeightToPt(double height, double x = 1) =>
		Math.Round(Math.Log(3 * height + 1) * 3 * x + height * 0.3 * x, 2);

	private bool IsOutsideGraphArea(Point p) {
		if (p.X < PADDING_X)
			return true;
		if (p.X > Bounds.Width - PADDING_X)
			return true;
		if (p.Y < PADDING_Y)
			return true;
		if (p.Y > Bounds.Height - PADDING_Y)
			return true;
		return false;
	}

	public Rect GetTaskRectanlge(Task task, double additionalWidth, double additionalHeight, int i) {
		double proportion = GRAPH_AREA_WIDTH/TIME_INTERVALL_DURATION;
		double graphPosX = (task.start - TIME_INTERVALL_START_SECONDS) * proportion + PADDING_X;
		long duration = task.running ? DateTimeService.ToSeconds(DateTime.Now) - task.start : task.finish - task.start;
		double graphLength = duration * proportion;
		double width = Math.Max(graphLength, GRAPH_MINIMAL_WIDTH) + additionalWidth * 2;
		Rect res = new(
			graphPosX - additionalWidth,
			Y_AXIS_SEGMENT_SIZE * (i % (MAX_TASKS / TASK_GRAPH_COLUMN_COUNT)) * 1.5 - additionalHeight + PADDING_Y,
			width,
			Y_AXIS_SEGMENT_SIZE + additionalHeight * 2
		);
		return res;
	}

	protected abstract void DrawTimeline(DrawingContext context);

	private void DrawColumnMarkers(DrawingContext context) {
		Brush markedBrush = new SolidColorBrush(Color.FromArgb(100, 100, 100, 200));
		Brush blockedBrush = new SolidColorBrush(Color.FromArgb(100, 80, 80, 80));
		double x = PADDING_X + 2;
		double y = PADDING_Y + 2;
		double width = X_AXIS_SEGMENT_SIZE - 4;
		double height = GRAPH_AREA_HEIGTH - 5;
		for (int i = 0; i < X_AXIS_SEGMENT_COUNT; i++) {
			if (Model.MarkedColumns[i])
				context.FillRectangle(markedBrush, new Rect(x, y, width, height));
			if (Model.BlockedColumns[i])
				context.FillRectangle(blockedBrush, new Rect(x, y, width, height));
			x += X_AXIS_SEGMENT_SIZE;
		}
	}

	private void DrawTaskGraph(DrawingContext context, Database.Models.Task task, int i) {
		Rect rect = GetTaskRectanlge(task, 0, 0, i);
		Color gradientStartColor = Color.FromArgb(255, task.displayColorRed, task.displayColorGreen, task.displayColorBlue);
		Color gradientFinishColor = Color.FromArgb(20, task.displayColorRed, task.displayColorGreen, task.displayColorBlue);
		
		Brush brush = task.running
			? new LinearGradientBrush() {
				StartPoint = new RelativePoint(0.0, 0.5, RelativeUnit.Relative),
				EndPoint = new RelativePoint(1.0, 0.5, RelativeUnit.Relative),
				GradientStops = {
					new GradientStop(gradientStartColor, 0.0),
					new GradientStop(gradientFinishColor, 1.0)
				}
			}
			: new SolidColorBrush(task.DisplayColor);
		double r = Math.Min(GRAPH_CORNER_RADIUS, rect.Height / 4);
		r = Math.Min(r, rect.Width /2);
		RectangleGeometry rrect = new(rect) { RadiusX = r, RadiusY = r };
		context.DrawGeometry(brush, null, rrect);
		DrawTaskDescriptionStub(context, task, rect);
	}

	private void DrawTaskDescriptionStub(DrawingContext context, Database.Models.Task task, Rect taskRect) {
		var formattedText = new FormattedText(
			task.description.Length <= MAX_TASK_DESCRIPTION_CHARS ? task.description : task.description[..MAX_TASK_DESCRIPTION_CHARS] + "...",
			System.Globalization.CultureInfo.CurrentCulture,
			FlowDirection.LeftToRight,
			new Typeface("Arial"),
			Math.Max(2.0, ArialHeightToPt(Y_AXIS_SEGMENT_SIZE)),
			new SolidColorBrush(Colors.Black)
		);
		Point p = new(taskRect.X - formattedText.Width - TASK_DESCRIPTION_GRAPH_SPAGE, taskRect.Y + taskRect.Height / 2 - formattedText.Height / 2);
		context.DrawText(formattedText, p);
	}

	private void DrawMouseRectangle(DrawingContext context) {
		if (RightMouseDown) {
			Brush borderBrush = new SolidColorBrush(Color.FromArgb(200, 100, 100, 100));
			Brush areaBrush = new SolidColorBrush(Color.FromArgb(150, 150, 220, 255));
			Pen pen = new Pen(borderBrush, 2);
			context.FillRectangle(areaBrush, MarkerDragRectangle);
			context.DrawRectangle(pen, MarkerDragRectangle);
		}
	}

	public override void Render(DrawingContext context) {
		if (!IsVisible)
			return;
		var brush = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
		context.FillRectangle(brush, new Rect(Bounds.X + PADDING_X, Bounds.Y + PADDING_Y, Bounds.Width - 2 * PADDING_X, Bounds.Height - 2 * PADDING_Y));
		DrawTimeline(context);
		DrawColumnMarkers(context);
		DrawMouseRectangle(context);
        base.Render(context);
	}

	private void ShowReasonContextMenu() {

		void Callback(BlockedTimeIntervallType reason) {
			(DataContext as GraphPanelViewModelBase)?.OnMissingContextMenuClicked(reason);
			InvalidateVisual();
		}

		_contextMenu = new ContextMenu();
		_contextMenu.ItemsSource = new List<MenuItem>() {
			new() { 
				Header = "Krank",
				Command = new RelayCommand(()=>Callback(BlockedTimeIntervallType.Sick))
			},
			new() {
				Header = "Feiertag",
				Command = new RelayCommand(()=>Callback(BlockedTimeIntervallType.Holiday))
			},
			new() {
				Header = "Urlaub",
				Command = new RelayCommand(()=>Callback(BlockedTimeIntervallType.Vacant))
			},
			new() {
				Header = "Heimarbeitstag",
				Command = new RelayCommand(()=>Callback(BlockedTimeIntervallType.HomeWork))
			},
			new() {
				Header = "Unentschuldigt",
				Command = new RelayCommand(()=>Callback(BlockedTimeIntervallType.NoExcuse))
			},
			new() {
				Header = "Anwesend",
				Command = new RelayCommand(()=>Callback(BlockedTimeIntervallType.None))
			}
		};
		_contextMenu?.Open(this);
	}
}