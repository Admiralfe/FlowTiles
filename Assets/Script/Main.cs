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
    public TileGrid TileGrid_1;
	public TileGrid TileGrid_2;
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

	private void loadAgentIntoTile(GameObject agent) 
	{
		int[] rowColIndex = TileGrid_1.GetRowColIndexes(transform.position.x / BackGroundScale,
        	transform.position.y / BackGroundScale);

		TileGrid_1.GetFlowTile(TileGridDimension - rowColIndex[0] - 1, rowColIndex[1]).Agents.Add(agent);
	}	
	
	private void Start ()
	{
        TileGrid_1 = GridBuilder.BuildFromXML("/home/felix/FTGridBuilding/Tilings/Curve.xml");
        //TileGrid_2 = GridBuilder.BuildFromXML("/home/felix/FTGridBuilding/Tilings/TileGridVertical.xml");

		TileGrid_1.SmoothenEdges();

        //TileGrid_1 = GridBuilder.BuildFromXML(@"C:\Users\Felix Liu\source\repos\FTGridBuilding\Tilings\Curve.xml");
        //TileGrid_2 = GridBuilder.BuildFromXML(@"C:\Users\Felix Liu\source\repos\FTGridBuilding\TileGridVertical.xml");
        
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
			GameObject p = Instantiate(Point, new Vector3(Random.Range(13f, 15f), Random.Range(15f, 17f), 0), Quaternion.identity);
			p.GetComponent<MovingPoint>().FollowingLayer = 1;
			p.GetComponent<MovingPoint>().MainRef = this;
			loadAgentIntoTile(p);
		}

        /*
		for (int i = 0; i < numberOfAgents / 2; i++)
		{
			GameObject p = Instantiate(Point, new Vector3(Random.Range(9f, 11f), Random.Range(5f, 7f), 0),
				Quaternion.identity);
			p.GetComponent<MovingPoint>().FollowingLayer = 2;
			p.GetComponent<MovingPoint>().MainRef = this;
			loadAgentIntoTile(p);
		}
		*/
        
	}
}
