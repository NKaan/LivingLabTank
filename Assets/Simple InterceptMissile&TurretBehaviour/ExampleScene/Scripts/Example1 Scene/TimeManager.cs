using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour {

	public float slowdownFactor = 0.05f;
	public float slowdownLength = 2f;
	private bool _isSlowMo = false;

	
	// Update is called once per frame
	void Update () {

		if(Input.GetKeyDown(KeyCode.Q))
			_isSlowMo = !_isSlowMo;

		if(_isSlowMo)
		{
			DoSlowMotion();
		}
		else
		{
			Reset();
		}
	}

	void DoSlowMotion()
	{
		Time.timeScale = slowdownFactor;
		Time.fixedDeltaTime = 0.02f * Time.timeScale;
		
	}

	void Reset()
	{
		Time.timeScale += (1f / slowdownLength) * Time.unscaledDeltaTime;
		Time.timeScale = Mathf.Clamp(Time.timeScale, 0f, 1f);
	}

}
