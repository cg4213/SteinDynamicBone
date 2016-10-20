using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HelloVerletProjection : MonoBehaviour
{

	public class VerletParticle
	{
		public Vector3 position;
		public Vector3 prevPosition;
		public Vector3 force;
	}

	public  List<VerletParticle> particles;
	public Vector3 gravity;

	public float timeStep;
	public int num;

	void Verlet (float deltaT)
	{
		VerletParticle tmpParticle;
		Vector3 positionCache;
		for (int i = 0; i < particles.Count; ++i)
		{
			//verlet 
			tmpParticle = particles [i];
			positionCache = tmpParticle.position;
			tmpParticle.position += tmpParticle.position - tmpParticle.prevPosition + tmpParticle.force * deltaT * deltaT;
			tmpParticle.prevPosition = positionCache;
		}
	}

	void AccumulateForces ()
	{
		// apply grivaty
		for (int i = 0; i < particles.Count; ++i)
		{
			particles [i].force = gravity;
		}
	}

	void SatisfyConstraints ()
	{
		//constrain in a box

		for (int i = 0; i < particles.Count; ++i)
		{
			particles [i].position = Vector3.Min (Vector3.Max (particles [i].position, new Vector3 (0, 0, 0)), new Vector3 (1000, 1000, 1000));
		}
	}

	// Use this for initialization
	void Start ()
	{
		particles = new List<VerletParticle> ();
		for (int i = 0; i < num; ++i)
		{
			VerletParticle newParticle = new VerletParticle ();
			newParticle.position = newParticle.prevPosition = transform.position;
			particles.Add (newParticle);
		}
	}
	
	// Update is called once per frame
	void Update ()
	{
		AccumulateForces ();
		if (timeStep == 0)
		{
			Verlet (Time.deltaTime);
		}
		else
		{
			Verlet (timeStep);
		}
		SatisfyConstraints ();
	}

	void OnDrawGizmos ()
	{
		if (particles != null)
		{
			for (int i = 0; i < particles.Count; ++i)
			{
				Gizmos.DrawSphere (particles [i].position, 1);
			}
		}
	}
}
