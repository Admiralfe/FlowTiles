using System;
namespace FlowTilesUtils
{

    public class FlowTile
    {
        public Flux Flux;
        public CornerVelocities CornerVelocities;
        public int GridSize;
        public Vector2D[,] VelocityGrid;
        public float[,] StreamFunctionGrid;

        public FlowTile(int gridSize, Flux flux, CornerVelocities cornerVelocities)

        {
            GridSize = gridSize;
            StreamFunctionGrid = new float[gridSize, gridSize];
            Flux = flux;
            CornerVelocities = cornerVelocities;
            //GenerateStreamFunctionGrid();

        }

        void GenerateStreamFunctionGrid()
        {
            int n = GridSize;
            //set the corners
            SetStreamFunction(x: -0.5f, y: -0.5f, value: 0.0f);
            SetStreamFunction(x: n + 0.5f, y: -0.5f, value: Flux.bottomEdge);
            SetStreamFunction(x: -0.5f, y: n + 0.5f, value: -Flux.leftEdge);
            SetStreamFunction(x: n + 0.5f, y: n + 0.5f,
                              value: StreamFunction(x: -0.5f, y: n + 0.5f) + Flux.topEdge);

            //set points around the corners
            //bottom left
            SetStreamFunction(x: -0.5f, y: 0.5f, value: StreamFunction(x: -0.5f, y: -0.5f) + CornerVelocities.bottomLeft.x);
            SetStreamFunction(x: 0.5f, y: -0.5f, value: StreamFunction(x: -0.5f, y: -0.5f) - CornerVelocities.bottomLeft.y);
            SetStreamFunction(x: 0.5f, y: 0.5f, value: StreamFunction(x: 0.5f, y: -0.5f) + CornerVelocities.bottomLeft.x);

            //top left
            SetStreamFunction(x: -0.5f, y: n - 0.5f, value: StreamFunction(x: -0.5f, y: n + 0.5f) - CornerVelocities.topLeft.x);
            SetStreamFunction(x: 0.5f, y: n + 0.5f, value: StreamFunction(x: -0.5f, y: n + 0.5f) - CornerVelocities.topLeft.y);
            SetStreamFunction(x: 0.5f, y: n - 0.5f, value: StreamFunction(x: 0.5f, y: n + 0.5f) - CornerVelocities.topLeft.x);

            //top right
            SetStreamFunction(x: n + 0.5f, y: n - 0.5f, value: StreamFunction(x: n + 0.5f, y: n + 0.5f) + CornerVelocities.topRight.x);
            SetStreamFunction(x: n - 0.5f, y: n + 0.5f, value: StreamFunction(x: n + 0.5f, y: n + 0.5f) - CornerVelocities.topRight.y);
            SetStreamFunction(x: n - 0.5f, y: n - 0.5f, value: StreamFunction(x: n - 0.5f, y: n + 0.5f) + CornerVelocities.topRight.x);

            //bottom right
            SetStreamFunction(x: n + 0.5f, y: 0.5f, value: StreamFunction(x: n + 0.5f, y: -0.5f) + CornerVelocities.bottomRight.x);
            SetStreamFunction(x: n - 0.5f, y: -0.5f, value: StreamFunction(x: n + 0.5f, y: 0.5f) - CornerVelocities.bottomRight.y);
            SetStreamFunction(x: n - 0.5f, y: 0.5f, value: StreamFunction(x: n - 0.5f, y: -0.5f) + CornerVelocities.bottomRight.x);



        }

        public Vector2D Veloctiy(FlowTileCoordinate Point)
        {
            Vector2D velocity = new Vector2D { x = 0, y = 0 };
            velocity.x = StreamFunction(Point.x, Point.y - 0.5f) - StreamFunction(Point.x, Point.y + 0.5f);
            velocity.y = StreamFunction(Point.x + 0.5f, Point.y) - StreamFunction(Point.x - 0.5f, Point.y);
            return velocity;
        }

        public float StreamFunction(float x, float y)
        {
            if (x < -0.5 || x > GridSize - 0.5)
            {
                throw new ArgumentOutOfRangeException(x.ToString(), "x-coordinate out of bounds");
            }
            if (y < -0.5 || y > GridSize - 0.5)
            {
                throw new ArgumentOutOfRangeException(y.ToString(), "y-coordinate out of bounds");
            }
            if (x % 1.0 == 0.5 && x % 1 == 0.5)
            {
                return StreamFunctionGrid[Convert.ToUInt32(x + 0.5), Convert.ToInt32(y + 0.5)];
            }
            return 0;
        }

        void SetStreamFunction(FlowTileCoordinate Point, float value)
        {
            int x = Convert.ToInt32(Point.y + 0.5);
            int y = Convert.ToInt32(Point.y + 0.5);
            StreamFunctionGrid[x, y] = value;
        }

        void SetStreamFunction(float x, float y, float value)
        {
            SetStreamFunction(new FlowTileCoordinate(x, y), value);
        }
    }
}