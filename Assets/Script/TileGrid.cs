using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileGrid
{
    public int Dimension;
    
    //Rows are flow and velocity values for tile on the position given by the column index.
    public int[,][] TileSet;

    public TileGrid(int gridDimensionIn)
    {
        Dimension = gridDimensionIn;
        
        TileSet = new int[Dimension, Dimension][];
    }

    public bool hasTile(int rowIndex, int colIndex)
    {
        return (TileSet[rowIndex, colIndex] != null);
    }

    public void addTile(int rowIndex, int colIndex, int[] tileValues)
    {
        TileSet[rowIndex, colIndex] = tileValues;
    }

}
