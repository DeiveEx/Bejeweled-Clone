using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public class GamePiece : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public int typeID;
	public RectTransform rectTransform { get; private set; }
	[HideInInspector] public Vector2Int boardPos;

	public event System.Action<GamePiece> touchedPieceEvent;
	public event System.Action<GamePiece> releasedPieceEvent;

	private void Awake()
	{
		rectTransform = GetComponent<RectTransform>();
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		touchedPieceEvent?.Invoke(this);
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		releasedPieceEvent?.Invoke(this);
	}
}
