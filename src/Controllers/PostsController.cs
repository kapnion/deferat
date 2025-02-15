
using Deferat.Models;
using Deferat.Repository;
using Deferat.Services;
using Deferat.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace Deferat.Controllers
{
    public class PostsController : Controller
    {
        // use a multiple of 3 for best appearance
        private const int PostsPerPage = 6;
        private IRepositoryContainer _repositories;
        private Settings _settings;
        public PostsController(IRepositoryContainer repositories, ISiteInfo siteInfo)
        {
            _repositories = repositories;
            _settings = siteInfo.Settings;
        }

        // GET: Posts
        [HttpGet]
        public ActionResult Index(string tag, int pageNumber = 1, bool showDraft = false, string filter = null)
        {
            var pageCount = 0;

            var posts = _repositories.Posts.Get(filter: p => showDraft ? true: !p.Draft,  orderBy: list => list.OrderByDescending(post => post.Date));
            if(!String.IsNullOrEmpty(tag))
            {
                posts = posts.Where(p => p.Categories.Contains(tag));
            }

            if (!String.IsNullOrEmpty(filter))
            {
                posts = posts.Where(p => p.Title.ToLower().Contains(filter.ToLower()));
            }

            pageCount = (posts.Count() - 1) / PostsPerPage + 1;

            if (pageNumber > pageCount)
                throw new ArgumentException($"Requested page {pageNumber} but there are only {pageCount} pages");

            posts = posts.Skip((pageNumber - 1) * PostsPerPage).Take(PostsPerPage);

            var viewModel = new PostListViewModel()
            {
                Settings = _settings,
                Posts = posts.Select(p => CreatePostViewModel(p)),
                PageCount = pageCount,
                CurrentPage = pageNumber,
                Tag = tag
            };

            return View(viewModel);
        }

        public ActionResult Read(string id)
        {
            var post = _repositories.Posts.Get(id.ToLower());
            if(post == null)
                return new StatusCodeResult(404);
            
            return View(CreatePostViewModel(post));

        }

        private PostViewModel CreatePostViewModel(Post post)
        {
            PostViewModel postViewModel = new PostViewModel()
            {
                Post = post,
                Settings = _settings
            };

            var author = _repositories.Authors.Get(post.Author.ToLower());
            
            postViewModel.AuthorName = author != null ? author.DisplayName : "Anonymous";
            postViewModel.AuthorImage = author != null ? author.ImageUrl : "";
            postViewModel.AuthorTwitter = author != null ? author.Twitter : "";

            return postViewModel;         
        }

    }
}