using UnityEngine;
using UnityEngine.SceneManagement;

public class BootstrapManager : MonoBehaviour {
	public void Scene1()
	{
		SceneManager.LoadScene("Example Scene");
	}

	public void Scene2()
	{
		SceneManager.LoadScene("Example1 Scene");
	}
}
