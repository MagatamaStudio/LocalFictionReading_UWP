using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using System.Text;
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
            // UWP/CoreCLR 默认不注册代码页编码（GBK=936 等）。不注册的话 BookParserService
            // 解码 GBK 小说时会抛 NotSupportedException 并回退成 UTF-8，中文仍然乱码。
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
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