using System;
namespace Script.FlowTileUtils
{
    public struct Flux
    {
        public int leftEdge;
        public int rightEdge;
        public int topEdge;
        public int bottomEdge;
    };

    
    public struct Vector2D
    {
        public float x;
        public float y;
        public float Norm() { return (float)Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2)); }
    };


    public struct CornerVelocities
    {
        public Vector2D topLeft;
        public Vector2D topRight;
        public Vector2D bottomLeft;
        public Vector2D bottomRight;
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
