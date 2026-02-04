using System;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : PersistentMonoSingleton<BoardManager>
{
	public event Action<int, bool> OnGameFinished;
	public event Action OnReset;

	[SerializeField] private List<Cell> cellsList = new();

	private readonly int[][] winConditions =
	{
			new[] { 0, 1, 2 }, // Row 1
            new[] { 3, 4, 5 }, // Row 2
            new[] { 6, 7, 8 }, // Row 3
            new[] { 0, 3, 6 }, // Column 1
            new[] { 1, 4, 7 }, // Column 2
            new[] { 2, 5, 8 }, // Column 3
            new[] { 0, 4, 8 }, // Diagonal 1
            new[] { 2, 4, 6 }  // Diagonal 2
        };
	private void Start()
	{
		ResetGame(); // game start point
	}
	private void OnEnable()
	{
		foreach (var cell in cellsList) cell.OnValueChanged += CheckWinCondition;
	}
	private void OnDisable()
	{
		foreach (var cell in cellsList) cell.OnValueChanged -= CheckWinCondition;
	}
	private void CheckWinCondition(int cellId, int value)
	{
		foreach (var condition in winConditions)
			if (cellsList[condition[0]].value == value &&
				cellsList[condition[1]].value == value &&
				cellsList[condition[2]].value == value &&
				value != 0)
			{
				cellsList[condition[0]].SetResult(true);
				cellsList[condition[1]].SetResult(true);
				cellsList[condition[2]].SetResult(true);

				for (var i = 0; i < cellsList.Count; i++)
					if (i != condition[0] && i != condition[1] && i != condition[2])
						cellsList[i].SetResult(false);

				OnGameFinished?.Invoke(value, true);
				return;
			}
		var allCellFilled = true;
		foreach (var cell in cellsList)
			if (cell.value == 0)
			{
				allCellFilled = false;
				break;
			}
		if (allCellFilled)
		{
			foreach (var cell in cellsList) cell.SetResult(false);
			OnGameFinished?.Invoke(0, false); // Tie
		}
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
