﻿using UnityEngine;
using UnityEngine.UI; 
using System.Collections;

public class l6variables : MonoBehaviour {

	public GameObject level;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		string lvl = level.GetComponent<TextMesh>().text;
		if (lvl == "6") {
			this.GetComponent<Text>().enabled = true;
		}
		else{
			this.GetComponent<Text>().enabled = false;
		}
	}
}
