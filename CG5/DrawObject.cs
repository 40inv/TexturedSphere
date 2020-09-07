using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace CG5
{
    abstract public class DrawObject
    {
        public string name;
        public byte red;
        public byte green;
        public byte blue;
        public List<Point> points = new List<Point>();
        public DrawObject() { }

        protected bool canBeSet(int x, int y, Bitmap bmp)
        {
            if (x >= bmp.Width || x <= 0 || y >= bmp.Height || y < 0)
                return false;
            return true;
        }

        public DrawObject(List<Point> points, Color color, string name)
        {
            this.name = name;
            red = color.R;
            green = color.G;
            blue = color.B;
            foreach (var point in points)
                this.points.Add(point);
        }

        public DrawObject(Color color, string name)
        {
            this.name = name;
            red = color.R;
            green = color.G;
            blue = color.B;

        }

        public void addPoint(Point point)
        {
            points.Add(point);
        }

        public abstract void drawPoints(Bitmap bmp, BitmapData bitmapData, ref byte[] pixels);
    }
}
