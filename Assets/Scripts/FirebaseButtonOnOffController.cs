using UnityEngine;

public class FirebaseButtonOnOffController : ButtonOnOffController
{

	public static FirebaseButtonOnOffController Instance { get; private set; }
	
	private void Awake()
	{
		if (Instance != null) {
			Destroy(gameObject);
			return;
		}
		Instance = this;
		Debug.Log("Firebase Button Instance SET");
	}

}
