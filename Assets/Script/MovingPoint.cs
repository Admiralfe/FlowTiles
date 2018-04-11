using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPoint : MonoBehaviour
{
	public Vector2 Velocity;
	public Main MainRef;

	// Use this for initialization
	void Start()
	{
		Velocity = Random.insideUnitCircle * 5f;
	}
	
	//Called every frame
	void Update()
	{
		int[] rowColIndex = MainRef.tileGrid.GetRowColIndexes(transform.position.x / MainRef.BackgroundScale,
			transform.position.y / MainRef.BackgroundScale);
		
		
		//Relative position the point has IN the tile it is currently in, from 0 to 1.
		float relXPos = (transform.position.x - rowColIndex[0] * MainRef.getTileWidth()) / MainRef.getTileWidth();
		float relYPos = (transform.position.y - rowColIndex[1] * MainRef.getTileWidth()) / MainRef.getTileWidth();
		
		
		
		//transform.Translate(Velocity * Time.deltaTime);
	}
}