using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;

public class Block : MonoBehaviour
{
	private BlockShapeData shapeData;
	public SpriteRenderer spriteRenderer;
	public GameObject blockPrefab;
	private bool isDragging = false;
	private Vector3 dragScale;
	private Vector3 originalScale;
	private Vector3 offset;
	private Vector3 originalPos;
	private List<Color> originalColors = new List<Color>();
	private bool isInitialized = false;

	public void Init(BlockShapeData data)
	{
		this.shapeData = data;
		foreach (Vector2 pos in data.columns)
		{
			GameObject part = Instantiate(blockPrefab, transform);
			part.transform.localPosition = pos;
			var sr = part.GetComponent<SpriteRenderer>();
			if(sr != null)
			{
				sr.sprite = data.blockSprite;
				sr.sortingOrder = 10;
			}
		}
		originalScale = transform.localScale;
		dragScale = originalScale * 1.2f;
		originalPos = transform.position;
		SaveOriginalColors();
	}

	void Update()
	{
		if (Input.GetMouseButtonUp(0))
		{
			if (isDragging)
			{
				dropBlock();
				// QUAN TRỌNG: Thả tay ra thì phải xóa bóng đi ngay
				BoardManager.Instance.ClearShadow();
			}
			isDragging = false;
		}
		if (Input.GetMouseButtonDown(0))
		{
			checkClick();
		}
		if (Input.GetMouseButton(0) && isDragging)
		{
			dragBlock();

			// QUAN TRỌNG: Khi đang kéo, nhờ BoardManager vẽ bóng hộ
			BoardManager.Instance.ShowShadow(this);
		}
	}

	private void checkClick()
	{
		Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);
		Collider2D hit = Physics2D.OverlapPoint(mousePos2D);

		if (hit != null && hit.transform.IsChildOf(transform))
		{
			isDragging = true;
			offset = transform.position - mousePos;
			transform.localScale = dragScale;
		}
	}

	private void dragBlock()
	{
		Vector3 mousepos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		transform.position = new Vector3(mousepos.x + offset.x, mousepos.y + offset.y, 0f);
	}

	private void dropBlock() // Bỏ tham số bool isDragging đi cho gọn
	{
		// 1. Gọi sang BoardManager để nhờ kiểm tra
		bool success = BoardManager.Instance.CheckAndPlaceBlock(this);

		if (success)
		{
			// 2a. Nếu đặt thành công
			// Hủy khối block đang cầm trên tay đi (vì đã "in" vào bảng rồi)
			Destroy(gameObject);

			// TODO: Sau này sẽ thêm lệnh Spawn block mới ở đây
		}
		else
		{
			// 2b. Nếu thất bại -> Về chỗ cũ
			transform.position = originalPos;
			transform.localScale = originalScale;
		}
	}

	private void SaveOriginalColors()
	{
		originalColors.Clear();
		foreach (Transform child in transform)
		{
			var sr = child.GetComponent<SpriteRenderer>();
			if (sr != null)
			{
				originalColors.Add(sr.color);
			}
		}
		isInitialized = true;
	}

	// Hàm đổi màu (True = biến thành xám, False = trả về màu gốc)
	public void SetGrayState(bool isGray)
	{
		if (!isInitialized) SaveOriginalColors();

		int index = 0;
		foreach (Transform child in transform)
		{
			var sr = child.GetComponent<SpriteRenderer>();
			if (sr != null && index < originalColors.Count)
			{
				if (isGray)
				{
					// Màu xám nhạt (giữ Alpha để không bị mất hình)
					sr.color = new Color(0.5f, 0.5f, 0.5f, 0.6f);
					// Hoặc dùng Color.gray nếu muốn đậm hơn
				}
				else
				{
					// Trả lại màu gốc rực rỡ
					sr.color = originalColors[index];
				}
			}
			index++;
		}
	}

}
