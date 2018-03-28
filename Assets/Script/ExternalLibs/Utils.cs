using System;
using UnityEngine;
namespace FlowTilesUtils
{
    public struct Flux
    {
        public int leftEdge;
        public int rightEdge;
        public int topEdge;
        public int bottomEdge;
    };
    
    /*
    public struct Vector2D
    {
        public double x;
        public double y;
        public double Norm() { return Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2)); }
    };
    */

    public struct CornerVelocities
    {
        public Vector2 topLeft;
        public Vector2 topRight;
        public Vector2 bottomLeft;
        public Vector2 bottomRight;
    };
    public struct FlowTileCoordinate
    {
        public FlowTileCoordinate(float xIn, float yIn) { x = xIn; y = yIn; }
        public float x;
        public float y;
    };
    public struct VelocityGridPoint
    {
        public int x;
        public int y;
        //should use setter to check if it is ok
    };
    public struct StreamGridPoint
    {
        public float x;
        public float y;
        //should use setter to check if it is ok
    };
}
