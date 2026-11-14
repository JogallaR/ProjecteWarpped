using System;
using UnityEngine;
using UnityEngine.InputSystem;

// Accions del joc que es poden reassignar des d'Options > Controls
public enum GameAction
{
    CameraLeft,
    CameraRight,
    CameraUp,
    CameraDown,
    MoveUp,
    MoveDown,
    MoveLeft,
    MoveRight,
    Attack1,
    Attack2,
    Attack3,
    SkipTurn,
    TogglePath,
    ToggleHelp
}

// Configuracio global (velocitat de camera, invertir eixos i tecles). Es desa a PlayerPrefs
// i funciona igual al MainMenu que dins la partida.
public static class GameSettings
{
    public const float MinCameraSpeed = 0.25f;
    public const float MaxCameraSpeed = 3f;

    private const string CameraSpeedKey = "Settings_CameraSpeed";
    private const string InvertHKey = "Settings_InvertH";
    private const string InvertVKey = "Settings_InvertV";
    private const string BindingPrefix = "Settings_Key_";

    public static event Action Changed;

    // Frame en que la UI d'opcions ha fet servir la tecla Esc (perque el PauseMenu no la torni a processar)
    public static int EscapeHandledFrame = -1;
    // Cert mentre la UI d'opcions vol gestionar ella mateixa la tecla Esc (esperant tecla o a la llista de tecles)
    public static bool UiWantsEscape;

    public static bool EscapeConsumedByUi => UiWantsEscape || EscapeHandledFrame == Time.frameCount;

    private static bool loaded;
    private static float cameraSpeed = 1f;
    private static bool invertHorizontal;
    private static bool invertVertical;
    private static readonly Key[] bindings = new Key[Enum.GetValues(typeof(GameAction)).Length];

    public static readonly GameAction[] AllActions = (GameAction[])Enum.GetValues(typeof(GameAction));

    public static Key DefaultKey(GameAction action)
    {
        switch (action)
        {
            case GameAction.CameraLeft: return Key.A;
            case GameAction.CameraRight: return Key.D;
            case GameAction.CameraUp: return Key.W;
            case GameAction.CameraDown: return Key.S;
            case GameAction.MoveUp: return Key.UpArrow;
            case GameAction.MoveDown: return Key.DownArrow;
            case GameAction.MoveLeft: return Key.LeftArrow;
            case GameAction.MoveRight: return Key.RightArrow;
            case GameAction.Attack1: return Key.Digit1;
            case GameAction.Attack2: return Key.Digit2;
            case GameAction.Attack3: return Key.Digit3;
            case GameAction.SkipTurn: return Key.Enter;
            case GameAction.TogglePath: return Key.C;
            case GameAction.ToggleHelp: return Key.Tab;
        }
        return Key.None;
    }

    // Tecla alternativa que nomes funciona si l'accio te la tecla per defecte (com abans: teclat numeric)
    private static Key DefaultAlternate(GameAction action)
    {
        switch (action)
        {
            case GameAction.Attack1: return Key.Numpad1;
            case GameAction.Attack2: return Key.Numpad2;
            case GameAction.Attack3: return Key.Numpad3;
            case GameAction.SkipTurn: return Key.NumpadEnter;
        }
        return Key.None;
    }

    public static string ActionLabel(GameAction action)
    {
        switch (action)
        {
            case GameAction.CameraLeft: return "CAMERA LEFT";
            case GameAction.CameraRight: return "CAMERA RIGHT";
            case GameAction.CameraUp: return "CAMERA UP";
            case GameAction.CameraDown: return "CAMERA DOWN";
            case GameAction.MoveUp: return "MOVE UP";
            case GameAction.MoveDown: return "MOVE DOWN";
            case GameAction.MoveLeft: return "MOVE LEFT";
            case GameAction.MoveRight: return "MOVE RIGHT";
            case GameAction.Attack1: return "ATTACK 1";
            case GameAction.Attack2: return "ATTACK 2";
            case GameAction.Attack3: return "ATTACK 3";
            case GameAction.SkipTurn: return "SKIP TURN";
            case GameAction.TogglePath: return "PATH MODE";
            case GameAction.ToggleHelp: return "SHOW CONTROLS";
        }
        return action.ToString().ToUpper();
    }

    private static void EnsureLoaded()
    {
        if (loaded) return;
        loaded = true;

        cameraSpeed = Mathf.Clamp(PlayerPrefs.GetFloat(CameraSpeedKey, 1f), MinCameraSpeed, MaxCameraSpeed);
        invertHorizontal = PlayerPrefs.GetInt(InvertHKey, 0) == 1;
        invertVertical = PlayerPrefs.GetInt(InvertVKey, 0) == 1;

        foreach (GameAction action in AllActions)
        {
            int stored = PlayerPrefs.GetInt(BindingPrefix + action, (int)DefaultKey(action));
            Key key = Enum.IsDefined(typeof(Key), stored) ? (Key)stored : DefaultKey(action);
            if (key == Key.None || key == Key.Escape) key = DefaultKey(action);
            bindings[(int)action] = key;
        }
    }

    private static void Save()
    {
        PlayerPrefs.Save();
        Changed?.Invoke();
    }

    // --- Camera ---

    public static float CameraSpeed
    {
        get { EnsureLoaded(); return cameraSpeed; }
        set
        {
            EnsureLoaded();
            cameraSpeed = Mathf.Clamp(value, MinCameraSpeed, MaxCameraSpeed);
            PlayerPrefs.SetFloat(CameraSpeedKey, cameraSpeed);
            Save();
        }
    }

    public static bool InvertHorizontal
    {
        get { EnsureLoaded(); return invertHorizontal; }
        set
        {
            EnsureLoaded();
            invertHorizontal = value;
            PlayerPrefs.SetInt(InvertHKey, value ? 1 : 0);
            Save();
        }
    }

    public static bool InvertVertical
    {
        get { EnsureLoaded(); return invertVertical; }
        set
        {
            EnsureLoaded();
            invertVertical = value;
            PlayerPrefs.SetInt(InvertVKey, value ? 1 : 0);
            Save();
        }
    }

    // --- Tecles ---

    public static Key GetKey(GameAction action)
    {
        EnsureLoaded();
        return bindings[(int)action];
    }

    // Assigna una tecla. Si ja la feia servir una altra accio, s'intercanvien.
    public static void SetKey(GameAction action, Key key)
    {
        EnsureLoaded();
        if (key == Key.None || key == Key.Escape) return;

        Key previous = bindings[(int)action];
        foreach (GameAction other in AllActions)
        {
            if (other != action && bindings[(int)other] == key)
            {
                bindings[(int)other] = previous;
                PlayerPrefs.SetInt(BindingPrefix + other, (int)previous);
            }
        }

        bindings[(int)action] = key;
        PlayerPrefs.SetInt(BindingPrefix + action, (int)key);
        Save();
    }

    public static void ResetControls()
    {
        EnsureLoaded();
        cameraSpeed = 1f;
        invertHorizontal = false;
        invertVertical = false;
        PlayerPrefs.SetFloat(CameraSpeedKey, cameraSpeed);
        PlayerPrefs.SetInt(InvertHKey, 0);
        PlayerPrefs.SetInt(InvertVKey, 0);

        foreach (GameAction action in AllActions)
        {
            bindings[(int)action] = DefaultKey(action);
            PlayerPrefs.SetInt(BindingPrefix + action, (int)DefaultKey(action));
        }
        Save();
    }

    public static bool WasPressed(GameAction action)
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return false;

        Key key = GetKey(action);
        if (key != Key.None && keyboard[key].wasPressedThisFrame) return true;

        Key alt = DefaultAlternate(action);
        return alt != Key.None && key == DefaultKey(action) && keyboard[alt].wasPressedThisFrame;
    }

    public static bool IsHeld(GameAction action)
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return false;

        Key key = GetKey(action);
        return key != Key.None && keyboard[key].isPressed;
    }

    public static string KeyName(GameAction action)
    {
        return KeyName(GetKey(action));
    }

    public static string KeyName(Key key)
    {
        switch (key)
        {
            case Key.UpArrow: return "UP";
            case Key.DownArrow: return "DOWN";
            case Key.LeftArrow: return "LEFT";
            case Key.RightArrow: return "RIGHT";
            case Key.Enter: return "ENTER";
            case Key.NumpadEnter: return "NUM ENTER";
            case Key.Space: return "SPACE";
            case Key.Backspace: return "BACKSPACE";
            case Key.LeftShift: return "L SHIFT";
            case Key.RightShift: return "R SHIFT";
            case Key.LeftCtrl: return "L CTRL";
            case Key.RightCtrl: return "R CTRL";
            case Key.LeftAlt: return "L ALT";
            case Key.RightAlt: return "R ALT";
            case Key.None: return "-";
        }

        string name = key.ToString();
        if (name.StartsWith("Digit")) return name.Substring(5);
        if (name.StartsWith("Numpad")) return "NUM " + name.Substring(6).ToUpper();
        return name.ToUpper();
    }
}
