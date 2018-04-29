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
        if (!HasTile(rowIndex, colIndex))
        {
            throw new ArgumentException("No flow tile on that slot.");
        }

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
