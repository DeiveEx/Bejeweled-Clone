using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetShaderFloat : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private string propertyName;
    [SerializeField] private bool useRandomRange;
    [SerializeField] private float value;

	private void Awake()
	{
		Material m = image.material;

		if (useRandomRange)
		{
			m.SetFloat(propertyName, Random.Range(0, value));
		}
		else
		{
			m.SetFloat(propertyName, value);
		}
	}
}
