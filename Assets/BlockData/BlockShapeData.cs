using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
[CreateAssetMenu(fileName = "BlockShapeData", menuName = "Scriptable Objects/BlockShapeData")]
public class BlockShapeData : ScriptableObject
{
	[Header("Cấu hình Khối")]
	// Tên gợi nhớ (ví dụ: "Block L", "Block Vuông")
	public string blockName;

	[Tooltip("Tập hợp toạ độ các ô. (0,0) là ô gốc.")]
	public Vector2Int[] cellPositions;

	[Header("Giao diện")]
	public Sprite blockSprite;

	public Color color = Color.white; // Màu của khối

	public List<Vector2> columns = new List<Vector2>();
}
