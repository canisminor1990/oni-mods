===== ResourceSet =====
using System;

[Serializable]
public abstract class ResourceSet : Resource
{
	public abstract int Count { get; }

	public ResourceSet()
	{
	}

	public ResourceSet(string id, ResourceSet parent)
		: base(id, parent)
	{
	}

	public abstract Resource Add(Resource resource);

	public abstract void Remove(Resource resource);

	public abstract Resource GetResource(int idx);
}

===== MISSING ResourceSet1 =====

