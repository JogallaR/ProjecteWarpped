using UnityEngine;
using UnityEngine.Audio;

// Llista de sons del joc. L'asset es a Assets/Resources/GameSoundLibrary.asset
// i es pot editar des de l'Inspector per canviar qualsevol so.
// Si un camp es buit, GameAudio fa servir un so generat per codi.
[CreateAssetMenu(fileName = "GameSoundLibrary", menuName = "Warped/Game Sound Library")]
public class GameSoundLibrary : ScriptableObject
{
    [Header("Mixer")]
    public AudioMixerGroup sfxGroup;

    [Header("Player")]
    public AudioClip footstep;
    public AudioClip meleeSwing;
    public AudioClip meleeHit;
    public AudioClip arrowShoot;
    public AudioClip arrowHit;
    public AudioClip magicCast;
    public AudioClip magicHit;
    public AudioClip playerHurt;
    public AudioClip heal;
    public AudioClip playerDeath;
    public AudioClip actionDenied;

    [Header("Enemy")]
    public AudioClip enemyRoar;
    public AudioClip enemyWarning;
    public AudioClip enemySlam;
    public AudioClip enemyHurt;
    public AudioClip enemyDeath;

    [Header("Game")]
    public AudioClip victory;

    [Header("Volume")]
    [Range(0f, 1f)] public float masterSfxVolume = 1f;
}
