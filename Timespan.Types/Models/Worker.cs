namespace Timespan.Types.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Worker {

	[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public long id { set; get; } = 0;
	public string name { set; get; } = "";
}
