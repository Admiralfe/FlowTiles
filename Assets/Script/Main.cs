using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Script.FlowTileUtils;
using Script.GridBuilding;
using UnityEngine.WSA;

public class Main : MonoBehaviour
{

	public GameObject Point;

    public TileGrid TileGrid;
	
	//Set this in Unity UI
	public int numberOfAgents;
    public float BackGroundScale;
	public int TileGridDimension;
	// Use this for initialization
	private void Start ()
	{
        //Makes the camera square.
        Camera.main.aspect = 1;

        Camera.main.orthographicSize = BackGroundScale / 2f;
		//Translate the camera so lower left corner has coordinates (0, 0)
		Camera.main.transform.Translate(new Vector3(Camera.main.orthographicSize * Camera.main.aspect,
			Camera.main.orthographicSize));
		
		GridBuilder gridBuilder = new GridBuilder(-4, 4, -4, 4, TileGridDimension);
		TileGrid = gridBuilder.BuildRandomTileGrid();

		for (int i = 0; i < numberOfAgents; i++)
		{
			Instantiate(Point, new Vector3(Random.Range(0, 1), Random.Range(0, 1), 0), Quaternion.identity);
		}
	}
}
