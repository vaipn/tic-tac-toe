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
    public GameMode currentMode;
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
        yield return new WaitForSeconds(1f);
        
        if (!isGameActive) yield break;

        Cell targetCell = null;

        // Determine Intelligence based on Difficulty (Grid Size)
        // Easy (3): 60% chance to be smart
        // Medium (4): 65% chance
        // Hard (5): 80% chance
        float smartChance = 0f;
        switch(currentGridSize)
        {
            case 3: smartChance = 0.6f; break;
            case 4: smartChance = 0.7f; break;
            case 5: smartChance = 0.8f; break;
        }

        // Roll dice - should AI be smart this turn?
        bool beSmart = UnityEngine.Random.value <= smartChance;

        if (beSmart)
        {
            // 1. Try to Win (AI = 2)
            targetCell = FindBestMoveFor(2);
            
            // 2. Block Player (Player = 1)
            if (targetCell == null)
                targetCell = FindBestMoveFor(1);
        }

        // 3. Pick Center (Best for 3x3)
        if (targetCell == null && currentGridSize == 3 && beSmart)
        {
            Cell center = cellsList[4]; // ID 4 is center of 0-8
            if (center.value == 0) targetCell = center;
        }

        // 4. Random (Fallback)
        if (targetCell == null)
        {
            var emptyCells = new List<Cell>();
            foreach(var c in cellsList)
            {
                if (c.value == 0) emptyCells.Add(c);
            }
            if (emptyCells.Count > 0)
                targetCell = emptyCells[UnityEngine.Random.Range(0, emptyCells.Count)];
        }

        if (targetCell != null)
        {
            targetCell.SetValue(2); // AI is always O
            TurnManager.Instance.SwitchTurn();
        }
    }

    private Cell FindBestMoveFor(int playerValue)
    {
        int size = currentGridSize;
        // Check Rows
        for (int row = 0; row < size; row++)
        {
            var cell = CheckLineForMove(row * size, 1, size, playerValue);
            if (cell != null) return cell;
        }
        // Check Columns
        for (int col = 0; col < size; col++)
        {
            var cell = CheckLineForMove(col, size, size, playerValue);
            if (cell != null) return cell;
        }
        // Check Diagonals
        var diag1 = CheckLineForMove(0, size + 1, size, playerValue);
        if (diag1 != null) return diag1;

        var diag2 = CheckLineForMove(size - 1, size - 1, size, playerValue);
        if (diag2 != null) return diag2;

        return null; // No winning/blocking move found
    }

    private Cell CheckLineForMove(int startIndex, int step, int count, int valueToCheck)
    {
        int countMatch = 0;
        Cell emptyCell = null;
        int countEmpty = 0;

        for (int i = 0; i < count; i++)
        {
            int index = startIndex + (i * step);
            Cell c = cellsList[index];

            if (c.value == valueToCheck)
            {
                countMatch++;
            }
            else if (c.value == 0)
            {
                emptyCell = c;
                countEmpty++;
            }
            else
            {
                // Occluded by other player/AI
                return null; 
            }
        }

        // If we have (Size - 1) marks and 1 empty spot, that's the spot!
        if (countMatch == count - 1 && countEmpty == 1)
        {
            return emptyCell;
        }

        return null;
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
