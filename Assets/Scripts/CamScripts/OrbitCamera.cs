
using UnityEngine;
using UnityEngine.InputSystem;

public class OrbitCamera : MonoBehaviour
{
    [Header("Objectiu")]
    [SerializeField] private Transform enemyTarget;
    [SerializeField] private Vector3 targetOffset = new Vector3(0f, 1.5f, 0f);

    [Header("Configuració")]
    [SerializeField] private float distance = 0f;
    [SerializeField] private float rotationSpeed = 90f;
    [SerializeField] private float minVerticalAngle = -20f;
    [SerializeField] private float maxVerticalAngle = 60f;

    [Header("Zoom (rodeta del ratoli)")]
    [Tooltip("Camera que s'apropa/allunya. Si es buit, s'agafa la camera filla d'aquest objecte.")]
    [SerializeField] private Transform zoomCamera;
    [SerializeField] private float minZoom = 0.45f;   // 0.45 = un 55% mes a prop que la posicio inicial
    [SerializeField] private float maxZoom = 1.5f;    // 1.5 = un 50% mes lluny
    [SerializeField] private float zoomStep = 0.1f;   // canvi per cada "clic" de la rodeta
    [SerializeField] private float zoomSmooth = 10f;

    private Vector3 zoomBaseLocalPosition;
    private float targetZoom = 1f;
    private float currentZoom = 1f;
    private float baseDistance;

    private float horizontalAngle;
    private float verticalAngle = 15f;

    private void Start()
    {
        if (enemyTarget == null)
        {
            Debug.LogError("Assigna l'enemic al camp Enemy Target.", this);
            enabled = false;
            return;
        }

        horizontalAngle = transform.eulerAngles.y;

        if (zoomCamera == null)
        {
            Camera cam = GetComponentInChildren<Camera>();
            if (cam != null && cam.transform != transform) zoomCamera = cam.transform;
        }
        if (zoomCamera != null) zoomBaseLocalPosition = zoomCamera.localPosition;
        baseDistance = distance;
    }

    private void UpdateZoom()
    {
        Mouse mouse = Mouse.current;
        if (mouse != null && !PauseMenu.IsPaused && Time.timeScale > 0f)
        {
            float scroll = mouse.scroll.ReadValue().y;
            if (Mathf.Abs(scroll) > 0.01f)
            {
                // Rodeta endavant = apropar
                targetZoom = Mathf.Clamp(targetZoom - Mathf.Sign(scroll) * zoomStep, minZoom, maxZoom);
            }
        }

        currentZoom = Mathf.Lerp(currentZoom, targetZoom, 1f - Mathf.Exp(-zoomSmooth * Time.unscaledDeltaTime));

        if (zoomCamera != null)
            zoomCamera.localPosition = zoomBaseLocalPosition * currentZoom;
        else
            distance = baseDistance * currentZoom;
    }

    private void LateUpdate()
    {
        if (enemyTarget == null || Keyboard.current == null)
            return;

        UpdateZoom();

        float horizontalInput = 0f;
        float verticalInput = 0f;

        // Tecles configurables des d'Options > Controls
        if (GameSettings.IsHeld(GameAction.CameraRight))
            horizontalInput = -1f;

        if (GameSettings.IsHeld(GameAction.CameraLeft))
            horizontalInput = 1f;

        if (GameSettings.IsHeld(GameAction.CameraDown))
            verticalInput = 1f;

        if (GameSettings.IsHeld(GameAction.CameraUp))
            verticalInput = -1f;

        if (GameSettings.InvertHorizontal) horizontalInput = -horizontalInput;
        if (GameSettings.InvertVertical) verticalInput = -verticalInput;

        float speed = rotationSpeed * GameSettings.CameraSpeed;

        horizontalAngle += horizontalInput * speed * Time.deltaTime;
        verticalAngle += verticalInput * speed * Time.deltaTime;

        verticalAngle = Mathf.Clamp(
            verticalAngle,
            minVerticalAngle,
            maxVerticalAngle
        );

        transform.rotation = Quaternion.Euler(
            verticalAngle,
            horizontalAngle,
            0f
        );

        transform.position =
            enemyTarget.position + targetOffset
            - transform.forward * distance;
    }
}
