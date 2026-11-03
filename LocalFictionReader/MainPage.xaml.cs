using LocalFictionReader.Models;
using LocalFictionReader.ViewModels;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Windows.UI.Xaml.Controls;

namespace LocalFictionReader
{
    public sealed partial class MainPage : Page
    {
        public MainViewModel ViewModel { get; } = Ioc.Default.GetService<MainViewModel>();

        public MainPage()
        {
            this.InitializeComponent();
            DataContext = ViewModel;
        }

        // 选中书籍统一由 ViewModel 管理（ListView.SelectedItem 双向绑定到 ViewModel.SelectedBook），
        // 这里仅做只读转发，方便 code-behind 使用。
        public Book SelectedBook => ViewModel.SelectedBook;
    }
}
