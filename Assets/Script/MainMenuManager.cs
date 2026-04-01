using UnityEngine;
using UnityEngine.SceneManagement; // Cần thư viện này để chuyển cảnh

public class MainMenuManager : MonoBehaviour
{
	public string gameSceneName = "GameScene";

	public void OnStartButtonClick()
	{
		SceneManager.LoadScene(gameSceneName);
	}

	public void OnExitButtonClick()
	{
		Debug.Log("Đã thoát game!"); 
		Application.Quit();
	}

	public void OnRestartClick()
	{
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}

	public void OnHomeClick()
	{
		SceneManager.LoadScene("MenuScene");
	}
}
