namespace Timespan.Types.Models;

using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;


public class Ticket {

	[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public long id { set; get; }
	public string name { set; get; }
	public string description { set; get; }
	public Project project { set; get; }
}
