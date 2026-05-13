namespace Hourglass.GUI.Views.Components.GraphPanels;

using Avalonia.Media;
using Avalonia;

public partial class DayGraphPanelView : GraphPanelViewBase {

    public DayGraphPanelView() : base() {
		base.InitializeComponent();
		InitializeComponent();
	}
	
	protected override void DrawTimeline(DrawingContext context) {
		Pen timeLine = new(new SolidColorBrush(Colors.Black));
		Pen hintLine = new(new SolidColorBrush(Color.FromArgb(255, 170, 170, 170)));
		Brush textBrush = new SolidColorBrush(Colors.Gray);
		context.DrawLine(timeLine, new(PADDING_X, Bounds.Height - PADDING_Y), new(Bounds.Width - PADDING_X, Bounds.Height - PADDING_Y));
        double textSize = Math.Round(PADDING_Y * 0.7, 1);
        for (int i = 0; i < 25; i++) {
            double xPos = X_AXIS_SEGMENT_SIZE * i + PADDING_X;
            context.DrawLine(hintLine, new Point(xPos, Bounds.Height - PADDING_Y), new Point(xPos, PADDING_Y));
			context.DrawLine(timeLine, new Point(xPos, Bounds.Height - PADDING_Y), new Point(xPos, Bounds.Height - PADDING_Y - TIMELINE_MARK_HEIGHT));
			var formattedText = new FormattedText(
				Convert.ToString(i) + ":00",
				System.Globalization.CultureInfo.CurrentCulture,
				FlowDirection.LeftToRight,
				new Typeface("Arial"),
				textSize,
				textBrush
			);
            Point textPos = new(xPos - formattedText.Width / 2.0, Bounds.Height - (PADDING_Y * 0.85));
            context.DrawText(
				formattedText,
				textPos
			);
		}
	}
}