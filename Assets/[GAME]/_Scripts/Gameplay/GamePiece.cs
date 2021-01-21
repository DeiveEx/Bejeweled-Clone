using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GamePiece : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public int typeID;
	public RectTransform rectTransform { get; private set; }
	public Image image;
	[HideInInspector] public Vector2Int boardPos;

	public event System.Action<GamePiece> touchedPieceEvent;
	public event System.Action<GamePiece> releasedPieceEvent;

	private Board board;

	private void Awake()
	{
		rectTransform = GetComponent<RectTransform>();
		board = FindObjectOfType<Board>();
	}

	private void OnDestroy()
	{
		if(image != null)
			Destroy(image.gameObject);
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		touchedPieceEvent?.Invoke(this);
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		releasedPieceEvent?.Invoke(this);
	}

	public void Initialize()
	{
		Vector2Int start = new Vector2Int(boardPos.x, Mathf.FloorToInt(boardPos.y + (board.boardSize.y * 1.25f)));
		Vector2Int end = boardPos;
		AnimateImagePositionOnGrid(start, end, board.fallDuration, board.fallCurve);
	}

	public void AnimateImagePosition(Vector2 start, Vector2 end, float duration, AnimationCurve curve)
	{
		StopAllCoroutines();
		StartCoroutine(AnimateImagePosition_Routine(start, end, duration, curve));
	}

	public void AnimateImagePositionOnGrid(Vector2Int start, Vector2Int end, float duration, AnimationCurve curve)
	{
		AnimateImagePosition(board.ConvertGridPosToAnchoredPos(start.x, start.y), board.ConvertGridPosToAnchoredPos(end.x, end.y), duration, curve);
	}

	private IEnumerator AnimateImagePosition_Routine(Vector2 start, Vector2 end, float duration, AnimationCurve curve)
	{
		float timePassed = 0;

		while (timePassed < duration)
		{
			timePassed += Time.deltaTime;

			rectTransform.anchoredPosition = Vector2.LerpUnclamped(start, end, curve.Evaluate(timePassed / duration));

			yield return null;
		}

		rectTransform.anchoredPosition = Vector2.LerpUnclamped(start, end, curve.Evaluate(1)); //We don't set directly to "end" because the curve might not finish with a value of 1
	}
}
