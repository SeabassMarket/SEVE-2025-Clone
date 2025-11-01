using UnityEngine;

public class WaterScroller : MonoBehaviour
{
    public float scrollSpeed = 0.1f;
    private Renderer rend;
    private Vector2 offset;

    void Start()
    {
        rend = GetComponent<Renderer>();
    }

    void Update()
    {
        offset.y += scrollSpeed * Time.deltaTime;
        rend.material.SetTextureOffset("_BaseMap", offset);
    }
}
