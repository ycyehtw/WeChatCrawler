using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using NLog;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.Enums;
using OpenQA.Selenium.Appium.MultiTouch;
using OpenQA.Selenium.Appium.Service;
using OpenQA.Selenium.Appium.Service.Options;
using OpenQA.Selenium.Support.UI;

namespace WeChatCrawler {
  class Crawler {
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();
    private static readonly ISet<string> hashs = new HashSet<string>(); // 記錄貼文的雜湊值，用以辨識貼文是否已抓取
    private static readonly Dictionary<string, Vendor> completedVendors = new Dictionary<string, Vendor>(); // 
    //private const string WeChatFilePath = "/storage/emulated/0/tencent/MicroMsg/WeChat";
    private const string WeChatFilePath = "/storage/emulated/0/Pictures/WeChat";
    private const string TempFilePath = "temp";
    private const int AnimationTime = 1000; // ms
    private const string DeviceName = "BlueStacks";
    private const string PlatformName = "Android";
    private const string PlatformVersion = "7.1.2";
    private const string AutomationName = "UiAutomator2";
    private const string AppiumServer = "http://localhost:4723/wd/hub";
    private const string AppPackage = "com.tencent.mm";
    private const string AppActivity = ".ui.LauncherUI";
    private const string LoginButtonId = "com.tencent.mm:id/g2t";
    private const string UseOtherLoginMethodButtonId = "com.tencent.mm:id/dre";
    private const string LoginByWeChatIdButtonId = "com.tencent.mm:id/h67";
    private const string LoginByWeChatIdButtonText = "用WeChat ID/QQ號碼/電子信箱登入";
    private const string EditTextBoxId = "com.tencent.mm:id/bn4";
    private const string UsernameEditBoxText = "WeChat ID/Email/QQ號碼";
    private const string PasswordEditBoxText = "請填寫密碼";
    private const string SubmitToLoginButtonId = "com.tencent.mm:id/dr8";
    private const string ConfirmPermissionButtonId = "com.tencent.mm:id/eb8";
    private const string PermissisionAllowButtonId = "com.android.packageinstaller:id/permission_allow_button";
    private const string AddressBookAllowButtonId = "com.tencent.mm:id/eb8";
    private const string AddressBookButtonId = "com.tencent.mm:id/cz6";
    private const string AddressBookButtonText = "通訊錄";
    private const string WeChatIdLabel = "com.tencent.mm:id/b5u";
    private const string LabelButtonId = "com.tencent.mm:id/g6";
    private const string LabelButtonText = "標籤";
    private const string VendorLabelButtonId = "com.tencent.mm:id/d_c";
    private const string VendorLabelButtonText = "供應商";
    private const string VendorNumberId = "com.tencent.mm:id/d_b";
    private const string AddMemberButtonId = "com.tencent.mm:id/fub";
    private const string AddMemberButtonText = "新增成員";
    private const string MemberNameButtonId = "com.tencent.mm:id/fu9";
    private const string FriendCircleButtonId = "com.tencent.mm:id/jj";
    private const string FriendCircleIsEmptyText = "個人相簿，共0張";
    private const string PostButtonId = "com.tencent.mm:id/b6q";
    private const string PostListTimeStampId = "com.tencent.mm:id/gfl";
    private const string PostListThumbnailId = "com.tencent.mm:id/d1k";
    private const string CommentButtonId = "com.tencent.mm:id/b7_";
    private const string CommentButtonClassName = "android.widget.ImageView";
    private const string ContentElementId = "com.tencent.mm:id/bdt";
    private const string PublishedTimeElementId = "com.tencent.mm:id/is";
    private const string ThumbnailListElementId = "com.tencent.mm:id/f2b";
    private const string ThumbnailListElementClassName = "android.view.View";
    private const string PictureElementId = "com.tencent.mm:id/db9";
    private const string VideoElementId = "com.tencent.mm:id/hjg";
    private const string SavePictureButtonId = "com.tencent.mm:id/h67";
    private const string SavePictureButtonText = "保存圖片";
    private const string SaveVideoButtonId = "com.tencent.mm:id/h67";
    private const string SaveVideoButtonText = "儲存影片";

    private Config config;
    private AppiumOptions options;
    private AndroidDriver<AndroidElement> driver;
    private ImageCodecInfo jpgEncoder;
    private EncoderParameters myEncoderParameters;
    private bool isDebug = true;
    private DateTime deadline;
    public Crawler(Config config) {
      this.config = config;
      isDebug = config.IsDebug;
      deadline = DateTime.Now.AddHours(config.Duration);
      jpgEncoder = GetEncoder(ImageFormat.Jpeg);
      var myEncoder = System.Drawing.Imaging.Encoder.Quality;
      myEncoderParameters = new EncoderParameters(1);
      var myEncoderParameter = new EncoderParameter(myEncoder, 75L);
      myEncoderParameters.Param[0] = myEncoderParameter;

      options = new AppiumOptions();
      options.AddAdditionalCapability(MobileCapabilityType.DeviceName, DeviceName);
      options.AddAdditionalCapability(MobileCapabilityType.PlatformName, PlatformName);
      options.AddAdditionalCapability(MobileCapabilityType.PlatformVersion, PlatformVersion);
      options.AddAdditionalCapability(MobileCapabilityType.AutomationName, AutomationName);
      options.AddAdditionalCapability(AndroidMobileCapabilityType.AppPackage, AppPackage);
      options.AddAdditionalCapability(AndroidMobileCapabilityType.AppActivity, AppActivity);
      options.AddAdditionalCapability(MobileCapabilityType.NoReset, true);
    }
    private static FileInfo logFile = new FileInfo("C:/Application/Crawler/logs/Appium.log");
    public void Run() {
      var args = new OptionCollector().AddArguments(new KeyValuePair<string, string>("--relaxed-security", null)).AddArguments(new KeyValuePair<string, string>("--local-timezone", null));
      var builder = new AppiumServiceBuilder().UsingAnyFreePort().WithArguments(args);
      if (config.EnableAppiumLog)
        builder = builder.WithLogFile(logFile);
      using (var service = builder.Build()) {
        service.Start();
        using (driver = new AndroidDriver<AndroidElement>(service, options)) {
          DoWork();
        }
      }
    }
    public void DoWork() {
      bool isLogined = false;
      for (var i = 0; i < 3; i++) {
        try {
          WaitForElement(IdType.Id, AddressBookButtonId);
          isLogined = true;
          break;
        } catch (WebDriverTimeoutException) {
          if (Log.IsInfoEnabled) Log.Info("尚未登入，啟動登入程序");
          Login(config.User, config.Password);
        }
      }
      if (!isLogined) {
        if (Log.IsInfoEnabled) Log.Info("無法登入，結束程式");
        return;
      }
      SwitchToAddressBook();
      SwitchToVendorList();
      var vendors = new Dictionary<string, Point>();
      var elements = driver.FindElementsById(MemberNameButtonId);
      foreach (var element in elements) {
        var name = element.Text;
        var rect = element.Rect;
        vendors.Add(name, new Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2));
      }
      foreach (var name in vendors.Keys) {
        if (completedVendors.ContainsKey(name)) continue;
        if (Log.IsInfoEnabled) Log.Info($"開始抓取「{name}」的貼文");
        var count = hashs.Count;
        GetPosts(name, vendors[name]);
        if (DateTime.Now > deadline) {
          if (Log.IsInfoEnabled) Log.Info($"已超過執行時間，即將結束程式，尚有{vendors.Count - completedVendors.Count}個供應商未抓取");
          break;
        }
        if (Log.IsInfoEnabled) Log.Info($"完成抓取「{name}」的貼文({hashs.Count - count}則), 總共已抓取{hashs.Count}則");
        Back();
      }
      if (Log.IsInfoEnabled) Log.Info($"全部完成，總共抓取{Crawler.completedVendors.Count}個供應商， {Crawler.hashs.Count}則貼文");
    }
    private void Login(string username, string password) {
      var passwordElement = driver.FindElementById(EditTextBoxId);
      passwordElement.SendKeys(password);
      var submitToLoginButton = driver.FindElementById(SubmitToLoginButtonId);
      submitToLoginButton.Click();
    }
    private static void Wait(int ms) {
      Thread.Sleep(ms);
    }
    private void SwitchToAddressBook() {
      WaitForElement(IdType.Id, AddressBookButtonId);
      var elements = driver.FindElementsById(AddressBookButtonId);
      foreach (var element in elements) {
        if (AddressBookButtonText.Equals(element.Text)) {
          element.Click();
          break;
        }
      }
    }
    private void SwitchToVendorList() {
      WaitForElement(IdType.Id, LabelButtonId);
      var elements = driver.FindElementsById(LabelButtonId);
      foreach (var element in elements) {
        if (LabelButtonText.Equals(element.Text)) {
          element.Click();
          break;
        }
      }
      WaitForElement(IdType.Id, VendorLabelButtonId);
      elements = driver.FindElementsById(VendorLabelButtonId);
      foreach (var element in elements) {
        if (VendorLabelButtonText.Equals(element.Text)) {
          element.Click();
          WaitForElement(IdType.Id, MemberNameButtonId);
          break;
        }
      }
    }
    private Image CropImage(Image source, Rectangle rect) {
      var destRect = new Rectangle(0, 0, rect.Width, rect.Height);
      var image = new Bitmap(rect.Width, rect.Height);
      using (var graphic = Graphics.FromImage(image)) {
        graphic.DrawImage(source, destRect, rect, GraphicsUnit.Pixel);
      }
      return image;
    }
    private bool IsFriendCricleIsNotEmpty() {
      try {
        driver.FindElementById(FriendCircleButtonId);
      } catch (NoSuchElementException) {
        return false;
      }
      try {
        driver.FindElementByAccessibilityId(FriendCircleIsEmptyText);
      } catch (NoSuchElementException) {
        return true;
      }
      return false;
    }
    private void GetPosts(string name, Point point) {
      // 點擊進入聯絡人頁面
      new TouchAction(driver).Tap(point.X, point.Y).Perform();
      Wait(AnimationTime);
      // 取得聯絡人 WeChat ID
      var wechatId = driver.FindElementById(WeChatIdLabel).Text;
      // 比對 WeChat ID，以確定是否新的聯絡人
      if (wechatId.StartsWith("WeChat ID：")) wechatId = wechatId.Substring(11).Trim();
      var vendor = WebApi.GetVendor(wechatId);
      if (vendor == null) {
        vendor = new Vendor { Name = name, WeChatId = wechatId };
        var vendorId = isDebug ? 1 : WebApi.Insert(vendor);
        vendor.VendorId = vendorId;
      } else if (!vendor.Name.Equals(name)) {
        vendor.Name = name;
        if (!isDebug) WebApi.Update(vendor);
      }
      if (IsFriendCricleIsNotEmpty()) {
        SwitchToFriendCircle();
        bool isBegin = false;
        bool isEnd = false;
        while (true) {
          var elements = driver.FindElementsById(PostButtonId);
          var rects = new List<Rectangle>();
          foreach (var element in elements) {
            try {
              var day = element.FindElementById(PostListTimeStampId);
              if ("昨天".Equals(day.Text)) // 抓昨天的貼文
                isBegin = true;
              else if (!"今天".Equals(day.Text)) { // 不是昨天的貼文不抓
                isEnd = true;
                break;
              }
            } catch (NoSuchElementException) { };
            if (isBegin) {
              try {
                var rect = element.FindElementById(PostListThumbnailId).Rect;
                if ((rect.Y + rect.Height / 2) > 175) rects.Add(rect); //縮圖可能被上方聯絡人資訊欄遮住，如果按不到的話就不抓 
              } catch (NoSuchElementException) { }; // 如果是轉發的就不會有PostListThumbnailId，不予理會
            }
          }
          foreach (var rect in rects) {
            EnterPost(rect);
            GetPost(vendor);
            if (DateTime.Now > deadline) return;
            Back();
            Back();
          }
          if (isEnd) break;
          ScrollForward();
        }
      }
      completedVendors.Add(name, vendor);
      Back();
    }
    // 向前捲動
    private void ScrollForward() {
      try {
        driver.FindElement(MobileBy.AndroidUIAutomator("new UiScrollable(new UiSelector().scrollable(true)).scrollForward()"));
        Wait(30 * 1000);
      } catch (NoSuchElementException) { };
    }
    // 回上頁
    private void Back() {
      driver.Navigate().Back();
      Wait(AnimationTime);
    }
    public enum IdType {
      Id, XPath, Name, ClassName
    }
    public void WaitForElement(IdType type, string id) {
      var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30)) {
        PollingInterval = TimeSpan.FromMilliseconds(250)
      };
      wait.IgnoreExceptionTypes(typeof(InvalidOperationException));

      wait.Until(driver => {
        int elementCount = 0;
        switch (type) {
          case IdType.Id:
            elementCount = driver.FindElements(MobileBy.Id(id)).Count;
            break;
          case IdType.XPath:
            elementCount = driver.FindElements(MobileBy.XPath(id)).Count;
            break;
          case IdType.Name:
            elementCount = driver.FindElements(MobileBy.Name(id)).Count;
            break;
          case IdType.ClassName:
            elementCount = driver.FindElements(MobileBy.ClassName(id)).Count;
            break;
        }
        return elementCount > 0;
      });
    }
    private void Tap(Rectangle rect) {
      new TouchAction(driver).Tap(rect.X + rect.Width / 2, rect.Y + rect.Height / 2).Perform();
    }
    private void EnterPost(Rectangle rect) {
      Tap(rect);
      WaitForElement(IdType.Id, CommentButtonId);
      var element = driver.FindElementById(CommentButtonId);
      var elements = element.FindElementsByClassName(CommentButtonClassName);
      elements[1].Click();
      WaitForElement(IdType.Id, PublishedTimeElementId);
    }
    // 貼文的時間(只處理昨天的貼文)
    private DateTime ParsePublishedTime(string s) {
      DateTime dt = DateTime.Today.AddDays(-1);
      if (s.StartsWith("昨天")) {
        var time = DateTime.Parse(s.Substring(3));
        dt = new DateTime(dt.Year, dt.Month, dt.Day, time.Hour, time.Minute, 0);
      }
      return dt;
    }
    private MD5 md5 = new MD5CryptoServiceProvider();
    public string GetMD5Hash(Stream stream) {
      byte[] value = md5.ComputeHash(stream);
      StringBuilder sb = new StringBuilder();
      for (int i = 0; i < value.Length; i++) {
        sb.Append(value[i].ToString("x2"));
      }
      return sb.ToString();
    }
    public string GetMD5Hash(byte[] source) {
      byte[] value = md5.ComputeHash(source);
      StringBuilder sb = new StringBuilder();
      for (int i = 0; i < value.Length; i++) {
        sb.Append(value[i].ToString("x2"));
      }
      return sb.ToString();
    }
    private void GetPost(Vendor vendor) {
      try {
        var elements = driver.FindElementById(ThumbnailListElementId).FindElementsByClassName(ThumbnailListElementClassName);
        // 取得所有縮圖的位置   
        IList<Rectangle> rects = new List<Rectangle>();
        foreach (var element in elements) {
          rects.Add(element.Rect);
        }
        using (var screenshot = new MemoryStream(driver.GetScreenshot().AsByteArray)) {
          using (var image = Image.FromStream(screenshot)) {
            using (var crop = CropImage(image, rects[0])) { // 取第一張縮圖
              using (var stream = new MemoryStream()) {
                crop.Save(stream, ImageFormat.Bmp);
                var hash = GetMD5Hash(stream.ToArray());
                //crop.Save($"crop/{hash}.png", ImageFormat.Bmp);
                if (hashs.Contains(hash)) return;
                hashs.Add(hash);
              }
            }
          }
        }
        var files = GetPictures(rects);
        var content = driver.FindElementById(ContentElementId);
        var publishedTime = driver.FindElementById(PublishedTimeElementId);
        var post = new Post() {
          VendorId = vendor.VendorId,
          Content = content.Text,
          Date = ParsePublishedTime(publishedTime.Text)
        };
        if (!isDebug) {
          post.Id = WebApi.Insert(post);
          post.Pictures = new List<Post.Picture>();
          for (var i = 0; i < files.Count; i++) {
            var file = files[i];
            var newFile = new FileInfo($"{TempFilePath}/{post.Id}-{i}{file.Extension}");
            using (var image = Image.FromFile(file.FullName)) {
              image.Save(newFile.FullName, jpgEncoder, myEncoderParameters);
            }
            WebApi.UploadPicture(newFile.FullName);
            post.Pictures.Add(new Post.Picture {
              Name = newFile.Name
            });
          }
          WebApi.Update(post);
        }
      } catch (NoSuchElementException) { }; // 沒有找到縮圖列表，此筆貼文不抓
    }
    private ImageCodecInfo GetEncoder(ImageFormat format) {
      ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
      foreach (ImageCodecInfo codec in codecs) {
        if (codec.FormatID == format.Guid) {
          return codec;
        }
      }
      return null;
    }
    // 切換到朋友圈頁面
    private void SwitchToFriendCircle() {
      var element = driver.FindElementById(FriendCircleButtonId);
      element.Click();
      WaitForElement(IdType.Id, PostButtonId);
    }
    private List<FileInfo> GetPictures(IList<Rectangle> rects) {
      // 先清除手機上的WeChat資料夾
      ClearFiles();
      // 依序點擊縮圖，進入顯示原圖畫面，取得所有原圖
      foreach (var rect in rects) {
        Tap(rect);
        WaitForElement(IdType.Id, PictureElementId);
        new TouchAction(driver).LongPress(driver.FindElementById(PictureElementId)).Perform();
        WaitForElement(IdType.Id, SavePictureButtonId);
        var menuItems = driver.FindElementsById(SavePictureButtonId);
        foreach (var menuItem in menuItems) {
          if (SavePictureButtonText.Equals(menuItem.Text)) {
            menuItem.Click();
            Wait(AnimationTime);
            break;
          }
        }
        Back();
      }
      // 抓取手機上的檔案，儲存到PC端
      var files = DownloadFile();
      return files;
    }
    // 清除本地及手機端的暫存資料夾
    private void ClearFiles() {
      var dir = new DirectoryInfo(TempFilePath);
      var files = dir.GetFiles();
      foreach (var file in files)
        file.Delete();
      var args = new Dictionary<string, string> { { "command", "rm" }, { "args", $"-f {WeChatFilePath}/*" } };
      driver.ExecuteScript("mobile: shell", args);
    }
    // 從手機端下載檔案到本地端的暫存資料夾
    private List<FileInfo> DownloadFile() {
      var list = new List<FileInfo>();
      var args = new Dictionary<string, string> { { "command", "ls" }, { "args", $"{WeChatFilePath}" } };
      var output = (string)driver.ExecuteScript("mobile: shell", args);
      var filenames = output.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
      foreach (var filename in filenames) {
        var buffer = driver.PullFile($"{WeChatFilePath}/{filename}");
        using (var file = new FileStream($"{TempFilePath}/{filename}", FileMode.Create)) {
          file.Write(buffer, 0, buffer.Length);
          list.Add(new FileInfo(file.Name));
        }
      }
      return list;
    }
  }
}
