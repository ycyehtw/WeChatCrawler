using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using WeChatCrawler;

namespace WeChatCrawlerWebApi.Controllers {
  [Route("api/[controller]")]
  [ApiController]
  public class PictureController : ControllerBase {
    private readonly ILogger<PictureController> _logger;
    private const int ThumbnailWidth = 150;
    private const int ThumbnailHeight = 150;

    public PictureController(ILogger<PictureController> logger) {
      _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> UploadPicture(IFormFile file) {
      var size = file.Length;
      if (file.Length > 0) {
        var path = $"{Constant.PictureFolder}/{file.FileName}";
        using var stream = new FileStream(path, FileMode.Create);
        await file.CopyToAsync(stream);
        await stream.FlushAsync();
        stream.Close();
        await MakeThumbnailAsync(file.FileName);
      }
      return Ok(new { file.FileName, size });
    }

    private static readonly ResizeOptions options = new ResizeOptions {
      Size = new Size(ThumbnailWidth, ThumbnailHeight),
      Mode = ResizeMode.Stretch,
      Sampler = KnownResamplers.Lanczos3
    };
    private static readonly JpegEncoder encoder = new JpegEncoder { Quality = 50 };
    private static async Task MakeThumbnailAsync(string filename) {
      using var image = Image.Load($"{Constant.PictureFolder}/{filename}");
      image.Mutate(x => x.Resize(options));
      await image.SaveAsync($"{Constant.ThumbnailFolder}/{filename}", encoder);
    }

    [HttpGet("{filename}")]
    public IActionResult DownloadPicture(string filename) {
      if (string.IsNullOrEmpty(filename)) {
        return NotFound();
      }
      var path = $"{Constant.PictureFolder}/{filename}";
      var stream = new FileStream(path, FileMode.Open);
      return new FileStreamResult(stream, "image/jpeg");
    }
  }
}
