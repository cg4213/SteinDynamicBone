using UnityEngine;
using System.Collections;

namespace Stein.DynamicBone
{

	public class SteinCapsuleCollider:SteinDynamicBoneCollider
	{
		public float radius;

		public Vector3 center0;
		public Vector3 center1;

		Vector3 mWorldCenter0;
		Vector3 mWorldCenter1;

		#region implemented abstract members of SteinDynamicBoneCollider

		protected override void CollideImpl (ref Vector3 position, float particleRadius)
		{
			mWorldCenter0 = transform.TransformPoint (center0);
			mWorldCenter1 = transform.TransformPoint (center1);

			Vector3 posDelta0 = position - mWorldCenter0;
//			Vector3 posDelta1 = position - mWorldCenter1;

			float r = radius + particleRadius;
			float r2 = r * r;
			Vector3 dir = mWorldCenter1 - mWorldCenter0;
			Vector3 d = position - mWorldCenter0;
			float t = Vector3.Dot (d, dir);

			if (t <= 0)
			{
				// check sphere1
				float len2 = d.sqrMagnitude;
				if (len2 > 0 && len2 < r2)
				{
					float len = Mathf.Sqrt (len2);
					position = mWorldCenter0 + d * (r / len);
				}
			}
			else
			{
				float dl = dir.sqrMagnitude;
				if (t >= dl)
				{
					// check sphere2
					d = position - mWorldCenter1;
					float len2 = d.sqrMagnitude;
					if (len2 > 0 && len2 < r2)
					{
						float len = Mathf.Sqrt (len2);
						position = mWorldCenter1 + d * (r / len);
					}
				}
				else if (dl > 0)
				{
					// check cylinder
					t /= dl;
					d -= dir * t;
					float len2 = d.sqrMagnitude;
					if (len2 > 0 && len2 < r2)
					{
						float len = Mathf.Sqrt (len2);
						position += d * ((r - len) / len);
					}
				}
			}
		}

		#endregion

		void OnDrawGizmosSelected ()
		{
			if (!enabled)
				return;
			mWorldCenter0 = transform.TransformPoint (center0);
			mWorldCenter1 = transform.TransformPoint (center1);

			Vector3 offsetDir = Vector3.Cross (mWorldCenter1 - mWorldCenter0, Vector3.up).normalized;
			Gizmos.DrawLine (mWorldCenter0 + offsetDir * radius, mWorldCenter1 + offsetDir * radius);

			offsetDir = Vector3.Cross (mWorldCenter1 - mWorldCenter0, Vector3.down).normalized;
			Gizmos.DrawLine (mWorldCenter0 + offsetDir * radius, mWorldCenter1 + offsetDir * radius);

			offsetDir = Vector3.Cross (mWorldCenter1 - mWorldCenter0, Vector3.forward).normalized;
			Gizmos.DrawLine (mWorldCenter0 + offsetDir * radius, mWorldCenter1 + offsetDir * radius);

			offsetDir = Vector3.Cross (mWorldCenter1 - mWorldCenter0, Vector3.back).normalized;
			Gizmos.DrawLine (mWorldCenter0 + offsetDir * radius, mWorldCenter1 + offsetDir * radius);
//			offsetDir = Vector3.Cross (center1 - center0, Vector3.down).normalized;
//			Gizmos.DrawLine (transform.TransformPoint (center0) + transform.TransformDirection (offsetDir * radius), transform.TransformPoint (center1) + transform.transform.TransformDirection (offsetDir * radius));
//			offsetDir = Vector3.Cross (center1 - center0, Vector3.forward).normalized;
//			Gizmos.DrawLine (transform.TransformPoint (center0) + transform.TransformDirection (offsetDir * radius), transform.TransformPoint (center1) + transform.transform.TransformDirection (offsetDir * radius));
//			offsetDir = Vector3.Cross (center1 - center0, Vector3.back).normalized;
//			Gizmos.DrawLine (transform.TransformPoint (center0) + transform.TransformDirection (offsetDir * radius), transform.TransformPoint (center1) + transform.transform.TransformDirection (offsetDir * radius));

			Gizmos.DrawWireSphere (transform.TransformPoint (center0), radius);
			Gizmos.DrawWireSphere (transform.TransformPoint (center1), radius);
		}
	}
}