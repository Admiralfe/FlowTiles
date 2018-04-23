using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Script.FlowTileUtils;
using Script.GridBuilding;
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine.WSA;

public class Main : MonoBehaviour
{
    public GameObject Point;
    public TileGrid TileGrid;
    public GameObject BackGround;
	
	//Set this in Unity UI
	public int numberOfAgents;
    public float BackGroundScale;
	public int TileGridDimension;
	// Use this for initialization

	public float GetTileWidth()
	{
        return BackGroundScale / TileGridDimension;
	}
	
	private void Start ()
	{
		GridBuilder gridBuilder = new GridBuilder(-4, 4, -4, 4, TileGridDimension, 10);
		
		for (int row = 0; row < gridBuilder.gridDimension; row++)
		{
			for (int col = 0; col < gridBuilder.gridDimension; col++)
			{
				Debug.Log(row + ", " + col);
				FlowTile tile = gridBuilder.AskUserForTile(row, col);
				gridBuilder.AddTile(row, col, tile);
			}
		}

		TileGrid = gridBuilder.GetTileGrid();
		
        //Makes the camera square.
        Camera.main.aspect = 1;

        Camera.main.orthographicSize = BackGroundScale / 2f;
		//Translate the camera so lower left corner has coordinates (0, 0)
		Camera.main.transform.Translate(new Vector3(Camera.main.orthographicSize * Camera.main.aspect,
			Camera.main.orthographicSize, 0));

        GameObject myBackGround = Instantiate(BackGround, new Vector3(0, 0, 0), Quaternion.identity);
        myBackGround.transform.localScale += new Vector3(BackGroundScale - 1, BackGroundScale - 1);
        myBackGround.transform.Translate(new Vector3(BackGroundScale / 2, BackGroundScale / 2));
		
		for (int i = 0; i < numberOfAgents; i++)
		{
			GameObject p = Instantiate(Point, new Vector3(Random.Range(0, BackGroundScale), Random.Range(0, BackGroundScale), 0),
				Quaternion.identity);
			p.GetComponent<MovingPoint>().MainRef = this;
		}
	}
}
