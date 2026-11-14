using UnityEngine;

public class MirarObjectiu : MonoBehaviour
{
    [SerializeField] private Transform objectiu;
    public Transform Objectiu => objectiu;
    public float velocitatRotacio = 5f;
    public float velocitatRotacioCaminant = 14f;

    // Mentre camina, el personatge mira cap on va en lloc de mirar l'objectiu
    private bool usarDireccio;
    private Vector3 direccioForcada;

    public void SetFaceDirection(Vector3 direccio)
    {
        direccio.y = 0f;
        if (direccio.sqrMagnitude < 0.0001f) return;
        direccioForcada = direccio.normalized;
        usarDireccio = true;
    }

    public void ClearFaceDirection()
    {
        usarDireccio = false;
    }

    void Awake()
    {
        // Desactivem Apply Root Motion de l'Animator perquè no reiniciï la rotació a cada frame
        foreach (Animator anim in GetComponentsInChildren<Animator>())
        {
            anim.applyRootMotion = false;
        }
    }

    // Aquesta és la funció que cridarem des de fora per presentar-los
    public void AssignarObjectiu(Transform nouObjectiu)
    {
        objectiu = nouObjectiu;
    }

    void LateUpdate()
    {
        if (usarDireccio)
        {
            Quaternion rot = Quaternion.LookRotation(direccioForcada);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, velocitatRotacioCaminant * Time.deltaTime);
            return;
        }

        if (objectiu != null)
        {
            Vector3 direccio = objectiu.position - transform.position;
            direccio.y = 0; 
            
            if (direccio.sqrMagnitude > 0.001f)
            {
                Quaternion rotacioFinal = Quaternion.LookRotation(direccio);
                transform.rotation = Quaternion.Slerp(transform.rotation, rotacioFinal, velocitatRotacio * Time.deltaTime);
            }
        }
    }
}