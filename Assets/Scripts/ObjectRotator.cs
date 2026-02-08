using UnityEngine;

public class ObjectRotator : MonoBehaviour
{
	public float rotationSpeed = 100f; // Degrees per second

	[SerializeField] private bool isRotating = true;

	void Update()
	{
		if (isRotating)
		{
			// Rotate the object around the Z-axis
			transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
		}
	}
}
