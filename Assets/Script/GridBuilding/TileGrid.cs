using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using Script.FlowTileUtils;
using UnityEngine;
using UnityEngine.XR.WSA.Persistence;

public class TileGrid : IEnumerable<FlowTile>
{
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

    public bool isFull()
    {
        for (int i = 0; i < Dimension; i++)
        {
            for (int j = 0; j < Dimension; j++)
            {
                if (!HasTile(i, j))
                {
                    return false;
                }
            }
        }

        return true;
    }

    /// <summary>
    /// Interpolates the velocities at the edges so that adjacent tiles edge velocities match. 
    /// </summary>
    /// <exception cref="MissingFieldException"></exception>
    public void SmoothenEdges()
    {
        int FlowTileSize = GetFlowTile(0, 0).GridSize;
        if (!isFull())
        {
            throw new MissingFieldException("The whole TileGrid must be filled before its edges can be smoothened.");
        }

        for (int i = 1; i < Dimension - 1; i++)
        {
            for (int j = 1; j < Dimension - 1; j++)
            {
                FlowTile tile = GetFlowTile(i, j);
                FlowTile tileAbove = GetFlowTile(i - 1, j);
                FlowTile tileRight = GetFlowTile(i, j + 1);
                FlowTile tileRightAbove = GetFlowTile(i - 1, j + 1);
                Vector2 interpolatedVelocity = (tile.GetVelocity(0, FlowTileSize - 1) +
                                                tileAbove.GetVelocity(FlowTileSize - 1, FlowTileSize - 1) +
                                                tileRight.GetVelocity(0, 0) +
                                                tileRightAbove.GetVelocity(FlowTileSize - 1, 0)) / 4;
                tile.SetVelocity(0, FlowTileSize - 1, interpolatedVelocity);
                tileAbove.SetVelocity(FlowTileSize - 1, FlowTileSize - 1, interpolatedVelocity);
                tileRight.SetVelocity(0, 0, interpolatedVelocity);
                tileRightAbove.SetVelocity(FlowTileSize - 1, 0, interpolatedVelocity);

            }
        }
        for (int i = 0; i < Dimension; i++)
        {
            for (int j = 0; j < Dimension; j++)
            {
                FlowTile tile = GetFlowTile(i, j);
                
                if(i != 0)
                {
                    FlowTile tileAbove = GetFlowTile(i - 1, j);
                    for (int k = 1; k < FlowTileSize - 1; k++)
                    {
                        Vector2 interpolatedVelocity =
                            (tile.GetVelocity(0, k) + tileAbove.GetVelocity(FlowTileSize - 1, k)) / 2;
                        tile.SetVelocity(0, k, interpolatedVelocity);
                        tile.SetVelocity(FlowTileSize - 1, k, interpolatedVelocity);
                    }
                }

                if (j != Dimension - 1)
                {
                    for (int k = 1; k < FlowTileSize - 1; k++)
                    {
                        FlowTile tileRight = GetFlowTile(i, j + 1);
                        Vector2 interpolatedVelocity = (tile.GetVelocity(k, FlowTileSize - 1) + tileRight.GetVelocity(k, 0)) / 2;
                        tile.SetVelocity(k, FlowTileSize -1, interpolatedVelocity );
                        tileRight.SetVelocity(k, 0, interpolatedVelocity);
                    }
                }
            }
        }
    }

    public int[] GetRowColIndexes(float x, float y)
    {
        int rowIndex = (int) Math.Floor(y * Dimension);
        int colIndex = (int) Math.Floor(x * Dimension);

        return new int[] {rowIndex, colIndex};
    }

    public void WriteToFile(string filename)
    {
        using (StreamWriter writer = new StreamWriter(filename))
        {
            for (int row = 0; row < Dimension; row++)
            {
                for (int col = 0; col < Dimension; col++)
                {
                    GetFlowTile(row,col).WriteToFile(writer);
                }
            }
        }
    }

    public void WriteToXML(string filename)
    {
        XmlDocument xmlDoc = new XmlDocument();
        XmlElement root = xmlDoc.CreateElement("tilegrid");

        for (int i = 0; i < Dimension; i++)
        {
            for (int j = 0; j < Dimension; j++)
            {
                var tile = GetFlowTile(i, j);
                if(tile == null) continue;
                XmlElement xmlTile = tile.ToXmlElement(xmlDoc);
                xmlTile.SetAttribute("row", i.ToString());
                xmlTile.SetAttribute("col", j.ToString());
                root.AppendChild(xmlTile);
            }
        }
        xmlDoc.Save(filename);
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
