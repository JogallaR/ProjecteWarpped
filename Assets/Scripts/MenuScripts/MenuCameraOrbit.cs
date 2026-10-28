using UnityEngine;

public class MenuCameraOrbit : MonoBehaviour
{
    [SerializeField] private Transform targetPoint; // El punt del mapa sobre el qual girarà
    [SerializeField] private float rotationSpeed = 5f; // Velocitat de gir lenta
    [SerializeField] private float distance = 10f;     // Distància respecte al punt

    private void Update()
    {
        if (targetPoint == null) return;

        // Fent girar la càmera al voltant del punt fixat
        transform.RotateAround(targetPoint.position, Vector3.up, rotationSpeed * Time.deltaTime);
        
        // Assegura que la càmera sempre mira cap al punt d'interès
        transform.LookAt(targetPoint);
    }
}