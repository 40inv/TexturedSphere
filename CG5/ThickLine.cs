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
    public class ThickLine : DrawObject
    {
        public int thickness;
        public ThickLine() { }
        public ThickLine(List<Point> points, Color color, string name, int thickness) : base(points, color, name)
        {
            this.thickness = thickness;
        }
        public override void drawPoints(Bitmap bmp, BitmapData bitmapData, ref byte[] pixels)
        {

            Point startPoint = this.points[0];
            Point endPoint = this.points[1];
            if (Math.Abs(endPoint.Y - startPoint.Y) < Math.Abs(endPoint.X - startPoint.X))
            {
                if (startPoint.X > endPoint.X)
                    drawMoreHorizontal(endPoint.X, endPoint.Y, startPoint.X, startPoint.Y, bmp, bitmapData, ref pixels);
                else
                    drawMoreHorizontal(startPoint.X, startPoint.Y, endPoint.X, endPoint.Y, bmp, bitmapData, ref pixels);
            }
            else
            {
                if (startPoint.Y > endPoint.Y)
                    drawMoreVertical(endPoint.X, endPoint.Y, startPoint.X, startPoint.Y, bmp, bitmapData, ref pixels);
                else
                    drawMoreVertical(startPoint.X, startPoint.Y, endPoint.X, endPoint.Y, bmp, bitmapData, ref pixels);
            }
        }

        void setPixel(int x, int y, ref byte[] pixels, BitmapData bitmapData)
        {
            int currentLine = y * bitmapData.Stride;
            pixels[currentLine + x * 4] = blue; //blue
            pixels[currentLine + x * 4 + 1] = green; //green
            pixels[currentLine + x * 4 + 2] = red; //red
        }

        void drawMoreHorizontal(int x1, int y1, int x2, int y2, Bitmap bmp, BitmapData bitmapData, ref byte[] pixels)
        {
            int numOfIter = (thickness / 3);

            int dx = x2 - x1;
            int dy = y2 - y1;

            int yi = 1;
            if (dy < 0)
            {
                yi = -1;
                dy = -dy;
            }

            int d = 2 * dy - dx;
            int dE = 2 * dy;
            int dNE = 2 * (dy - dx);
            int xf = x1; int yf = y1;
            int xb = x2; int yb = y2;
            if (canBeSet(xf, yf, bmp))
                setPixel(xf, yf, ref pixels, bitmapData);
            if (canBeSet(xb, yb, bmp))
                setPixel(xb, yb, ref pixels, bitmapData);
            ;
            for (int currentIter = -numOfIter; currentIter <= numOfIter; currentIter++)
            {
                if (currentIter == 0) continue;

                if (canBeSet(xf, yf + currentIter, bmp))
                    setPixel(xf, yf + currentIter, ref pixels, bitmapData);
                if (canBeSet(xb, yb + currentIter, bmp))
                    setPixel(xb, yb + currentIter, ref pixels, bitmapData);
            }
            while (xf < xb)
            {
                ++xf;
                --xb;
                if (d < 0)
                    d += dE;
                else
                {
                    d += dNE;
                    yf = yf + yi;
                    yb = yb - yi;
                }
                if (canBeSet(xf, yf, bmp))
                    setPixel(xf, yf, ref pixels, bitmapData);
                if (canBeSet(xb, yb, bmp))
                    setPixel(xb, yb, ref pixels, bitmapData);

                for (int currentIter = -numOfIter; currentIter <= numOfIter; currentIter++)
                {
                    if (currentIter == 0) continue;
                    if (canBeSet(xf, yf + currentIter, bmp))
                        setPixel(xf, yf + currentIter, ref pixels, bitmapData);
                    if (canBeSet(xb, yb + currentIter, bmp))
                        setPixel(xb, yb + currentIter, ref pixels, bitmapData);
                }
            }



        }

        void drawMoreVertical(int x1, int y1, int x2, int y2, Bitmap bmp, BitmapData bitmapData, ref byte[] pixels)
        {

            int numOfIter = (thickness / 3);

            int dx = x2 - x1;
            int dy = y2 - y1;

            int xi = 1;
            if (dx < 0)
            {
                xi = -1;
                dx = -dx;
            }

            int d = (2 * dx) - dy;
            int dE = 2 * dx;
            int dNE = 2 * (dx - dy);
            int xf = x1; int yf = y1;
            int xb = x2; int yb = y2;
            if (canBeSet(xf, yf, bmp))
                setPixel(xf, yf, ref pixels, bitmapData);
            if (canBeSet(xb, yb, bmp))
                setPixel(xb, yb, ref pixels, bitmapData);

            for (int currentIter = -numOfIter; currentIter <= numOfIter; currentIter++)
            {
                if (currentIter == 0) continue;
                if (canBeSet(xf + currentIter, yf, bmp))
                    setPixel(xf + currentIter, yf, ref pixels, bitmapData);
                if (canBeSet(xb + currentIter, yb, bmp))
                    setPixel(xb + currentIter, yb, ref pixels, bitmapData);

            }
            while (yf < yb)
            {
                ++yf;
                --yb;
                if (d < 0)
                    d += dE;
                else
                {
                    d += dNE;
                    xf = xf + xi;
                    xb = xb - xi;
                }

                if (canBeSet(xf, yf, bmp))
                    setPixel(xf, yf, ref pixels, bitmapData);
                if (canBeSet(xb, yb, bmp))
                    setPixel(xb, yb, ref pixels, bitmapData);
                for (int currentIter = -numOfIter; currentIter <= numOfIter; currentIter++)
                {
                    if (currentIter == 0) continue;

                    if (canBeSet(xf + currentIter, yf, bmp))
                        setPixel(xf + currentIter, yf, ref pixels, bitmapData);
                    if (canBeSet(xb + currentIter, yb, bmp))
                        setPixel(xb + currentIter, yb, ref pixels, bitmapData);

                }
            }
        }
    }
}
