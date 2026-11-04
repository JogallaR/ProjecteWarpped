using UnityEngine;

public class WaterMovement : MonoBehaviour
{
    public Material waterMaterial;

    public Vector2 waterSpeed = new Vector2(0.02f, 0.02f);
    public Vector2 foamSpeed = new Vector2(-0.01f, 0.01f);

    public float foamStrength = 1.5f;

    private Vector2 waterOffset;
    private Vector2 foamOffset;

    void Start()
    {
        waterMaterial.SetFloat("_DetailNormalMapScale", foamStrength);
    }

    void Update()
    {
        waterOffset += waterSpeed * Time.deltaTime;
        foamOffset += foamSpeed * Time.deltaTime;

        // Water movement
        waterMaterial.mainTextureOffset = waterOffset;

        // Foam movement
        waterMaterial.SetTextureOffset("_DetailAlbedoMap", foamOffset);
    }
}