using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HelloVerlet_CubeProjection_RestLength : MonoBehaviour
{
	[System.Serializable]
	public class VerletParticle
	{
		public Vector3 position;
		public Vector3 prevPosition;
		public Vector3 force;
	}

	public  List<VerletParticle> particles;
	public Vector3 gravity;
	/// <summary>
	/// 粒子间距
	/// </summary>
	public float restLength;
	public int numOfIter = 1;
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
		for (int iter = 0; iter < numOfIter; ++iter)
		{
			//constrain in a box

			for (int i = 0; i < particles.Count; ++i)
			{
				particles [i].position = Vector3.Min (Vector3.Max (particles [i].position, new Vector3 (0, 0, 0)), new Vector3 (1000, 1000, 1000));
			}


			//rest length constraints
			//就是限制两个粒子间距是restlength：|p2-p1| = restlength
			//求出位置差，两头分别移动一半距离
			for (int i = 1; i < particles.Count; ++i)
			{
				VerletParticle prevParticle = particles [i - 1];
				VerletParticle particle = particles [i];
				Vector3 delta = particle.position - prevParticle.position;

				//正常的计算
				float deltaLength = delta.magnitude;//Mathf.Sqrt (delta * delta);
				float diff = (deltaLength - restLength) / deltaLength;
				prevParticle.position += delta * 0.5f * diff;
				particle.position -= delta * 0.5f * diff;

				//优化平方根（泰勒展开）
//				delta = delta * restLength * restLength / ((Vector3.Dot (delta, delta) + restLength * restLength) - 0.5f);
//				prevParticle.position += delta;
//				particle.position -= delta;
			}
		}
	}
	// Use this for initialization
	void Start ()
	{
		particles = new List<VerletParticle> ();
		for (int i = 0; i < num; ++i)
		{
			VerletParticle newParticle = new VerletParticle ();
			newParticle.position = newParticle.prevPosition = transform.position + new Vector3 (10 * i, 0, 0);
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
		Gizmos.DrawWireCube (new Vector3 (500, 500, 500), new Vector3 (1000, 1000, 1000));
		if (particles != null)
		{
			for (int i = 0; i < particles.Count; ++i)
			{
				Gizmos.DrawSphere (particles [i].position, 5);
			}
		}
	}
}
