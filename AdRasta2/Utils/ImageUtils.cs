using System.Globalization;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace AdRasta2.Utils;

public class ImageUtils
{
    public static Bitmap CreateBlankImage(int width, int height)
    {
        var renderTarget = new RenderTargetBitmap(new PixelSize(width, height), new Vector(96, 96));

        using (var ctx = renderTarget.CreateDrawingContext(true))
        {
            // fill with transparent or solid color
            ctx.FillRectangle(Brushes.WhiteSmoke, new Rect(0, 0, width, height));
            
            // Prepare watermark text
            var text = new FormattedText(
                "No Image",
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("Segoe UI",FontStyle.Italic,FontWeight.Medium),
                30,
                new SolidColorBrush(Color.FromArgb(128, 0, 0, 0)) // 50% transparent black
            );
            
            // Center the text
            var y = (height - text.Height) / 2;
            var x = (width - text.Width) / 2;

            ctx.DrawText(text, new Point(x, y));
        }
        
        return renderTarget;
    }
}