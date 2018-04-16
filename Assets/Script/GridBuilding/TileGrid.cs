using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Script.FlowTileUtils;
using UnityEngine.XR.WSA.Persistence;

public class TileGrid : IEnumerable<FlowTile>
{

    /*
    public class FlowTile
    {
        //public Vector2 cornerVelocity;

        public int TopFlux { get; }
        public int RightFlux { get; }
        public int BottomFlux { get; }
        public int LeftFlux { get; }
     
        public FlowTile(int topFluxIn, int rightFluxIn, int bottomFluxIn, int leftFluxIn)
        {
            TopFlux = topFluxIn;
            RightFlux = rightFluxIn;
            BottomFlux = bottomFluxIn;
            LeftFlux = leftFluxIn;
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
        if (rowIndex < 0 || colIndex < 0 || rowIndex > Dimension - 1 || colIndex > Dimension - 1)
            return false;
        
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

    public int[] GetRowColIndexes(float x, float y)
    {
        int rowIndex = (int) Math.Floor(y * Dimension);
        int colIndex = (int) Math.Floor(x * Dimension);

        return new int[] {rowIndex, colIndex};
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
