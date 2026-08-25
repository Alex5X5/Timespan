namespace Timespan.GUI.Views.Graphs;

using Avalonia.Input;
using Avalonia.Media;

using Timespan.GUI.Types;
using Timespan.Util.Attributes;
using Timespan.Util.Services;

public partial class DayPanelView : GraphPanelViewBase {

	[TranslateMember("Views.Pages.Timer.Labels.Title", "Timer")]
	public string TitleLabelText { get; set; } = "";

	public DayPanelView() : base() {
	}

	protected override int GetTaskRow(ObservableTask task) {
		return 0;
	}

	protected override int GetTaskColummn(ObservableTask task) {
		return 0;
	}
	
	protected override void DrawTimeline(DrawingContext context) {
		Pen timeLine = new(new SolidColorBrush(Colors.Black));
		Pen hintLine = new(new SolidColorBrush(Color.FromArgb(255, 170, 170, 170)));
		Brush textBrush = new SolidColorBrush(Colors.Gray);
		//context.DrawLine(timeLine, new(PaddingX, Bounds.Height - PaddingY), new(Bounds.Width - PaddingX, Bounds.Height - PaddingY));
		double textSize = ArialHeightToPt(PaddingY, 0.7);
		for (int i = 0; i < 25; i++) {
			double xPos = XAxisSegmentSize * i + PaddingX;
			context.DrawLine(hintLine, new Point(xPos, Bounds.Height - PaddingY), new Point(xPos, PaddingY));
			//context.DrawLine(timeLine, new Point(xPos, Bounds.Height - PaddingY), new Point(xPos, Bounds.Height - PaddingY * 1.5));
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
}