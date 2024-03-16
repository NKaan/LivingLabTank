using UnityEngine;

public class CameraControllerEx : MonoBehaviour {
	public bool LockMouse;
	[Range(0,50)]
	public float Sensitivity = 20f;
	[Range(0,100)]
	public float Speed = 20f;
	private float AdvanceSpeed = 1f;
	private float axisX, axisY;
	private float lastAxisX, lasAxisY;
	

	private void LateUpdate()
	{	
		// Left click on the mouse to lock the cursor
		if(Input.GetMouseButtonDown(0))
		{
			LockMouse =! LockMouse;
			if(!LockMouse)
			{
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
			}
			else
			{
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;
			}
		}

		if(Input.GetKeyDown(KeyCode.Escape))
		{
			Application.Quit();
		}

		// camera movement script
		CameraMovement();

	}

	private void CameraMovement()
	{
		if(LockMouse)
		{
			axisX = Input.GetAxis("Mouse X") + lastAxisX * 0.9f;

			axisY = -Input.GetAxis("Mouse Y") + lasAxisY * 0.9f;

			lastAxisX = axisX;
			lasAxisY = axisY;

			transform.Rotate(Sensitivity * axisY * Time.deltaTime, Sensitivity * axisX * Time.deltaTime, 0f);
		}

		if(Input.GetKey(KeyCode.Space))
		{
			AdvanceSpeed = 2f;
		}
		else
		{
			AdvanceSpeed = 1f;
		}

		transform.Translate(AdvanceSpeed * Speed * Input.GetAxis("Horizontal") * Time.deltaTime, 0, AdvanceSpeed * Speed * Input.GetAxis("Vertical") * Time.deltaTime);
		
		Vector3 stayHorizontal = transform.eulerAngles;
		stayHorizontal.z = 0f;
		transform.eulerAngles = stayHorizontal; 
	}
}
