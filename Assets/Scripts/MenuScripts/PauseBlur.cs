
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class PauseBlur : MonoBehaviour
{
    [SerializeField] private Shader blurShader;
    [SerializeField, Range(0.5f, 5f)]
    private float blurSize = 2f;

    private Material blurMaterial;

    public bool BlurEnabled { get; set; }

    private void Awake()
    {
        if (blurShader == null)
            blurShader = Shader.Find("Custom/PauseBlur");

        if (blurShader != null)
        {
            blurMaterial = new Material(blurShader);
        }
        else
        {
            Debug.LogError("No s'ha trobat el shader PauseBlur.");
        }
    }

    private void OnRenderImage(RenderTexture source,
        RenderTexture destination)
    {
        if (!BlurEnabled || blurMaterial == null)
        {
            Graphics.Blit(source, destination);
            return;
        }

        blurMaterial.SetFloat("_BlurSize", blurSize);

        RenderTexture temp = RenderTexture.GetTemporary(
            source.width,
            source.height,
            0,
            source.format
        );

        Graphics.Blit(source, temp, blurMaterial, 0);
        Graphics.Blit(temp, destination, blurMaterial, 1);

        RenderTexture.ReleaseTemporary(temp);
    }

    private void OnDestroy()
    {
        if (blurMaterial != null)
            Destroy(blurMaterial);
    }
}
