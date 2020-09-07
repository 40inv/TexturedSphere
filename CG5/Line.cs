using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace CG5
{
    //public class Line : DrawObject
    //{

    //    public Line() { }
    //    public Line(List<Point> points, Color color, string name) : base(points, color, name) { }
    //    public override BitmapImage drawPoints(BitmapImage sourceImg)
    //    {
    //        if (this.points.Count() != 2)
    //            return sourceImg;

    //        Point startPoint = this.points[0];
    //        Point endPoint = this.points[1];
    //        if (Math.Abs(endPoint.Y - startPoint.Y) < Math.Abs(endPoint.X - startPoint.X))
    //        {
    //            if (startPoint.X > endPoint.X)
    //                return drawMoreHorizontal(endPoint.X, endPoint.Y, startPoint.X, startPoint.Y, sourceImg);
    //            else
    //                return drawMoreHorizontal(startPoint.X, startPoint.Y, endPoint.X, endPoint.Y, sourceImg);
    //        }
    //        else
    //        {
    //            if (startPoint.Y > endPoint.Y)
    //                return drawMoreVertical(endPoint.X, endPoint.Y, startPoint.X, startPoint.Y, sourceImg);
    //            else
    //                return drawMoreVertical(startPoint.X, startPoint.Y, endPoint.X, endPoint.Y, sourceImg);
    //        }

    //    }

    //    void setPixel(int x, int y, ref byte[] pixels, BitmapData bitmapData)
    //    {
    //        int currentLine = y * bitmapData.Stride;
    //        pixels[currentLine + x * 4] = blue; //blue
    //        pixels[currentLine + x * 4 + 1] = green; //green
    //        pixels[currentLine + x * 4 + 2] = red; //red
    //    }

    //    BitmapImage drawMoreHorizontal(int x1, int y1, int x2, int y2, BitmapImage sourceImg)
    //    {

    //        BitmapConverter bc = new BitmapConverter();
    //        Bitmap bmp = bc.ToBitmap(sourceImg);

    //        BitmapData bitmapData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, bmp.PixelFormat);
    //        int bytesPerPixel = Bitmap.GetPixelFormatSize(bmp.PixelFormat) / 8;
    //        int byteCount = bitmapData.Stride * bmp.Height;
    //        byte[] pixels = new byte[byteCount];
    //        IntPtr ptrFirstPixel = bitmapData.Scan0;
    //        Marshal.Copy(ptrFirstPixel, pixels, 0, pixels.Length);

    //        int dx = x2 - x1;
    //        int dy = y2 - y1;

    //        int yi = 1;
    //        if (dy < 0)
    //        {
    //            yi = -1;
    //            dy = -dy;
    //        }

    //        int d = 2 * dy - dx;
    //        int dE = 2 * dy;
    //        int dNE = 2 * (dy - dx);
    //        int xf = x1; int yf = y1;
    //        int xb = x2; int yb = y2;

    //        setPixel(xf, yf, ref pixels, bitmapData);
    //        setPixel(xb, yb, ref pixels, bitmapData);

    //        while (xf < xb)
    //        {
    //            ++xf;
    //            --xb;
    //            if (d < 0)
    //                d += dE;
    //            else
    //            {
    //                d += dNE;
    //                yf = yf + yi;
    //                yb = yb - yi;
    //            }
    //            setPixel(xf, yf, ref pixels, bitmapData);
    //            setPixel(xb, yb, ref pixels, bitmapData);

    //        }

    //        Marshal.Copy(pixels, 0, ptrFirstPixel, pixels.Length);
    //        bmp.UnlockBits(bitmapData);
    //        return bc.ToBitmapImage(bmp);
    //    }

    //    BitmapImage drawMoreVertical(int x1, int y1, int x2, int y2, BitmapImage sourceImg)
    //    {

    //        BitmapConverter bc = new BitmapConverter();
    //        Bitmap bmp = bc.ToBitmap(sourceImg);

    //        BitmapData bitmapData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, bmp.PixelFormat);
    //        int bytesPerPixel = Bitmap.GetPixelFormatSize(bmp.PixelFormat) / 8;
    //        int byteCount = bitmapData.Stride * bmp.Height;
    //        byte[] pixels = new byte[byteCount];
    //        IntPtr ptrFirstPixel = bitmapData.Scan0;
    //        Marshal.Copy(ptrFirstPixel, pixels, 0, pixels.Length);

    //        int dx = x2 - x1;
    //        int dy = y2 - y1;

    //        int xi = 1;
    //        if (dx < 0)
    //        {
    //            xi = -1;
    //            dx = -dx;
    //        }

    //        int d = (2 * dx) - dy;
    //        int dE = 2 * dx;
    //        int dNE = 2 * (dx - dy);
    //        int xf = x1; int yf = y1;
    //        int xb = x2; int yb = y2;

    //        setPixel(xf, yf, ref pixels, bitmapData);
    //        setPixel(xb, yb, ref pixels, bitmapData);

    //        while (yf < yb)
    //        {
    //            ++yf;
    //            --yb;
    //            if (d < 0)
    //                d += dE;
    //            else
    //            {
    //                d += dNE;
    //                xf = xf + xi;
    //                xb = xb - xi;
    //            }

    //            setPixel(xf, yf, ref pixels, bitmapData);
    //            setPixel(xb, yb, ref pixels, bitmapData);

    //        }

    //        Marshal.Copy(pixels, 0, ptrFirstPixel, pixels.Length);
    //        bmp.UnlockBits(bitmapData);
    //        return bc.ToBitmapImage(bmp);
    //    }

    //}

}
