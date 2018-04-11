using System;
using System.IO;
using UnityEngine;

namespace Script.FlowTileUtils
{
    public class BezierInterpolation
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="n">Grid size</param>
        /// <returns></returns>
        public static int ControlPointIndexToGridIndex(int x, int n)
        {
            int y;
            switch (x)
            {
                case 0: y = 0; break;
                case 1: y = 1; break;
                case 2: y = -2; break;
                case 3: y = -1; break;
                default: y = 0; break;
            }

            return (y + n) % n;
        }
        
        public static Vector2[] Bezier2D(Vector2[] controlPoints, int noOfInterpolationPoints, int steps = 1000)
        {
            int n = noOfInterpolationPoints;
            float xStep = (controlPoints[3].x - controlPoints[0].x) / (n - 1);
            var InterpolatedData = new Vector2[n];
            InterpolatedData[0] = controlPoints[0];
            InterpolatedData[n - 1] = controlPoints[3];
            float t = 0.0F;
            for (int i = 1; i < n; i++)
            {
                float nextX = controlPoints[0].x + i * xStep;
                Vector2 closestVector = new Vector2();
                bool closestSet = false;
                while (t < 1)
                {
                    var B = ((float)Math.Pow(1 - t, 3)) * controlPoints[0] +
                            3 * (float)Math.Pow(1 - t, 2) * t * controlPoints[1] +
                            3 * (1 - t) * (float)Math.Pow(t, 2) * controlPoints[2] +
                            (float)Math.Pow(t, 3) * controlPoints[3];
                    t += 1.0F/ steps;
                    if (!closestSet)
                    {
                        closestVector = B;
                        closestSet = true;
                        continue;
                    }
                    if (Math.Abs(B.x - nextX) < Math.Abs(closestVector.x - nextX))
                    {
                        closestVector = B;
                        continue;
                    }
                    break;
                }
                InterpolatedData[i] = closestVector;
            }
           

            return InterpolatedData;
        }

        public static Vector3[,] Bezier3D(Vector3[,] controlPoints, int noOfInterpolationPoints, int steps = 1000)
        {
            //Check that all the points are in line, otherwise it is pretty hard to interpolate.
            for (int i = 0; i < 4; i++)
            {
                if (controlPoints[i, 0].y == controlPoints[i, 1].y && 
                    controlPoints[i, 1].y == controlPoints[i, 2].y && 
                    controlPoints[i, 2].y == controlPoints[i, 3].y)
                {
                    if (controlPoints[0, i].x == controlPoints[1, i].x &&
                        controlPoints[1, i].x == controlPoints[2, i].x &&
                        controlPoints[2, i].x == controlPoints[3, i].x)
                    {
                        continue;
                    }
                }
                throw new ArgumentException();
            }
            
            int n = noOfInterpolationPoints;
            Vector3[,] InterpolatedData = new Vector3[n, n];
            
            
            for (int i = 0; i < 4; i++)
            {
                Vector2[] controlPointsRow = new Vector2[4];
                for (int j = 0; j < 4; j++)
                {
                    controlPointsRow[j] = new Vector2(controlPoints[i,j].x, controlPoints[i,j].z);   
                }
                Vector2[] dataRow = Bezier2D(controlPointsRow, n, steps);
                for (int j = 0; j < n; j++)
                {
                    InterpolatedData[ControlPointIndexToGridIndex(i, n),j] = new Vector3(dataRow[j].x, controlPoints[i,1].y, dataRow[j].y);
                }
            }


            for (int j = 0; j < n; j++)
            {
                Vector2[] controlPointsCol = new Vector2[4];
                for (int i = 0; i < 4; i++)
                {
                    controlPointsCol[i] = new Vector2(InterpolatedData[ControlPointIndexToGridIndex(i, n), j].y,
                        InterpolatedData[ControlPointIndexToGridIndex(i, n), j].z);
                }
                
                Vector2[] dataCol = Bezier2D(controlPointsCol, n, steps);
                for (int i = 0; i < n; i++)
                {
                    InterpolatedData[i,j] = new Vector3(InterpolatedData[1,j].x, dataCol[i].x, dataCol[i].y);
                }
            }
                
            
            return InterpolatedData;   
        }

        public static void Test2D()
        {
            Vector2[] ControlPoints = {new Vector2(0, 1), new Vector2(1, 5), new Vector2(8, 3), new Vector2(10, 2)};
            
            var data = Bezier2D(ControlPoints, 50);

            using (StreamWriter file =
                new StreamWriter("data.csv"))
            {
                foreach (var vector in data)
                {
                    file.WriteLine(vector.x + ", " + vector.y);
                }
            }

            using (StreamWriter file =
                new StreamWriter("ControlPoints.csv"))
            {
                foreach (var vector in ControlPoints)
                {
                    file.WriteLine(vector.x + ", " + vector.y);
                }
            }
        }

        public static void Test3D()
        {
            int n = 11;
            System.Random rnd = new System.Random();
            Vector3[,] ControlPoints = new Vector3[4,4];
            int[] xyInts = {0, 1, 9, 10};
            using (StreamWriter file = new StreamWriter("ControlPoints3d.csv"))
            {
                for (int i = 0; i < 4; i++)
                {
                    var y = xyInts[i];
                    for (int j = 0; j < 4; j++)
                    {
                        var x = xyInts[j];
                        float s = (float) (rnd.NextDouble() * 5.0);
                        ControlPoints[i,j] = new Vector3(x, y, s);
                        file.WriteLine(x + ", " + y + ", " + s);
                    }
                }
            }
            
            var data = Bezier3D(ControlPoints, n);
            using (StreamWriter file = new StreamWriter("data3d.csv"))
            {
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        var vec = data[i, j];
                        file.WriteLine(vec.x + ", "+ vec.y + ", " + vec.z);
                    }
                }
            }

            {
                
            }

        }

    }
}