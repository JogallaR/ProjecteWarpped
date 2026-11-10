
using UnityEngine;
using TMPro;

public class CharacterSelector : MonoBehaviour
{
    [Header("Llista de Personatges")]
    [SerializeField] private GameObject[] characters;

    [Tooltip("Opcional: Noms personalitzats per mostrar a la UI.")]
    [SerializeField] private string[] customNames;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI characterNameText;

    [Header("Configuració de Rotació")]
    [SerializeField] private float rotationSpeed = 30f;

    private int currentIndex = 0;
    private Quaternion initialRotation;
    private bool selectionInitialized = false;

    private void Start()
    {
        initialRotation = transform.rotation;

        if (characters == null || characters.Length == 0)
        {
            Debug.LogError("Error: No has assignat cap personatge!", this);
            return;
        }

        // No mostrem cap personatge en iniciar el menú.
        foreach (GameObject character in characters)
        {
            if (character != null)
            {
                character.SetActive(false);
            }
        }

        // Ocultem també el nom fins a obrir la selecció.
        if (characterNameText != null)
        {
            characterNameText.text = "";
        }
    }

    private void Update()
    {
        // Només rotem quan la selecció està oberta.
        if (selectionInitialized)
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }
    }

    public void InitializeCharacterSelection()
    {
        if (characters == null || characters.Length == 0)
        {
            Debug.LogError("Error: No hi ha personatges assignats!", this);
            return;
        }

        // Seleccionem sempre el primer personatge de l'array.
        currentIndex = 0;

        selectionInitialized = true;
        ShowCharacter(currentIndex);
    }

    public void NextCharacter()
    {
        if (!selectionInitialized ||
            characters == null ||
            characters.Length == 0)
        {
            return;
        }

        currentIndex++;

        if (currentIndex >= characters.Length)
        {
            currentIndex = 0;
        }

        ShowCharacter(currentIndex);
    }

    public void PreviousCharacter()
    {
        if (!selectionInitialized ||
            characters == null ||
            characters.Length == 0)
        {
            return;
        }

        currentIndex--;

        if (currentIndex < 0)
        {
            currentIndex = characters.Length - 1;
        }

        ShowCharacter(currentIndex);
    }

    private void ShowCharacter(int index)
    {
        if (index < 0 || index >= characters.Length)
        {
            return;
        }

        transform.rotation = initialRotation;

        for (int i = 0; i < characters.Length; i++)
        {
            if (characters[i] == null)
            {
                continue;
            }

            bool isActive = (i == index);
            characters[i].SetActive(isActive);

            // Reiniciem l'animació del personatge seleccionat.
            if (isActive)
            {
                Animator anim = characters[i].GetComponent<Animator>();

                if (anim != null)
                {
                    anim.Play("Idle", 0, 0f);
                }
            }
        }

        PlayerPrefs.SetInt("SelectedCharacterIndex", index);
        PlayerPrefs.Save();

        UpdateNameText(index);
    }

    private void UpdateNameText(int index)
    {
        if (characterNameText == null)
        {
            return;
        }

        if (customNames != null &&
            index < customNames.Length &&
            !string.IsNullOrEmpty(customNames[index]))
        {
            characterNameText.text = customNames[index];
        }
        else if (characters[index] != null)
        {
            characterNameText.text = characters[index].name;
        }
        else
        {
            characterNameText.text = "";
        }
    }
}
