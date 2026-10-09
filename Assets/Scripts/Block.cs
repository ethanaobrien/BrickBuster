using UnityEngine;

// a single brick. records its size for collision math, and applies the sprite and color at spawn.
public class Block : MonoBehaviour
{
    // world-unit size, used by Game.cs instead of reading the transform
    public Vector2 Size { get; private set; }

    // sets the brick's size and tint. called once at spawn
    public void Configure(Vector2 size, Sprite sprite, Color color)
    {
        Size = size;
        transform.localScale = new Vector3(size.x, size.y, 1f);

        var renderer = GetComponent<SpriteRenderer>();
        if (renderer == null) renderer = gameObject.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = color;
    }
}
