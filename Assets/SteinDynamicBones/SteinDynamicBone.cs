using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Stein.DynamicBone
{
	/// <summary>
	/// 链式的动态骨骼，模拟头发尾巴之类的
	/// 
	/// 关于最后一个虚节点，两种情况有用
	/// 1.需要正确处理最后一个节点的旋转时，正常情况下最后一个节点并不能正确的旋转，这时就需要额外一个节点来参与计算
	/// 2.只有一个节点时
	/// </summary>
	public class SteinDynamicBone: MonoBehaviour
	{
		public enum ExtraEndParticle
		{
			None,
			Length,
			Offset,
		}

		float fixedDeltaTime;

		float frameDeltaTime;

		List<BoneParticle> bones;
		float boneTotalLength;

		[Tooltip ("动态骨骼根节点")]
		public Transform root;

		/// <summary>
		/// 模拟更新帧率
		/// </summary>
		[Tooltip ("模拟更新帧率")]
		public int frameRate = 60;

		[Tooltip ("碰撞")]
		public List<SteinDynamicBoneCollider> colliders;

		/// <summary>
		/// 不受动态骨骼控制的部分
		/// </summary>
		[Tooltip ("不受动态骨骼控制的部分")]
		public List<Transform> excludtions;

		[Header ("结尾虚结点")]
		/// <summary>
	/// 结尾虚结点控制
	/// </summary>
		public ExtraEndParticle extraEndPointType;
		/// <summary>
		/// 以距离控制虚结点位置
		/// </summary>
		public float extraEndPointDist;
		/// <summary>
		/// 虚结点位置
		/// </summary>
		public Vector3 extraEndPointOffset;

		[Header ("骨骼参数")]

		public float basicRadius = 0.1f;
		public AnimationCurve radiusDist;
		/// <summary>
		/// 基础质量
		/// </summary>
		public float basicMass = 1;
		/// <summary>
		/// 质量分布
		/// </summary>
		public AnimationCurve massDist;

		[Range (0, 1)]
		public float damp = 0.1f;
		public AnimationCurve dampDist;

		/// <summary>
		/// 弯曲刚性
		/// </summary>	
		[Range (0, 1)]
		public float stiffnessBend = 0.01f;

		public AnimationCurve stiffnessBendDist;

		/// <summary>
		/// 拉伸刚性
		/// </summary>
		[Range (0, 1)]
		public float stiffnessStretch = 0.9f;
		/// <summary>
		/// 拉伸刚性分布
		/// </summary>
		public AnimationCurve stiffnessStretchDist;

		[Header ("骨骼受力")]
		/// <summary>
	/// 动态骨骼受到的全局重力
	/// </summary>
		public Vector3 gravity;

		/// <summary>
		/// 外部力
		/// </summary>
		public Vector3 externalForce;

		bool InExcludtion (Transform bone)
		{
			for (int i = 0; i < excludtions.Count; ++i)
			{
				if (excludtions [i] == bone)
					return true;
			}

			return false;
		}

		float EvaluateParam (float basic, AnimationCurve curve, BoneParticle bone)
		{
			if (curve != null && curve.keys.Length > 0)
				return basic * curve.Evaluate (bone.boneTotalLength / boneTotalLength);
			else
				return basic;
		}

		void AppendBone (Transform bone, BoneParticle parentBone, Vector3 extraEndBoneOffset = default(Vector3))
		{
			BoneParticle particle = new BoneParticle ();

			if (bone != null)
			{
				particle.position = particle.prevPosition = bone.position;
				particle.transform = bone;
				particle.initLocalPosition = bone.localPosition;
				particle.initLocalRotation = bone.localRotation;
			}
			else
			{
				//虚结点,应该不会出现虚结点没有父节点的情况……
				particle.position = particle.prevPosition = parentBone.position + extraEndBoneOffset;
				particle.initLocalPosition = parentBone.transform.InverseTransformPoint (particle.position);
				particle.initLocalRotation = Quaternion.identity;
			}


			if (parentBone != null)
			{
				particle.boneInitLength = Vector3.Distance (particle.position, parentBone.position);
				particle.boneTotalLength += particle.boneInitLength;
				boneTotalLength = Mathf.Max (boneTotalLength, particle.boneTotalLength);

				//建立树形关系
				parentBone.children.Add (particle);
				particle.parent = parentBone;
			}
			else
			{
				//第一个骨骼这个节点是
				particle.kinematic = true;
			}

			bones.Add (particle);
			if (bone != null)
			{
				bool addNode = false;
				int childCount = bone.childCount;

				for (int i = 0; i < childCount; ++i)
				{
					if (!InExcludtion (bone.GetChild (i)))
					{
						AppendBone (bone.GetChild (i), particle);
						addNode = true;
					}
				}

				if (!addNode)
				{
					//虚结点
					switch (extraEndPointType)
					{
						case ExtraEndParticle.Length:
							AppendBone (null, particle, Vector3.Normalize (particle.position - parentBone.position) * extraEndPointDist);
							break;
						case ExtraEndParticle.Offset:
							AppendBone (null, particle, extraEndPointOffset);
							break;
						default:
						//do nothing no extra end bone appended;
							break;
					}
				}
			}
		}
		// Use this for initialization
		void Start ()
		{
			bones = new List<BoneParticle> ();
			fixedDeltaTime = 1.0f / frameRate;
			AppendBone (root, null);

			//用分布曲线设置骨骼参数
			BoneParticle tmpBone;
			for (int i = 0; i < bones.Count; ++i)
			{
				tmpBone = bones [i];
				tmpBone.radius = EvaluateParam (basicRadius, radiusDist, tmpBone);
				tmpBone.Mass = EvaluateParam (basicMass, massDist, tmpBone);
				tmpBone.damp = Mathf.Clamp01 (EvaluateParam (damp, massDist, tmpBone));
				tmpBone.stiffnessBend = Mathf.Clamp01 (EvaluateParam (stiffnessBend, stiffnessBendDist, tmpBone));
				tmpBone.stiffnessStretch = Mathf.Clamp01 (EvaluateParam (stiffnessStretch, stiffnessStretchDist, tmpBone));
			}
		}

		void Update ()
		{
			ResetTransforms ();
		}

		void LateUpdate ()
		{
			frameDeltaTime += Time.deltaTime;
			while (frameDeltaTime >= fixedDeltaTime)
			{
				frameDeltaTime -= fixedDeltaTime;
				UpdateBoneChain (fixedDeltaTime);
			}

			ApplyTransform ();
		}

		void ResetTransforms ()
		{
			BoneParticle particle;
			for (int i = 0; i < bones.Count; ++i)
			{
				particle = bones [i];
				if (particle.transform != null)
				{
					particle.transform.localPosition = particle.initLocalPosition;
					particle.transform.localRotation = particle.initLocalRotation;
				}
			}
		}

		void UpdateBoneChain (float delta)
		{
			AccumulateForces ();
			Verlet (delta);
			ApplyConstraint ();
		}



		void AccumulateForces ()
		{
			for (int i = 0; i < bones.Count; ++i)
			{
				bones [i].force = gravity + externalForce;
			}
		}

		void Verlet (float deltaTime)
		{
			BoneParticle particle = null;
			Vector3 posCache;

			for (int i = 0; i < bones.Count; ++i)
			{
				particle = bones [i];
				if (particle.kinematic)
				{
					particle.prevPosition = particle.position;
					particle.position = particle.transform.position;
				}
				else
				{
					posCache = particle.position;
					particle.position += (particle.position - particle.prevPosition) * (1 - particle.damp) + particle.force * particle.InvMass * deltaTime * deltaTime;
					particle.prevPosition = posCache;
				}
			}
		}

		void ApplyConstraint ()
		{
			for (int iter = 0; iter < 10; ++iter)
			{

				for (int i = 0; i < bones.Count; ++i)
				{
					BoneParticle bone = bones [i];

					BoneParticle parentBone = bone.parent;
					if (parentBone == null)
						continue;
				
					Vector3 originOffset = bone.position - parentBone.position;
					float originLength = Mathf.Sqrt (Vector3.Dot (originOffset, originOffset));

					//Stiffness_Bend,弯曲刚性
					//弯曲刚性应该不影响长度，只影响方向，正常的算法是需要acos的，不过这里用两步来趋近
					Matrix4x4 parentLocalToWorld = parentBone.transform.localToWorldMatrix;
					parentLocalToWorld.SetColumn (3, parentBone.position);//parent的位置可能是有变化的，修正坐标转换矩阵

					Vector3 restPos = parentLocalToWorld.MultiplyPoint3x4 (bone.initLocalPosition);
					Vector3 bendStiffnessVector = restPos - bone.position;
					bone.position += bendStiffnessVector * bone.stiffnessBend;
					//此时两个particle之间的距离已经发生了变化，在计算线性刚性之前要修正一下,这个修正只影响子节点就可以
					Vector3 bended = bone.position - parentBone.position;
					float bendedLength = Mathf.Sqrt (Vector3.Dot (bended, bended));
					bone.position += bended * (originLength - bendedLength) / bendedLength;

					//Stiffness_Strecth,线性刚性
					//维持长度，将骨骼长度修复回原长度
					Vector3 delta = bone.position - parentBone.position;
					float deltaLength = Mathf.Sqrt (Vector3.Dot (delta, delta));
					float diff = (deltaLength - bone.boneInitLength) / (deltaLength * (parentBone.InvMass + bone.InvMass));
					diff *= bone.stiffnessStretch;

					parentBone.position += diff * delta * parentBone.InvMass;
					bone.position -= diff * delta * bone.InvMass;

					//处理碰撞
					for (int cIndex = 0; cIndex < colliders.Count; ++cIndex)
					{
						colliders [cIndex].Collide (ref bone.position, bone.radius);
					}

				}
			}
		}



		void ApplyTransform ()
		{
			for (int i = 0; i < bones.Count; ++i)
			{
				BoneParticle bone = bones [i];
				if (!bone.kinematic)
				{	
					//fix rotation
					if (bone.parent != null && bone.parent.children.Count <= 1)//有多个子节点的就忽略
					{
						BoneParticle parent = bone.parent;
						Vector3 dir0;
						if (bone.transform != null)
						{
							dir0 = bone.transform.localPosition;
						}
						else
						{
							dir0 = bone.initLocalPosition;
						}
						Vector3 dir = bone.position - parent.position;
						parent.transform.rotation = Quaternion.FromToRotation (parent.transform.TransformDirection (dir0), dir) * parent.transform.rotation;
					}

					if (bone.transform != null)
						bone.transform.position = bone.position;
		
				}
			}
		}



		void OnDrawGizmos ()
		{
			if (bones != null)
			{
				for (int i = 0; i < bones.Count; ++i)
				{
					if (bones [i].parent != null)
					{
						Gizmos.DrawLine (bones [i].parent.position, bones [i].position);
					}
					Gizmos.DrawSphere (bones [i].position, bones [i].radius);
				}
			}
		}

	}
}