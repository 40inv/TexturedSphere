using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace CG5
{

    public partial class MainWindow : Window
    {
        bool isTextured = true; // if false without textures; true with textures
        int mode = 1;

        int HEIGHT = 600;
        int WIDTH = 600;
        string pathImageToFill;
        int bytesPerPixelForTexture;
        int textureHeight;
        int textureWidth;
        int textureStride;
        byte[] textureRGB;

        double theta = Math.PI / 1.7;
        double s;
        int r = 5; //radius
        int m = 150; //number of meridians
        int n = 15; //number of parallels in the subdivision

        double CAMERA_POSITION_X = 0;
        double CAMERA_POSITION_Y = 0;
        double CAMERA_POSITION_Z;
        double increment = 0.1;
        double zoom_increment = 0.2;

        double avgtextz;
        double alpha_x = 0;
        double alpha_y = 0;
        double alpha_z = 0;

        double[,] Teditable;
        double[,] Tbasic;
        double[,] P;
        Vertex[] vertices;
        Polygon[] triangles;

        public MainWindow()
        {
            string currectDirectory = Directory.GetCurrentDirectory();
            pathImageToFill = Path.GetFullPath(Path.Combine(currectDirectory, @"..\..\earth.bmp")); // or change to your image path

            InitializeComponent();
            initializeImage();
            openTexture();
            CAMERA_POSITION_Z = r * 1.5;

            s = (WIDTH / 2) * (1 / Math.Tan(theta / 2));
            
            P = new double[,]{
                { s, 0, WIDTH / 2, 0} ,
                { 0, -s, HEIGHT / 2, 0 },
                { 0, 0, 0, 1},
                { 0, 0, 1, 0 }
            };

            Teditable = new double[,]
            {
            { 1, 0, 0, CAMERA_POSITION_X},
            { 0, 1, 0, CAMERA_POSITION_Y},
            { 0, 0, 1, CAMERA_POSITION_Z},
            { 0, 0, 0, 1}
            };

            Tbasic = new double[,]
            {
            {1, 0, 0, CAMERA_POSITION_X},
            {0, 1, 0, CAMERA_POSITION_Y},
            {0, 0, 1, CAMERA_POSITION_Z},
            {0, 0, 0, 1}
            };

            vertices = new Vertex[m * n + 2];
            initializeVertices(vertices);
            produceTransformations();
            projectVertices(vertices);
            triangles = new Polygon[n * m * 2];
            create_triangles(triangles, vertices);
            draw_mesh(triangles);
        }

        void openTexture()
        {
            Bitmap bmpFill = new Bitmap(pathImageToFill);
            textureHeight = bmpFill.Height;
            textureWidth = bmpFill.Width;
            Rectangle rect = new Rectangle(0, 0, bmpFill.Width, bmpFill.Height);
            BitmapData bmpFillData = bmpFill.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite,
                 bmpFill.PixelFormat);
            bytesPerPixelForTexture = Bitmap.GetPixelFormatSize(bmpFill.PixelFormat) / 8;
            textureStride = bmpFillData.Stride;
            IntPtr ptr = bmpFillData.Scan0;
            int bytes = Math.Abs(bmpFillData.Stride) * bmpFillData.Height;
            textureRGB = new byte[bytes];
            System.Runtime.InteropServices.Marshal.Copy(ptr, textureRGB, 0, bytes);
            bmpFill.UnlockBits(bmpFillData);

        }

        void initializeImage()
        {
            int rgb = 255;
            BitmapConverter bc = new BitmapConverter();
            Bitmap bmp = new Bitmap(WIDTH, HEIGHT);
            using (Graphics gfx = Graphics.FromImage(bmp))
            using (SolidBrush brush = new SolidBrush(System.Drawing.Color.FromArgb(rgb, rgb, rgb)))
            {
                gfx.FillRectangle(brush, 0, 0, WIDTH, HEIGHT);
            }
            drawingAreaImage.Source = bc.ToBitmapImage(bmp);
        }

        void initializeVertices(Vertex[] vertices)
        {
            vertices[0] = new Vertex(0, r, 0, r);

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    double a = r * Math.Cos(2 * Math.PI * j / m) * Math.Sin(Math.PI * (i + 1) / (n + 1));
                    double b = r * Math.Cos(Math.PI * (i + 1) / (n + 1));
                    double c = r * Math.Sin(2 * Math.PI * j / m) * Math.Sin(Math.PI * (i + 1) / (n + 1));
                    vertices[i * m + j + 1] = new Vertex(a, b, c, r);
                }

            }

            vertices[m * n + 1] = new Vertex(0, -r, 0, r);
            avgtextz = 1.0 / (m - 1.0);
            //top pole
            vertices[0].texture[0] = 0.5; //for x
            vertices[0].texture[1] = 0; // for y
            vertices[0].average = avgtextz;

            //other vertices
            for (int i = 0; i < n; i++)
                for (int j = 0; j < m; j++)
                {
                    vertices[i * m + j + 1].average = avgtextz;
                    vertices[i * m + j + 1].texture[0] = (double)j / (double)(m - 1.0);
                    vertices[i * m + j + 1].texture[1] = ((double)(i + 1.0) / (double)(n + 1.0));
                }
            //bottom pole
            vertices[m * n + 1].average = avgtextz;
            vertices[m * n + 1].texture[0] = 0.5;
            vertices[m * n + 1].texture[1] = 1;
        }

        void create_triangles(Polygon[] triangles, Vertex[] vertices)
        {
            for (int i = 0; i <= m - 2; i++)
            {
                triangles[i] = new Polygon(vertices[0], vertices[i + 2], vertices[i + 1]);
                triangles[2 * (n - 1) * m + i + m] = new Polygon(vertices[m * n + 1], vertices[(n - 1) * m + i + 1], vertices[(n - 1) * m + i + 2]);
            }
            triangles[m - 1] = new Polygon(vertices[0], vertices[1], vertices[m]);
            triangles[2 * (n - 1) * m + m - 1 + m] = new Polygon(vertices[m * n + 1], vertices[m * n], vertices[(n - 1) * m + 1]);
            for (int i = 0; i <= n - 2; i++)
            {
                for (int j = 1; j <= m - 1; j++)
                {
                    triangles[(2 * i + 1) * m + j - 1] = new Polygon(vertices[i * m + j], vertices[i * m + j + 1], vertices[(i + 1) * m + j + 1], avgtextz);
                    triangles[(2 * i + 2) * m + j - 1] = new Polygon(vertices[i * m + j], vertices[(i + 1) * m + j + 1], vertices[(i + 1) * m + j], avgtextz);

                }

                triangles[(2 * i + 1) * m + m - 1] = new Polygon(vertices[(i + 1) * m], vertices[i * m + 1], vertices[(i + 1) * m + 1], avgtextz);
                triangles[(2 * i + 2) * m + m - 1] = new Polygon(vertices[(i + 1) * m], vertices[(i + 1) * m + 1], vertices[(i + 2) * m], avgtextz);
            }
        }


       void produceTransformations()
        {
            Tbasic[0, 3] = CAMERA_POSITION_X;
            Tbasic[1, 3] = CAMERA_POSITION_Y;
            Tbasic[2,3] = CAMERA_POSITION_Z;
            double[,] Rx =
            {
                { 1, 0, 0, 0} ,
                { 0, Math.Cos(alpha_x), -Math.Sin(alpha_x), 0},
                { 0, Math.Sin(alpha_x), Math.Cos(alpha_x), 0},
                { 0, 0, 0, 1 }
            };

            double[,] Ry = {
                { Math.Cos(alpha_y), 0, Math.Sin(alpha_y), 0},
                { 0, 1, 0, 0},
                { -Math.Sin(alpha_y), 0, Math.Cos(alpha_y), 0},
                { 0, 0, 0, 1}
            };
            double[,] Rz = {
                { Math.Cos(alpha_z), -Math.Sin(alpha_z), 0, 0},
		        { Math.Sin(alpha_z), Math.Cos(alpha_z), 0, 0},
		        { 0, 0, 1, 0},
		        { 0, 0, 0, 1}
            };

            double[,] tmpT = {
            {0, 0, 0, 0},
            {0, 0, 0, 0},
            {0, 0, 0, 0},
            {0, 0, 0, 0}
            };
            matrixMultiplication(tmpT, Tbasic, Rx);
            double[,] tmpT2 = {
            {0, 0, 0, 0},
            {0, 0, 0, 0},
            {0, 0, 0, 0},
            {0, 0, 0, 0}
            };
            matrixMultiplication(tmpT2, tmpT, Ry);
            double[,] tmpT3 = {
                { 0, 0, 0, 0},
	            { 0, 0, 0, 0},
	            { 0, 0, 0, 0},
	            { 0, 0, 0, 0}
            };
            matrixMultiplication(tmpT3, tmpT2, Rz);
            for (int i = 0; i < 4; ++i)
                for (int j = 0; j < 4; ++j)
                    Teditable[i,j] = tmpT3[i,j];

        }

        void matrixMultiplication(double[,] outMatrix, double[,] a, double[,] b)
        {
            for (int i = 0; i < 4; ++i)
                for (int j = 0; j < 4; ++j)
                    for (int k = 0; k < 4; ++k)
                    {
                        outMatrix[i,j] += a[i,k] * b[k,j];
                    }
        }


        void projectVertices(Vertex[] vertices)
        {
            for (int itr = 0; itr < m * n + 2; itr++)
            {
                double[,] transformedP = {
                    {0, 0, 0, 0},
                    {0, 0, 0, 0},
                    {0, 0, 0, 0},
                    {0, 0, 0, 0}
                    };

                matrixMultiplication(transformedP, P, Teditable);
                double[,] p2 = { { 0 }, { 0 }, { 0 }, { 0 } };

                for (int i = 0; i < 4; ++i)
                    for (int j = 0; j < 1; ++j)
                        for (int k = 0; k < 4; ++k)
                        {
                            p2[i, j] += transformedP[i, k] * vertices[itr].vertexNotScaledPostion[k, j];
                        }

                for (int i = 0; i < 4; i++)
                {
                    vertices[itr].vertexScaledPostion[i, 0] = p2[i, 0] / p2[3, 0];
                }

            }

        }

        double crossProductForBF(Vector3D v1, Vector3D v2)
        {
            return v1.X * v2.Y - v2.X * v1.Y;
        }

        void draw_mesh(Polygon[] T)
        {
            BitmapConverter bc = new BitmapConverter();
            Bitmap bmp = bc.ToBitmap((BitmapImage)drawingAreaImage.Source);

            BitmapData bitmapData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, bmp.PixelFormat);
            int byteCount = bitmapData.Stride * bmp.Height;
            byte[] pixels = new byte[byteCount];
            IntPtr ptrFirstPixel = bitmapData.Scan0;
            Marshal.Copy(ptrFirstPixel, pixels, 0, pixels.Length);

            for (int i = 0; i < 2 * n * m; i++)
            //for (int i = 0; i < 14; i++)
            {
                double x1 = T[i].vertices[0].vertexScaledPostion[0, 0];
                double y1 = T[i].vertices[0].vertexScaledPostion[1, 0];
                double x2 = T[i].vertices[1].vertexScaledPostion[0, 0];
                double y2 = T[i].vertices[1].vertexScaledPostion[1, 0];
                double x3 = T[i].vertices[2].vertexScaledPostion[0, 0];
                double y3 = T[i].vertices[2].vertexScaledPostion[1, 0];
                double check = crossProductForBF(new Vector3D(x2 - x1, y2 - y1, 0), new Vector3D(x3 - x1, y3 - y1, 0));
                if(check > 0)
                {
                    if(isTextured)
                    {
                        T[i].drawPointsTextured(bmp, bitmapData, ref pixels, textureWidth, textureRGB, textureStride, bytesPerPixelForTexture, textureHeight, textureWidth,mode);
                    }
                    else
                    {
                        T[i].drawPoints(bmp, bitmapData, ref pixels);
                    }
                }
            }
            Marshal.Copy(pixels, 0, ptrFirstPixel, pixels.Length);
            bmp.UnlockBits(bitmapData);
            drawingAreaImage.Source =  bc.ToBitmapImage(bmp);
        }

        void draw()
        {
            initializeImage();
            produceTransformations();
            projectVertices(vertices);
            draw_mesh(triangles);
        }

        private void HandleKeyPress(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.W)
            {
                alpha_x -= increment;
                draw();
            }
            else if (e.Key == Key.S)
            {
                alpha_x += increment;
                draw();
            }
            else if (e.Key == Key.D)
            {
                alpha_y += increment;
                draw();
            }
            else if (e.Key == Key.A)
            {
                alpha_y -= increment;
                draw();
            }
            else if (e.Key == Key.E)
            {
                alpha_z -= increment;
                draw();
            }
            else if (e.Key == Key.Q)
            {
                alpha_z += increment;
                draw();
            }
            else if (e.Key == Key.PageUp)
            {
                if (Tbasic[2,3] > r * 1.3)
                {
                    CAMERA_POSITION_Z -= zoom_increment;
                    draw();
                }

            }
            else if (e.Key == Key.PageDown)
            {
                CAMERA_POSITION_Z += zoom_increment;
                draw();
            }
            else if (e.Key == Key.J)
            {
                    CAMERA_POSITION_X -= zoom_increment;
                    draw();

            }
            else if (e.Key == Key.L)
            {
                CAMERA_POSITION_X += zoom_increment;
                draw();
            }
            else if (e.Key == Key.I)
            {
                CAMERA_POSITION_Y -= zoom_increment;
                draw();

            }
            else if (e.Key == Key.K)
            {
                CAMERA_POSITION_Y += zoom_increment;
                draw();
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            window.KeyDown += HandleKeyPress;
        }

        private void WireBut_Click(object sender, RoutedEventArgs e)
        {
            isTextured = false;
            mode = 0;
            draw();
        }

        private void TextBut_Click(object sender, RoutedEventArgs e)
        {
            isTextured = true;
            mode = 1;
            draw();
        }

        private void WireTextBut_Click(object sender, RoutedEventArgs e)
        {
            isTextured = true;
            mode = 2;
            draw();
        }

    }
}
