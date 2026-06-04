using System.Threading.Tasks;

using Timespan.Database.Services.Interfaces;
using Timespan.Util.Services;

namespace Timespan.GUI.ViewModels.Graphs;

public partial class MonthViewModel : GraphViewModelBase {

	public MonthViewModel() : this(null, null) {

	}

	public MonthViewModel(GUI.Services.CacheService cacheService, IHourglassDbService dbService) : base(cacheService, dbService, 1, 24, 3600) {

	}

	public override string GetDateString() {
		string month = CacheService.SelectedDay.Month switch {
			1 => TranslatorService.Singleton["Months.January"],
			2 => TranslatorService.Singleton["Months.February"],
			3 => TranslatorService.Singleton["Months.March"],
			4 => TranslatorService.Singleton["Months.April"],
			5 => TranslatorService.Singleton["Months.May"],
			6 => TranslatorService.Singleton["Months.June"],
			7 => TranslatorService.Singleton["Months.July"],
			8 => TranslatorService.Singleton["Months.August"],
			9 => TranslatorService.Singleton["Months.September"],
			10 => TranslatorService.Singleton["Months.October"],
			11 => TranslatorService.Singleton["Months.November"],
			12 => TranslatorService.Singleton["Months.December"],
			_ => ""
		};
		return $"{month} {CacheService.SelectedDay.Year}";
	}

	public async override Task<List<Timespan.Types.Models.Task>> GetTasksAsync() {
		return [];
	}
}
