using UnityEngine;

public class GemSelectEffect : MonoBehaviour
{
    private bool isSelected = false;
    private SpriteRenderer sr;
    private Vector3 baseScale;
    private Color baseColor;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        baseScale = transform.localScale;
        baseColor = sr.color;
    }

    // Update is called once per frame
    void Update()
    {
        if (isSelected)
        {
            //scale
            float t = Time.time * 8f;
            float scale = 1.05f + Mathf.Sin(t) * 0.02f;
            transform.localScale = baseScale * scale;
            //jiggle
            float rot = Mathf.Sin(t * 1.2f) * 5f;
            transform.rotation = Quaternion.Euler(0, 0, rot);
            //bright
            float brightness = 1.1f + Mathf.Sin(t) * 0.05f;
            sr.color = baseColor * brightness;
        }
    }

    public void Select()
    {
        isSelected = true;
    }

    public void Deselect()
    {
        isSelected = false;
        sr.color = baseColor;
        transform.localScale = baseScale;
        transform.rotation = Quaternion.identity;
    }
}
