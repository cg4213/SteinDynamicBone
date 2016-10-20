using UnityEngine;
using System.Collections;

public class SimpleVerlet : MonoBehaviour
{

	public Transform root;

	public float Elasticity;
	public float originalLength;

	public float position;
	public float prevPosition;
	public float weight;
	public float v0;

	public float energy;
	public float damp;
	float lastDeltaT;

	void Start ()
	{
		prevPosition = transform.position.x;
		position = transform.position.x;
//		originalLength = root.position.x
	}

	void Update ()
	{
//		position =  transform.position.x;
	}

	void LateUpdate ()
	{
		//f = ma = kx
		//s = s0+ v0*deltaT+kx/m * deltaT^2
		//v = v0+kx/m * deltaT

		//v0 = (prevPosition -position)/deltaT
		if (lastDeltaT == 0)
			lastDeltaT = Time.deltaTime;
		v0 = (position - prevPosition) * (1 - damp) / lastDeltaT;
		float positionCache = position;

		position = position + v0 * Time.deltaTime + 0.5f * (Elasticity * (originalLength - positionCache) * (1 - damp) / weight) * Time.deltaTime * Time.deltaTime;
//		position = position +(position - prevPosition) * (1 - damp) + 0.5f * (Elasticity * (originalLength - positionCache) * (1 - damp) / weight) * Time.deltaTime * Time.deltaTime;

		prevPosition = positionCache;
//		v0 = v0 + (Elasticity * (originalLength - positionCache) / weight) * Time.deltaTime;
//		v0 = (position - prevPosition) * (1 - damp) / Time.deltaTime;
		lastDeltaT = Time.deltaTime;
		transform.position = new Vector3 (position, 0, 0);

		energy = weight * v0 * v0 * 0.5f + Mathf.Abs (0.5f * (originalLength - position) * (originalLength - position) * Elasticity);
	}
}
