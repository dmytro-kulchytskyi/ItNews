﻿using ItNews.Business.Entities;
using ItNews.Business.Managers;
using ItNews.Mvc.ViewModels.Article;
using ItNews.MVC.ViewModels.Article;
using Microsoft.AspNet.Identity;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Web.Mvc;
using ItNews.Mvc.ModelBinders.Article;
using System.Net;
using ItNews.Mvc.Attributes;
using System.Runtime.Remoting.Messaging;

namespace ItNews.Controllers
{
    public class ArticleController : Controller
    {
        private AppUserManager userManager;

        private ArticleManager articleManager;

        private CommentManager commentManager;

        public ArticleController(AppUserManager userManager, ArticleManager articleManager, CommentManager commentManager)
        {
            this.userManager = userManager;
            this.articleManager = articleManager;
            this.commentManager = commentManager;
        }

        [HttpGet]
        public async Task<ActionResult> Index(
            [PageNumber]int page,
            [ModelBinder(typeof(ItemsOnPageCountModelBinder))]int itemsCount)
        {
            var articles = await articleManager.GetPage(itemsCount, page, true);

            var previewLength = int.Parse(WebConfigurationManager.AppSettings["ArticleTextPreviewLength"]);
            var previewEnding = WebConfigurationManager.AppSettings["ArticleTextPreviewEnding"];

            var model = new ArticlesListViewModel
            {
                PageCount = (int)Math.Ceiling(await articleManager.GetCount() / (double)itemsCount),
                Articles = articles.Select(it => new ArticlesListPageItem
                {
                    Title = it.Title,
                    Id = it.Id,
                    Author = it.Author.UserName,
                    ImageName = it.ImageName,
                    Date = it.Date,
                    TextPreview = (it.Text.Length > previewLength) ? it.Text.Substring(0, previewLength) + previewEnding : it.Text
                }).ToList(),
                PageSize = itemsCount,
                PageNumber = page
            };

            return View(model);
        }

        [HttpGet]
        public async Task<ActionResult> Details(string id, [ModelBinder(typeof(PageNumberModelBinder))]int commentPage)
        {
            if (string.IsNullOrEmpty(id))
                return HttpNotFound();

            var article = await articleManager.GetById(id);

            if (article == null)
                return HttpNotFound();

            var comments = (await commentManager.GetArticleComments(id, 10, commentPage))
               .Select(comment => new CommentViewModel
               {
                   Author = comment.Author.UserName,
                   Id = comment.Id,
                   Date = comment.Date,
                   Text = comment.Text
               }).ToList();

            var model = new ArticleDetailsViewModel
            {
                Id = article.Id,
                Title = article.Title,
                AuthorName = article.Author.UserName,
                Date = article.Date,
                Content = article.Text,
                ControlsAvailable = (User.Identity.IsAuthenticated && article.Author.Id == User.Identity.GetUserId()),
                Comments = comments,
                CommentPagesCount = (int)Math.Ceiling(await commentManager.GetArticleCommentsCount(id) / 10.0),
                CommentPageNumber = commentPage
            };

            if (!string.IsNullOrEmpty(article.ImageName))
            {
                model.HasImage = true;
                model.Image = Path.Combine(WebConfigurationManager.AppSettings["ImagesFolder"], article.ImageName);
            }

            return View(model);
        }

        [Authorize]
        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var article = new Article
            {
                Text = model.Text,
                Title = model.Title
            };

            if (model.Image != null && model.Image.ContentLength > 0)
            {
                var directory = Server.MapPath(WebConfigurationManager.AppSettings["ImagesFolder"]);
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.Image.FileName);
                model.Image.SaveAs(Path.Combine(directory, fileName));
                article.ImageName = fileName;
            }

            await articleManager.CreateArticle(article, User.Identity.GetUserId());

            return RedirectToAction("Details", new { id = article.Id });
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
                return HttpNotFound();

            var article = await articleManager.GetById(id);

            if (article == null)
                return HttpNotFound();

            var model = new EditViewModel
            {
                Id = article.Id,
                Text = article.Text,
                Title = article.Title,
            };

            if (article.ImageName != null)
                model.OldImageName = Path.Combine(WebConfigurationManager.AppSettings["ImagesFolder"], article.ImageName);

            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(EditViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var article = await articleManager.GetById(model.Id);

            if (article.Author.Id != User.Identity.GetUserId())
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden, "Forbidden");

            var updatedArticle = new Article
            {
                Id = model.Id,
                Title = model.Title,
                Text = model.Text,
                ImageName = article.ImageName
            };

            if (model.UploadedImage != null)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.UploadedImage.FileName);
                model.UploadedImage.SaveAs(Path.Combine(Server.MapPath(WebConfigurationManager.AppSettings["ImagesFolder"]), fileName));

                updatedArticle.ImageName = fileName;
            }

            await articleManager.UpdateArticle(updatedArticle, User.Identity.GetUserId());

            return RedirectToAction("Details", new { id = article.Id });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmation(string id)
        {
            if (string.IsNullOrEmpty(id))
                return HttpNotFound();

            var article = await articleManager.GetById(id);

            if (article.Author.Id != User.Identity.GetUserId())
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden, "Forbidden");

            await articleManager.DeleteArticle(article, article.Id);

            return RedirectToAction("Index");
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
                return HttpNotFound();

            var article = await articleManager.GetById(id);

            if (article == null)
                return HttpNotFound();

            var model = new DeleteViewModel
            {
                ImagePath = article.ImageName,
                Title = article.Title,
                Text = article.Text,
                Date = article.Date.ToString("f")
            };

            if (!string.IsNullOrEmpty(article.ImageName))
            {
                model.ImagePath = Url.Content(Path.Combine(WebConfigurationManager.AppSettings["ImagesFolder"], article.ImageName));
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<ActionResult> CreateComment(CreateCommentViewModel model)
        {
            if (string.IsNullOrEmpty(model.ArticleId))
                return HttpNotFound();

            if (!ModelState.IsValid)
                return RedirectToAction("Details", "Article", new { Id = model.ArticleId });

            var article = await articleManager.GetById(model.ArticleId);
            if (article == null)
                return HttpNotFound();

            await commentManager.CreateComment(model.Text, User.Identity.GetUserId(), model.ArticleId);

            return RedirectToAction("Details", new { id = model.ArticleId });
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult> DeleteComment(string id, string articleId)
        {
            if (string.IsNullOrEmpty(id))
                return HttpNotFound();

            var comment = await commentManager.GetById(id);

            if (comment.Author.Id != User.Identity.GetUserId())
                return HttpNotFound();

            await commentManager.DeleteComment(comment, comment.Id);

            return RedirectToAction("Details", new { id = articleId });
        }

        //[Authorize(Roles = "Admin")]
        public async Task<ActionResult> CreateIndex()
        {
            await articleManager.CreateSearchIndex();
            return Content("OK");
        }

        [HttpPost]
        public async Task<ActionResult> Search(string query)
        {
            if (!Request.IsAjaxRequest())
                return HttpNotFound();

            var previewLength = int.Parse(WebConfigurationManager.AppSettings["ArticleTextPreviewLength"]);
            var previewEnding = WebConfigurationManager.AppSettings["ArticleTextPreviewEnding"];

            var results = (await articleManager.Search(query)).ToList();

            var list = results.Select(it => new ArticleSearchItem
            {
                Id = it.Id,
                Author = it.Author.UserName,
                Date = it.Date,
                TextPreview = (it.Text.Length > previewLength) ? it.Text.Substring(0, previewLength) + previewEnding : it.Text,
                Title = it.Title
            });

            return Json(list, JsonRequestBehavior.DenyGet);
        }
    }
}