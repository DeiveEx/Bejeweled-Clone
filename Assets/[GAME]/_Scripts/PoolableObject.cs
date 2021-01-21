using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolableObject : MonoBehaviour
{
    [HideInInspector] public ObjectPool_SO parentPool;

    public void ReturnToPool()
	{
		parentPool.ReturnToPool(this);
	}
}
