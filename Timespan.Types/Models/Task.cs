namespace Timespan.Types.Models;

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Avalonia.Media;

public class Task {

	[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public long Id { set; get; } = 0;
    public string description { set; get; } = "";
	
	public bool running { set; get; } = false;
	public long start { set; get; } = 0;
	public long finish { set; get; } = 0;
	
	public Worker? owner { set; get; }
	public Project? project { set; get; }
	public Ticket? ticket { set; get;}
	
	public byte displayColorRed { set; get; } = 255;
	public byte displayColorGreen { set; get; } = 255;
	public byte displayColorBlue { set; get; } = 255;
	
	public BlockedTimeIntervallType blocksTime { set; get; } = BlockedTimeIntervallType.None;

    [NotMapped]
    public long Duration => finish - start;

    [NotMapped]
	public DateTime StartDateTime {
		set => start = value.Ticks / TimeSpan.TicksPerSecond;
		get => DateTime.MinValue.AddSeconds(start);
	}

	[NotMapped]
	public DateTime FinishDateTime {
		set => finish = value.Ticks / TimeSpan.TicksPerSecond;
		get => DateTime.MinValue.AddSeconds(finish);
	}

	[NotMapped]
	public Color DisplayColor {
		set {
			displayColorRed = value.R;
			displayColorGreen = value.G;
			displayColorBlue = value.B;
		}
		get => new Color(255, displayColorRed, displayColorGreen, displayColorBlue);
	}

	public Task Clone() =>
		(Task)MemberwiseClone();
}