using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using LocalFictionReader.Services;
using LocalFictionReader.ViewModels;

namespace LocalFictionReader
{
    sealed partial class App : Application
    {
        public App()
        {
            this.InitializeComponent();
            ConfigureServices();
        }

        private void ConfigureServices()
        {
            Ioc.Default.ConfigureServices(
                new ServiceCollection()
                .AddSingleton<BookParserService>()
                .AddTransient<MainViewModel>()
                .BuildServiceProvider());
        }

        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            Window.Current.Content = new MainPage();
            Window.Current.Activate();
        }
    }
}