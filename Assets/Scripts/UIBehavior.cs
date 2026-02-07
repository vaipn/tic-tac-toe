using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIBehavior : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI gameStatusText;
    [SerializeField] private TextMeshProUGUI turnIndicatorText;
	[SerializeField] private Button restartButton;
    [SerializeField] private Button backButton;
	[SerializeField] private GameObject gameEndPanel;
    [SerializeField] private TextMeshProUGUI gameDescriptionText;
    [SerializeField] private Image sliderFillImage;
    [SerializeField] private Image pveButtonImage;
    [SerializeField] private Image sliderHandleImage;
    [SerializeField] private Image smileyIcon;
    [SerializeField] private RectTransform smileyIconRectTransform;
    [SerializeField] private Sprite easyIcon;
    [SerializeField] private Sprite mediumIcon;
    [SerializeField] private Sprite hardIcon;
    [SerializeField] private Image gameScreenBgImage;
    [SerializeField] private TMP_Text gameScreenDifficultyText;
    [SerializeField] private TMP_Text gameScreenYourScoreText;
    [SerializeField] private TMP_Text gameScreenBotScoreText;
    [SerializeField] private Animator yourTurnAnimator;
    [SerializeField] private Animator botTurnAnimator;
    
    [Header("Difficulty Selection")]
    [SerializeField] private GameObject difficultyPanel;
    [SerializeField] private Slider difficultySlider; 
    [SerializeField] private TextMeshProUGUI difficultyText;
    [SerializeField] private Color difficultyColor;
	[SerializeField] private Color easyColor;
	[SerializeField] private Color mediumColor;
	[SerializeField] private Color hardColor;
    [SerializeField] private Color xTurnColor;
    [SerializeField] private Color oTurnColor;
	[SerializeField] private Button playPvPButton;
    [SerializeField] private Button playPvEButton;

    [Header("Grid Generation")]
    [SerializeField] private Transform gridContainer;
    [SerializeField] private UICellBehavior cellPrefab;

    private Difficulty currentDifficulty = Difficulty.Easy; // Default
    private GameMode currentMode = GameMode.PvP;

	private void OnEnable()
	{
		BoardManager.Instance.OnGameFinished += HandleGameFinished;
		BoardManager.Instance.OnReset += ResetUI;
        BoardManager.Instance.OnBoardSetup += GenerateGrid;
        
        // Listen to Turn Changes for UI Feedback
        if (TurnManager.Instance != null)
             TurnManager.Instance.OnTurnChanged += UpdateTurnInfo;
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
             TurnManager.Instance.OnTurnChanged -= UpdateTurnInfo;
	}
	private void Start()
	{
        backButton.onClick.AddListener(GoToDifficultyPanel);
		restartButton.onClick.AddListener(RestartGame);
        
        playPvPButton.onClick.AddListener(()=> StartGame(GameMode.PvP));
        playPvEButton.onClick.AddListener(()=> StartGame(GameMode.PvE));
        
        difficultySlider.onValueChanged.AddListener(OnDifficultyChanged);

        // Initialize Defaults
        difficultySlider.value = 0; // Easy
        OnDifficultyChanged(0);

        // Show difficulty panel first
        difficultyPanel.SetActive(true);
        gameEndPanel.SetActive(false);
        if(turnIndicatorText) turnIndicatorText.text = ""; 
	}

    private void OnDifficultyChanged(float value)
    {
        // 0 = Easy, 1 = Medium, 2 = Hard
        int intVal = Mathf.RoundToInt(value);
        switch(intVal)
        {
            case 0:
                currentDifficulty = Difficulty.Easy;
                difficultyText.text = "EASY";
                difficultyText.color = easyColor;
                gameScreenDifficultyText.text = "EASY";
                sliderFillImage.color = easyColor;
                pveButtonImage.color = easyColor;
                sliderHandleImage.color = easyColor;
                smileyIcon.sprite = easyIcon;
                smileyIconRectTransform.sizeDelta = new Vector2(200, smileyIconRectTransform.sizeDelta.y);
                gameDescriptionText.text = "Red is cross, blue is circle!\r\nGet three in a row to win!";
				break;
            case 1:
                currentDifficulty = Difficulty.Medium;
                difficultyText.text = "MEDIUM";
                difficultyText.color = mediumColor;
				gameScreenDifficultyText.text = "MEDIUM";
				sliderFillImage.color = mediumColor;
                pveButtonImage.color = mediumColor;
                sliderHandleImage.color = mediumColor;
                smileyIcon.sprite = mediumIcon;
				smileyIconRectTransform.sizeDelta = new Vector2(200, smileyIconRectTransform.sizeDelta.y);
				gameDescriptionText.text = "Red is cross, blue is circle!\r\nGet four in a row to win!";
				break;
            case 2:
                currentDifficulty = Difficulty.Hard;
                difficultyText.text = "HARD";
                difficultyText.color = hardColor;
				gameScreenDifficultyText.text = "HARD";
				sliderFillImage.color = hardColor;
                pveButtonImage.color = hardColor;
                sliderHandleImage.color = hardColor;
                smileyIcon.sprite = hardIcon;
				smileyIconRectTransform.sizeDelta = new Vector2(300, smileyIconRectTransform.sizeDelta.y);
				gameDescriptionText.text = "Red is cross, blue is circle!\r\nGet five in a row to win!";
				break;
        }
    }

    private void UpdateTurnInfo(bool isXTurn)
    {
        if (difficultyPanel.activeSelf || gameEndPanel.activeSelf) 
        {
            if (turnIndicatorText) turnIndicatorText.text = "";
            return;
        }

        //if (turnIndicatorText) 
        //    turnIndicatorText.text = isXTurn ? "Player X Turn" : "Player O Turn";

        gameScreenBgImage.color = isXTurn ? xTurnColor : oTurnColor;

        if (isXTurn)
        {
            yourTurnAnimator.SetTrigger("Play Popup");
        }
        else if (!isXTurn && BoardManager.Instance.currentMode == GameMode.PvE)
        {
            botTurnAnimator.SetTrigger("Play Popup");
        }

    }

    private void StartGame(GameMode mode)
    {
        currentMode = mode;
        
        difficultyPanel.SetActive(false);
        BoardManager.Instance.InitializeGame(currentDifficulty, currentMode);
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
			//string winner = value == 1 ? "X" : "O";
			//gameStatusText.text = $"{winner} Player Wins!";
            if (value == 1)
            {
                gameScreenYourScoreText.text = "1";
            }
            else
            {
                gameScreenBotScoreText.text = "1";
            }
		}
		else
		{
			//gameStatusText.text = "It's a Draw! Try Again.";
		}
		gameEndPanel.SetActive(true);
	}
	private void ResetUI()
	{
		gameStatusText.text = "";
		gameEndPanel.SetActive(false);
        gameScreenYourScoreText.text = "0";
        gameScreenBotScoreText.text = "0";

        TurnManager.Instance.ResetTurn();
        // We stay in game unless we want to go back to difficulty menu?
        // Logic says "ResetGame" usually just restarts current match.
	}

    private void GoToDifficultyPanel()
    {
        difficultyPanel.SetActive(true);
    }

	private void RestartGame()
	{
		BoardManager.Instance.InitializeGame(currentDifficulty, currentMode);
	}
}
