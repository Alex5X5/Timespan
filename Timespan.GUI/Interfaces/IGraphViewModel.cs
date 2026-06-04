using System.Threading.Tasks;

namespace Timespan.GUI.Interfaces;

public interface IGraphViewModel {
	
	public Task<List<Timespan.Types.Models.Task>> GetTasksAsync();
}
