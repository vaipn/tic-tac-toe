using UnityEngine;

public class TurnManager : PersistentMonoSingleton<TurnManager>
{
    public event System.Action<bool> OnTurnChanged; // true = X, false = O
	private bool xUserTurn;

    public bool IsXTurn => xUserTurn;

	private void Start()
	{
		ResetTurn();
	}

    public void ResetTurn()
    {
        xUserTurn = true;
        OnTurnChanged?.Invoke(xUserTurn);
    }

	public void SwitchTurn()
	{
		xUserTurn = !xUserTurn;
        OnTurnChanged?.Invoke(xUserTurn);
	}

	protected override void Initialize()
	{
	}
}
