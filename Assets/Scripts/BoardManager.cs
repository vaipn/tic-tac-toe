using System;
using System.Collections.Generic;
using UnityEngine;

public enum Difficulty
{
    Easy = 3,
    Medium = 4,
    Hard = 5
}

public enum GameMode
{
    PvP,
    PvE
}

public class BoardManager : PersistentMonoSingleton<BoardManager>
{
    public event Action<int, bool> OnGameFinished;
    public event Action OnReset;
    public event Action<int, List<Cell>> OnBoardSetup; 

    private List<Cell> cellsList = new();
    private int currentGridSize;
    private GameMode currentMode;
    private bool isGameActive;

    private void Start()
    {
        TurnManager.Instance.OnTurnChanged += HandleTurnChanged;
    }
    
    private void OnDisable()
    {
        if(TurnManager.Instance != null)
             TurnManager.Instance.OnTurnChanged -= HandleTurnChanged;
    }

    public void InitializeGame(Difficulty difficulty, GameMode mode)
    {
        ClearBoard();
        currentGridSize = (int)difficulty;
        currentMode = mode;
        isGameActive = true;

        for (int i = 0; i < currentGridSize * currentGridSize; i++)
        {
            Cell newCell = ScriptableObject.CreateInstance<Cell>();
            newCell.id = i;
            newCell.OnValueChanged += CheckWinCondition;
            cellsList.Add(newCell);
        }

        OnBoardSetup?.Invoke(currentGridSize, cellsList);
        ResetGame();
    }

    private void HandleTurnChanged(bool isXTurn)
    {
        if (!isGameActive) return;
        
        // If PvE and it's O's turn (False), triggers AI
        if (currentMode == GameMode.PvE && !isXTurn)
        {
            StartCoroutine(PerformAIMove());
        }
    }

    private System.Collections.IEnumerator PerformAIMove()
    {
        // Small delay for UX
        yield return new WaitForSeconds(0.5f);
        
        if (!isGameActive) yield break;

        // Simple Random AI
        var emptyCells = new List<Cell>();
        foreach(var c in cellsList)
        {
            if (c.value == 0) emptyCells.Add(c);
        }

        if (emptyCells.Count > 0)
        {
            var target = emptyCells[UnityEngine.Random.Range(0, emptyCells.Count)];
            target.SetValue(2); // AI is always O
            TurnManager.Instance.SwitchTurn();
        }
    }

    private void ClearBoard()
    {
        foreach (var cell in cellsList)
        {
            cell.OnValueChanged -= CheckWinCondition;
        }
        cellsList.Clear();
    }

    private void CheckWinCondition(int cellId, int value)
    {
        if (CheckWin(value))
        {
            isGameActive = false;
            OnGameFinished?.Invoke(value, true);
        }
        else if (IsDraw())
        {
            isGameActive = false;
            foreach (var cell in cellsList) cell.SetResult(false);
            OnGameFinished?.Invoke(0, false);
        }
    }

    private bool CheckWin(int playerValue)
    {
        if (playerValue == 0) return false;
        int size = currentGridSize;

        // Rows
        for (int row = 0; row < size; row++)
        {
            if (CheckLine(row * size, 1, size, playerValue)) return true;
        }

        // Columns
        for (int col = 0; col < size; col++)
        {
            if (CheckLine(col, size, size, playerValue)) return true;
        }

        // Diagonals
        if (CheckLine(0, size + 1, size, playerValue)) return true; // Top-left to bottom-right
        if (CheckLine(size - 1, size - 1, size, playerValue)) return true; // Top-right to bottom-left

        return false;
    }

    private bool CheckLine(int startIndex, int step, int count, int value)
    {
        List<Cell> winningCells = new List<Cell>();
        for (int i = 0; i < count; i++)
        {
            int index = startIndex + (i * step);
            if (cellsList[index].value != value) return false;
            winningCells.Add(cellsList[index]);
        }

		// If we get here, we found a win line. Highlight them.
		foreach (var c in winningCells) c.SetResult(true);

		// Disable the rest
		foreach (var c in cellsList)
        {
            if (!winningCells.Contains(c)) c.SetResult(false);
        }

        return true;
    }

    private bool IsDraw()
    {
        foreach (var cell in cellsList)
        {
            if (cell.value == 0) return false;
        }
        return true;
    }

    public void ResetGame()
    {
        isGameActive = true;
        TurnManager.Instance.ResetTurn();
        foreach (var cell in cellsList) cell.Reset();
        OnReset?.Invoke();
    }

    protected override void Initialize()
    {
    }
}
