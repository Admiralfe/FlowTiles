using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FlowTilesUtils;

public class TileGrid : IEnumerable<FlowTile>
{
    /*
    public class FlowTile
    {

        private readonly int topFlux, rightFlux, bottomFlux, leftFlux;
        //public Vector2 cornerVelocity;

        public int TopFlux
        {
            get { return topFlux; }
        }
        public int RightFlux
        {
            get { return rightFlux; }
        }
        public int BottomFlux
        {
            get { return bottomFlux; }
        }
        public int LeftFlux
        {
            get { return leftFlux; }
        }

        public FlowTile(int topFluxIn, int rightFluxIn, int bottomFluxIn, int leftFluxIn)
        {
            topFlux = topFluxIn;
            rightFlux = rightFluxIn;
            bottomFlux = bottomFluxIn;
            leftFlux = leftFluxIn;
            //cornerVelocity = cornerVelocityIn;
        }
    }
    */

    public int Dimension;

    //Rows and columns in the 2d part
    private FlowTile[,] TileSet;

    public TileGrid(int gridDimensionIn)
    {
        Dimension = gridDimensionIn;

        TileSet = new FlowTile[Dimension, Dimension];
    }

    public bool HasTile(int rowIndex, int colIndex)
    {
        return (TileSet[rowIndex, colIndex] != null);
    }

    public void AddTile(int rowIndex, int colIndex, FlowTile flowTile)
    {
        TileSet[rowIndex, colIndex] = flowTile;
    }

    public FlowTile GetFlowTile(int rowIndex, int colIndex)
    {
        return TileSet[rowIndex, colIndex];
    }

    public IEnumerator<FlowTile> GetEnumerator()
    {
        for (int row = 0; row < Dimension; row++)
        {
            for (int col = 0; col < Dimension; col++)
            {
                yield return TileSet[row, col];
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
