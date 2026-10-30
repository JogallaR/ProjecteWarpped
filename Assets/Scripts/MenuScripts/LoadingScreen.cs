
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LoadingScreen : MonoBehaviour
{
    [Header("Text de càrrega")]
    [SerializeField] private TMP_Text loadingText;

    [Header("Configuració")]
    [SerializeField] private float tempsCarrega = 3f;
    [SerializeField] private float velocitatAnimacio = 0.4f;

    [Header("Escena del joc")]
    [SerializeField] private string escenaJoc = "SampleScene";
    //[SerializeField] private string[] escenaJoc = {"SampleScene","SampleScene2"};

    private void Start()
    {
        StartCoroutine(AnimarICarregar());
    }

    private IEnumerator AnimarICarregar()
    {
        float temps = 0f;
        int punts = 0;

        while (temps < tempsCarrega)
        {
            if (loadingText != null)
            {
                loadingText.text = "Loading" + new string('.', punts);
            }

            punts = (punts + 1) % 4;

            yield return new WaitForSeconds(velocitatAnimacio);
            temps += velocitatAnimacio;
        }
        SceneManager.LoadScene(escenaJoc);
        //SceneManager.LoadScene(escenaJoc[Random.Range(0, escenaJoc.Length + 1)]);
    }
}
