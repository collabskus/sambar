using System.Windows.Media.Imaging;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using System.Diagnostics;

namespace sambar;

public static class Extensions
{
    public static bool ContainsFlag(this uint flag, uint flagToCheck)
    {
       if((flag & flagToCheck) != 0)
       {
            return true;
       }
        return false;
    } 

    public static BitmapImage FromBitmap(this BitmapImage self, Bitmap bitmap)
    {
        using(MemoryStream ms = new())
        {
            bitmap.Save(ms, ImageFormat.Png);
            ms.Position = 0;

            self.BeginInit();
            self.StreamSource = ms;
            self.CacheOption = BitmapCacheOption.OnLoad;
            self.EndInit();
            self.Freeze();

            Debug.WriteLine($"ICON_B: {bitmap.Width}x{bitmap.Height}");
            Debug.WriteLine($"ICON_BP: {self.Width}x{self.Height}");
            return self;
        }
    }
}
