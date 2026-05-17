namespace Timespan.Types.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Project {

	[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public string Name { set; get; }
}
