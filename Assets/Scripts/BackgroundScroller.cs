using UnityEngine;

public class BackgroundScroller : MonoBehaviour
{
    [Header("Scrolling")]
    public float scrollSpeed = 0.5f;
    public float resetPosition = -20f;
    public float startPosition = 20f;

    [Header("Stars")]
    public int starCount = 50;
    public GameObject starPrefab;
    public float minStarSize = 0.02f;
    public float maxStarSize = 0.08f;
    public float minBrightness = 0.3f;
    public float maxBrightness = 1f;

    private Transform[] stars;

    void Start()
    {
        CreateStars();
    }

    void CreateStars()
    {
        stars = new Transform[starCount];

        for (int i = 0; i < starCount; i++)
        {
            GameObject star;

            if (starPrefab != null)
            {
                star = Instantiate(starPrefab, transform);
            }
            else
            {
                // Create simple star sprite
                star = new GameObject("Star");
                star.transform.parent = transform;

                SpriteRenderer sr = star.AddComponent<SpriteRenderer>();
                sr.sprite = CreateCircleSprite();

                float brightness = Random.Range(minBrightness, maxBrightness);
                sr.color = new Color(brightness, brightness, brightness, brightness);
            }

            // Random position
            float x = Random.Range(-4f, 4f);
            float y = Random.Range(-6f, 6f);
            star.transform.position = new Vector3(x, y, 1f);

            // Random size
            float size = Random.Range(minStarSize, maxStarSize);
            star.transform.localScale = Vector3.one * size;

            stars[i] = star.transform;
        }
    }

    Sprite CreateCircleSprite()
    {
        int size = 32;
        Texture2D texture = new Texture2D(size, size);

        Color[] pixels = new Color[size * size];
        float center = size / 2f;
        float radius = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                if (dist < radius)
                {
                    float alpha = 1f - (dist / radius);
                    pixels[y * size + x] = new Color(1, 1, 1, alpha);
                }
                else
                {
                    pixels[y * size + x] = Color.clear;
                }
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();

        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }

    void Update()
    {
        if (stars == null) return;

        for (int i = 0; i < stars.Length; i++)
        {
            if (stars[i] == null) continue;

            // Move star down
            stars[i].Translate(Vector3.down * scrollSpeed * Time.deltaTime, Space.World);

            // Reset position if off screen
            if (stars[i].position.y < resetPosition)
            {
                float x = Random.Range(-4f, 4f);
                stars[i].position = new Vector3(x, startPosition, 1f);
            }
        }
    }
}
