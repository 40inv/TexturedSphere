using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Lifetime;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace CG5
{

    public class Polygon : DrawObject //sequence of thick lines
    {
        bool crossProduct(Point a, Point b, Point c)
        {
            Point u = new Point(b.X - a.X, b.Y - a.Y);
            Point v = new Point(c.X - b.X, c.Y - b.Y);
            if (u.X * v.Y - u.Y * v.X < 0)
                return false;
            else
                return true;
        }

        private List<ThickLine> thickLines = new List<ThickLine>();



        public bool isClosed = false;
        public int thickness;
        double avgtextz;
        public Polygon() { }

        public Polygon(List<Point> points, Color color, string name, int thickness) : base(points, color, name)
        {
            this.thickness = thickness;
        }
        public List<Vertex> vertices = new List<Vertex>();

        public Polygon(Vertex v1,Vertex v2, Vertex v3) : base(Color.FromArgb(255,125,125,125), "Triangle")
        {
            vertices.Add(v1);
            vertices.Add(v2);
            vertices.Add(v3);

            this.thickness = 3;
            isClosed = true;
        }

        public Polygon(Vertex v1, Vertex v2, Vertex v3,double avgtz) : base(Color.FromArgb(255, 125, 125, 125), "Triangle")
        {
            avgtextz = avgtz;
            vertices.Add(v1);
            vertices.Add(v2);
            vertices.Add(v3);

            this.thickness = 3;
            isClosed = true;
        }

        public override void drawPoints(Bitmap bmp, BitmapData bitmapData, ref byte[] pixels)
        {
            thickLines.Clear();
            points.Clear();
            points.Add(new Point((int)vertices[0].vertexScaledPostion[0, 0], (int)vertices[0].vertexScaledPostion[1, 0]));
            points.Add(new Point((int)vertices[1].vertexScaledPostion[0, 0], (int)vertices[1].vertexScaledPostion[1, 0]));
            points.Add(new Point((int)vertices[2].vertexScaledPostion[0, 0], (int)vertices[2].vertexScaledPostion[1, 0]));

            createSeqOfLines();

            foreach (ThickLine thickLine in thickLines)
            {
                thickLine.drawPoints(bmp, bitmapData,ref pixels);
            }

        }

        public void drawPointsTextured(Bitmap bmp, BitmapData bitmapData, ref byte[] pixels,
            int bmpFillWidth, byte[] rgbValues, int stride, int bytesPerPixelForImage,
            int textureHeight, int textureWidth, int mode)
        {
            thickLines.Clear();
            points.Clear();
            points.Add(new Point((int)vertices[0].vertexScaledPostion[0, 0], (int)vertices[0].vertexScaledPostion[1, 0]));
            points.Add(new Point((int)vertices[1].vertexScaledPostion[0, 0], (int)vertices[1].vertexScaledPostion[1, 0]));
            points.Add(new Point((int)vertices[2].vertexScaledPostion[0, 0], (int)vertices[2].vertexScaledPostion[1, 0]));

            createSeqOfLines();
            fill(bmp, bitmapData, ref pixels, rgbValues, stride, bytesPerPixelForImage,
            textureHeight, textureWidth);

            //wire + text mode
            if (mode == 2)
            {
                foreach (ThickLine thickLine in thickLines)
                {
                    thickLine.drawPoints(bmp, bitmapData, ref pixels);
                }
            }


        }

        public List<int> makeIndicesList()
        {
            List<Point> sortedPoints = new List<Point>(points);
            sortedPoints = sortedPoints.OrderBy(p => p.Y).ToList();
            List<int> indices = new List<int>();
            for (int i = 0; i < sortedPoints.Count(); i++)
            {
                for (int j = 0; j < points.Count; j++)
                {
                    if (sortedPoints[i] == points[j])
                    {
                        indices.Add(j);
                    }
                }
            }
            return indices;
        }

        //scanline algorithm but only for triangles
        public void fill(Bitmap bmp, BitmapData bitmapData, ref byte[] pixels, byte[] rgbValues, int stride, int bytesPerPixelForImage,
    int textureHeight, int textureWidth)
        {
            int currentLine;
            List<int> indices = makeIndicesList();
            double dx1;
            double dx2;
            double dx3;

            double du;
            double du1;
            double du2;
            double du3;

            double dv;
            double dv1;
            double dv2;
            double dv3;

            Vertex A = new Vertex(vertices[indices[0]]);
            Vertex B = new Vertex(vertices[indices[1]]);
            Vertex C = new Vertex(vertices[indices[2]]);
            if((int)A.vertexScaledPostion[1, 0] == (int)B.vertexScaledPostion[1, 0]) //swap A and B if A is higher than B before casting to int (after swap A will be the left corner and B right)
            {
                if(A.vertexScaledPostion[1, 0] - B.vertexScaledPostion[1, 0] > 0)
                {
                    Vertex tmp = new Vertex(A);
                    A = new Vertex(B);
                    B = new Vertex(tmp);
                }
            }

            //Last slice case when some vertices have texture[0] = 0 and the other ones texture[0] = 1
            //the only way to do it more accurate is to give texture coordinates different than it is written in the given pdf.
            // or add more meridians than this problem is almost not visible
            List<Vertex> ord = new List<Vertex> { vertices[indices[0]], vertices[indices[1]], vertices[indices[2]] };
            ord = ord.OrderBy(p => p.vertexScaledPostion[0, 0]).ToList();
            double URight = Math.Max(vertices[0].texture[0], Math.Max(vertices[1].texture[0], vertices[2].texture[0]));
            double ULeft = Math.Min(vertices[0].texture[0], Math.Min(vertices[1].texture[0], vertices[2].texture[0]));
            if (URight - ULeft == 1) // the last slice problem 
            {
                if (C.texture[0] == 0)
                    C.texture[0] = 1;
                if (A.texture[0] == 0)
                    A.texture[0] = 1;
                if (B.texture[0] == 0)
                    B.texture[0] = 1;
            }



            if ((B.vertexScaledPostion[1, 0] - A.vertexScaledPostion[1, 0]) > 0)
            {
                dx1 = (B.vertexScaledPostion[0, 0] - A.vertexScaledPostion[0, 0]) / (B.vertexScaledPostion[1, 0] - A.vertexScaledPostion[1, 0]);
                du1 = (B.texture[0] - A.texture[0]) / (B.vertexScaledPostion[1, 0] - A.vertexScaledPostion[1, 0]);
                dv1 = (B.texture[1] - A.texture[1]) / (B.vertexScaledPostion[1, 0] - A.vertexScaledPostion[1, 0]);
            }
            else
            {
                dx1 = 0;
                du1 = 0;
                dv1 = 0;
            }
            if ((C.vertexScaledPostion[1, 0] - A.vertexScaledPostion[1, 0]) > 0)
            {
                dx2 = (C.vertexScaledPostion[0, 0] - A.vertexScaledPostion[0, 0]) / (C.vertexScaledPostion[1, 0] - A.vertexScaledPostion[1, 0]);
                du2 = (C.texture[0] - A.texture[0]) / ((C.vertexScaledPostion[1, 0] - A.vertexScaledPostion[1, 0]));
                dv2 = (C.texture[1] - A.texture[1]) / (C.vertexScaledPostion[1, 0] - A.vertexScaledPostion[1, 0]);
            }
            else
            {
                dx2 = 0;
                du2 = 0;
                dv2 = 0;
            }
            if ((C.vertexScaledPostion[1, 0] - B.vertexScaledPostion[1, 0]) > 0)
            {
                dx3 = (C.vertexScaledPostion[0, 0] - B.vertexScaledPostion[0, 0]) / (C.vertexScaledPostion[1, 0] - B.vertexScaledPostion[1, 0]);
                du3 = (C.texture[0] - B.texture[0]) / (C.vertexScaledPostion[1, 0] - B.vertexScaledPostion[1, 0]);
                dv3 = (C.texture[1] - B.texture[1]) / (C.vertexScaledPostion[1, 0] - B.vertexScaledPostion[1, 0]);
            }
            else
            {
                dx3 = 0;
                du3 = 0;
                dv3 = 0;
            }


            Vertex S = new Vertex(A);
            Vertex E = new Vertex(A);
            //(B.vertexScaledPostion[1, 0] - A.vertexScaledPostion[1, 0]) == 0 is quite rare case but if it is occured it should follow this path or x1 will be greater than x1 
            if (dx1 >= dx2 || (B.vertexScaledPostion[1, 0] - A.vertexScaledPostion[1, 0])==0)
            {
                for (; (int)S.vertexScaledPostion[1, 0] < (int)B.vertexScaledPostion[1, 0]; S.vertexScaledPostion[1, 0]++, E.vertexScaledPostion[1, 0]++, S.vertexScaledPostion[0, 0] += dx2, E.vertexScaledPostion[0, 0] += dx1)
                {

                    int x1 = (int)S.vertexScaledPostion[0, 0];
                    int x2 = (int)E.vertexScaledPostion[0, 0];

                    if (x2 - x1 > 0)
                    {
                        du = (E.texture[0] - S.texture[0]) / (x2 - x1);
                        dv = (E.texture[1] - S.texture[1]) / (x2 - x1);
                    }
                    else
                    {
                        du = 0;
                        dv = 0;
                    }
                    Vertex P = new Vertex(S);

                    while (x1 <= x2)
                    {
                        currentLine = (int)((int)S.vertexScaledPostion[1, 0] * bitmapData.Stride);
                        int imageCurrentLine = (int)(P.texture[1] * (textureHeight - 1)) * stride;
                        int xImageIndex = (int)(P.texture[0] * (textureWidth - 1));
                        if (canBeSet((int)x1, (int)S.vertexScaledPostion[1, 0], bmp))
                        {

                            pixels[currentLine + x1 * 4] = rgbValues[imageCurrentLine + xImageIndex * bytesPerPixelForImage]; //blue
                            pixels[1 + currentLine + x1 * 4] = rgbValues[1 + imageCurrentLine + xImageIndex * bytesPerPixelForImage]; //green
                            pixels[2 + currentLine + x1 * 4] = rgbValues[2 + imageCurrentLine + xImageIndex * bytesPerPixelForImage]; //red
                        }

                        P.texture[0] += du;
                        P.texture[1] += dv;
                        x1++;
                    }
                    S.texture[0] += du2;
                    S.texture[1] += dv2;

                    E.texture[0] += du1;
                    E.texture[1] += dv1;
                }

                E = new Vertex(B);
                for (; (int)S.vertexScaledPostion[1, 0] < (int)C.vertexScaledPostion[1, 0]; S.vertexScaledPostion[1, 0]++, E.vertexScaledPostion[1, 0]++, S.vertexScaledPostion[0, 0] += dx2, E.vertexScaledPostion[0, 0] += dx3)
                {
                    int x1 = (int)S.vertexScaledPostion[0, 0];
                    int x2 = (int)E.vertexScaledPostion[0, 0];
                    if (x2 < x1)
                        ;

                    Vertex P;

                    P = new Vertex(S);
                    if (x2 - x1 > 0)
                    {
                        du = (E.texture[0] - S.texture[0]) / (x2 - x1);
                        dv = (E.texture[1] - S.texture[1]) / (x2 - x1);

                    }
                    else
                    {
                        du = 0;
                        dv = 0;
                    }


                while (x1 <= x2)
                    {
                        if (P.texture[1] < 0)
                            P.texture[1] = 0;
                        else if (P.texture[1] > 1)
                            P.texture[1] = 1;
                        currentLine = (int)((int)S.vertexScaledPostion[1, 0] * bitmapData.Stride);
                        int imageCurrentLine = (int)(P.texture[1] * (textureHeight - 1)) * stride;
                        int xImageIndex = (int)(P.texture[0] * (textureWidth - 1));

                        if (canBeSet((int)x1, (int)S.vertexScaledPostion[1, 0], bmp))
                        {
                            pixels[currentLine + x1 * 4] = rgbValues[imageCurrentLine + xImageIndex * bytesPerPixelForImage]; //blue
                            pixels[1 + currentLine + x1 * 4] = rgbValues[1 + imageCurrentLine + xImageIndex * bytesPerPixelForImage]; //green
                            pixels[2 + currentLine + x1 * 4] = rgbValues[2 + imageCurrentLine + xImageIndex * bytesPerPixelForImage]; //red
                        }

                        P.texture[0] += du;
                        P.texture[1] += dv;
                        x1++;
                    }
                    S.texture[0] += du2;
                    S.texture[1] += dv2;

                    E.texture[0] += du3;
                    E.texture[1] += dv3;
                }
            }
            else
            {
                for (; (int)S.vertexScaledPostion[1, 0] < (int)B.vertexScaledPostion[1, 0]; S.vertexScaledPostion[1, 0]++, E.vertexScaledPostion[1, 0]++, S.vertexScaledPostion[0, 0] += dx1, E.vertexScaledPostion[0, 0] += dx2)
                {
                    int x1 = (int)S.vertexScaledPostion[0, 0];
                    int x2 = (int)E.vertexScaledPostion[0, 0];
                    if (x2 - x1 > 0)
                    {
                        du = (E.texture[0] - S.texture[0]) / (x2 - x1);
                        dv = (E.texture[1] - S.texture[1]) / (x2 - x1);
                    }
                    else
                    {
                        du = 0;
                        dv = 0;
                    }
                    Vertex P = new Vertex(S);

                    while (x1 < x2)
                    {
                        currentLine = (int)((int)S.vertexScaledPostion[1, 0] * bitmapData.Stride);
                        int imageCurrentLine = (int)(P.texture[1] * (textureHeight - 1)) * stride;
                        int xImageIndex = (int)(P.texture[0] * (textureWidth - 1));

                        if (canBeSet((int)x1, (int)S.vertexScaledPostion[1, 0], bmp))
                        {
                            pixels[currentLine + x1 * 4] = rgbValues[imageCurrentLine + xImageIndex * bytesPerPixelForImage]; //blue
                            pixels[1 + currentLine + x1 * 4] = rgbValues[1 + imageCurrentLine + xImageIndex * bytesPerPixelForImage]; //green
                            pixels[2 + currentLine + x1 * 4] = rgbValues[2 + imageCurrentLine + xImageIndex * bytesPerPixelForImage]; //red
                        }

                        P.texture[0] = P.texture[0] + du;
                        P.texture[1] = P.texture[1] + dv;
                        x1++;
                    }

                    S.texture[0] += du1;
                    S.texture[1] += dv1;

                    E.texture[0] += du2;
                    E.texture[1] += dv2;

                }

                S = new Vertex(B);
                for (; (int)S.vertexScaledPostion[1, 0] < (int)C.vertexScaledPostion[1, 0]; S.vertexScaledPostion[1, 0]++, E.vertexScaledPostion[1, 0]++, S.vertexScaledPostion[0, 0] += dx3, E.vertexScaledPostion[0, 0] += dx2)
                {
                    int x1 = (int)S.vertexScaledPostion[0, 0];
                    int x2 = (int)E.vertexScaledPostion[0, 0];

                    if (x2 - x1 > 0)
                    {

                        du = (E.texture[0] - S.texture[0]) / (x2 - x1);
                        dv = (E.texture[1] - S.texture[1]) / (x2 - x1);

                    }
                    else
                    {
                        du = 0;
                        dv = 0;
                    }

                    Vertex P = new Vertex(S);

                    while (x1 < x2)
                    {
                        currentLine = (int)((int)S.vertexScaledPostion[1, 0] * bitmapData.Stride);
                        if (P.texture[1] < 0)
                            P.texture[1] = 0;
                        else if (P.texture[1] > 1)
                            P.texture[1] = 1;
                        int imageCurrentLine = (int)(P.texture[1] * (textureHeight - 1)) * stride;
                        int xImageIndex = (int)(P.texture[0] * (textureWidth - 1));

                        if (canBeSet((int)x1, (int)S.vertexScaledPostion[1, 0], bmp))
                        {
                            pixels[currentLine + x1 * 4] = rgbValues[imageCurrentLine + xImageIndex * bytesPerPixelForImage]; //blue
                            pixels[1 + currentLine + x1 * 4] = rgbValues[1 + imageCurrentLine + xImageIndex * bytesPerPixelForImage]; //green
                            pixels[2 + currentLine + x1 * 4] = rgbValues[2 + imageCurrentLine + xImageIndex * bytesPerPixelForImage]; //red
                        }

                        P.texture[0] += du;
                        P.texture[1] += dv;
                        x1++;
                    }
                    S.texture[0] += du3;
                    S.texture[1] += dv3;

                    E.texture[0] += du2;
                    E.texture[1] += dv2;

                }
            }


        }

        void createSeqOfLines()
        {
            for (int i = 0; i < points.Count(); i++)
            {
                List<Point> tmpPoints = new List<Point>();
                tmpPoints.Add(points[i]);
                if (i + 1 == points.Count)
                    tmpPoints.Add(points[0]);
                else
                    tmpPoints.Add(points[i + 1]);
                thickLines.Add(new ThickLine(tmpPoints, System.Drawing.Color.FromArgb(red, green, blue), "thick line", thickness));
            }
        }


    }
}
