using UnityEngine;
using System.Collections;

public class FadeIn : MonoBehaviour {

	void Start () {
		TurnOffBody();
	}

	public GameObject[] bodyParts;

	void TurnOffBody() {
		for(int i = 0; i < bodyParts.Length; i++) {
			//Change Standard shader type to Transparent. This could be done in inspector instead.
			Material material = bodyParts[i].GetComponent<Renderer>().material;
			material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
			material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
			material.SetInt("_ZWrite", 0);
			material.DisableKeyword("_ALPHATEST_ON");
			material.DisableKeyword("_ALPHABLEND_ON");
			material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
			material.renderQueue = 3000;
			material.SetColor("_Color", material.GetColor("_Color") * new Color(1f, 1f, 1f, 0f)); //Set his transparency off full
		}
	}

	IEnumerator TurnOnBody(float fadeInTime, float pauseInBetween) {
		for(int i = 0; i < bodyParts.Length; i++) {
			bodyParts[i].SetActive(true);
			float time = 0f;
			while(time <= fadeInTime) {
				time += Time.deltaTime;
				Material material = bodyParts[i].GetComponent<Renderer>().material;
				Color color = material.GetColor("_Color");
				color.a = time/fadeInTime;
				material.SetColor("_Color", color); //Set his transparency back to on incrementally
				yield return null;
			}
			yield return new WaitForSeconds(pauseInBetween);
		}
	}

	void OnGUI() {

	}



	[Tooltip("Specify Size and Elements/ No None Objects ")] public GameObject[] gameObjArr; // [Tooltip("Number of seconds between each object")] 
	public float waitSeconds = 1f; 
	[Tooltip("Cycle between On/Off State?")] 
	public bool cycle = true;
	//To see if initial start of Scene
	private bool initStart;

	//Sets all to SetActive(false) at Start()
	void Start() 
	{
		initStart = true;
		StartCoroutine(centralMethod());
		//Call to the IEnumerator
		//StartCoroutine (TurnOnPartsWithWait());

	}

	//MainMethod
	IEnumerator centralMethod() {
		while (cycle) {
			TurnOffPartsInstant ();
			StartCoroutine (TurnOnPartsWithWait ());
			yield return null;
		}
	}


	//TurnOn Functions Here:

	//Instaneously SetActive(true) all objects
	void TurnOnPartsInstant() {
		for (int i = 0; i < gameObjArr.Length; i++) 
		{
			gameObjArr [i].SetActive (true);
		}
	}
	//SetActive(true) with WaitForSeconds Calls
	IEnumerator TurnOnPartsWithWait()
	{
		for (int i = 0; i < gameObjArr.Length; i++) 
		{
			//Only if first Element should be delayed
			if (i == 0)
				yield return new WaitForSeconds (waitSeconds);

			gameObjArr [i].SetActive (true);
			yield return new WaitForSeconds (waitSeconds);
		}
	}


	//TurnOff Functions Here:

	//Instaneously SetActive(false) All Elements
	void TurnOffPartsInstant() {
		for (int i = 0; i < gameObjArr.Length; i++) 
		{
			gameObjArr [i].SetActive (false);
		}
	}
	//SetActive(false)
	IEnumerator TurnOffPartsWithWait()
	{
		for (int i = 0; i < gameObjArr.Length; i++) 
		{
			//Only if first Element should be delayed
			if (i == 0)
				yield return new WaitForSeconds (waitSeconds);

			gameObjArr [i].SetActive (false);
			yield return new WaitForSeconds (waitSeconds);
		}
	}

	IEnumerator TurnOffPartsReverseWithWait()
	{
		for (int i = gameObjArr.Length - 1; i > -1; i--) 
		{
			//Only if first Element should be delayed
			if (i == gameObjArr.Length - 1)
				yield return new WaitForSeconds (waitSeconds);

			gameObjArr [i].SetActive (false);
			yield return new WaitForSeconds (waitSeconds);
		}
	}
}
