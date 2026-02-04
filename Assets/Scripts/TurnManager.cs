using UnityEngine;

public class TurnManager : PersistentMonoSingleton<TurnManager>
{
	private bool xUserTurn;

	private void Start()
	{
		xUserTurn = true; // X player starts first
	}

	public bool GetTurn()
	{
		bool turn = xUserTurn;
		xUserTurn = !xUserTurn; // Switch turns between X and O
		return turn;
	}

	protected override void Initialize()
	{
		// additional initialization can be placed here
	}
}
