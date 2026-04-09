using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using SkyFocus.Services;
using SkyFocus.ViewModels;
using SkyFocus.Views;

namespace SkyFocus;

public partial class App : Application
{
    private readonly ServiceProvider _services;
    private Window? _window;

    public App()
    {
        InitializeComponent();
        _services = ConfigureServices();
    }

    public static App CurrentApp => (App)Current;

    public IServiceProvider Services => _services;

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        _window = _services.GetRequiredService<MainWindow>();
        if (_window is MainWindow mainWindow)
        {
            await mainWindow.InitializeAsync();
        }

        _window.Activate();
    }

    private static ServiceProvider ConfigureServices()
    {
        ServiceCollection services = new();

        services.AddSingleton<ITextService, TextService>();
        services.AddSingleton<IStoragePathProvider, PackagedStoragePathProvider>();
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<ISessionRepository, SessionRepository>();
        services.AddSingleton<IReminderService, ReminderService>();
        services.AddSingleton<IBlockerService, BlockerService>();
        services.AddSingleton<IAudioService, AudioService>();
        services.AddSingleton<IFocusSessionService, FocusSessionService>();
        services.AddSingleton<IStatisticsService, StatisticsService>();
        services.AddSingleton<INavigationService, NavigationService>();

        services.AddSingleton<ShellViewModel>();
        services.AddSingleton<HomePageViewModel>();
        services.AddSingleton<FocusPageViewModel>();
        services.AddSingleton<HistoryPageViewModel>();
        services.AddSingleton<SoundsPageViewModel>();
        services.AddSingleton<SettingsPageViewModel>();

        services.AddSingleton<HomePage>();
        services.AddSingleton<FocusPage>();
        services.AddSingleton<HistoryPage>();
        services.AddSingleton<SoundsPage>();
        services.AddSingleton<SettingsPage>();

        services.AddSingleton<MainWindow>();

        return services.BuildServiceProvider();
    }
}