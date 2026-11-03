using LocalFictionReader.Models;
using LocalFictionReader.Services;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Popups;

namespace LocalFictionReader.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private readonly BookParserService _parserService;
        private Book _selectedBook;

        public ObservableCollection<Book> Books { get; } = new ObservableCollection<Book>();

        /// <summary>
        /// 当前选中的书籍；带变更通知，章节列表才能随选中项刷新。
        /// </summary>
        public Book SelectedBook
        {
            get => _selectedBook;
            set => SetProperty(ref _selectedBook, value);
        }

        public IAsyncRelayCommand OpenFileCommand { get; }

        public MainViewModel(BookParserService parserService)
        {
            _parserService = parserService;
            OpenFileCommand = new AsyncRelayCommand(OpenFileAsync);
        }

        private async Task OpenFileAsync()
        {
            try
            {
                var picker = new FileOpenPicker
                {
                    ViewMode = PickerViewMode.List,
                    SuggestedStartLocation = PickerLocationId.DocumentsLibrary
                };
                picker.FileTypeFilter.Add(".txt");

                StorageFile file = await picker.PickSingleFileAsync();
                if (file == null)
                {
                    return;
                }

                // 按路径去重：同一本书只加载一次
                Book existing = Books.FirstOrDefault(b =>
                    string.Equals(b.FilePath, file.Path, StringComparison.OrdinalIgnoreCase));
                if (existing != null)
                {
                    SelectedBook = existing;
                    return;
                }

                Book book = await _parserService.ParseBookAsync(file);
                Books.Add(book);
                SelectedBook = book;
            }
            catch (Exception ex)
            {
                await ShowMessageAsync("打开文件失败", "无法解析该文件：" + ex.Message);
            }
        }

        private static async Task ShowMessageAsync(string title, string message)
        {
            var dialog = new MessageDialog(message, title);
            dialog.Commands.Add(new UICommand("确定"));
            await dialog.ShowAsync();
        }
    }
}
