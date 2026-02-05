using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIBehavior : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI gameStatusText;
    [SerializeField] private TextMeshProUGUI turnIndicatorText; // Add this
	[SerializeField] private Button restartButton;
	[SerializeField] private GameObject gameEndPanel;
    
    [Header("Difficulty Selection")]
    [SerializeField] private GameObject difficultyPanel;
    [SerializeField] private Toggle aiModeToggle; // CHECK THIS IN INSPECTOR
    [SerializeField] private Button easyButton;
    [SerializeField] private Button mediumButton;
    [SerializeField] private Button hardButton;

    [Header("Grid Generation")]
    [SerializeField] private Transform gridContainer;
    [SerializeField] private UICellBehavior cellPrefab;

    private Difficulty currentDifficulty = Difficulty.Easy;
    private GameMode currentMode = GameMode.PvP;

	private void OnEnable()
	{
		BoardManager.Instance.OnGameFinished += HandleGameFinished;
		BoardManager.Instance.OnReset += ResetUI;
        BoardManager.Instance.OnBoardSetup += GenerateGrid;
        
        // Listen to Turn Changes for UI Feedback
        if (TurnManager.Instance != null)
             TurnManager.Instance.OnTurnChanged += UpdateTurnText;
	}
	private void OnDisable()
	{
        if (BoardManager.Instance != null)
        {
			BoardManager.Instance.OnGameFinished -= HandleGameFinished;
			BoardManager.Instance.OnReset -= ResetUI;
			BoardManager.Instance.OnBoardSetup -= GenerateGrid;
		}

        if (TurnManager.Instance != null)
             TurnManager.Instance.OnTurnChanged -= UpdateTurnText;
	}
	private void Start()
	{
		restartButton.onClick.AddListener(RestartGame);
        
        easyButton.onClick.AddListener(()=> StartGame(Difficulty.Easy));
        mediumButton.onClick.AddListener(()=> StartGame(Difficulty.Medium));
        hardButton.onClick.AddListener(()=> StartGame(Difficulty.Hard));

        // Show difficulty panel first
        difficultyPanel.SetActive(true);
        gameEndPanel.SetActive(false);
        if(turnIndicatorText) turnIndicatorText.text = ""; // Hide initially
	}

    private void UpdateTurnText(bool isXTurn)
    {
        if (difficultyPanel.activeSelf || gameEndPanel.activeSelf) 
        {
            if (turnIndicatorText) turnIndicatorText.text = "";
            return;
        }

        if (turnIndicatorText) 
            turnIndicatorText.text = isXTurn ? "Player X Turn" : "Player O Turn";
    }

    private void StartGame(Difficulty difficulty)
    {
        currentDifficulty = difficulty;
        currentMode = aiModeToggle.isOn ? GameMode.PvE : GameMode.PvP;
        
        difficultyPanel.SetActive(false);
        BoardManager.Instance.InitializeGame(difficulty, currentMode);
    }

    private void GenerateGrid(int gridSize, System.Collections.Generic.List<Cell> cells)
    {
        // cleanup old children
        foreach(Transform child in gridContainer)
        {
            Destroy(child.gameObject);
        }

        // Setup Grid Layout
        var gridLayout = gridContainer.GetComponent<UnityEngine.UI.GridLayoutGroup>();
        if(gridLayout != null)
        {
            gridLayout.constraint = UnityEngine.UI.GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = gridSize;
        }

        // Instantiate cells
        foreach(var cellData in cells)
        {
            var cellUI = Instantiate(cellPrefab, gridContainer);
            cellUI.Setup(cellData);
        }
    }

	private void HandleGameFinished(int value, bool isWin)
	{
		if (isWin)
		{
			string winner = value == 1 ? "X" : "O";
			gameStatusText.text = $"{winner} Player Wins!";
		}
		else
		{
			gameStatusText.text = "It's a Draw! Try Again.";
		}
		gameEndPanel.SetActive(true);
	}
	private void ResetUI()
	{
		gameStatusText.text = "";
		gameEndPanel.SetActive(false);
        // We stay in game unless we want to go back to difficulty menu?
        // Logic says "ResetGame" usually just restarts current match.
	}
	private void RestartGame()
	{
		BoardManager.Instance.InitializeGame(currentDifficulty, currentMode);
	}
}
