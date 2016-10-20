using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Stein.DynamicBone
{
	//[System.Serializable]
	public class BoneParticle
	{

		#region 属性

		float invMass;

		float mass;

		public float Mass
		{
			set
			{
				mass = value;
				if (mass != 0)
				{
					invMass = 1 / mass;
				}
				else
					invMass = float.PositiveInfinity;
			}
			get
			{
				return mass;
			}
		}

		/// <summary>
		///质量倒数
		/// </summary>
		public float InvMass
		{
			get
			{
				if (kinematic)
					return 0;
				else
					return invMass;
			}
		}

		public float radius;
		/// <summary>
		/// The kinematic.
		/// 质量无穷，不会被弹性影响
		/// </summary>
		public bool kinematic;
		/// <summary>
		/// The damp.
		/// 速度衰减
		/// </summary>
		public float damp;
		public float stiffnessBend;
		public float stiffnessStretch;

		#endregion

		/// <summary>
		/// 当前位置
		/// </summary>
		public Vector3 position;
		/// <summary>
		/// 上一次的位置
		/// </summary>
		public Vector3 prevPosition;
		/// <summary>
		/// 受力
		/// </summary>
		public Vector3 force;

		public Transform transform;

		/// <summary>
		/// 原始骨骼长度，也是弹簧的 rest length
		/// </summary>
		public float boneInitLength;

		/// <summary>
		/// 从根节点到当前骨骼的长度
		/// </summary>
		public float boneTotalLength;

		/// <summary>
		/// 父节点
		/// </summary>
		public BoneParticle parent;
		/// <summary>
		/// 子节点
		/// </summary>
		public List<BoneParticle> children = new List<BoneParticle> ();

		public Vector3 initLocalPosition;
		public Quaternion initLocalRotation;
	}
}