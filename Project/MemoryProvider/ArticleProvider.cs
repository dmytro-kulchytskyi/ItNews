﻿using ItNews.Business.Entities;
using ItNews.Business.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItNews.FileProvider
{
    public class ArticleProvider : FileProvider<Article>, IArticleProvider
    {
        private UserProvider userProvider;

        public ArticleProvider()
        {
            userProvider = new UserProvider();
        }

        public async Task<IList<Article>> GetListSegment(int count, DateTime startDate)
        {
            return (await GetList()).OrderByDescending(it => it.Date)
                .Where(it => it.Date < startDate).Take(count).ToList();
        }

        public async Task<IList<Article>> GetPage(int count, int pageNumber)
        {
            return (await GetList()).OrderByDescending(it => it.Date).Skip(pageNumber * count).Take(count).ToList();
        }
    }
}
