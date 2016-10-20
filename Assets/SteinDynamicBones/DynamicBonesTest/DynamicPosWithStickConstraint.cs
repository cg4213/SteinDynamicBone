using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Stein.DynamicBone;

/// <summary>
/// Construct bones.
/// 关于最后一个虚节点，两种情况有用
/// 1.需要正确处理最后一个节点的旋转时，正常情况下最后一个节点并不能正确的旋转，这时就需要额外一个节点来参与计算
/// 2.只有一个节点时
/// </summary>
public class DynamicPosWithStickConstraint : MonoBehaviour
{
	public enum ExtraEndParticle
	{
		None,
		Length,
		Offset,
	}

	public Transform root;
	public List<BoneParticle> bones;

	public List<Transform> excludtions;

	public ExtraEndParticle extraEndPointType;
	public float extraEndPointDist;
	public Vector3 extraEndPointOffset;

	public Vector3 gravity;

	public float damp;

	public float stiffnessBend;
	public float stiffnessStretch;

	bool InExcludtion (Transform bone)
	{
		for (int i = 0; i < excludtions.Count; ++i)
		{
			if (excludtions [i] == bone)
				return true;
		}

		return false;
	}

	void AppendBone (Transform bone, BoneParticle parentBone, Vector3 extraEndBoneOffset = default(Vector3))
	{
		BoneParticle particle = new BoneParticle ();

		if (bone != null)
		{
			particle.position = particle.prevPosition = bone.position;
			particle.transform = bone;
			particle.initLocalPosition = bone.localPosition;
		}
		else
		{
			//虚结点,应该不会出现虚结点没有父节点的情况……
			particle.position = particle.prevPosition = parentBone.position + extraEndBoneOffset;
			particle.initLocalPosition = parentBone.transform.InverseTransformPoint (particle.position);
		}


		if (parentBone != null)
		{
			particle.boneInitLength = Vector3.Distance (particle.position, parentBone.position);
			particle.Mass = 1;//HACK 先将质量都设成1
		}
		else
		{
			//第一个骨骼这个节点是
			particle.kinematic = true;
		}
		particle.damp = damp;
		particle.stiffnessBend = stiffnessBend;
		particle.stiffnessStretch = stiffnessStretch;

		bones.Add (particle);
		if (bone != null)
		{
			int childCount = bone.childCount;
			if (childCount > 0)
			{
				for (int i = 0; i < childCount; ++i)
				{
					if (!InExcludtion (bone.GetChild (i)))
					{
						AppendBone (bone.GetChild (i), particle);
					}
				}
			}
			else
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
		AppendBone (root, null);
	}
	
	// Update is called once per frame
	void Update ()
	{
		AccumulateForces ();
		Verlet (Time.deltaTime);
		Constraint ();
		ApplyTransform ();
	}

	void OnDrawGizmos ()
	{
		if (bones != null)
		{
			for (int i = 0; i < bones.Count; ++i)
			{
				Gizmos.DrawSphere (bones [i].position, 0.02f);
			}
		}
	}

	void AccumulateForces ()
	{
		for (int i = 0; i < bones.Count; ++i)
		{
			bones [i].force = gravity;
		}
	}

	void Verlet (float deltaTime)
	{
		BoneParticle particle = null;
		Vector3 posCache;

		//TODO 第一个节点需要特殊处理
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
				particle.position += (particle.position - particle.prevPosition) * (1 - particle.damp) + particle.force * deltaTime * deltaTime / particle.Mass;
				particle.prevPosition = posCache;
			}
		}
	}

	void Constraint ()
	{
		for (int iter = 0; iter < 10; ++iter)
		{
//			for (int i = 0; i < bones.Count; ++i)
//			{
//				//pos restraints
//			}

			for (int i = 1; i < bones.Count; ++i)
			{
				BoneParticle parentBone = bones [i - 1];
				BoneParticle bone = bones [i];

				Vector3 originOffset = bone.position - parentBone.position;
				float originLength = Mathf.Sqrt (Vector3.Dot (originOffset, originOffset));


				//需要保持原形状
				//Stiffness_Bend,弯曲刚性
				//弯曲刚性应该不影响长度，只影响方向，正常的算法是需要acos的，不过这里用两步来趋近
				Matrix4x4 parentLocalToWorld = parentBone.transform.localToWorldMatrix;
				parentLocalToWorld.SetColumn (3, parentBone.position);//parent的位置可能是有变化的，修正坐标转换矩阵
			
				//				Vector3 restPos = parentLocalToWorld.MultiplyPoint3x4 (bone.boneInitOffset);
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

//				bone.boneCurLength = Vector3.Distance (bone.position, parentBone.position);

				//				float diff = (Vector3.Dot (delta, delta) - bone.boneInitDelta.magnitude * bone.boneInitDelta.magnitude) / (Vector3.Dot (delta, delta) - bone.boneInitDelta.magnitude * bone.boneInitDelta.magnitude);
				//				diff = diff / (parentBone.invMass + bone.invMass);
				//				parentBone.position += diff * delta * parentBone.invMass;
				//				bone.position -= diff * delta * bone.invMass;

			}
		}
	}

	void ApplyTransform ()
	{
		for (int i = 0; i < bones.Count; ++i)
		{
			if (!bones [i].kinematic && bones [i].transform != null)
			{
				bones [i].transform.position = bones [i].position;
			}
		}
	}
}
