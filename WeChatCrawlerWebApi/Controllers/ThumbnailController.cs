using System.IO;
using WeChatCrawler;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace WeChatCrawlerWebApi.Controllers {
  [Route("api/[controller]")]
  [ApiController]
  public class ThumbnailController : ControllerBase {
    private readonly ILogger<ThumbnailController> _logger;

    public ThumbnailController(ILogger<ThumbnailController> logger) {
      _logger = logger;
    }

    [HttpGet("{filename}")]
    public IActionResult DownloadThumbnail(string filename) {
      var path = $"{Constant.ThumbnailFolder}/{filename}";
      var stream = new FileStream(path, FileMode.Open);
      return new FileStreamResult(stream, "image/jpeg");
    }
  }
}
