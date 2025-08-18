using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using mauiApp1Prueba.Models;
using mauiApp1Prueba.Services;
using Microsoft.Maui.ApplicationModel;
using System.Collections.ObjectModel;

namespace mauiApp1Prueba.ViewModels
{
    public partial class NewsViewModel : ObservableObject
    {
        private readonly NewsService _newsService;

        [ObservableProperty] private string? keyword;
        [ObservableProperty] private bool isBusy;
        [ObservableProperty] private bool isRefreshing;

        public ObservableCollection<NewsArticle> Items { get; } = new();

        public NewsViewModel(NewsService newsService)
        {
            _newsService = newsService;
        }

        [RelayCommand]
        public async Task LoadAsync(bool clearItems = true)
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                if (clearItems) Items.Clear();

                var res = await _newsService.GetUruguayNewsAsync(
                    Keyword,
                    null,
                    10
                );

                if (res?.results != null)
                {
                    foreach (var article in res.results)
                        Items.Add(article);
                }
            }
            finally
            {
                IsBusy = false;
                IsRefreshing = false;
            }
        }

        [RelayCommand]
        public Task LoadMoreAsync() => LoadAsync(clearItems: false);

        [RelayCommand]
        public Task RefreshAsync() => LoadAsync(clearItems: true);

        [RelayCommand]
        public async Task OpenArticleAsync(NewsArticle? article)
        {
            if (article?.link is null) return;
            await Browser.OpenAsync(article.link, BrowserLaunchMode.SystemPreferred);
        }
    }
}
