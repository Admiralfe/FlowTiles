using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FlowTilesUtils;

public class Main : MonoBehaviour
{

	public GameObject Point;
	
	//Set this in Unity UI
	public int numberOfAgents;
	// Use this for initialization
	private void Start () 
	{
        int dimension = 3;
        GridBuilder gridBuilder = new GridBuilder(-1, 1, -1, 1, dimension);
        TileGrid tileGrid = gridBuilder.BuildRandomTileGrid();
        for (int row = 0; row < dimension; row++)
        {
            for (int col = 0; col < dimension; col++)
            {
                FlowTile currentTile = tileGrid.GetFlowTile(row, col);
                System.Console.WriteLine("Position " + "(" + row + "," + col + ")" + " : top = " +
                currentTile.Flux.topEdge + ", right = " + currentTile.Flux.rightEdge +
                ", bottom = " + currentTile.Flux.bottomEdge + ", left = " + currentTile.Flux.leftEdge);
            }
        }
    }
}
