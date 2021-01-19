using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Board : MonoBehaviour
{
#if UNITY_EDITOR
	[SerializeField] private bool showDebugInfo;
#endif

	[SerializeField] private CanvasScaler canvas;
	[SerializeField] private Vector2Int boardSize;
	[SerializeField] private RectOffset padding;
    [SerializeField] private Vector2 spacing;
	[SerializeField] private RectTransform piecesParent;
	[SerializeField] private float minDragDistance = 30;

	public GamePiece[] availablePieces;

	private GamePiece[,] currentPieces;
	private Vector2 pieceSize;
	private GamePiece selectedPiece;
	private Vector2 touchPos;

	private void Start()
	{
		//Since the minDragDistance is based on pixels, different resolutions will result in different distances, so we recalculate the distance based on the current resolution
		minDragDistance = (Screen.width * minDragDistance) / canvas.referenceResolution.x; //TODO check if this is right

		GenerateBoard();
	}

	private void Update()
	{
#if UNITY_EDITOR
		//Debug Commands
		if (Keyboard.current[Key.Space].wasPressedThisFrame)
		{

		}

		//if (Mouse.current.leftButton.wasPressedThisFrame)
		//{
		//	Debug.Log(Mouse.current.position.ReadValue());
		//}
#endif
	}

	public void GenerateBoard()
	{
		//Remove all existing pieces
		if(currentPieces != null)
		{
			for (int x = 0; x < boardSize.x; x++)
			{
				for (int y = 0; y < boardSize.y; y++)
				{
					if (currentPieces[x, y] != null)
						Destroy(currentPieces[x, y].gameObject);
				}
			}
		}

		currentPieces = new GamePiece[boardSize.x, boardSize.y];
		pieceSize = new Vector2() {
			x = (piecesParent.rect.width - padding.left - padding.right) / boardSize.x,
			y = (piecesParent.rect.height - padding.top - padding.bottom) / boardSize.y
		};

		//Create new pieces and position them into the grid
		for (int x = 0; x < boardSize.x; x++)
		{
			for (int y = 0; y < boardSize.y; y++)
			{
				GamePiece piece = Instantiate(availablePieces[Random.Range(0, availablePieces.Length)], piecesParent);
				piece.rectTransform.anchorMin = piece.rectTransform.anchorMax = Vector2.one * .5f; //Centralizes the anchors
				piece.rectTransform.sizeDelta = pieceSize - spacing;
				SetPieceGridPosition(piece, x, y);
				piece.touchedPieceEvent += SelectPiece;
				piece.releasedPieceEvent += ReleasePiece;
			}
		}
	}

	private void SetPieceGridPosition(GamePiece p, int x, int y)
	{
		p.rectTransform.anchoredPosition = piecesParent.rect.position + (pieceSize * .5f) + new Vector2((pieceSize.x * x) + padding.left, (pieceSize.y * y) + padding.bottom);
		p.boardPos = new Vector2Int(x, y);
		currentPieces[x, y] = p;
	}

	private void SelectPiece(GamePiece c)
	{
		//If we selected the same piece twice, we deselect it by clearing its reference
		if (c == selectedPiece)
		{
			selectedPiece = null;
			return;
		}

		//If we don't have any piece selected at the moment, we select it
		if(selectedPiece == null)
		{
			selectedPiece = c;
			//TODO change this to use the input system actions instead...?
			touchPos = Mouse.current.position.ReadValue();
			return;
		}

		//If we selected a different piece than the current selcted one, we do a swap
		SwapPiecesAndCheckForMatches(selectedPiece, c);
	}

	private void ReleasePiece(GamePiece c)
	{
		if (selectedPiece == null)
			return;

		Vector2 releasePos = Mouse.current.position.ReadValue();
		Vector2 direction = releasePos - touchPos;

		//If the distance between the click down and the click up is greater than the minDragDistance, we consider that we want to swap the pieces with a swipe
		if (direction.magnitude > minDragDistance)
		{
			Vector2Int targetPos = c.boardPos;

			//Check the direction we want to swap
			if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
			{
				if(direction.x > 0)
				{
					targetPos.x += 1; //Right
				}
				else
				{
					targetPos.x -= 1; //Left
				}
			}
			else
			{
				if(direction.y > 0)
				{
					targetPos.y += 1; //Up
				}
				else
				{
					targetPos.y -= 1; //Down
				}
			}

			//With the direction, we try to find the piece we want to swap with
			GamePiece targetPiece = null;

			if( targetPos.x >= 0 &&
				targetPos.y >= 0 &&
				targetPos.x < boardSize.x &&
				targetPos.y < boardSize.y)
			{
				targetPiece = currentPieces[targetPos.x, targetPos.y];
			}
			
			//If we found the piece we want to swap places, do the swap. Otherwise, deselect the piece
			if(targetPiece != null)
			{
				SwapPiecesAndCheckForMatches(c, targetPiece);
			}
			else
			{
				selectedPiece = null;
				Debug.Log("Stuck"); //TODO maybe do a "stuck" animation when trying to swap to outside the board?
			}
		}
	}

	private void SwapPiecesAndCheckForMatches(GamePiece p1, GamePiece p2)
	{
		SwapPieces(p1, p2);

		//If after the swipe a match was NOT found, we return the pieces to their original positions
		//if (!CheckForMatches()) //TODO uncomment this
		//{
		//	SwapPieces(p1, p2);
		//}
	}

	public void SwapPieces(GamePiece p1, GamePiece p2)
	{
		//Check if we can swap these pieces by verifying their board positions. We can only swap adjacent pieces that are not in a diagonal
		int dstX = Mathf.Abs(p1.boardPos.x - p2.boardPos.x);
		int dstY = Mathf.Abs(p1.boardPos.y - p2.boardPos.y);

		if ((dstX == 1 || dstY == 1) && dstX + dstY == 1)
		{
			Vector2Int pos1 = p1.boardPos;
			SetPieceGridPosition(p1, p2.boardPos.x, p2.boardPos.y);
			SetPieceGridPosition(p2, pos1.x, pos1.y);
		}

		//After a swap is executed (successfully or not), we clear the selected piece
		selectedPiece = null;
	}

	public bool CheckForMatches()
	{
		for (int i = 0; i < boardSize.x; i++)
		{
			for (int j = 0; j < boardSize.y; j++)
			{
				
			}
		}

		return false;
	}

#if UNITY_EDITOR
	private void OnGUI()
	{
		if (!showDebugInfo)
			return;

		//GUIStyle style = new GUIStyle();
		//style.fontSize = 25;
		//style.fontStyle = FontStyle.Bold;
		//style.normal.textColor = Color.white;

		for (int x = 0; x < boardSize.x; x++)
		{
			for (int y = 0; y < boardSize.y; y++)
			{
				GamePiece c = currentPieces[x, y];
				GUI.Box(new Rect(c.transform.position.x, Screen.height - c.transform.position.y, 60, 40), $"({c.boardPos.x}, {c.boardPos.y})");
			}
		}
	}
#endif
}
