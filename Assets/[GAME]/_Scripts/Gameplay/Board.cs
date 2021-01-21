using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class Board : MonoBehaviour
{
#if UNITY_EDITOR
	[SerializeField] private bool showDebugInfo;
	[SerializeField] private bool useSeed;
	[SerializeField] private int seed;
#endif

	public Vector2Int boardSize;
	[SerializeField] private RectOffset padding;
    [SerializeField] private Vector2 spacing;
	public RectTransform piecesParent;
	public ObjectPool_SO[] availablePieces;
	public GamePiece[,] grid;
	[HideInInspector] public Vector2 pieceSize;
	public float swapDuration = 1;
	public AnimationCurve swapCurve = AnimationCurve.Linear(0, 0, 1, 1);
	public float fallDuration = 1;
	public AnimationCurve fallCurve = AnimationCurve.Linear(0, 0, 1, 1);

	public System.Action<GamePiece> pieceCreatedEvent;
	public System.Action<GamePiece> pieceDestroyedEvent;

	private Game_Manager manager;


	private void Awake()
	{
		manager = FindObjectOfType<Game_Manager>();
	}

	[ContextMenu("Regenerate Board")]
	public void GenerateBoard()
	{
#if UNITY_EDITOR
		if(useSeed)
			Random.InitState(seed);
#endif

		//Remove all existing pieces
		if (grid != null)
		{
			for (int x = 0; x < boardSize.x; x++)
			{
				for (int y = 0; y < boardSize.y; y++)
				{
					if (grid[x, y] != null)
						RemovePiece(grid[x, y]);
				}
			}
		}

		for (int i = 0; i < availablePieces.Length; i++)
		{
			availablePieces[i].Initialize();
		}

		grid = new GamePiece[boardSize.x, boardSize.y];
		pieceSize = new Vector2() {
			x = (piecesParent.rect.width - padding.left - padding.right) / boardSize.x,
			y = (piecesParent.rect.height - padding.top - padding.bottom) / boardSize.y
		};

		//Create new pieces and position them into the grid
		for (int x = 0; x < boardSize.x; x++)
		{
			for (int y = 0; y < boardSize.y; y++)
			{
				int pieceID = 0;
				int maxTries = 0;

				//We don't want the starting board to already have matches, so we make sure that the current piece being created
				//have a different ID than the pieces at (x - (minMatchCount - 1)) and (y - (minMatchCount - 1)) positions
				//(since the board is create up then right, we can only check for pieces at the bottom and left)
				do
				{
					maxTries++;
					pieceID = Random.Range(0, availablePieces.Length);

					//Pieces at the starting corner of the board doesn't need to be checked
					if (x < manager.minMatchCount - 1 && y < manager.minMatchCount - 1)
					{
						break;
					}

					Vector2Int targetPos = new Vector2Int(x - (manager.minMatchCount - 1), y - (manager.minMatchCount - 1));

					//Check the pieces at the fisr rows and colums
					if (x < manager.minMatchCount - 1 && grid[x, targetPos.y].typeID != pieceID)
					{
						break;
					}

					if (y < manager.minMatchCount - 1 && grid[targetPos.x, y].typeID != pieceID)
					{
						break;
					}

					//Check any other piece that doesn't fit the above conditions
					if (x >= manager.minMatchCount - 1 && y >= manager.minMatchCount - 1)
					{
						if (grid[x, targetPos.y].typeID != pieceID && grid[targetPos.x, y].typeID != pieceID)
						{
							break;
						}
					}
				}
				while (maxTries < 100); //For safety. Should never reach this many tries

				CreatePieceAtPosition(availablePieces[pieceID], x, y);
			}
		}
	}

	public GamePiece CreatePieceAtPosition(ObjectPool_SO pool, int x, int y)
	{
		GamePiece piece = pool.GetPooledObject<GamePiece>();
		piece.rectTransform.SetParent(piecesParent);
		piece.rectTransform.anchorMin = piece.rectTransform.anchorMax = Vector2.one * .5f; //Centralizes the anchors
		piece.rectTransform.sizeDelta = pieceSize - spacing;
		piece.rectTransform.localScale = Vector3.one;
		SetPieceGridPosition(piece, x, y);
		piece.Initialize();

		pieceCreatedEvent?.Invoke(piece);
		return piece;
	}

	public void RemovePiece(GamePiece p)
	{
		grid[p.boardPos.x, p.boardPos.y] = null;
		pieceDestroyedEvent?.Invoke(p);

		p.GetComponent<PoolableObject>().ReturnToPool();
	}

	public void SetPieceGridPosition(GamePiece p, int x, int y)
	{
		p.boardPos = new Vector2Int(x, y);
		grid[x, y] = p;
	}

	public void SwapPieces(GamePiece p1, GamePiece p2)
	{
		//Check if we can swap these pieces by verifying their board positions. We can only swap adjacent pieces that are not in a diagonal
		int dstX = Mathf.Abs(p1.boardPos.x - p2.boardPos.x);
		int dstY = Mathf.Abs(p1.boardPos.y - p2.boardPos.y);

		if ((dstX == 1 || dstY == 1) && dstX + dstY == 1) //TODO We can easily adapt the game to accept diagonal swaps by changing this condition
		{
			Vector2Int pos1 = p1.boardPos;
			SetPieceGridPosition(p1, p2.boardPos.x, p2.boardPos.y);
			SetPieceGridPosition(p2, pos1.x, pos1.y);
		}
	}

	public void DropPiece(GamePiece p)
	{
		if (p != null && p.boardPos.y >= 1)//Pieces at the bottom (y = 0) don't need to be dropped
		{
			Vector2Int targetPos = new Vector2Int(p.boardPos.x, p.boardPos.y - 1);

			if (grid[targetPos.x, targetPos.y] == null)
			{
				grid[p.boardPos.x, p.boardPos.y] = null;
				SetPieceGridPosition(p, targetPos.x, targetPos.y);
				DropPiece(p); //Recursively call this method until the piece can't be dropped anymore
			}
		}
	}

	public Vector2 ConvertGridPosToAnchoredPos(int x, int y)
	{
		return piecesParent.rect.position + (pieceSize * .5f) + new Vector2((pieceSize.x * x) + padding.left, (pieceSize.y * y) + padding.bottom);
	}

#if UNITY_EDITOR
	private void OnGUI()
	{
		if (!showDebugInfo)
			return;

		//TODO remove this?
		GUIStyle style = new GUIStyle();
		style.fontSize = 25;
		style.fontStyle = FontStyle.Bold;
		style.normal.textColor = Color.magenta;

		for (int x = 0; x < boardSize.x; x++)
		{
			for (int y = 0; y < boardSize.y; y++)
			{
				GamePiece c = grid[x, y];
				GUI.Label(new Rect(c.transform.position.x, Screen.height - c.transform.position.y, 60, 40), $"({c.boardPos.x}, {c.boardPos.y})", style);
			}
		}
	}
#endif
}
