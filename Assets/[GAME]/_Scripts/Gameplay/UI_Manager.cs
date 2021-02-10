using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Manager : MonoBehaviour
{
	[SerializeField] private Image target;
	[SerializeField] private Image background;
	[SerializeField] private Sprite[] bgSprites;
	[SerializeField] private ObjectPool_SO textEffectPool;
	[SerializeField] private TMP_Text pointsText;
	[SerializeField] private float pointEffectScale = 1;
	[SerializeField] private float animDuration = 1;
	[SerializeField] private AnimationCurve animCurve = AnimationCurve.Linear(0, 0, 1, 1);
	[SerializeField] private GameObject gameOverPanel;

	private Board board;
	private Game_Manager manager;


	private void Awake()
	{
		board = FindObjectOfType<Board>();
		manager = FindObjectOfType<Game_Manager>();

		manager.pieceSelectedEvent += OnPieceSelected;
		manager.pieceDeselectedEvent += OnPieceDeselected;
		manager.matchFoundEvent += OnMatchFound;
		manager.crossMatchFoundEvent += OnCrossMatchFound;
		manager.gameStartEvent += OnGameStart;
		manager.gameOverEvent += OnGameOver;

		target.raycastTarget = false; //This is important
	}

	private void OnGameStart()
	{
		target.gameObject.SetActive(false);
		background.sprite = bgSprites[Random.Range(0, bgSprites.Length)];
		pointsText.text = "0";
		gameOverPanel.SetActive(false);
	}

	private void OnDestroy()
	{
		if (manager == null)
			return;

		manager.pieceSelectedEvent -= OnPieceSelected;
		manager.pieceDeselectedEvent -= OnPieceDeselected;
	}

	private void OnPieceSelected(GamePiece p)
	{
		if (manager.currentState == GameState.wait)
			return;

		target.gameObject.SetActive(true);
		target.transform.position = p.transform.position;
	}

	private void OnPieceDeselected(GamePiece p)
	{
		target.gameObject.SetActive(false);
	}

	private void OnMatchFound(List<GamePiece> pieces, int points)
	{
		int middlePieceID = Mathf.FloorToInt(pieces.Count / 2f);
		TextEffect pointsText = CreateTextObject();
		pointsText.textObj.text = "" + points;
		pointsText.transform.position = pieces[middlePieceID].transform.position + Random.insideUnitSphere * 10f;
		pointsText.Animate();
		AnimatePointsText(points);
	}

	private void OnCrossMatchFound(List<GamePiece> pieces, int points)
	{
		int middlePieceID = Mathf.FloorToInt(pieces.Count / 2f);
		TextEffect pointsText = CreateTextObject();
		pointsText.textObj.text = "" + points;
		pointsText.transform.position = pieces[middlePieceID].transform.position + Random.insideUnitSphere * 10f;
		pointsText.Animate();
		AnimatePointsText(points);
	}

	private TextEffect CreateTextObject()
	{
		TextEffect t = textEffectPool.GetPooledObject<TextEffect>();
		t.transform.SetParent(board.transform);
		t.transform.SetAsLastSibling();
		t.transform.localScale = Vector3.one;

		return t;
	}

	private void AnimatePointsText(float scale)
	{
		StopAllCoroutines();
		StartCoroutine(AnimatePointsText_Routine(scale));
	}

	private IEnumerator AnimatePointsText_Routine(float scale)
	{
		yield return null;
		pointsText.text = "" + manager.points;

		float timePassed = 0;

		while (timePassed < animDuration)
		{
			timePassed += Time.deltaTime;

			//The size of the animation depends on how many points you made
			pointsText.transform.localScale = Vector3.LerpUnclamped(Vector3.one, Vector3.one * (1 + (scale / 10f)), animCurve.Evaluate(timePassed / animDuration));

			yield return null;
		}

		pointsText.transform.localScale = Vector3.one;
	}

	private void OnGameOver()
	{
		gameOverPanel.SetActive(true);
	}
}
