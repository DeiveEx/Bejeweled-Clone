using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Custom/Object Pool")]
public class ObjectPool_SO : ScriptableObject
{
    public PoolableObject prefab;
	[SerializeField] private int initialAmount;

    private Queue<PoolableObject> pool = new Queue<PoolableObject>();
	private Transform poolParent;

	public void Initialize()
	{
		if (pool.Count >= initialAmount)
			return;

		for (int i = 0; i < initialAmount; i++)
		{
			PoolableObject obj = CreateObject();
			ReturnToPool(obj);
		}
	}

    public PoolableObject GetPooledObject()
	{
		PoolableObject obj = pool.Count > 0 && !pool.Peek().gameObject.activeSelf ? pool.Dequeue() : CreateObject();
		obj.gameObject.SetActive(true);
		return obj;
	}

	public T GetPooledObject<T>() where T : Component
	{
		return GetPooledObject().GetComponent<T>();
	}

    private PoolableObject CreateObject()
	{
		if(poolParent == null)
		{
			poolParent = new GameObject(this.name + "(Pool)").transform;
			DontDestroyOnLoad(poolParent);
		}

		PoolableObject obj = Instantiate(prefab.gameObject).GetComponent<PoolableObject>();
		obj.parentPool = this;
		obj.transform.SetParent(poolParent);

		return obj;
	}

    public void ReturnToPool(PoolableObject obj)
	{
		obj.transform.SetParent(poolParent);
		obj.gameObject.SetActive(false);
		pool.Enqueue(obj);
	}

	private void OnDestroy()
	{
		if (poolParent != null)
			DestroyImmediate(poolParent);
	}
}
