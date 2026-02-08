using UnityEngine;
using UnityEngine.EventSystems;

public class BreathingEffect : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
	[Header("Pulse Settings")]
	public float minScale = 0.8f;
	public float maxScale = 1.2f;
	public float speed = 2.0f;

	[SerializeField] private GameObject objectToEffect;

	private bool isInteracting = false;
	private Vector3 originalScale;

	void Start()
	{
		// Remember the initial scale so we can return to it later
		originalScale = objectToEffect.transform.localScale;
	}

	void Update()
	{
		// Only pulse if the user is NOT interacting with the object
		if (!isInteracting)
		{
			ApplyPulse();
		}
	}

	void ApplyPulse()
	{
		// Mathf.Sin cycles between -1 and 1. 
		// We map that to a value between 0 and 1.
		float sinWave = (Mathf.Sin(Time.time * speed) + 1.0f) / 2.0f;

		// Lerp creates a smooth transition between min and max based on the wave
		float scale = Mathf.Lerp(minScale, maxScale, sinWave);

		objectToEffect.transform.localScale = new Vector3(scale, scale, 1f);
	}

	// Called automatically when user clicks/touches the handle
	public void OnPointerDown(PointerEventData eventData)
	{
		isInteracting = true;
		// Optional: Snap back to original size immediately when grabbed
		objectToEffect.transform.localScale = originalScale;
	}

	// Called automatically when user releases the handle
	public void OnPointerUp(PointerEventData eventData)
	{
		isInteracting = false;
	}
}
