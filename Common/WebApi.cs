using System.Collections.Generic;
using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace WeChatCrawler {
  class Number {
    public int Integer { set; get; }
  }
  public class WebApi {
    public static string WebApiServer = "http://localhost:5000";
    //public const string WebApiServer = "https://35.229.207.111";
    public static string User = "user";

    public static void DisableAllVendors() {
      using (var client = new WebClient()) {
        client.Headers["User"] = User;
        client.DownloadString($"{WebApiServer}/api/vendors/disableAll");
      }
    }

    public static void Clear(int days) {
      using (var client = new WebClient()) {
        client.Headers["User"] = User;
        client.DownloadString($"{WebApiServer}/api/posts/clear/{days}");
      }
    }

    public static int GetPostsCountAfter(int id) {
      using (var client = new WebClient()) {
        client.Headers["User"] = User;
        string s = client.DownloadString($"{WebApiServer}/api/posts/count/{id}");
        return JsonConvert.DeserializeObject<Number>(s).Integer;
      }
    }
    public static IEnumerable<Post> GetPostsByVendorAfterPost(int vendorId, int postId) {
      using (var client = new WebClient()) {
        client.Encoding = Encoding.UTF8;
        client.Headers["User"] = User;
        string s = client.DownloadString($"{WebApiServer}/api/vendors/{vendorId}/posts/{postId}");
        return JsonConvert.DeserializeObject<IList<Post>>(s);
      }
    }

    public static IEnumerable<Vendor> GetAllVendors() {
      using (var client = new WebClient()) {
        client.Encoding = Encoding.UTF8;
        client.Headers["User"] = User;
        string s = client.DownloadString($"{WebApiServer}/api/vendors");
        return JsonConvert.DeserializeObject<IList<Vendor>>(s);
      }
    }

    public static int Insert(Vendor vendor) {
      using (var client = new WebClient()) {
        client.Encoding = Encoding.UTF8;
        client.Headers["User"] = User;
        client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
        var s = client.UploadString($"{WebApiServer}/api/vendors", "POST", JsonConvert.SerializeObject(vendor));
        return JsonConvert.DeserializeObject<Number>(s).Integer;
      }
    }
    public static Vendor GetVendor(string wechatId) {
      using (var client = new WebClient()) {
        client.Encoding = Encoding.UTF8;
        client.Headers["User"] = User;
        Vendor vendor = null;
        try {
          string s = client.DownloadString($"{WebApiServer}/api/vendors/wechatid/{wechatId}");
          vendor = JsonConvert.DeserializeObject<Vendor>(s);
        } catch (JsonReaderException) { };
        return vendor;
      }
    }

    public static void DownloadPicture(string filename) {
      using (var client = new WebClient()) {
        client.Headers["User"] = User;
        client.DownloadFile($"{WebApiServer}/api/picture/{filename}", $"{Constant.PictureFolder}/{filename}");
      }
    }

    public static void UploadPicture(string filename) {
      using (var client = new WebClient()) {
        client.Headers["User"] = User;
        client.UploadFile($"{WebApiServer}/api/picture", filename);
      }
    }
    public static void DownloadThumbnail(string filename) {
      using (var client = new WebClient()) {
        client.Headers["User"] = User;
        client.DownloadFile($"{WebApiServer}/api/thumbnail/{filename}", $"{Constant.ThumbnailFolder}/{filename}");
      }
    }

    public static int Insert(Post post) {
      using (var client = new WebClient()) {
        client.Encoding = Encoding.UTF8;
        client.Headers["User"] = User;
        client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
        var s = client.UploadString($"{WebApiServer}/api/posts", "POST", JsonConvert.SerializeObject(post));
        return JsonConvert.DeserializeObject<Number>(s).Integer;
      }
    }

    public static void Update(Post post) {
      using (var client = new WebClient()) {
        client.Encoding = Encoding.UTF8;
        client.Headers["User"] = User;
        client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
        var result = client.UploadString($"{WebApiServer}/api/posts", "PUT", JsonConvert.SerializeObject(post));
      }
    }

    public static void Update(Vendor vendor) {
      using (var client = new WebClient()) {
        client.Encoding = Encoding.UTF8;
        client.Headers["User"] = User;
        client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
        var result = client.UploadString($"{WebApiServer}/api/vendors", "PUT", JsonConvert.SerializeObject(vendor));
      }
    }
  }
}
