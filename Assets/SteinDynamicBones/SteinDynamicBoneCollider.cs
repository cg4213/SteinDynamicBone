using UnityEngine;
using System.Collections;

namespace Stein.DynamicBone
{
	public abstract class SteinDynamicBoneCollider : MonoBehaviour
	{
		protected abstract void CollideImpl (ref Vector3 position, float particleRadius);

		public void Collide (ref Vector3 position, float particleRadius)
		{
			CollideImpl (ref position, particleRadius);
		}
	}
}