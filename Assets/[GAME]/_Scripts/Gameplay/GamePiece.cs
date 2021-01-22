using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GamePiece : MonoBehaviour
{
    public int typeID;
	public Transform myTransform { get; private set; }
	public Vector2Int boardPos;

	private Board board;

	private void Awake()
	{
		myTransform = transform;
		board = FindObjectOfType<Board>();
	}

	public void Initialize()
	{
		Vector2Int start = new Vector2Int(boardPos.x, Mathf.FloorToInt(boardPos.y + (board.boardSize.y * 1.25f)));
		Vector2Int end = boardPos;
		AnimatePositionOnGrid(start, end, board.fallDuration, board.fallCurve);
	}

	public void AnimatePosition(Vector2 start, Vector2 end, float duration, AnimationCurve curve)
	{
		StopAllCoroutines();
		StartCoroutine(AnimatePosition_Routine(start, end, duration, curve));
	}

	public void AnimatePositionOnGrid(Vector2Int start, Vector2Int end, float duration, AnimationCurve curve)
	{
		AnimatePosition(board.ConvertGridPosToWorldPos(start.x, start.y), board.ConvertGridPosToWorldPos(end.x, end.y), duration, curve);
	}

	private IEnumerator AnimatePosition_Routine(Vector2 start, Vector2 end, float duration, AnimationCurve curve)
	{
		float timePassed = 0;

		while (timePassed < duration)
		{
			timePassed += Time.deltaTime;

			myTransform.position = Vector2.LerpUnclamped(start, end, curve.Evaluate(timePassed / duration));

			yield return null;
		}

		myTransform.position = Vector2.LerpUnclamped(start, end, curve.Evaluate(1)); //We don't set directly to "end" because the curve might not finish with a value of 1
	}
}
