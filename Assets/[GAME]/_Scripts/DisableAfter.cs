using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableAfter : MonoBehaviour
{
    public bool playOnEnable = true;
	public float delay = 1;

    private PoolableObject poolable;

	private void Awake()
	{
		poolable = GetComponent<PoolableObject>();
	}

	private void OnEnable()
	{
		if (playOnEnable)
			Invoke(nameof(DisableObject), delay);
	}

	public void DisableObject()
	{
		//If this is a poolable object, we return it to pool, and the pool will disable it
		if(poolable != null)
		{
			poolable.ReturnToPool();
			return;
		}

		//Otehrwise, we just disable it
		gameObject.SetActive(false);
	}
}
