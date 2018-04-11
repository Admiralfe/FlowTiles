using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Script.FlowTileUtils
{     public class FlowTile
    {  
        public Flux Flux;
        public CornerVelocities CornerVelocities;
        public int GridSize;
        
        /// <summary>
        /// The velocity grid is GridSize * GridSize large and holds a velocity in every point. The index x=0, y=0
        /// represents the bottom left corner and the first index represents y-coordinates, i.e rows.
        /// </summary>
        private Vector2[,] VelocityGrid;
        
        
        /// <summary>
        /// The StreamFunctionGrid is only for internal use to calculate the velocites. The size of the StreamFunctionGrid
        /// is (GridSize + 1) * (GridSize + 1) since it represents points between the points of Velocity. A point at index [y, x]
        /// in the StreamFunctionGrid represtens a physical point at (x-0.5, y-0.5).
        /// </summary>
        private float[,] StreamFunctionGrid;

        /// <summary>
        /// The control points are the points given to the Bezier interpolation function.
        /// </summary>
        private Vector3[,] ControlPoints;

        public FlowTile(int gridSize, Flux flux, CornerVelocities cornerVelocities)
        {
            GridSize = gridSize;
            StreamFunctionGrid = new float[gridSize+1, gridSize+1];
            Flux = flux;
            CornerVelocities = cornerVelocities;
            VelocityGrid = new Vector2[GridSize, GridSize];
            GenerateStreamFunctionGrid();
            GenerateVelocityGrid();
            ControlPoints = new Vector3[4,4];
        }
        


        private void GenerateStreamFunctionGrid()
        {
            //Size of the StreamFunctionGrid
            int n = GridSize + 1;
            SetControlPoint(x : -0.5, y : -0.5 , value: 0.0);
            SetControlPoint(x : n + 0.5, y : -0.5, value: Flux.bottomEdge);
            SetControlPoint(x : -0.5, y : n + 0.5, value: -Flux.leftEdge);
            SetControlPoint(x : n + 0.5, y : n + 0.5 , 
                              value: GetControlPoint(x: -0.5, y: n + 0.5).z + Flux.topEdge);

            //set points around the corners
            //bottom left
            SetControlPoint(x: -0.5, y: 0.5, value: GetControlPoint(x: -0.5, y: -0.5).z + CornerVelocities.bottomLeft.x);
            SetControlPoint(x: 0.5, y: -0.5, value: GetControlPoint(x: -0.5, y: -0.5).z - CornerVelocities.bottomLeft.y);
            SetControlPoint(x: 0.5, y: 0.5, value: GetControlPoint(x: 0.5, y: -0.5).z + CornerVelocities.bottomLeft.x);

            //top left
            SetControlPoint(x: -0.5, y: n - 0.5, value: GetControlPoint(x: -0.5, y: n + 0.5).z - CornerVelocities.topLeft.x);
            SetControlPoint(x: 0.5, y: n + 0.5, value: GetControlPoint(x: -0.5, y: n + 0.5).z - CornerVelocities.topLeft.y);
            SetControlPoint(x: 0.5, y: n - 0.5, value: GetControlPoint(x: 0.5, y: n + 0.5).z - CornerVelocities.topLeft.x);

            //top right
            SetControlPoint(x: n + 0.5, y: n - 0.5, value: GetControlPoint(x: n + 0.5, y: n + 0.5).z + CornerVelocities.topRight.x);
            SetControlPoint(x: n - 0.5, y: n + 0.5, value: GetControlPoint(x: n + 0.5, y: n + 0.5).z - CornerVelocities.topRight.y);
            SetControlPoint(x: n- 0.5, y: n - 0.5, value: GetControlPoint(x: n - 0.5, y: n + 0.5).z + CornerVelocities.topRight.x);

            //bottom right
            SetControlPoint(x: n + 0.5, y: 0.5, value: GetControlPoint(x: n + 0.5, y: -0.5).z + CornerVelocities.bottomRight.x);
            SetControlPoint(x: n - 0.5, y: -0.5, value: GetControlPoint(x: n + 0.5, y: 0.5).z - CornerVelocities.bottomRight.y);
            SetControlPoint(x: n - 0.5, y: 0.5, value: GetControlPoint(x: n - 0.5, y: -0.5).z + CornerVelocities.bottomRight.x);

            var streamVector3s = BezierInterpolation.Bezier3D(ControlPoints, n);
            foreach (var streamVector in streamVector3s)
            {
                SetStreamFunction(streamVector);
            }
            {
                
            }



        }

        private void GenerateVelocityGrid()
        {
             for(int y = 0; y < GridSize; y++)
            {
                for (int x = 0; x < GridSize; x++)
                {
                    VelocityGrid[y, x] = CalculateVeloctiy(x, y);
                }
            }   
        }
        

        public Vector2 Velocity(int x, int y)
        {
            return VelocityGrid[y, x];
        }
        
        /// <summary>
        /// Sets the control point at the point (x,y)
        /// </summary>
        /// <param name="x">Physical x-coordinate, should be integer - 0.5</param>
        /// <param name="y">Physical y-coordinate, should be integer - 0.5</param>
        /// <param name="value"></param>
        void SetControlPoint(double x, double y, double value)
        {
            int n = GridSize + 1;
            int i = CoordinateToIndex(y);
            int j = CoordinateToIndex(x);
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
        
        
        /// <summary>
        /// Returns the ControlPoint at the point (x,y)
        /// </summary>
        /// <param name="x">Physical x-coordinate, should be integer - 0.5</param>
        /// <param name="y">Physical y-coordinate, should be integer - 0.5</param>
        /// <returns></returns>
        Vector3 GetControlPoint(double x, double y)
        {
            int n = GridSize + 1;
            int i = CoordinateToIndex(y);
            int j = CoordinateToIndex(x);
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
        
        private Vector2 CalculateVeloctiy(int x, int y)
        {
            Vector2 velocity = new Vector2 ();
            velocity.x = StreamFunction(x, y - 0.5) - StreamFunction(x, y + 0.5);
            velocity.y = StreamFunction(x + 0.5, y) - StreamFunction(x - 0.5, y);
            return velocity;
        }
        
        /// <summary>
        /// Returns the float value of the stream function at point x and y.
        /// The arguments x and y are the physical coordinates from where the StreamFunction should be retrived.
        /// </summary>
        /// <param name="x">Physical x-coordinate, should be integer - 0.5</param>
        /// <param name="y">Physical y-coordinate, should be integer - 0.5</param>
        /// <returns></returns>
        private float StreamFunction(double x, double y)
        {
            return StreamFunctionGrid[CoordinateToIndex(y), CoordinateToIndex(x)];
        }
        
        /// <summary>
        /// Returns a Vector3 representing a triplet with the x- and y-coordinate and the stream function value.
        /// The parameters x and y are the physical coordinates from where the StreamFunction vector should be retrived. 
        /// </summary>
        /// <param name="x">Physical x-coordinate, should be integer - 0.5</param>
        /// <param name="y">Physical y-coordinate, should be integer - 0.5</param>
        /// <returns></returns>
        private Vector3 StreamFunctionVector(double x, double y)
        {
            return new Vector3((float) x, (float) y, StreamFunction(x, y));
        }

        private void SetStreamFunction(Vector3 xys)
        {
            SetStreamFunction(xys.x, xys.y, xys.z);
        }

        /// <summary>
        /// Sets the stream function value at the point (x,y) to <c>value</c>.
        /// The parameters x and y are the physical coordinates where the stream function should be set.
        /// </summary>
        /// <param name="x">Physical x-coordinate, should be integer - 0.5</param>
        /// <param name="y">Physical y-coordinate, should be integer - 0.5</param>
        /// <param name="value">The value the set the StreamFunction to</param>
        private void SetStreamFunction(float x, float y, float value)
        {
            StreamFunctionGrid[Convert.ToInt32(y + 0.5), Convert.ToInt32(x + 0.5)] = value;
        }
        
        /// <summary>
        /// Returns the index in the StreamFunctionGrid corresponding to the coordinate given. Does not apply
        /// to the VelocityGrid
        /// </summary>
        /// <param name="coordinate"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private int CoordinateToIndex(double coordinate)
        {
            if ((coordinate + 0.5) % 1.0 > 0.05)
            {
                throw new ArgumentException();
            }

            return Convert.ToInt32(coordinate + 0.5);
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