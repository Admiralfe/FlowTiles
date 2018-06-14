using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Script.FlowTileUtils;
using Script.GridBuilding;
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine.WSA;

public class Main3D : MonoBehaviour
{
    public GameObject Agent;
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
	
	/// <summary>
	/// Loads agent into bucket according to their current tile. This is used for collision detection between agents.
	/// </summary>
	/// <param name="agent"> Agent to be loaded. </param>
	private void loadAgentIntoTile(GameObject agent) 
	{
		int[] rowColIndex = TileGrid_1.GetRowColIndexes(agent.transform.position.x / BackGroundScale,
        	agent.transform.position.z / BackGroundScale);

		Debug.Log(rowColIndex[0] + " " + rowColIndex[1]);

		TileGrid_1.GetFlowTile(TileGridDimension - rowColIndex[0] - 1, rowColIndex[1]).Agents.Add(agent);
	}	
	
	private void Start ()
	{
        TileGrid_1 = GridBuilder.BuildFromXML("/home/felix/FTGridBuilding/Tilings/Curve.xml");
        //TileGrid_2 = GridBuilder.BuildFromXML("/home/felix/FTGridBuilding/Tilings/TileGridVertical.xml");

        //TileGrid_1 = GridBuilder.BuildFromXML(@"C:\Users\Felix Liu\source\repos\FTGridBuilding\Tilings\Curve.xml");
        //TileGrid_2 = GridBuilder.BuildFromXML(@"C:\Users\Felix Liu\source\repos\FTGridBuilding\TileGridVertical.xml");

        TileGrid_1.SmoothenEdges();

        //Makes the camera square.
        Camera.main.aspect = 1;

		/*
        GameObject myBackGround = Instantiate(BackGround, new Vector3(0, 0, 0), Quaternion.identity);
        myBackGround.transform.localScale += new Vector3(BackGroundScale - 1, BackGroundScale - 1);
        myBackGround.transform.Translate(new Vector3(BackGroundScale / 2, BackGroundScale / 2));
		*/

		for (int i = 0; i < numberOfAgents; i++)
		{
			GameObject agent = Instantiate(Agent, new Vector3(Random.Range(1f, 18f), 0, Random.Range(14f, 19f)), Quaternion.identity);
			agent.transform.rotation = Quaternion.LookRotation(new Vector3(1f, 0, 0));
			agent.GetComponent<Agent3D>().FollowingLayer = 1;
			agent.GetComponent<Agent3D>().MainRef = this;
			loadAgentIntoTile(agent);
		}
		
		for (int i = 0; i < numberOfAgents; i++)
		{
			GameObject agent = Instantiate(Agent, new Vector3(Random.Range(14f, 19f), 0, Random.Range(1f, 12f)), Quaternion.identity);
			agent.transform.rotation = Quaternion.LookRotation(new Vector3(1f, 0, 0));
			agent.GetComponent<Agent3D>().FollowingLayer = 1;
			agent.GetComponent<Agent3D>().MainRef = this;
			loadAgentIntoTile(agent);
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
