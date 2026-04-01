using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement; // Cần cái này để Restart hoạt động

public class GameManager : MonoBehaviour
{
	[Header("UI Thông báo")]
	public GameObject noMovesTextObj;

	[Header("Game Over UI")]
	public GameObject gameOverPanel;  
	public TextMeshProUGUI finalScoreValue; 

	[Header("Score UI (In-Game)")]
	public TextMeshProUGUI scoreText; 
	private int currentScore = 0;

	[Header("Animation")]
	public Animator gameOverAnimator; // Kéo object GameOverPanel vào ô này trong Inspector

	// Biến nội bộ
	private Coroutine scoreAnimCoroutine;
	private Vector3 originalScale;

	void Start()
	{
		if (scoreText != null) originalScale = scoreText.transform.localScale;

		// Đảm bảo lúc đầu game Panel và Text thông báo phải tắt
		if (gameOverPanel != null) gameOverPanel.SetActive(false);
		if (noMovesTextObj != null) noMovesTextObj.SetActive(false);

		UpdateScoreUI();
		// -----------------------------------------------------------
		// [QUAN TRỌNG] Bật lại nhạc nền (Vì lúc Game Over mình đã tắt nó đi)
		// -----------------------------------------------------------
		if (AudioManager.instance != null)
		{
			// Gọi hàm PlayMusic và đưa bài nhạc nền vào
			AudioManager.instance.PlayMusic(AudioManager.instance.backgroundMusic);
		}
	}

	// --- CÁC HÀM XỬ LÝ NÚT BẤM (Restart / Exit) ---
	public void OnRestartClick()
	{
		if (AudioManager.instance != null)
		{
			// 1. Tắt ngay tiếng Game Over đang kêu dở
			AudioManager.instance.StopSFX();

			// 2. Sau đó mới phát tiếng Click (để tiếng click không bị tắt theo)
			AudioManager.instance.PlayClickSound();
		}
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}

	public void OnHomeClick()
	{
		if (AudioManager.instance != null)
		{
			// Tắt tiếng Game Over trước khi về menu
			AudioManager.instance.StopSFX();
			AudioManager.instance.PlayClickSound();
		}
		SceneManager.LoadScene("MenuScene"); // Nhớ đổi tên đúng Scene Menu của bạn
	}

	public void OnExitGameClick()
	{
		Application.Quit();
	}
	// ----------------------------------------------

	public void AddScore(int amount)
	{
		currentScore += amount;
		UpdateScoreUI();
		if (scoreText != null)
		{
			if (scoreAnimCoroutine != null) StopCoroutine(scoreAnimCoroutine);
			scoreAnimCoroutine = StartCoroutine(AnimateScorePop());
		}
	}

	private void UpdateScoreUI()
	{
		if (scoreText != null) scoreText.text = currentScore.ToString();
	}

	IEnumerator AnimateScorePop()
	{
		float duration = 0.1f;
		float timer = 0;
		Vector3 targetScale = originalScale * 1.5f;

		while (timer < duration)
		{
			timer += Time.deltaTime;
			scoreText.transform.localScale = Vector3.Lerp(originalScale, targetScale, timer / duration);
			yield return null;
		}

		timer = 0;
		while (timer < duration)
		{
			timer += Time.deltaTime;
			scoreText.transform.localScale = Vector3.Lerp(targetScale, originalScale, timer / duration);
			yield return null;
		}
		scoreText.transform.localScale = originalScale;
	}

	// --- LOGIC GAME OVER ---
	public void OnGameOver()
	{
		// 1. Cập nhật điểm số cuối cùng vào Panel trước (dù chưa hiện)


		// 2. Hiện chữ "Hết nước đi"
		if (noMovesTextObj != null)
		{
			noMovesTextObj.SetActive(true);
			// 3. Gọi hiệu ứng nảy chữ, sau đó mới hiện Panel
			StartCoroutine(AnimatePopupAndShowPanel(noMovesTextObj.transform));
		}
		else
		{
			if (gameOverPanel != null)
			{
				gameOverPanel.SetActive(true);

				// [THÊM DÒNG NÀY] Gọi Animator chạy hiệu ứng phóng to
				if (gameOverAnimator != null)
				{
					gameOverAnimator.SetTrigger("GameOver");
				}

				// Nếu vào thẳng đây cũng phải gọi chạy số
				StartCoroutine(AnimateScoreCounting());
			}
		}
		// Tắt nhạc nền và bật tiếng thua
		AudioManager.instance.musicSource.Stop(); // Tắt nhạc vui
		if (AudioManager.instance.gameOverSound != null)
		{
			AudioManager.instance.PlaySFX(AudioManager.instance.gameOverSound); // Bật nhạc buồn
		}
	}

	// [QUAN TRỌNG] Coroutine này xử lý trình tự hiển thị
	IEnumerator AnimatePopupAndShowPanel(Transform target)
	{
		// --- GIAI ĐOẠN 1: Hiệu ứng chữ "Hết nước đi" ---
		target.localScale = Vector3.zero;
		float duration = 0.5f;
		float timer = 0;

		while (timer < duration)
		{
			timer += Time.deltaTime;
			float progress = timer / duration;
			// Hiệu ứng nảy (Overshoot)
			float scale = Mathf.Lerp(0, 1.2f, progress);
			target.localScale = Vector3.one * scale;
			yield return null;
		}
		target.localScale = Vector3.one;

		// --- GIAI ĐOẠN 2: Chờ người chơi đọc chữ ---
		yield return new WaitForSeconds(1.0f); // Đợi 1 giây

		// --- GIAI ĐOẠN 3: Hiện Game Over Panel ---
		if (gameOverPanel != null)
		{
			if (finalScoreValue != null) finalScoreValue.text = "0";

			gameOverPanel.SetActive(true);
			// Chèn 2 dòng này ngay bên dưới
			if (gameOverAnimator != null)
			{
				gameOverAnimator.SetTrigger("GameOver");
			}

			// --- GỌI HIỆU ỨNG CHẠY SỐ Ở ĐÂY ---
			StartCoroutine(AnimateScoreCounting());
			noMovesTextObj.SetActive(false); 
		}
	}

	public void GoToMenu()
	{
		SceneManager.LoadScene("MenuScene");
	}

	// --- HIỆU ỨNG CHẠY SỐ ĐIỂM (Number Rolling) ---
	IEnumerator AnimateScoreCounting()
	{
		float duration = 1.0f; // Thời gian chạy số (1 giây)
		float timer = 0;
		int startValue = 0;    // Bắt đầu từ 0

		// Đảm bảo Text không bị null
		if (finalScoreValue == null) yield break;

		while (timer < duration)
		{
			timer += Time.deltaTime;
			float progress = timer / duration;

			// Dùng Lerp để tính giá trị ở thời điểm hiện tại
			// Ví dụ: progress = 0.5 (một nửa thời gian) -> giá trị = 50% tổng điểm
			int currentValue = (int)Mathf.Lerp(startValue, currentScore, progress);

			finalScoreValue.text = currentValue.ToString();

			yield return null; // Chờ frame tiếp theo
		}

		// Chốt hạ: Đảm bảo hiển thị chính xác số điểm cuối cùng (tránh sai số)
		finalScoreValue.text = currentScore.ToString();
	}
}