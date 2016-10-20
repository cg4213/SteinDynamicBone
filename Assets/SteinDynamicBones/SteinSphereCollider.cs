using UnityEngine;
using System.Collections;

namespace Stein.DynamicBone
{

	public class SteinSphereCollider:SteinDynamicBoneCollider
	{
		public float radius;
		public Vector3 center;

		Vector3 mWorldCenter;

		#region implemented abstract members of SteinDynamicBoneCollider

		protected override void CollideImpl (ref Vector3 position, float particleRadius)
		{
			mWorldCenter = transform.position + center;
			Vector3 posDelta = position - mWorldCenter;

			float sqrDistance = posDelta.sqrMagnitude;
			float minDistance = radius + particleRadius;
			float minSqrDist = minDistance * minDistance;

			if (sqrDistance < minSqrDist)
			{
				position = mWorldCenter + posDelta.normalized * radius;
			}
		}

		#endregion

		void OnDrawGizmosSelected ()
		{
			if (!enabled)
				return;

			Gizmos.DrawWireSphere(transform.TransformPoint(center), radius);
		}
	}
	
}