using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Board : MonoBehaviour
{
#if UNITY_EDITOR
	[SerializeField] private bool showDebugInfo;
	[SerializeField] private int seed;
#endif

	[SerializeField] private CanvasScaler canvas;
	[SerializeField] private Vector2Int boardSize;
	[SerializeField] private RectOffset padding;
    [SerializeField] private Vector2 spacing;
	[SerializeField] private RectTransform piecesParent;
	[SerializeField] private float minDragDistance = 30;
	[SerializeField] private int minMatchCount = 3;
	public GamePiece[] availablePieces; //TODO change from prefabs to pools?

	public System.Action<List<GamePiece>> matchFound;

	private GamePiece[,] grid;
	private Vector2 pieceSize;
	private GamePiece selectedPiece;
	private Vector2 touchPos;
	private List<List<GamePiece>> matches = new List<List<GamePiece>>();

	private void Start()
	{
		//Since the minDragDistance is based on pixels, different resolutions will result in different distances, so we recalculate the distance based on the current resolution
		minDragDistance = (Screen.width * minDragDistance) / canvas.referenceResolution.x; //TODO check if this is right

		GenerateBoard();
	}

	private void Update()
	{
#if UNITY_EDITOR
		//TODO remove this?
		//Debug Commands
		if (Keyboard.current[Key.Space].wasPressedThisFrame)
		{

		}

		//TODO if we click on a piece and then click on nothing, the current piece is not deselected. Maybe to a check if we clicked outside the board, and so deselect the piece in that case?
		//if (Mouse.current.leftButton.wasPressedThisFrame)
		//{
		//	Debug.Log(Mouse.current.position.ReadValue());
		//}
#endif
	}

	public void GenerateBoard()
	{
#if UNITY_EDITOR
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
						Destroy(grid[x, y].gameObject);
				}
			}
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
				CreatePieceAtPosition(x, y);
			}
		}
	}

	private GamePiece CreatePieceAtPosition(int x, int y)
	{
		GamePiece piece = Instantiate(availablePieces[Random.Range(0, availablePieces.Length)], piecesParent);
		piece.rectTransform.anchorMin = piece.rectTransform.anchorMax = Vector2.one * .5f; //Centralizes the anchors
		piece.rectTransform.sizeDelta = pieceSize - spacing;
		SetPieceGridPosition(piece, x, y);
		piece.touchedPieceEvent += PieceWasSelected;
		piece.releasedPieceEvent += PieceWasReleased;

		return piece;
	}

	private void SetPieceGridPosition(GamePiece p, int x, int y)
	{
		p.rectTransform.anchoredPosition = piecesParent.rect.position + (pieceSize * .5f) + new Vector2((pieceSize.x * x) + padding.left, (pieceSize.y * y) + padding.bottom);
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

		//After a swap is executed (successfully or not), we clear the selected piece
		selectedPiece = null;
	}

	private void PieceWasSelected(GamePiece c)
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

	private void PieceWasReleased(GamePiece c)
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
				targetPiece = grid[targetPos.x, targetPos.y];
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
		if (CheckForMatches() == 0)
		{
			SwapPieces(p1, p2);
			return;
		}

		//If at least one match WAS found, we make a loop removing all matches, until there's no more matches to remove
		do
		{
			//Remove all matched pieces
			foreach (var match in matches)
			{
				//If this match has a piece that was already removed, that means this match was part of another match,
				//so we have a "cross match", which we can use to give bonus points or something
				if (match.Any(p => grid[p.boardPos.x, p.boardPos.y] == null))
				{
					matchFound?.Invoke(match);
				}
				else
				{
					matchFound?.Invoke(match);
				}

				for (int i = 0; i < match.Count; i++)
				{
					RemovePiece(match[i]);
				}
			}

			//Drop the remaining pieces down to fill the gaps left by the removed pieces
			for (int x = 0; x < boardSize.x; x++)
			{
				for (int y = 1; y < boardSize.y; y++)
				{
					DropPiece(grid[x, y]);
				}
			}

			//Add new random pieces in the remaining empty spaces
			for (int x = 0; x < boardSize.x; x++)
			{
				for (int y = 1; y < boardSize.y; y++)
				{
					if (grid[x, y] == null)
						CreatePieceAtPosition(x, y);
				}
			}
		}
		while (CheckForMatches() > 0);
	}

	private void RemovePiece(GamePiece p)
	{
		grid[p.boardPos.x, p.boardPos.y] = null;
		Destroy(p.gameObject); //TODO return to pool
	}

	private void DropPiece(GamePiece p)
	{
		if(p != null && p.boardPos.y >= 1)//Pieces at the bottom (y = 0) don't need to be dropped
		{
			Vector2Int targetPos = new Vector2Int(p.boardPos.x, p.boardPos.y - 1);
			
			if(grid[targetPos.x, targetPos.y] == null)
			{
				grid[p.boardPos.x, p.boardPos.y] = null;
				SetPieceGridPosition(p, targetPos.x, targetPos.y);
				DropPiece(p); //Recursively call this method until the piece can't be dropped anymore
			}
		}
	}

	public int CheckForMatches()
	{
		matches.Clear();

		for (int i = 0; i < boardSize.x; i++)
		{
			for (int j = 0; j < boardSize.y; j++)
			{
				CheckSurroundingPieces(grid[i, j]);
			}
		}

		//Did we find any macth?
		return matches.Count;
	}

	private void CheckSurroundingPieces(GamePiece p)
	{
		//Check all valid directions
		for (int x = -1; x <= 1; x++)
		{
			for (int y = -1; y <= 1; y++)
			{
				if(Mathf.Abs(x) + Mathf.Abs(y) == 1) //TODO If we remove this condition, we can check for the diagonal pieces too!
				{
					List<GamePiece> currentMatch = new List<GamePiece>();
					CheckNextPiece(p, currentMatch, x, y);

					if (currentMatch.Count >= minMatchCount)
						matches.Add(currentMatch);
				}
			}
		}
	}

	private void CheckNextPiece(GamePiece p, List<GamePiece> currentMatchList, int dirX, int dirY)
	{
		//Add the current piece to the list
		currentMatchList.Add(p);

		//Check if the next piece in the defined direction exists
		GamePiece nextPiece = null;
		Vector2Int nextPos = new Vector2Int(p.boardPos.x + dirX, p.boardPos.y + dirY);

		if ( nextPos.x >= 0 &&
			 nextPos.y >= 0 &&
			 nextPos.x < boardSize.x &&
			 nextPos.y < boardSize.y)
		{
			nextPiece = grid[nextPos.x, nextPos.y];
		}

		//If the next piece doesn't exist, we return
		if (nextPiece == null)
			return;

		//Check if this piece and the next are part of any existing matches, which would mean we're looking at pieces we already looked and in the same direction
		if (matches.Any(x => x.Contains(p) && x.Contains(nextPiece)))
			return;

		//If the next piece is of the same type as the one that was passed, we call this method again,
		//but passing the next piece instead, until we can't find any more similar/valid pieces
		if (nextPiece.typeID == p.typeID)
		{
			CheckNextPiece(nextPiece, currentMatchList, dirX, dirY);
		}
	}

#if UNITY_EDITOR
	private void OnGUI()
	{
		if (!showDebugInfo)
			return;

		//TODO remove this?
		//GUIStyle style = new GUIStyle();
		//style.fontSize = 25;
		//style.fontStyle = FontStyle.Bold;
		//style.normal.textColor = Color.white;

		for (int x = 0; x < boardSize.x; x++)
		{
			for (int y = 0; y < boardSize.y; y++)
			{
				GamePiece c = grid[x, y];
				GUI.Box(new Rect(c.transform.position.x, Screen.height - c.transform.position.y, 60, 40), $"({c.boardPos.x}, {c.boardPos.y})");
			}
		}
	}
#endif
}
