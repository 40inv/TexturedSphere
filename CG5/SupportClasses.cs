using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CG5
{
    public class Vertex
    {
        public double[,] vertexNotScaledPostion;
        public double[,] vertexScaledPostion = { { 0 }, { 0 }, { 0 }, { 0 } };
        public double average;
        public double[] texture = { 0, 0 };

        public Vertex() { }
        public Vertex(Vertex v) {
            vertexNotScaledPostion = new double[4, 1];
            for(int i = 0; i < 4; i++)
            {
                vertexNotScaledPostion[i, 0] = v.vertexNotScaledPostion[i, 0];
                vertexScaledPostion[i, 0] = v.vertexScaledPostion[i, 0];
            }
            texture[0] = v.texture[0];
            texture[1] = v.texture[1];
            average = v.average;

        }

        public Vertex(double px, double py, double pz, int r)
        {
            vertexNotScaledPostion = new double[4, 1];

            vertexNotScaledPostion[0, 0] = px;
            vertexNotScaledPostion[1, 0] = py;
            vertexNotScaledPostion[2, 0] = pz;
            vertexNotScaledPostion[3, 0] = 1;
        }
    };
}
