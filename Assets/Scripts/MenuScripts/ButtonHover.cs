using UnityEngine;
using UnityEngine.EventSystems;
using TMPro; // Imprescindible per utilitzar TextMeshPro

public class ButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Configuració de l'Escala")]
    [SerializeField] private float hoverScaleFactor = 1.15f; // Ampliació del text
    [SerializeField] private float speed = 12f;             // Velocitat d'animació

    [Header("Configuració de Color de Text")]
    [SerializeField] private Color hoverColor = new Color(0.5f, 0.5f, 0.5f, 1f); // Color del text en enfosquir (Gris fosc)

    private Vector3 originalScale;
    private Vector3 targetScale;

    private TextMeshProUGUI tmpText;
    private Color originalColor;
    private Color targetColor;

    private void Awake()
    {
        // Obtenim el component de text que està al mateix GameObject
        tmpText = GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;

        if (tmpText != null)
        {
            originalColor = tmpText.color;
            targetColor = originalColor;
        }
    }

    private void Update()
    {
        // Utilitzem Time.unscaledDeltaTime perquè funcioni fins i tot amb el joc en pausa (Time.timeScale = 0)
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.unscaledDeltaTime * speed);

        if (tmpText != null)
        {
            tmpText.color = Color.Lerp(tmpText.color, targetColor, Time.unscaledDeltaTime * speed);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = originalScale * hoverScaleFactor;
        if (tmpText != null)
        {
            targetColor = hoverColor;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = originalScale;
        if (tmpText != null)
        {
            targetColor = originalColor;
        }
    }

    private void OnDisable()
    {
        transform.localScale = originalScale;
        targetScale = originalScale;

        if (tmpText != null)
        {
            tmpText.color = originalColor;
            targetColor = originalColor;
        }
    }
}