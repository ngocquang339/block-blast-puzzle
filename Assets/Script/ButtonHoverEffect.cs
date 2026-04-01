using UnityEngine;
using UnityEngine.EventSystems; // Cần thư viện này để bắt sự kiện chuột

// Script này kế thừa 2 interface để biết khi nào chuột vào và ra
public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	[Header("Cấu hình")]
	public float scaleAmount = 1.1f; // Phóng to lên 1.1 lần (tăng 10%)
	public float speed = 15f;        // Tốc độ phóng to (càng lớn càng nhanh)

	private Vector3 originalScale;   // Lưu kích thước gốc
	private Vector3 targetScale;     // Kích thước mong muốn hiện tại

	void Start()
	{
		// 1. Lưu lại kích thước ban đầu (thường là 1,1,1)
		originalScale = transform.localScale;
		targetScale = originalScale;
	}

	void Update()
	{
		// 2. Dùng hàm Lerp để thay đổi kích thước từ từ cho mượt
		// Thay vì giật đùng đùng, nó sẽ co giãn mềm mại
		transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * speed);
	}

	// Khi chuột CHẠM vào nút
	public void OnPointerEnter(PointerEventData eventData)
	{
		// Đặt mục tiêu là phóng to lên
		targetScale = originalScale * scaleAmount;
		// [THÊM DÒNG NÀY]
		if (AudioManager.instance != null)
		{
			AudioManager.instance.PlaySFX(AudioManager.instance.hoverSound);
		}
	}

	// Khi chuột RỜI KHỎI nút
	public void OnPointerExit(PointerEventData eventData)
	{
		// Đặt mục tiêu là về lại kích thước cũ
		targetScale = originalScale;
	}

	// (Tùy chọn) Khi bấm vào nút thì trả về gốc ngay cho đỡ bị kẹt
	public void OnDisable()
	{
		transform.localScale = originalScale;
	}
}