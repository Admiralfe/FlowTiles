using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Script.FlowTileUtils;
using Script.GridBuilding;
using UnityEngine.WSA;

public class Main : MonoBehaviour
{

	public GameObject Point;
	public GameObject Background;
	
	//Set this in Unity UI
	public int numberOfAgents;
	public float BackgroundScale;
	public int TileGridDimension;
	// Use this for initialization

	public TileGrid tileGrid;

	public float getTileWidth()
	{
		return BackgroundScale / (float) TileGridDimension;
	}
	
	private void Start ()
	{
		//Debug.Log(System.Environment.CurrentDirectory);
		
		Camera.main.aspect = 1;
		
		//Translate the camera so lower left corner has coordinates (0, 0)
		Camera.main.transform.Translate(new Vector3(Camera.main.orthographicSize * Camera.main.aspect,
			Camera.main.orthographicSize));

		GameObject myBackground = Instantiate(Background, new Vector3(0, 0, 0), Quaternion.identity);
		myBackground.transform.localScale += new Vector3(BackgroundScale - 1, BackgroundScale - 1, 0);
		myBackground.transform.Translate(new Vector3(BackgroundScale / 2, BackgroundScale / 2, 0));
		
		GridBuilder gridBuilder = new GridBuilder(-4, 4, -4, 4, TileGridDimension);
		tileGrid = gridBuilder.BuildRandomTileGrid();
		for (int i = 0; i < numberOfAgents; i++)
		{
			Instantiate(Point, new Vector3(Random.Range(0, BackgroundScale), Random.Range(0, BackgroundScale), 0),
				Quaternion.identity);
		}
	}
}
