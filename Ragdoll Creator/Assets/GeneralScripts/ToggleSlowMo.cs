using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleSlowMo : MonoBehaviour
{
	public float slomoOnKeyN = .3f;
	bool slomo = false;
	void Update() // The camera is not smooth unless in FixedUpdate
	{
		if (Input.GetKeyDown(KeyCode.N) && !slomo)
		{
			Time.timeScale = slomoOnKeyN;
			slomo = true;
			Debug.Log("SLOW MO IS ON");
		}
		else if (slomo && Input.GetKeyDown(KeyCode.N))
		{
			Time.timeScale = 1f;
			slomo = false;
		}
	}
}