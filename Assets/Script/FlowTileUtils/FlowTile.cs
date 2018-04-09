using System;
//using System.Numerics;
using UnityEngine;

namespace Script.FlowTileUtils
{    public class FlowTile : MonoBehaviour
    {  
        public Flux Flux;
        public CornerVelocities CornerVelocities;
        public int GridSize;
        public Vector3[,] StreamFunctionGrid;

        public FlowTile(int gridSize, Flux flux, CornerVelocities cornerVelocities)
        {
            GridSize = gridSize;
            StreamFunctionGrid = new Vector3[gridSize+1, gridSize+1];
            Flux = flux;
            CornerVelocities = cornerVelocities;
            GenerateStreamFunctionGrid();
        }

        void GenerateStreamFunctionGrid()
        {
            
            int n = GridSize+1; //size of the StreamFuncitonGrid
            //set the corners
            Vector3[,] ControlPoints = new Vector3[n,n];

            void SetControlPoint(double x, double y, double value)
            {
                int i = Convert.ToInt32(y + 0.5);
                int j = Convert.ToInt32(x + 0.5);
                if (i != 0 || i != 1)
                {
                    i = i - n + 4;
                }

                if (j != 0 || j != 1)
                {
                    j = i - n + 4;
                }

                ControlPoints[i, j] = new Vector3((float) x, (float)y, (float)value);
            }

            Vector3 ControlPoint(double x, double y)
            {
                int i = Convert.ToInt32(y + 0.5);
                int j = Convert.ToInt32(x + 0.5);
                if (i != 0 || i != 1)
                {
                    i = i - n + 4;
                }

                if (j != 0 || j != 1)
                {
                    j = i - n + 4;
                }

                return ControlPoints[i, j];
            }

            SetControlPoint(x : -0.5, y : -0.5 , value: 0.0);
            SetControlPoint(x : n + 0.5, y : -0.5, value: Flux.bottomEdge);
            SetControlPoint(x : -0.5, y : n + 0.5, value: -Flux.leftEdge);
            SetControlPoint(x : n + 0.5, y : n + 0.5 , 
                              value: ControlPoint(x: -0.5, y: n + 0.5).z + Flux.topEdge);

            //set points around the corners
            //bottom left
            SetControlPoint(x: -0.5, y: 0.5, value: ControlPoint(x: -0.5, y: -0.5).z + CornerVelocities.bottomLeft.x);
            SetControlPoint(x: 0.5, y: -0.5, value: ControlPoint(x: -0.5, y: -0.5).z - CornerVelocities.bottomLeft.y);
            SetControlPoint(x: 0.5, y: 0.5, value: ControlPoint(x: 0.5, y: -0.5).z + CornerVelocities.bottomLeft.x);

            //top left
            SetControlPoint(x: -0.5, y: n - 0.5, value: ControlPoint(x: -0.5, y: n + 0.5).z - CornerVelocities.topLeft.x);
            SetControlPoint(x: 0.5, y: n + 0.5, value: ControlPoint(x: -0.5, y: n + 0.5).z - CornerVelocities.topLeft.y);
            SetControlPoint(x: 0.5, y: n - 0.5, value: ControlPoint(x: 0.5, y: n + 0.5).z - CornerVelocities.topLeft.x);

            //top right
            SetControlPoint(x: n + 0.5, y: n - 0.5, value: ControlPoint(x: n + 0.5, y: n + 0.5).z + CornerVelocities.topRight.x);
            SetControlPoint(x: n - 0.5, y: n + 0.5, value: ControlPoint(x: n + 0.5, y: n + 0.5).z - CornerVelocities.topRight.y);
            SetControlPoint(x: n- 0.5, y: n - 0.5, value: ControlPoint(x: n - 0.5, y: n + 0.5).z + CornerVelocities.topRight.x);

            //bottom right
            SetControlPoint(x: n + 0.5, y: 0.5, value: ControlPoint(x: n + 0.5, y: -0.5).z + CornerVelocities.bottomRight.x);
            SetControlPoint(x: n - 0.5, y: -0.5, value: ControlPoint(x: n + 0.5, y: 0.5).z - CornerVelocities.bottomRight.y);
            SetControlPoint(x: n - 0.5, y: 0.5, value: ControlPoint(x: n - 0.5, y: -0.5).z + CornerVelocities.bottomRight.x);

            StreamFunctionGrid = BezierPatch.Bezier3D(ControlPoints, n);



        }

        public Vector2 Veloctiy(Vector2 Point)
        {
            Vector2 velocity = new Vector2 ();
            velocity.x = StreamFunction(Point.x, Point.y - 0.5).z - StreamFunction(Point.x, Point.y + 0.5).z;
            velocity.y = StreamFunction(Point.x + 0.5, Point.y).z - StreamFunction(Point.x - 0.5, Point.y).z;
            return velocity;
        }

        public Vector3 StreamFunction(Vector2 Point)
        {
            return StreamFunction(Point.x, Point.y);
        }

        public Vector3 StreamFunction(double x, double y)
        {
            if (x < -0.5 || x > GridSize - 0.5 ){
                throw new ArgumentOutOfRangeException(nameof(x), "x-coordinate out of bounds");
            } 
            if (y < -0.5 || y > GridSize - 0.5) {
                throw new ArgumentOutOfRangeException(nameof(y), "y-coordinate out of bounds");
            }
            if (x % 1.0 == 0.5 && y % 1.0 == 0.5){
                return StreamFunctionGrid[Convert.ToUInt32(x + 0.5), Convert.ToInt32(y + 0.5)];
            }
            return new Vector3(0,0,0);
        }

        void SetStreamFunction(Vector2 Point, float value)
        {
            int i = Convert.ToInt32(Point.y + 0.5);
            int j = Convert.ToInt32(Point.x + 0.5);
            StreamFunctionGrid[i, j] = new Vector3(Point.x, Point.y, value);
        }

        void SetStreamFunction(float x, float y, float value)
        {
            SetStreamFunction(new Vector2 (x, y), value);
        }
    }
    public struct Flux
    {
        public int leftEdge;
        public int rightEdge;
        public int topEdge;
        public int bottomEdge;
    };

    public struct CornerVelocities
    {
        public Vector2 topLeft;
        public Vector2 topRight;
        public Vector2 bottomLeft;
        public Vector2 bottomRight;
    };
}