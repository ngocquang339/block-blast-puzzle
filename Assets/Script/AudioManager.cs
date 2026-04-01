using UnityEngine;

public class AudioManager : MonoBehaviour
{
	// Tạo Singleton để gọi từ các script khác dễ dàng
	public static AudioManager instance;

	[Header("Nguồn phát (Kéo AudioSource vào đây)")]
	public AudioSource musicSource; // Để phát nhạc nền
	public AudioSource sfxSource;   // Để phát tiếng động (ăn điểm, nút bấm...)

	[Header("Danh sách các File âm thanh")]
	public AudioClip backgroundMusic;
	public AudioClip clearLineSound;
	public AudioClip clickSound;
	public AudioClip gameOverSound;
	public AudioClip hoverSound; // Tiếng khi di chuột vào nút
	public AudioClip putBlockSound;

	void Awake()
	{
		// Đảm bảo chỉ có 1 AudioManager tồn tại
		if (instance == null)
		{
			instance = this;
			DontDestroyOnLoad(gameObject); // Giữ cho nhạc không bị ngắt khi chuyển Scene
		}
		else
		{
			Destroy(gameObject);
		}
	}

	void Start()
	{
		// Tự động phát nhạc nền khi vào game
		PlayMusic(backgroundMusic);
	}

	// Hàm phát nhạc nền
	public void PlayMusic(AudioClip clip)
	{
		if (clip != null)
		{
			// [MỚI] Nếu đang phát đúng bài nhạc này rồi thì thôi, không chạy lại nữa
			if (musicSource.clip == clip && musicSource.isPlaying) return;
			musicSource.clip = clip;
			musicSource.Play();
		}
	}

	// Hàm phát tiếng động (SFX)
	public void PlaySFX(AudioClip clip)
	{
		if (clip != null)
		{
			// PlayOneShot giúp các tiếng chồng lên nhau được (ví dụ ăn 2 hàng liên tiếp)
			sfxSource.PlayOneShot(clip);
		}
	}

	// Các hàm gọi nhanh (để gắn vào Button trong Inspector cho tiện)
	public void PlayClickSound()
	{
		PlaySFX(clickSound);
	}

	// Hàm để tắt ngay lập tức mọi tiếng động (SFX) đang phát
	public void StopSFX()
	{
		sfxSource.Stop();
	}
}