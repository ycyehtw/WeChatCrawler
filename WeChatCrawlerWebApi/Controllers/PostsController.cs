using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WeChatCrawler;

namespace WeChatCrawlerWebApi.Controllers {
  [Route("api/[controller]")]
  [ApiController]
  public class PostsController : ControllerBase {
    private readonly ILogger<PostsController> _logger;

    public PostsController(ILogger<PostsController> logger) {
      _logger = logger;
    }

    [Route("clear/{days}")]
    [HttpGet]
    public void Clear(int days) {
      var thisDay = DateTime.Today.AddDays(-days);
      using var db = new Database();
      var postIds = db.GetPostIdsAfterDate(thisDay);
      foreach (var id in postIds) {
        var pictures = db.GetPicturesByPost(id);
        foreach (var picture in pictures) {
          new FileInfo($"{Constant.ThumbnailFolder}/{picture.Name}").Delete();
          new FileInfo($"{Constant.PictureFolder}/{picture.Name}").Delete();
        }
        db.DeletePost(id);
      }
      // 更新廠商狀態
      var vendors = db.GetAllVendors();
      foreach (var vendor in vendors) {
        vendor.IsEnabled = db.GetPostCountByVendor(vendor) > 0;
        db.Update(vendor);
      }
    }

    [Route("count/{id}")]
    [HttpGet]
    public IActionResult GetPostsCountAfter(int id) {
      using var db = new Database();
      var number = db.GetPostsCountAfter(id);
      return Ok(new { Integer = number });
    }

    [Route("{postId}/pictures")]
    public IEnumerable<Post.Picture> GetPicturesByPost(int postId) {
      using var db = new Database();
      return db.GetPicturesByPost(postId);
    }

    [HttpPost]
    public IActionResult Insert(Post post) {
      using var db = new Database();
      post.Id = db.GetLastPostId() + 1;
      db.Insert(post);
      return Ok(new { Integer = post.Id });
    }

    [HttpPut]
    public void UpdatePost([FromBody] Post post) {
      using var db = new Database();
      if (db.IsExist(post)) {
        db.Update(post);
        if (post.Status == PostStatus.Exported) {
          var exportedPost = new ExportedPost() { PostId = post.Id, PostedDate = post.Date, ExportedDate = DateTime.Now };
          if (!db.IsExist(exportedPost)) db.Insert(exportedPost);
        }
      }
    }
  }
}
