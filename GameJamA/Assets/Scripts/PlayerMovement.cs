using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(SpriteRenderer))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    private void Start()
    {
        var sr = GetComponent<SpriteRenderer>();
        if (sr.sprite == null)
            sr.sprite = CreateSquareSprite(Color.white, 64);
    }

    private void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        float x = (kb.dKey.isPressed ? 1f : 0f) - (kb.aKey.isPressed ? 1f : 0f);
        float y = (kb.wKey.isPressed ? 1f : 0f) - (kb.sKey.isPressed ? 1f : 0f);

        var direction = Vector2.ClampMagnitude(new Vector2(x, y), 1f);
        transform.Translate(direction * moveSpeed * Time.deltaTime);
    }

    private static Sprite CreateSquareSprite(Color color, int size)
    {
        var tex = new Texture2D(size, size);
        var pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = color;
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
}
