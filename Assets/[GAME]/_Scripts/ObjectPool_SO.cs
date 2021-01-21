using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Custom/Object Pool")]
public class ObjectPool_SO : ScriptableObject
{
    public PoolableObject prefab;

    private Queue<PoolableObject> pool = new Queue<PoolableObject>();
	private Transform poolParent;

    public PoolableObject GetPooledObject()
	{
		PoolableObject obj = pool.Count > 0 ? pool.Dequeue() : CreateObject();
		return obj;
	}

	public T GetPooledObject<T>() where T : MonoBehaviour
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
		pool.Enqueue(obj);
		obj.transform.SetParent(poolParent);
		obj.gameObject.SetActive(false);
	}

	private void OnDestroy()
	{
		if (poolParent != null)
			DestroyImmediate(poolParent);
	}
}
