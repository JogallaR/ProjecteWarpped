using UnityEngine;
using UnityEngine.SceneManagement;

public static class GameStats
{
    public static int torns, passos, malRebut, cures;

    [RuntimeInitializeOnLoadMethod]
    static void Init() => SceneManager.sceneLoaded += (s, m) => Reset();

    public static void Reset() { torns = passos = malRebut = cures = 0; }
}