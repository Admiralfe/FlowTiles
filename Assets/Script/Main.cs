using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{

	public GameObject Point;
	
	//Set this in Unity UI
	public int numberOfAgents;
	// Use this for initialization
	private void Start () 
	{
		for (int i = 0; i < numberOfAgents; i++)
		{
			Instantiate(Point, new Vector3(Random.value * 5, Random.value * 5, 0), Quaternion.identity);
		}
	}
	
	// Update is called once per frame
}
