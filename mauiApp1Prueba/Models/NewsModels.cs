using System;
using System.Collections.Generic;

namespace mauiApp1Prueba.Models
{
    public class NewsDataResponse
    {
        public string? status { get; set; }
        public int? totalResults { get; set; }
        public List<NewsArticle>? results { get; set; }
        public string? nextPage { get; set; }
    }

    public class NewsArticle
    {
        public string? title { get; set; }
        public string? link { get; set; }
        public string? description { get; set; }
        public string? image_url { get; set; }
        public string? pubDate { get; set; }
        public List<string>? category { get; set; }
        public string? source_id { get; set; }
    }
}
