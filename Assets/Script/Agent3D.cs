using System.Collections;
using System.Collections.Generic;
using Script.FlowTileUtils;
using Script.GridBuilding;
using UnityEngine;

public class Agent3D : MonoBehaviour
{
	public Vector3 Velocity;
	public Main3D MainRef;
    private int[] rowColIndex;
    private float collisionRadius;
    private Animator animator;
    
    //Which layer of the flow tile the point follows.
    public int FollowingLayer;
	//Use this for initialization

	void Start()
	{
		Velocity = Vector3.zero;
        collisionRadius = 1 f;
        animator = gameObject.GetComponent<Animator>();
	}   
	
    private void updateCurrentTile() 
    {
        if (MainRef.TileGrid_1.GetRowColIndexes(transform.position.x / MainRef.BackGroundScale,
        	transform.position.z / MainRef.BackGroundScale) != rowColIndex)
        {
            MainRef.TileGrid_1.GetFlowTile(MainRef.TileGridDimension - rowColIndex[0] - 1, rowColIndex[1]).Agents.Remove(gameObject);
            
            rowColIndex = MainRef.TileGrid_1.GetRowColIndexes(transform.position.x / MainRef.BackGroundScale,
        	transform.position.z / MainRef.BackGroundScale);

            MainRef.TileGrid_1.GetFlowTile(MainRef.TileGridDimension - rowColIndex[0] - 1, rowColIndex[1]).Agents.Add(gameObject);
        }
    }

    private void collisionAvoidance(float relXPos, float relZPos)
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

        if (relZPos < collisionRadius && rowColIndex[0] != 0)
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

        if (1 - relZPos < collisionRadius && rowColIndex[0] != MainRef.TileGridDimension - 1)
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
        	transform.position.z / MainRef.BackGroundScale);
        
        //Relative position the point has IN the tile it is currently in, from 0 to 1.
        float relXPos = (transform.position.x - rowColIndex[1] * MainRef.GetTileWidth()) / MainRef.GetTileWidth();
        float relZPos = (transform.position.z - rowColIndex[0] * MainRef.GetTileWidth()) / MainRef.GetTileWidth();

        //Debug.Log(relXPos + " " + relZPos);

        if (FollowingLayer == 1)
        {
            Velocity = 3f * MainRef.TileGrid_1.GetFlowTile(MainRef.TileGrid_1.Dimension - rowColIndex[0] - 1, rowColIndex[1]).Velocity3D(relXPos, relZPos);
        }
        else
        {
            Velocity = 3f * MainRef.TileGrid_2.GetFlowTile(MainRef.TileGrid_2.Dimension - rowColIndex[0] - 1, rowColIndex[1]).Velocity3D(relXPos, relZPos);
        }

        collisionAvoidance(relXPos, relZPos);

        //Debug.Log("Point speed: " + Velocity.magnitude);

        Debug.Log("x vel: " + Velocity.x);
        Debug.Log("z vel: " + Velocity.z);
        Debug.Log("x pos: " + transform.position.x);
        Debug.Log("z pos: " + transform.position.z);

        //transform.Translate(Velocity * Time.deltaTime);
        transform.position += Velocity * Time.deltaTime;
        if ((transform.rotation.eulerAngles.y - Quaternion.LookRotation(Velocity).eulerAngles.y) > 10f)
        {
            transform.Rotate(new Vector3(0, -10f, 0));
        }
        else if((transform.rotation.eulerAngles.y - Quaternion.LookRotation(Velocity).eulerAngles.y) < -10f)
        {
            transform.Rotate(new Vector3(0, 10f, 0));
        }
        else if (Mathf.Abs(transform.rotation.eulerAngles.y - Quaternion.LookRotation(Velocity).eulerAngles.y) > 5f)
        {
            transform.rotation = Quaternion.LookRotation(Velocity);
        }

        if (Velocity.magnitude < 0.05f) 
        {
            animator.speed = 0;
        } 
        else 
        {
            animator.speed = Velocity.magnitude / 2.5f;
        }

        if (rowColIndex[1] == 0 && relXPos < 0.05f)
        {
            Destroy(gameObject);
            MainRef.TileGrid_1.GetFlowTile(MainRef.TileGridDimension - rowColIndex[0] - 1, rowColIndex[1]).Agents.Remove(gameObject);
        }
        else if (rowColIndex[1] == MainRef.TileGridDimension - 1 && relXPos > 0.95f)
        {
            Destroy(gameObject);
            MainRef.TileGrid_1.GetFlowTile(MainRef.TileGridDimension - rowColIndex[0] - 1, rowColIndex[1]).Agents.Remove(gameObject);
        }
        else if (rowColIndex[0] == 0 && relZPos < 0.05f)
        {
            Destroy(gameObject);
            MainRef.TileGrid_1.GetFlowTile(MainRef.TileGridDimension - rowColIndex[0] - 1, rowColIndex[1]).Agents.Remove(gameObject);
        }
        else if (rowColIndex[0] == MainRef.TileGridDimension - 1 && relZPos > 0.95f)
        {
            Destroy(gameObject);
            MainRef.TileGrid_1.GetFlowTile(MainRef.TileGridDimension - rowColIndex[0] - 1, rowColIndex[1]).Agents.Remove(gameObject);

        }

        else
        {
            updateCurrentTile();
        }
    }
}
