using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class GridGenerator : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        GenerateGridSprite();
    }

    void GenerateGridSprite()
    {
        // 創建32x32的貼圖
        Texture2D tex = new Texture2D(32, 32);
        Color[] colors = new Color[32 * 32];

        for (int y = 0; y < tex.height; y++)
        {
            for (int x = 0; x < tex.width; x++)
            {
                int index = y * tex.width + x;
                // 如果是邊緣或格線，設為白色
                if (x == 0 || x == tex.width-1 || y == 0 || y == tex.height-1)
                    colors[index] = new Color(1f, 1f, 1f, 0.5f);
                else
                    colors[index] = new Color(1f, 1f, 1f, 0f);
            }
        }

        tex.SetPixels(colors);
        tex.Apply();

        // 設置貼圖參數
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Clamp;

        // 創建精靈
        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 32);
        spriteRenderer.sprite = sprite;
        spriteRenderer.sortingOrder = 1; // 確保在遊戲物件上方
    }
}
