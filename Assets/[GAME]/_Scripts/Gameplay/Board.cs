using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Board : MonoBehaviour
{
#if UNITY_EDITOR
	[SerializeField] private bool showDebugInfo;
#endif

	[SerializeField] private Vector2Int boardSize;
	[SerializeField] private RectOffset padding;
    [SerializeField] private Vector2 spacing;
	[SerializeField] private RectTransform cellsParent;

	public Cell[] availableCells;

	private Cell[,] currentCells;
	private Vector2 cellSize;

	private void Start()
	{
		GenerateBoard();
	}

	private void Update()
	{
#if UNITY_EDITOR
		//Debug Commands
		if (Keyboard.current[Key.Space].wasPressedThisFrame)
		{

		}
#endif
	}

	public void GenerateBoard()
	{
		//Remove all existing cells
		for (int x = 0; x < boardSize.x; x++)
		{
			for (int y = 0; y < boardSize.y; y++)
			{
				if(currentCells != null && currentCells[x, y] != null)
					Destroy(currentCells[x, y].gameObject);
			}
		}

		currentCells = new Cell[boardSize.x, boardSize.y];
		cellSize = new Vector2() {
			x = (cellsParent.rect.width - padding.left - padding.right) / boardSize.x,
			y = (cellsParent.rect.height - padding.top - padding.bottom) / boardSize.y
		};

		//Create new cells and position them into the grid
		for (int x = 0; x < boardSize.x; x++)
		{
			for (int y = 0; y < boardSize.y; y++)
			{
				Cell cell = Instantiate(availableCells[Random.Range(0, availableCells.Length)], cellsParent);
				cell.rectTransform.anchorMin = cell.rectTransform.anchorMax = Vector2.one * .5f; //Centralizes the anchors
				cell.rectTransform.sizeDelta = cellSize - spacing;
				SetCellGridPosition(x, y, cell);

				currentCells[x, y] = cell;
			}
		}
	}

	private void SetCellGridPosition(int x, int y, Cell cell)
	{
		cell.rectTransform.anchoredPosition = cellsParent.rect.position + (cellSize * .5f) + new Vector2((cellSize.x * x) + padding.left, (cellSize.y * y) + padding.bottom);
	}

	public void CheckForCombinations()
	{

	}

#if UNITY_EDITOR
	private void OnGUI()
	{
		if (!showDebugInfo)
			return;

		for (int x = 0; x < boardSize.x; x++)
		{
			for (int y = 0; y < boardSize.y; y++)
			{
				Cell c = currentCells[x, y];
				GUI.Box(new Rect(c.transform.position.x, Screen.height - c.transform.position.y, 40, 25), $"({x}, {y})");
			}
		}
	}
#endif
}
