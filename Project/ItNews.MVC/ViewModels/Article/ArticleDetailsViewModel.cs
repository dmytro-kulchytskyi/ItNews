﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItNews.Mvc.ViewModels.Article
{
    public class ArticleDetailsViewModel
    {
        [ScaffoldColumn(false)]
        public string Id { get; set; }

        [Display(Name = "Title")]
        public string Title { get; set; }

        [Display(Name = "Image")]
        [UIHint("Image")]
        public string Image { get; set; }

        [Display(Name = "Author")]
        public string AuthorName { get; set; }

        [Display(Name = "Created")]
        [DataType(DataType.DateTime)]
        public DateTime Date { get; set; }

        [Display(Name = "Content")]
        public string Content { get; set; }

        [ScaffoldColumn(false)]
        public bool HasImage { get; set; }

        [ScaffoldColumn(false)]
        public bool ControlsAvailable { get; set; }

        [ScaffoldColumn(false)]
        public IList<CommentViewModel> Comments { set; get; }

        [ScaffoldColumn(false)]
        public int CommentPagesCount { get; set; }

        [ScaffoldColumn(false)]
        public int CommentPageNumber { get; set; }
    }
}
