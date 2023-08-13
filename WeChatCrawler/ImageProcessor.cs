using System;
using System.Drawing;
using System.Drawing.Text;

namespace WeChatCrawler {
  class ImageProcessor {
    const string Logo = "logo.png";
    const string FontName = "華康少女文字w6";
    const int FontSize = 40;
    const float TemplateLogoRatio = 2 / 3f;
    const float TemplateWidth = 1024;
    const int TextRightMargin = 50;
    const int TextBottomMargin = 36;
    private readonly Image logo;
    private readonly Rectangle logoRect;
    private readonly SolidBrush brush;
    private readonly FontFamily fontFamily;
    public ImageProcessor() {
      logo = Image.FromFile(Logo);
      logoRect = new Rectangle(0, 0, logo.Width, logo.Height);
      brush = new SolidBrush(Color.White);
      var collection = new PrivateFontCollection();
      collection.AddFontFile($"{FontName}.ttf");
      fontFamily = new FontFamily(FontName, collection);
    }

    public Image DrawLogoAndText(Image image, string text) {
      var ratio = TemplateLogoRatio * image.Width / TemplateWidth;
      var font = new Font(fontFamily, Math.Max(FontSize * ratio, 32), FontStyle.Bold, GraphicsUnit.Pixel);
      using (var graphic = Graphics.FromImage(image)) {
        var textSize = graphic.MeasureString(text, font);
        var x = (int)Math.Min(image.Width - logo.Width * ratio, image.Width - textSize.Width - TextRightMargin);
        var destRect = new Rectangle(x, (int)(image.Height - logo.Height * ratio) / 2, (int)(logo.Width * ratio), (int)(logo.Height * ratio));
        // LOGO          
        graphic.DrawImage(logo, destRect, logoRect, GraphicsUnit.Pixel);
        // 文字          
        graphic.DrawString(text, font, brush, x, image.Height - textSize.Height - TextBottomMargin);
      }
      return image;
    }
  }
}
