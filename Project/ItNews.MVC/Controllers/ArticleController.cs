﻿using ItNews.Business.Entities;
using ItNews.Business.Managers;
using ItNews.Mvc.ViewModels.News;
using ItNews.MVC.ViewModels.News;
using Ninject;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Web.Mvc;

namespace ItNews.Controllers
{
    public class ArticleController : Controller
    {
        private readonly int defaultItemsOnPageCount = int.Parse(WebConfigurationManager.AppSettings["NewsListItemsOnPageCount"]);
        private readonly int articleTextPreviewLength = int.Parse(WebConfigurationManager.AppSettings["ArticleTextPreviewLength"]);

        private ArticleManager articleManager;

        public ArticleController(ArticleManager articleManager)
        {
            this.articleManager = articleManager;
        }

        [HttpGet]
        public async Task<ActionResult> Index(int page = 1, int itemsCount = 2)
        {
            if (itemsCount <= 0)
                itemsCount = defaultItemsOnPageCount;

            var articles = await articleManager.GetPage(itemsCount, page, true);

            var model = new ArticlesList();

            if (articles.Count > 0)
                model.NextAvailable = true;

            if (page > 1)
                model.PrevAvailable = true;

            model.Articles = articles.Select(it => new ArticlesListPageItem
            {
                Title = it.Title,
                UrlPath = it.Id,
                Author = it.Author.UserName,
                ImagePath = it.ImagePath,
                Date = it.Date,
                TextPreview = it.Text.Substring(0, articleTextPreviewLength > it.Text.Length ? it.Text.Length : articleTextPreviewLength)
            }).ToList();

            model.PageSize = itemsCount;
            model.PageNumber = page;

            return View(model);
        }

        [HttpGet]
        public async Task<ActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
                return HttpNotFound();

            var article = await articleManager.GetArticle(id);

            if (article == null)
                return HttpNotFound();

            var model = new ArticleDetailsViewModel
            {
                Id = article.Id,
                Title = article.Title,
                AuthorName = article.Author.UserName,
                Image = article.ImagePath,
                Date = article.Date
            };

            return View(model);
        }

        public async Task<ActionResult> TestCreate(string name)
        {
            await articleManager.CreateArticle(new Article
            {
                Text = name ?? "test",
                Title = "adasd"
            }, "1");

            return Content("dsa");
        }

        //[Authorize]
        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        //[Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateViewModel model)
        {

            if (!ModelState.IsValid)
                return View(model);

           

            var item = new Article
            {
                Text = model.Text,
                Title = model.Title
            };

            //string fileName = null;
           

            if (model.Image != null && model.Image.ContentLength > 0)
            {
                var directory = Server.MapPath(Url.Content(WebConfigurationManager.AppSettings["ImagesFolder"]));
                var fileName = Guid.NewGuid().ToString();
                model.Image.SaveAs(Path.Combine(directory, fileName));
                item.ImagePath = fileName;
            }
           
            // Authorize (id)
            await articleManager.CreateArticle(item, "1");

            return RedirectToAction("Index");
        }
    }
}