using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextEffect : MonoBehaviour
{
	public TMP_Text textObj;
	[SerializeField] private float distanceToMove = 1;
	[SerializeField] private float animDuration = 1;
	[SerializeField] private AnimationCurve animCurve = AnimationCurve.Linear(0, 0, 1, 1);

	private Transform myTransform;

	private void Awake()
	{
		myTransform = transform;
	}

	private void OnDisable()
	{
		StopAllCoroutines();
	}

	public void Animate()
	{
		StopAllCoroutines();
		StartCoroutine(Animation_Routine());
	}

	private IEnumerator Animation_Routine()
	{
		float timePassed = 0;
		Vector3 startPos = myTransform.position;
		Vector3 endPos = myTransform.position + Vector3.up * distanceToMove;

		while (timePassed < animDuration)
		{
			timePassed += Time.deltaTime;

			myTransform.position = Vector3.LerpUnclamped(startPos, endPos, animCurve.Evaluate(timePassed / animDuration));

			yield return null;
		}

		myTransform.position = Vector3.Lerp(startPos, endPos, animCurve.Evaluate(1));
	}
}
