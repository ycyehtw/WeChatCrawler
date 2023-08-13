using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WeChatCrawler;

namespace WeChatCrawlerWebApi.Controllers {
  [Route("api/[controller]")]
  [ApiController]
  public class VendorsController : ControllerBase {
    private readonly ILogger<VendorsController> _logger;

    public VendorsController(ILogger<VendorsController> logger) {
      _logger = logger;
    }

    [Route("disableAll")]
    [HttpGet]
    public void DisableAll() {
      using (var db = new Database()) {
        db.DisableAllVendors();
      }
    }

    [Route("{vendorId}/posts/{postId}")]
    [HttpGet]
    public IEnumerable<Post> GetPostsByVendorAfterPost(int vendorId, int postId) {
      using var db = new Database();
      return db.GetPostsByVendorAfterPost(vendorId, postId);
    }

    [HttpGet]
    public IEnumerable<Vendor> GetAllVendors() {
      using var db = new Database();
      return db.GetAllVendors();
    }

    [Route("wechatid/{weChatId}")]
    [HttpGet]
    public Vendor GetVendor(string weChatId) {
      using var db = new Database();
      return db.GetVendorByWeChatId(weChatId);
    }

    [HttpPost]
    public IActionResult Insert(Vendor vendor) {
      using var db = new Database();
      vendor.VendorId = db.GetLastVendorId() + 1;
      db.Insert(vendor);
      return Ok(new { Integer = vendor.VendorId });
    }

    [HttpPut]
    public void UpdateVendor([FromBody] Vendor vendor) {
      using var db = new Database();
      if (db.IsExist(vendor)) {
        db.Update(vendor);
      }
    }
  }
}
