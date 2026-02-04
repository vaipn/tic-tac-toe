using System;
using System.Collections.Generic;
using UnityEngine;

public enum Difficulty
{
    Easy = 3,
    Medium = 4,
    Hard = 5
}

public class BoardManager : PersistentMonoSingleton<BoardManager>
{
    public event Action<int, bool> OnGameFinished;
    public event Action OnReset;
    public event Action<int, List<Cell>> OnBoardSetup; // GridSize, List of Cells

    private List<Cell> cellsList = new();
    private int currentGridSize;

    private void Start()
    {
        // Don't auto-start. Wait for UI selection. 
        // Or default to Easy for testing if needed, but let's wait for UI.
    }

    public void InitializeGame(Difficulty difficulty)
    {
        ClearBoard();
        currentGridSize = (int)difficulty;

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

    private void ClearBoard()
    {
        foreach (var cell in cellsList)
        {
            cell.OnValueChanged -= CheckWinCondition;
            // ScriptableObjects created at runtime should ideally be destroyed if not needed, 
            // but for this simple scope letting GC handle it or just overwriting the list reference is okay.
            // Ideally: Destroy(cell); if it was a MonoBehaviour, but it's SO.
        }
        cellsList.Clear();
    }

    private void CheckWinCondition(int cellId, int value)
    {
        if (CheckWin(value))
        {
            // highlighting is handled in CheckLine
            
            OnGameFinished?.Invoke(value, true);
        }
        else if (IsDraw())
        {
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
        foreach(var c in winningCells) c.SetResult(true);
        
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
        foreach (var cell in cellsList) cell.Reset();
        OnReset?.Invoke();
    }

    protected override void Initialize()
    {
    }
}
