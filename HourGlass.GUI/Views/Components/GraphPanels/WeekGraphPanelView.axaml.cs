namespace Hourglass.GUI.Views.Components.GraphPanels;

using Avalonia.Media;
using Avalonia;

using Hourglass.GUI.ViewModels.Components.GraphPanels;

public partial class WeekGraphPanelView : GraphPanelViewBase {

    public WeekGraphPanelView() : base() {
		base.InitializeComponent();
		InitializeComponent();
	}

	protected override void DrawTimeline(DrawingContext context) {
        Brush weekedDayBackground = new SolidColorBrush(Color.FromArgb(255, 200, 200, 200));
        Brush todayBackgroundColor = new SolidColorBrush(Color.FromArgb(255, 237, 166, 166));
		Pen timeLine = new(new SolidColorBrush(Colors.Black));
		Pen hintLine = new(new SolidColorBrush(Color.FromArgb(255, 170, 170, 170)));
		Brush textBrush = new SolidColorBrush(Colors.Gray);
		string[] days = ["Mo", "Di", "Mi", "Do", "Fr", "Sa", "So"];
		for (int i = 0; i < 7; i++) {
			double xPos = X_AXIS_SEGMENT_SIZE * i + PADDING_X;
			if (i % 7 == 5 | i % 7 == 6)
				context.FillRectangle(weekedDayBackground, new(xPos+1, PADDING_Y, X_AXIS_SEGMENT_SIZE-2, Bounds.Height - (2 * PADDING_Y)));
			if (i + 1 == (int)DateTime.Today.DayOfWeek)
				if(DateTimeService.FloorWeek((DataContext as WeekGraphPanelViewModel)!.cacheService.SelectedDay) == DateTimeService.FloorWeek(DateTime.Now))
				context.FillRectangle(todayBackgroundColor, new(xPos+1, PADDING_Y, X_AXIS_SEGMENT_SIZE-2, Bounds.Height - (2 * PADDING_Y)));
		}
		context.DrawLine(timeLine, new(PADDING_X, Bounds.Height - PADDING_Y), new(Bounds.Width - PADDING_X, Bounds.Height - PADDING_Y));
		double textSize = Math.Round(PADDING_Y * 0.7, 1);
		for (int i = 0; i < 8; i++) {
            double xPos = X_AXIS_SEGMENT_SIZE * i + PADDING_X;
            context.DrawLine(hintLine, new Point(xPos, Bounds.Height - PADDING_Y), new Point(xPos, PADDING_Y));
			context.DrawLine(timeLine, new Point(xPos, Bounds.Height - PADDING_Y), new Point(xPos, Bounds.Height - PADDING_Y - TIMELINE_MARK_HEIGHT));
            if (i < 7) {
				var formattedText = new FormattedText(
					days[i],
					System.Globalization.CultureInfo.CurrentCulture,
					FlowDirection.LeftToRight,
					new Typeface("Arial"),
                    textSize,
					textBrush
				);
				Point textPos = new(xPos + X_AXIS_SEGMENT_SIZE / 2.0 - formattedText.Width / 2.0, Bounds.Height - (PADDING_Y * 0.85) );
				context.DrawText(
					formattedText,
					textPos
				);
			}
		}
    }
}