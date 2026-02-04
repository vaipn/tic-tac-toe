using UnityEngine;
using UnityEngine.UI;

public class UICellBehavior : MonoBehaviour
{
	[SerializeField] private Cell cell;
	[SerializeField] private Button button;
	[SerializeField] private Image image;
	[SerializeField] private Color defaultColor;
	[SerializeField] private Color winColor;
	[SerializeField] private Color failedColor;

	private Sprite xImage;
	private Sprite oImage;
	private Sprite blankImage;
	private void Awake()
	{
		xImage = Resources.Load<Sprite>("x");
		oImage = Resources.Load<Sprite>("o");
		blankImage = Resources.Load<Sprite>("cell");
	}
	private void Start()
	{
		button.onClick.AddListener(OnButtonClick);
	}
	private void OnEnable()
	{
		cell.OnValueChanged += OnValueChanged;
		cell.OnGameFinished += OnGameFinished;
	}
	private void OnDisable()
	{
		cell.OnValueChanged -= OnValueChanged;
		cell.OnGameFinished -= OnGameFinished;
	}
	private void OnValueChanged(int cell, int newValue)
	{
		image.sprite = newValue == 1 ? xImage : oImage;

		if (newValue != 0) return; // game restart
		image.sprite = blankImage;
		image.color = defaultColor;
	}
	private void OnGameFinished(bool isGameWin)
	{
		image.color = isGameWin ? winColor : failedColor;
	}
	private void OnButtonClick()
	{
		if (!cell.IsInteractive) return;
		if (cell.value == 0) // Only change if blank
		{
			bool isXTurn = TurnManager.Instance.GetTurn();
			int newValue = isXTurn ? 1 : 2;
			cell.SetValue(newValue);
		}
	}
}
