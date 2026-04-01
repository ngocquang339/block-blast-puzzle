using System.Collections.Generic;
using NUnit.Framework;
using TMPro;
using UnityEngine;
public class BoardManager : MonoBehaviour
{
    private int width = 7;
    private int height = 7;
    private GameObject[,] boards;
    private GameObject tile;
    private GameObject block;
    private float cellWidth;
    private float cellHeight;
    private Vector2 startPos;
	private int currentBlocksCount;
	private Vector2 curPos;
    public float scaleX;
    public float scaleY;
	private List<Block> spawnedBlocks = new List<Block>();
	private GameObject[,] blocks;
    private Vector2 tileSize;
	private List<GameObject> activeShadows = new List<GameObject>();
	private Vector2 pos;
	public static BoardManager Instance;
	private bool[,] isFull;
	[SerializeField] private SpriteMask maskRenderer;
    [SerializeField] private GameObject prefabTile;
    [SerializeField] private Block prefabBlock;
    [SerializeField] private Transform[] spawnPoint;
    [SerializeField] private List<BlockShapeData> data;

	[Header("Container")]
	// Kéo object FilledBlockContainer vừa tạo vào đây
	[SerializeField] private Transform filledBlockContainer;
	[Header("Shadow Container")]
	// Kéo object ShadowContainer vừa tạo ở Bước 1 vào đây
	[SerializeField] private Transform shadowContainer;

	[Header("Effects")]
	[SerializeField] private ParticleSystem breakEffectPrefab;

	[Header("Connections")]
	
	public GameManager gameManager;
	void Awake()
	{
		Instance = this;
	}
	void Start()
    {
        boards = new GameObject[width, height];
        blocks = new GameObject[width, height];
		isFull = new bool[width, height];
		generateBoard();
        generateThreeBlock();
    }

    private void generateBoard()
    {
        Bounds b = maskRenderer.bounds;
        cellWidth = b.size.x / width;
        cellHeight = cellWidth;
        startPos = new Vector2(b.min.x + cellWidth / 2, b.min.y + cellHeight / 2);

        for(int i = 0; i < boards.GetLength(0); i++)
        {
            for (int j = 0; j < boards.GetLength(1); j++) {
                pos = startPos + new Vector2(i * cellWidth, j * cellHeight) ;
                tile = Instantiate(prefabTile, pos, Quaternion.identity);
                boards[i, j] = tile;
                if ((i + j) % 2 == 0)
                {
                    tile.GetComponent<SpriteRenderer>().color = new Color32(92, 46 , 100, 150);

				}
				else {
                    tile.GetComponent<SpriteRenderer>().color = new Color32(74, 30, 80, 150);

				}
                var sr = tile.GetComponent<SpriteRenderer>();
                tileSize = sr.bounds.size;
                scaleX = cellWidth / tileSize.x;
                scaleY = cellHeight / tileSize.y;
                tile.transform.localScale = new Vector3 (scaleX, scaleY, 1f);
            }
        }
    }

    private void generateThreeBlock()
    {
		currentBlocksCount = 0;
        foreach(Transform pos in spawnPoint)
        {
            if(pos.childCount == 0)
            {
				int random = Random.Range(0, data.Count);
				BlockShapeData blData = data[random];
				Vector3 spawnPosition = new Vector3(pos.position.x, pos.position.y, 0f);
				Block block = Instantiate(prefabBlock, spawnPosition, Quaternion.identity, pos);
				block.Init(blData);
				currentBlocksCount++;
				spawnedBlocks.Add(block);
			}
			else
			{
				// Nếu ô đó đã có gạch (chưa bị kéo đi), ta cũng phải thêm nó vào list
				Block existingBlock = pos.GetComponentInChildren<Block>();
				if (existingBlock != null)
				{
					spawnedBlocks.Add(existingBlock);
				}
				currentBlocksCount++;
			}
        }
		// [QUAN TRỌNG] Kiểm tra thua ngay khi vừa sinh ra gạch mới
		// Vì có thể vừa đẻ ra đã không xếp được rồi
		CheckGameOver(spawnedBlocks);
	}

	public bool CheckAndPlaceBlock(Block block)
	{
		// [QUAN TRỌNG] KHÔNG ĐƯỢC trừ currentBlocksCount ở đây!
		// Nếu kiểm tra thất bại (return false), ta không được phép trừ.

		int childCount = block.transform.childCount; // Lấy số lượng con

		// --- PHẦN A: KIỂM TRA HỢP LỆ 
		for (int k = 0; k < childCount; k++)
		{
			// Lấy con theo chỉ số index, đảm bảo không bỏ sót
			Transform child = block.transform.GetChild(k);

			Vector3 unscaledPos = block.transform.position + new Vector3(child.localPosition.x * cellWidth, child.localPosition.y * cellHeight, 0);
			Vector2 distance = unscaledPos - (Vector3)startPos;
			int i = Mathf.FloorToInt((distance.x / cellWidth) + 0.5f);
			int j = Mathf.FloorToInt((distance.y / cellHeight) + 0.5f);

			// Kiểm tra biên
			if (i < 0 || i >= width || j < 0 || j >= height) return false;

			// Kiểm tra ô đã đầy
			if (isFull[i, j]) return false;
		}

		// --- PHẦN B: ĐẶT GẠCH (DÙNG VÒNG LẶP FOR) ---
		for (int k = 0; k < childCount; k++)
		{
			Transform child = block.transform.GetChild(k);

			Vector3 unscaledPos = block.transform.position + new Vector3(child.localPosition.x * cellWidth, child.localPosition.y * cellHeight, 0);
			Vector2 distance = unscaledPos - (Vector3)startPos;
			int i = Mathf.FloorToInt((distance.x / cellWidth) + 0.5f);
			int j = Mathf.FloorToInt((distance.y / cellHeight) + 0.5f);

			// 1. Đánh dấu đầy
			isFull[i, j] = true;

			// 2. Xóa ô nền cũ

			// 3. Lấy dữ liệu
			SpriteRenderer sourceSR = child.GetComponent<SpriteRenderer>();
			Sprite sourceSprite = sourceSR.sprite;
			Color sourceColor = sourceSR.color;

			// 4. TẠO MỚI (New GameObject)
			GameObject newPart = new GameObject($"Block_{i}_{j}");
			newPart.transform.SetParent(filledBlockContainer);

			// 5. Setup SpriteRenderer
			SpriteRenderer newSR = newPart.AddComponent<SpriteRenderer>();
			newSR.sprite = sourceSprite;
			newSR.color = sourceColor;

			// [CỰC KỲ QUAN TRỌNG] Setup Layer & Order
			// Đảm bảo tên Layer "FilledBlock" chính xác 100% trong Editor
			newSR.sortingOrder = 10; // Đặt hẳn lên 10 cho chắc chắn đè lên mọi thứ
			newSR.maskInteraction = SpriteMaskInteraction.None;

			// 6. Setup Vị trí & Scale
			Vector2 targetPos2D = startPos + new Vector2(i * cellWidth, j * cellHeight);

			// Z = 0f là chuẩn nhất nếu đã dùng Sorting Layer riêng
			newPart.transform.position = new Vector3(targetPos2D.x, targetPos2D.y, 0f);

			if (sourceSprite != null)
			{
				Bounds b = sourceSprite.bounds;
				float scaleX = cellWidth / b.size.x;
				float scaleY = cellHeight / b.size.y;
				newPart.transform.localScale = new Vector3(scaleX, scaleY, 1f);
			}

			// 7. Cập nhật mảng quản lý
			boards[i, j] = newPart;
			AudioManager.instance.PlaySFX(AudioManager.instance.putBlockSound);
		}

		// --- PHẦN C: XỬ LÝ SAU KHI ĐẶT THÀNH CÔNG ---

		// 1. KIỂM TRA ĂN ĐIỂM NGAY LẬP TỨC
		CheckLines();
		// Bây giờ mới được phép trừ số lượng
		currentBlocksCount--;

		if (spawnedBlocks.Contains(block))
		{
			spawnedBlocks.Remove(block);
		}

		if (currentBlocksCount <= 0)
		{
			Invoke("generateThreeBlock", 0.2f);
		}
		else
		{
			// [SỬA] Nếu vẫn còn gạch chờ, kiểm tra xem gạch còn lại có xếp được không
			CheckGameOver(spawnedBlocks);
		}
		return true;
	}


	// Hàm 1: Xóa bóng cũ đi (Trả lại màu checkerboard tím/xanh)
	public void ClearShadow()
	{
		// Duyệt qua danh sách các bóng ma đang hoạt động và tiêu diệt chúng
		foreach (GameObject shadow in activeShadows)
		{
			Destroy(shadow);
		}
		// Làm sạch danh sách để chuẩn bị cho lần vẽ bóng tiếp theo
		activeShadows.Clear();
	}

	public void ShowShadow(Block block)
	{
		// 1. Luôn xóa bóng cũ trước
		ClearShadow();

		bool isValid = true;
		// Danh sách tạm để lưu thông tin các vị trí cần tạo bóng
		List<(int i, int j, Sprite sprite)> shadowsToCreate = new List<(int i, int j, Sprite sprite)>();

		foreach (Transform child in block.transform)
		{
			// Vẫn dùng công thức Unscaled chuẩn xác
			Vector3 unscaledPos = block.transform.position + new Vector3(child.localPosition.x * cellWidth, child.localPosition.y * cellHeight, 0);

			Vector2 distance = unscaledPos - (Vector3)startPos;
			int i = Mathf.FloorToInt((distance.x / cellWidth) + 0.5f);
			int j = Mathf.FloorToInt((distance.y / cellHeight) + 0.5f);

			// Nếu bất kỳ phần nào ra ngoài hoặc bị đầy -> Hủy toàn bộ bóng (hoặc có thể hiện bóng đỏ báo lỗi tuỳ bạn)
			if (i < 0 || i >= width || j < 0 || j >= height || isFull[i, j])
			{
				isValid = false;
				break;
			}

			// Lấy sprite của viên gạch con đang xét
			Sprite childSprite = child.GetComponent<SpriteRenderer>().sprite;
			// Lưu lại thông tin để tí nữa tạo bóng
			shadowsToCreate.Add((i, j, childSprite));
		}

		// 3. Nếu hợp lệ thì tiến hành tạo các đối tượng bóng ma
		if (isValid)
		{
			foreach (var item in shadowsToCreate)
			{
				// Tạo một GameObject mới làm bóng
				GameObject shadowPart = new GameObject("ShadowPart");
				shadowPart.transform.SetParent(shadowContainer); // Đưa vào container cho gọn

				// Thêm SpriteRenderer
				SpriteRenderer shadowSR = shadowPart.AddComponent<SpriteRenderer>();

				// COPY SPRITE từ viên gạch gốc sang
				shadowSR.sprite = item.sprite;

				// CHỈNH MÀU: Màu trắng nhưng trong suốt (Alpha = 0.5f tức là 50%)
				// Bạn có thể chỉnh số 0.5f này để tăng giảm độ mờ
				shadowSR.color = new Color(1f, 1f, 1f, 0.5f);

				// Setup Layer: Nằm trên nền (Order 1) nhưng dưới gạch thật (Order 10)
				shadowSR.sortingLayerName = "Default"; // Hoặc layer chứa bàn cờ của bạn
				shadowSR.sortingOrder = 5;
				shadowSR.maskInteraction = SpriteMaskInteraction.None; // Bóng không cần bị mask

				// Đặt vị trí chuẩn vào ô cờ
				Vector2 targetPos2D = startPos + new Vector2(item.i * cellWidth, item.j * cellHeight);
				shadowPart.transform.position = new Vector3(targetPos2D.x, targetPos2D.y, 0f);

				// Tính toán Scale chuẩn (giống hệt lúc đặt gạch thật)
				if (item.sprite != null)
				{
					Bounds b = item.sprite.bounds;
					float scaleX = cellWidth / b.size.x;
					float scaleY = cellHeight / b.size.y;
					shadowPart.transform.localScale = new Vector3(scaleX, scaleY, 1f);
				}

				// Thêm vào danh sách quản lý để tí nữa xóa
				activeShadows.Add(shadowPart);
			}
		}
	}

	private void CheckLines()
	{
		// Danh sách lưu các hàng và cột cần xóa
		List<int> rowsToClear = new List<int>();
		List<int> colsToClear = new List<int>();

		for (int y = 0; y < height; y++)
		{
			bool isRowFull = true;
			for (int x = 0; x < width; x++)
			{
				if (!isFull[x, y]) // Chỉ cần 1 ô trống là coi như hàng chưa đầy
				{
					isRowFull = false;
					break;
				}
			}
			if (isRowFull) rowsToClear.Add(y);
		}

		for (int x = 0; x < width; x++)
		{
			bool isColFull = true;
			for (int y = 0; y < height; y++)
			{
				if (!isFull[x, y])
				{
					isColFull = false;
					break;
				}
			}
			if (isColFull) colsToClear.Add(x);
		}

		// 3. Tiến hành Xóa
		// Nếu có ít nhất 1 hàng hoặc 1 cột đầy thì mới xử lý
		if (rowsToClear.Count > 0 || colsToClear.Count > 0)
		{
			DeleteLines(rowsToClear, colsToClear);
		}
	}

	private void DeleteLines(List<int> rows, List<int> cols)
	{
		int linesCleared = rows.Count + cols.Count;
		if (linesCleared > 0)
		{
			int scoreReward = linesCleared * 100;

			// Gọi sang GameManager để cộng điểm
			if (gameManager != null)
			{
				gameManager.AddScore(scoreReward);
			}
			else
			{
				Debug.LogWarning("Chưa gắn GameManager vào BoardManager!");
			}
			AudioManager.instance.PlaySFX(AudioManager.instance.clearLineSound);
		}
		// Xử lý xóa Hàng
		foreach (int y in rows)
		{
			for (int x = 0; x < width; x++)
			{
				ClearCell(x, y);
			}
		}

		// Xử lý xóa Cột
		foreach (int x in cols)
		{
			for (int y = 0; y < height; y++)
			{
				ClearCell(x, y);
			}
		}
	}

	private void ClearCell(int x, int y)
	{
		// Kiểm tra nếu ô đó có block (tránh lỗi null)
		if (isFull[x, y])
		{
			// 1. Reset trạng thái
			isFull[x, y] = false;

			// 2. Xóa vật thể viên gạch
			if (boards[x, y] != null)
			{
				// --- LẤY DỮ LIỆU ĐỂ CHẠY HIỆU ỨNG ---
				SpriteRenderer sr = boards[x, y].GetComponent<SpriteRenderer>();
				if (sr != null)
				{
					// Gọi hàm hiệu ứng vỡ, truyền vào Vị trí, Sprite gốc và Màu
					PlayShatterEffect(boards[x, y].transform.position, sr.sprite, sr.color);
				}
				Destroy(boards[x, y]);
				boards[x, y] = null; // Gán về null để biết chỗ này giờ đã trống
			}
		}
	}

	private void PlayShatterEffect(Vector3 position, Sprite spriteToShatter, Color color)
	{
		if (breakEffectPrefab != null && spriteToShatter != null)
		{
			// 1. Tạo hiệu ứng
			ParticleSystem effect = Instantiate(breakEffectPrefab, position, Quaternion.identity);

			// 2. Truy cập vào module Texture Sheet Animation
			var textureSheetAnim = effect.textureSheetAnimation;

			// 3. QUAN TRỌNG NHẤT: Gán sprite của viên gạch vào cho hệ thống hạt
			// Nó sẽ dùng sprite này để cắt ra thành các mảnh
			textureSheetAnim.SetSprite(0, spriteToShatter);

			// 4. (Tùy chọn) Gán màu nếu sprite của bạn màu trắng và bạn tô màu bằng code
			var main = effect.main;
			main.startColor = color;

			effect.Play();
		}
	}

	// Kiểm tra xem shape có đặt vừa vào toạ độ startX, startY không
	private bool CanPlaceShape(Block block, int targetX, int targetY)
	{
		// Chúng ta cần tính toán offset của các khối con
		// Logic: Giả sử tâm của Block nằm đúng vị trí của ô (targetX, targetY)
		// Thì các con của nó sẽ nằm ở đâu?

		foreach (Transform child in block.transform)
		{
			// 1. Lấy toạ độ local của con (Ví dụ: 0,0 hoặc 0,1)
			// Lưu ý: Cần làm tròn số vì localPosition có thể lẻ
			int offsetX = Mathf.RoundToInt(child.localPosition.x * (block.transform.localScale.x) / cellWidth);
			// Lưu ý: Nếu block cha không bị scale thì bỏ phần * scale đi. 
			// Nhưng an toàn nhất là dùng công thức đơn giản này nếu child được sắp xếp theo đơn vị 1:

			// Cách đơn giản hơn dựa trên code cũ của bạn:
			// Trong Block.cs, các con thường cách nhau 1 đơn vị local.
			// Ta quy đổi toạ độ con ra toạ độ Grid tương đối.
			int childGridX = Mathf.RoundToInt(child.localPosition.x);
			int childGridY = Mathf.RoundToInt(child.localPosition.y);

			// Toạ độ thực tế trên bảng muốn kiểm tra
			int checkX = targetX + childGridX;
			int checkY = targetY + childGridY;

			// 2. Kiểm tra biên (Ra ngoài bảng)
			if (checkX < 0 || checkX >= width || checkY < 0 || checkY >= height)
			{
				return false;
			}

			// 3. Kiểm tra xem ô đó đã đầy chưa
			if (isFull[checkX, checkY])
			{
				return false;
			}
		}

		// Nếu tất cả các con đều nằm trong bảng và vào ô trống => OK
		return true;
	}

	// [SỬA] Hàm CheckGameOver nhận List<Block>
	public bool CheckGameOver(List<Block> currentBlocks)
	{
		if (currentBlocks == null || currentBlocks.Count == 0) return false;

		int playableBlocksCount = 0; // Đếm số lượng block còn dùng được

		foreach (var block in currentBlocks)
		{
			// Bỏ qua block đã bị tắt hoặc null
			if (block == null || !block.gameObject.activeSelf) continue;

			bool canFitSomewhere = false;

			// --- KIỂM TRA XEM BLOCK NÀY CÓ CHỖ ĐẶT KHÔNG ---
			// Duyệt toàn bộ bàn cờ
			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					if (CanPlaceShape(block, x, y))
					{
						canFitSomewhere = true;
						goto EndCheckForThisBlock;
					}
				}
			}

		EndCheckForThisBlock:

			if (canFitSomewhere)
			{
				block.SetGrayState(false); 
				playableBlocksCount++;
			}
			else
			{
				block.SetGrayState(true);
			}
		}

		if (playableBlocksCount == 0)
		{
			ShowGameOver();
			return true;
		}

		return false;
	}

	private void ShowGameOver()
	{

		// Thay vì bật Popup, ta gọi hàm bên GameManager
		if (gameManager != null)
		{
			gameManager.OnGameOver();
		}
		else
		{
			Debug.LogError("Chưa kéo script GameManager vào ô trống trong Inspector của BoardManager!");
		}
	}
}
