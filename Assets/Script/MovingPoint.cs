using System.Collections;
using System.Collections.Generic;
using Script.FlowTileUtils;
using Script.GridBuilding;
using UnityEngine;

public class MovingPoint : MonoBehaviour
{
	public Vector2 Velocity;
	public Main MainRef;
    private int[] rowColIndex;
	// Use this for initialization
	void Start()
	{
		Velocity = Vector2.zero;
	}
	
	//Called every frame
	void Update()
	{
        rowColIndex = MainRef.TileGrid.GetRowColIndexes(transform.position.x / MainRef.BackGroundScale,
        	transform.position.y / MainRef.BackGroundScale);

        //Relative position the point has IN the tile it is currently in, from 0 to 1.
        float relXPos = (transform.position.x - rowColIndex[1] * MainRef.GetTileWidth()) / MainRef.GetTileWidth();
        float relYPos = (transform.position.y - rowColIndex[0] * MainRef.GetTileWidth()) / MainRef.GetTileWidth();
        if (rowColIndex[1] == 0 && relXPos < 0.01f)
        {
            Destroy(gameObject);
        }
        else if (rowColIndex[1] == MainRef.TileGridDimension - 1 && relXPos > 0.99f)
        {
            Destroy(gameObject);
        }
        else if (rowColIndex[0] == 0 && relYPos < 0.01f)
        {
            Destroy(gameObject);
        }
        else if (rowColIndex[0] == 0 && relYPos > 0.99f)
        {
            Destroy(gameObject);
        }

        Velocity = MainRef.TileGrid.GetFlowTile(rowColIndex[0], rowColIndex[1]).Velocity(relXPos, relYPos);
		
		transform.Translate(Velocity * Time.deltaTime);
	}
}