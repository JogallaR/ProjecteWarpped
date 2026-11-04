
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
    }

    private void LateUpdate()
    {
        if (enemyTarget == null || Keyboard.current == null)
            return;

        float horizontalInput = 0f;
        float verticalInput = 0f;

        if (Keyboard.current.dKey.isPressed)
            horizontalInput = -1f;

        if (Keyboard.current.aKey.isPressed)
            horizontalInput = 1f;

        if (Keyboard.current.sKey.isPressed)
            verticalInput = 1f;

        if (Keyboard.current.wKey.isPressed)
            verticalInput = -1f;

        horizontalAngle += horizontalInput * rotationSpeed * Time.deltaTime;
        verticalAngle += verticalInput * rotationSpeed * Time.deltaTime;

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
