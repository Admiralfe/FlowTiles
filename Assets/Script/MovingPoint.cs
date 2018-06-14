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
    private float collisionRadius;
    
    //Which layer of the flow tile the point follows.
    public int FollowingLayer;
	//Use this for initialization

	void Start()
	{
		Velocity = Vector2.zero;
        collisionRadius = 0.1f;
    }
	
    /// <summary>
    /// Updates which tile the agent is currently in and removes agent from the bucket corresponding
    /// to its previous tile and adds it to the bucket of the current tile. This is for more efficient collision detection.
    /// </summary>
    private void updateCurrentTile() 
    {
        if (MainRef.TileGrid_1.GetRowColIndexes(transform.position.x / MainRef.BackGroundScale,
        	transform.position.y / MainRef.BackGroundScale) != rowColIndex)
        {
            MainRef.TileGrid_1.GetFlowTile(MainRef.TileGridDimension - rowColIndex[0] - 1, rowColIndex[1]).Agents.Remove(gameObject);
            
            rowColIndex = MainRef.TileGrid_1.GetRowColIndexes(transform.position.x / MainRef.BackGroundScale,
        	transform.position.y / MainRef.BackGroundScale);

            MainRef.TileGrid_1.GetFlowTile(MainRef.TileGridDimension - rowColIndex[0] - 1, rowColIndex[1]).Agents.Add(gameObject);
        }
    }

    /// <summary>
    /// Performs collision avoidance between an agent and other agents in its tile, including the neighboring tiles
    /// if the agent is close enough to the edge of a tile. Colldiing agents will adjust their velocities to move away from each other.
    /// </summary>
    /// <param name="relXPos"> A float between 0 and 1 describing the agents relative x-position in the tile </param>
    /// <param name="relYPos"> A float between 0 and 1 describing the agents relative y-position in the tile </param>
    private void collisionAvoidance(float relXPos, float relYPos)
    {
        FlowTile currentTile = MainRef.TileGrid_1.GetFlowTile(MainRef.TileGridDimension - rowColIndex[0] - 1, rowColIndex[1]);

        foreach (GameObject otherAgent in currentTile.Agents)
        {
            if (otherAgent != gameObject)
            {
                if ((transform.position - otherAgent.transform.position).magnitude < collisionRadius)
                {
                    Velocity = (transform.position - otherAgent.transform.position).normalized;
                }
            }
        }

        //Check for collisions in neighboring cell if agent is within collision radius of left edge of current cell.
        if (relXPos < collisionRadius && rowColIndex[1] != 0)
        {
            FlowTile leftTile = MainRef.TileGrid_1.GetFlowTile(MainRef.TileGridDimension - rowColIndex[0] - 1, rowColIndex[1] - 1);
            foreach (GameObject otherAgent in leftTile.Agents)
            {
                if (otherAgent != gameObject)
                {
                    if ((transform.position - otherAgent.transform.position).magnitude < collisionRadius)
                    {
                        Velocity = (transform.position - otherAgent.transform.position).normalized;
                    }
                }
            }
        }

        //Check for collisions in neighboring cell if agent is within collision radius of right edge of current cell.
        if (1 - relXPos < collisionRadius && rowColIndex[1] != MainRef.TileGridDimension - 1)
        {
            FlowTile rightTile = MainRef.TileGrid_1.GetFlowTile(MainRef.TileGridDimension - rowColIndex[0] - 1, rowColIndex[1] + 1);
            foreach (GameObject otherAgent in rightTile.Agents)
            {
                if (otherAgent != gameObject)
                {
                    if ((transform.position - otherAgent.transform.position).magnitude < collisionRadius)
                    {
                        Velocity = (transform.position - otherAgent.transform.position).normalized;
                    }
                }
            }
        }

        if (relYPos < collisionRadius && rowColIndex[0] != 0)
        {
            FlowTile bottomTile = MainRef.TileGrid_1.GetFlowTile(MainRef.TileGridDimension - rowColIndex[0], rowColIndex[1]);
            foreach (GameObject otherAgent in bottomTile.Agents)
            {
                if (otherAgent != gameObject)
                {
                    if ((transform.position - otherAgent.transform.position).magnitude < collisionRadius)
                    {
                        Velocity = (transform.position - otherAgent.transform.position).normalized;
                    }
                }
            }
        }

        if (1 - relYPos < collisionRadius && rowColIndex[0] != MainRef.TileGridDimension - 1)
        {
            FlowTile topTile = MainRef.TileGrid_1.GetFlowTile(MainRef.TileGridDimension - rowColIndex[0] - 2, rowColIndex[1]);
            foreach (GameObject otherAgent in topTile.Agents)
            {
                if (otherAgent != gameObject)
                {
                    if ((transform.position - otherAgent.transform.position).magnitude < collisionRadius)
                    {
                        Velocity = (transform.position - otherAgent.transform.position).normalized;
                    }
                }
            }
        }
    }

	//Called every frame
	void Update()
	{
        rowColIndex = MainRef.TileGrid_1.GetRowColIndexes(transform.position.x / MainRef.BackGroundScale,
        	transform.position.y / MainRef.BackGroundScale);
         
        //Relative position the point has IN the tile it is currently in, from 0 to 1.
        float relXPos = (transform.position.x - rowColIndex[1] * MainRef.GetTileWidth()) / MainRef.GetTileWidth();
        float relYPos = (transform.position.y - rowColIndex[0] * MainRef.GetTileWidth()) / MainRef.GetTileWidth();

        if (FollowingLayer == 1)
        {
            Velocity = MainRef.TileGrid_1.GetFlowTile(MainRef.TileGrid_1.Dimension - rowColIndex[0] - 1, rowColIndex[1]).Velocity(relXPos, relYPos);
        }
        else
        {
            Velocity = MainRef.TileGrid_2.GetFlowTile(MainRef.TileGrid_2.Dimension - rowColIndex[0] - 1, rowColIndex[1]).Velocity(relXPos, relYPos);
        }

        collisionAvoidance(relXPos, relYPos);

        //Debug.Log("Point speed: " + Velocity.magnitude);

        transform.Translate(Velocity * Time.deltaTime);

        if (rowColIndex[1] == 0 && relXPos < 0.01f)
        {
            Destroy(gameObject);
            MainRef.TileGrid_1.GetFlowTile(MainRef.TileGridDimension - rowColIndex[0] - 1, rowColIndex[1]).Agents.Remove(gameObject);
        }
        else if (rowColIndex[1] == MainRef.TileGridDimension - 1 && relXPos > 0.99f)
        {
            Destroy(gameObject);
            MainRef.TileGrid_1.GetFlowTile(MainRef.TileGridDimension - rowColIndex[0] - 1, rowColIndex[1]).Agents.Remove(gameObject);
        }
        else if (rowColIndex[0] == 0 && relYPos < 0.01f)
        {
            Destroy(gameObject);
            MainRef.TileGrid_1.GetFlowTile(MainRef.TileGridDimension - rowColIndex[0] - 1, rowColIndex[1]).Agents.Remove(gameObject);
        }
        else if (rowColIndex[0] == MainRef.TileGridDimension - 1 && relYPos > 0.99f)
        {
            Destroy(gameObject);
            MainRef.TileGrid_1.GetFlowTile(MainRef.TileGridDimension - rowColIndex[0] - 1, rowColIndex[1]).Agents.Remove(gameObject);

        }

        else
        {
            updateCurrentTile();
        }

        /*
        if (Velocity.x < 0) 
        {
            Debug.Log("y: " + transform.position.y);
            Debug.Log("RelY: " + relYPos);
        }
        */
	}
}