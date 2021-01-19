using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game_Manager : MonoBehaviour
{
	public static Game_Manager instance;

	private void Awake()
	{
		if(instance == null)
		{
			instance = this;
		}
		else if( instance != this)
		{
			Destroy(this.gameObject);
		}
	}
}
