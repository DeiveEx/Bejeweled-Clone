using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Cell : MonoBehaviour
{
    public int typeID;

    public RectTransform rectTransform { get; private set; }

	private void Awake()
	{
		rectTransform = GetComponent<RectTransform>();
	}
}
