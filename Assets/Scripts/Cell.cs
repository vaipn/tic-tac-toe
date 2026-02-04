using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Cell", menuName = "Game/Cell")]
public class Cell : ScriptableObject
{
	public int id;
	public int value; // 0: blank, 1: X, 2: O
	public bool IsInteractive { get; private set; }
	public event Action<int, int> OnValueChanged;
	public event Action<bool> OnGameFinished;
	public void SetValue(int newValue)
	{
		value = newValue;
		OnValueChanged?.Invoke(id, value);
	}
	public void SetResult(bool isWin)
	{
		OnGameFinished?.Invoke(isWin);
		IsInteractive = false;
	}
	public void Reset()
	{
		IsInteractive = true;
		SetValue(0);
	}
}
