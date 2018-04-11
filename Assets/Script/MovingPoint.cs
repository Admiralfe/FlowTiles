using System.Collections;
using System.Collections.Generic;
using Script.FlowTileUtils;
using Script.GridBuilding;
using UnityEngine;

public class MovingPoint : MonoBehaviour
{
	public Vector2 Velocity;
	public GameObject MainRef;
    private Main ScriptRef;
    private int[] rowColIndex;
	// Use this for initialization
	void Start()
	{
		Velocity = Random.insideUnitCircle * 5f;
        MainRef = GameObject.Find("Main");
        ScriptRef = MainRef.GetComponent<Main>();
    }
	
	//Called every frame
	void Update()
	{
        rowColIndex = ScriptRef.TileGrid.GetRowColIndexes(transform.position.x / ScriptRef.BackGroundScale,
        	transform.position.y / ScriptRef.BackGroundScale);

        //Relative position the point has IN the tile it is currently in, from 0 to 1.
        float relXPos = (transform.position.x - rowColIndex[1] * ScriptRef.GetTileWidth()) / ScriptRef.GetTileWidth();
        float relYPos = (transform.position.y - rowColIndex[0] * ScriptRef.GetTileWidth()) / ScriptRef.GetTileWidth();

        Velocity = ScriptRef.TileGrid.GetFlowTile(rowColIndex[0], rowColIndex[1]).Velocity(relXPos, relYPos);
		
		transform.Translate(Velocity * Time.deltaTime);
	}
}