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
public class ConstructBones : MonoBehaviour
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

	bool InExcludtion (Transform bone)
	{
		for (int i = 0; i < excludtions.Count; ++i)
		{
			if (excludtions [i] == bone)
				return true;
		}

		return false;
	}

	void AppendBone (Transform bone, BoneParticle parendBone, Vector3 extraEndBoneOffset = default(Vector3))
	{
		BoneParticle particle = new BoneParticle ();

		if (bone != null)
		{
			particle.position = particle.prevPosition = bone.position;
		}
		else
		{
			//虚结点,应该不会出现虚结点没有父节点的情况……
			particle.position = particle.prevPosition = parendBone.position + extraEndBoneOffset;
		}

		if (parendBone != null)
		{
		}

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
						AppendBone (null, particle, Vector3.Normalize (particle.position - parendBone.position) * extraEndPointDist);
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
}
