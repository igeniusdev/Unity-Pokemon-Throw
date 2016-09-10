using UnityEngine;
using System.Collections;

public class IGeniusButtonScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void OnIgeniusButtonClick(){
		Application.OpenURL("http://igeniusdev.com");
	}
}
