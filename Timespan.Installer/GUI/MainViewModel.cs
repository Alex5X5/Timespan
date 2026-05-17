namespace Hourglass.Installer.GUI;

using Timespan.Util.Services;
using ReactiveUI;

public partial class MainViewModel : ReactiveObject {

    private double _progressValue;
    public double ProgressBarValue {
        get => _progressValue;
        set => this.RaiseAndSetIfChanged(ref _progressValue, value);
    }

    public InstallerService? installerService;

    public MainViewModel() {
		ProgressBarValue = 100;
    }

    public MainViewModel(InstallerService installerService) {
        this.installerService = installerService;
        DoWorkAsync();
    }

    public async Task DoWorkAsync() {
        if (installerService == null)
            return;
        var progress = new Progress<int>( value => ProgressBarValue = value);
        await InstallerService.InstallAsync(progress);
    }
}

