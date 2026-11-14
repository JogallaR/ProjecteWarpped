using TMPro;
using UnityEngine;

// Fa que un text capiga sencer dins el seu requadre: prova una mida, mesura el text real
// que s'ha dibuixat i, si se'n surt, el va fent mes petit.
public static class UiTextFit
{
    public static void Fit(TMP_Text t, float maxSize, float minSize = 10f)
    {
        if (t == null || string.IsNullOrEmpty(t.text) || !t.isActiveAndEnabled) return;

        Rect r = t.rectTransform.rect;
        if (r.width <= 1f || r.height <= 1f) return;

        t.enableAutoSizing = false;
        float size = maxSize;

        for (int i = 0; i < 12; i++)
        {
            t.fontSize = size;
            t.ForceMeshUpdate();
            Vector3 b = t.textBounds.size;
            if (b.x <= 0.01f || b.y <= 0.01f) return;

            if (b.x <= r.width && b.y <= r.height) return;

            float k = Mathf.Min(r.width / b.x, r.height / b.y);
            float next = Mathf.Floor(size * Mathf.Min(k, 0.96f));
            if (next <= minSize)
            {
                t.fontSize = minSize;
                return;
            }
            size = next;
        }
    }
}
