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
        private string? _nextPage;
        private bool _hasMoreItems = true;
        private HashSet<string> _loadedArticleIds = new();

        [ObservableProperty] private string? keyword;
        [ObservableProperty] private bool isBusy;
        [ObservableProperty] private bool isRefreshing;

        public ObservableCollection<NewsArticle> Items { get; } = new();

        public NewsViewModel()
        {
            _newsService = new NewsService(new HttpClient());
        }

        [RelayCommand]
        public async Task LoadAsync(bool clearItems = true)
        {
            if (IsBusy) return;
            try
            {
                IsBusy = true;

                if (clearItems)
                {
                    Items.Clear();
                    _loadedArticleIds.Clear();
                    _nextPage = null;
                    _hasMoreItems = true;
                }

                var res = await _newsService.GetUruguayNewsAsync(keyword: Keyword, size: 10, nextPage: _nextPage);

                if (res?.Results != null)
                {
                    foreach (var article in res.Results)
                    {
                        var articleId = $"{article.Title}_{article.PubDate}_{article.SourceId}";
                        if (!_loadedArticleIds.Contains(articleId))
                        {
                            _loadedArticleIds.Add(articleId);
                            Items.Add(article);
                        }
                    }

                    _nextPage = res.NextPage;
                    _hasMoreItems = !string.IsNullOrEmpty(_nextPage) && res.Results.Count > 0;
                }
                else
                {
                    _hasMoreItems = false;
                }
            }
            finally
            {
                IsBusy = false;
                IsRefreshing = false;
            }
        }

        [RelayCommand]
        public async Task SearchAsync()
        {
            await LoadAsync(true);
        }

        [RelayCommand]
        public Task LoadMoreAsync() => _hasMoreItems ? LoadAsync(false) : Task.CompletedTask;

        [RelayCommand]
        public Task RefreshAsync() => LoadAsync(true);

        [RelayCommand]
        public async Task OpenArticleAsync(NewsArticle? article)
        {
            if (article?.Link == null) return;
            await Browser.OpenAsync(article.Link, BrowserLaunchMode.SystemPreferred);
        }

        [RelayCommand]
        public async Task ClearSearchAsync()
        {
            Keyword = string.Empty;
            await LoadAsync(true);
        }
    }
}
