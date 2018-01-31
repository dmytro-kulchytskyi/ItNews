﻿using ItNews.Business.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ItNews.Business.Providers;

namespace ItNews.Business.Managers
{
    public class ArticleManager : Manager<Article, IArticleProvider>
    {
        private AppUserManager userManager;

        public ArticleManager(IArticleProvider articleProvider, AppUserManager userManager)
            : base(articleProvider)
        {
            this.userManager = userManager;
        }

        public Task<IList<Article>> GetListSegmentAsync(int count, DateTime startDate, bool newFirst)
        {
            return provider.GetListSegment(count, startDate, newFirst);
        }

        public Task<IList<Article>> GetPage(int count, int pageNumber, bool newFirst)
        {
            return provider.GetPage(count, pageNumber, newFirst);
        }

        public Task<Article> GetArticle(string id)
        {
            return provider.Get(id);
        }

        public async Task CreateArticle(Article article, string authorId)
        {
            using (var uow = provider.GetUnitOfWork())
            {
                var user = await userManager.GetById(authorId);

                article.Author = user ?? throw new InvalidOperationException("No user with given id");
                article.Date = DateTime.Now;

                uow.BeginTransaction();
                await provider.SaveOrUpdate(article);
                uow.Commit();
            }
        }

        public Task<int> GetCount()
        {
            return provider.GetCount();
        }

        public async Task<bool> IsOwnedBy(string articleId, string userId)
        {
            if (string.IsNullOrEmpty(articleId))
                throw new ArgumentNullException(nameof(articleId));

            if (string.IsNullOrEmpty(userId))
                throw new ArgumentNullException(nameof(userId));

            var article = await provider.Get(articleId);

            return (article?.Author.Id == userId);
        }

        public async Task UpdateArticle(Article article, string authorId)
        {
            if (string.IsNullOrEmpty(authorId))
                throw new ArgumentNullException(nameof(authorId));

            using (var uow = provider.GetUnitOfWork())
            {
                var oldArticle = await provider.Get(article.Id) ?? throw new ArgumentException("Article with given id does not exists"); 

                var author = await userManager.GetById(authorId) ?? throw new ArgumentException("User with given id does not exists");

                if (oldArticle.Author.Id != authorId)
                    throw new InvalidOperationException("Article is not owned by current user");

                article.Date = DateTime.Now;
                article.Author = author;

                uow.BeginTransaction();
                await provider.SaveOrUpdate(article);
                uow.Commit();
            }
        }

        public async Task DeleteArticle(Article article, string authorId)
        {
            if (string.IsNullOrEmpty(authorId))
                throw new ArgumentNullException(nameof(authorId));

            using (var uow = provider.GetUnitOfWork())
            {
                uow.BeginTransaction();
                await provider.Delete(article);
                uow.Commit();
            }
        }
    }
}