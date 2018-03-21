using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPoint : MonoBehaviour
{
	public Vector2 velocity;

	// Use this for initialization
	void Start()
	{
		velocity = Random.insideUnitCircle * 0.01f;
	}
	
	//Called every frame
	void Update()
	{
		transform.Translate(velocity);	
	}
}