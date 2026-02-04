using UnityEngine;
using UnityEngine.UI;

public class UICellBehavior : MonoBehaviour
{
	private Cell cell;
	
	public void Setup(Cell newCell)
	{
		cell = newCell;
		// Re-subscribe if we are recycling this UI object, but typically we instantiate fresh ones or clean them up.
		// For simplicity assuming fresh instantiation or properly handled lifecycle.
		// Actually, we need to bind events here if OnEnable was called before Setup.
		// But in this flow, Start/OnEnable might run before Setup.
		
		// Let's safe guard.
		if (isActiveAndEnabled)
		{
			// Unsubscribe from potential old one
			// (If pooling, this is important. If just instantiating, not so much)
			// Since we don't have the old reference easily unless we check null...
			
			cell.OnValueChanged += OnValueChanged;
			cell.OnGameFinished += OnGameFinished;
            
            // Sync initial state
            OnValueChanged(cell.id, cell.value);
		}
	}
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
		if (cell != null)
		{
			cell.OnValueChanged += OnValueChanged;
			cell.OnGameFinished += OnGameFinished;
		}
	}
	private void OnDisable()
	{
		if (cell != null)
		{
			cell.OnValueChanged -= OnValueChanged;
			cell.OnGameFinished -= OnGameFinished;
		}
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
        if (cell == null) return;
		if (!cell.IsInteractive) return;
		if (cell.value == 0) // Only change if blank
		{
			bool isXTurn = TurnManager.Instance.GetTurn();
			int newValue = isXTurn ? 1 : 2;
			cell.SetValue(newValue);
		}
	}
}
