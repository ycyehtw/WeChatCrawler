using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using NLog;

namespace WeChatCrawler {
  class Program {
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();
    private const string LogFile = @"logs\Crawler.log";
    private const string AwakeProgram = "Awake.exe";
    private const string BlueStacksProgram = @"C:\Program Files\BlueStacks\Bluestacks.exe";
    private static Config config;
    static void Main(string[] args) {
      if (Log.IsInfoEnabled) Log.Info("程式開始執行");
      if (Log.IsInfoEnabled) Log.Info("啟動 Awake");
      Process awakeProc = Process.Start(AwakeProgram);
      // 讀取設定
      config = JsonConvert.DeserializeObject<Config>(File.ReadAllText("Config.json"));
      if (!config.IsDebug) {
        var startTime = TimeSpan.Parse(config.StartTime);
        var now = DateTime.Now;
        if (now.TimeOfDay < startTime) {
          var waitTime = startTime - now.TimeOfDay;
          if (Log.IsInfoEnabled) Log.Info($"還沒到達開始時間，等待{waitTime}之後開始執行");
          Thread.Sleep(waitTime);
        } else {
          if (Log.IsInfoEnabled) Log.Info($"已超過開始時間，結束程式");
          awakeProc?.Kill();
          Environment.Exit(0);
        }
      }
      // 不檢查 SSL 憑證合法性
      ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(delegate { return true; });
      WebApi.WebApiServer = config.WebApiServer;
      WebApi.User = config.User;

      var isCompleted = false;
      Process blueStacksProc = null;
      for (var i = 0; i < 10; i++) {
        try {
          if (Log.IsInfoEnabled) Log.Info("啟動 BlueStacks");
          blueStacksProc = Process.Start(BlueStacksProgram);
          Thread.Sleep(30 * 1000);
          if (Log.IsInfoEnabled) Log.Info("啟動 Crawler");
          var crawler = new Crawler(config);
          crawler.Run();
          isCompleted = true;
        } catch (Exception e) {
          if (Log.IsErrorEnabled) Log.Error(e, e.Message);
        } finally {
          blueStacksProc?.Kill();
        }
        if (isCompleted)
          break;
      }
      if (!config.IsDebug) WebApi.Clear(config.CacheLifeTime); //清理過期的資料
      SendMail();
      awakeProc?.Kill();
    }
    private static void SendMail() {
      var subject = $"{DateTime.Today:yyyy-MM-dd} 微信爬蟲日誌";
      // 取得當日Log記錄
      StringBuilder content = new StringBuilder();
      if (File.Exists(LogFile)) {
        using (StreamReader reader = new StreamReader(new FileStream(LogFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), Encoding.UTF8)) {
          string line;
          while ((line = reader.ReadLine()) != null) {
            content.Append($"{line}<br/>");
          }
        }
      }
      Mail(subject, content.ToString());
    }
    private static void Mail(string subject, string content) {
      using (MailMessage msg = new MailMessage()) {
        msg.From = new MailAddress("ycyehtw168@gmail.com", "微信爬蟲");
        msg.To.Add("yingchengyeh@gmail.com");
        msg.Subject = subject;
        msg.SubjectEncoding = Encoding.UTF8;
        msg.Body = content;
        msg.BodyEncoding = Encoding.UTF8;
        msg.IsBodyHtml = true;
        using (SmtpClient client = new SmtpClient("smtp.gmail.com", 587)) {
          client.Credentials = new NetworkCredential("ycyehtw168@gmail.com", "izf851fk6497");
          client.EnableSsl = true;
          client.Send(msg);
        }
      }
    }
  }
}
