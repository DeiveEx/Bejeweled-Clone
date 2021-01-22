using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Manager : MonoBehaviour
{
	[SerializeField] private Image target;

    private Board board;
    private Game_Manager manager;
	

	private void Awake()
	{
		board = FindObjectOfType<Board>();
		manager = FindObjectOfType<Game_Manager>();

		manager.pieceSelectedEvent += OnPieceSelected;
		manager.pieceDeselectedEvent += OnPieceDeselected;

		target.raycastTarget = false; //This is important
		target.gameObject.SetActive(false);
	}

	private void OnDestroy()
	{
		if (manager == null)
			return;

		manager.pieceSelectedEvent -= OnPieceSelected;
		manager.pieceDeselectedEvent -= OnPieceDeselected;
	}

	private void OnPieceSelected(GamePiece obj)
	{
		if (manager.currentState == GameState.wait)
			return;

		target.gameObject.SetActive(true);
		target.transform.position = obj.transform.position;
	}

	private void OnPieceDeselected(GamePiece obj)
	{
		target.gameObject.SetActive(false);
	}
}
