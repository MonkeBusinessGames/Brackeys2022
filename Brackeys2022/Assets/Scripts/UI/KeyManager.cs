using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class KeyManager
{

    public Dictionary<Key, KeyCode> keys;

    /// <summary>
    /// Creates a new keymanager with default keys
    /// </summary>
    public KeyManager()
    {
        keys = new Dictionary<Key, KeyCode>();
        keys.Add(Key.Up, KeyCode.UpArrow);
        keys.Add(Key.Down, KeyCode.DownArrow);
        keys.Add(Key.Left, KeyCode.LeftArrow);
        keys.Add(Key.Right, KeyCode.RightArrow);
        keys.Add(Key.Jump, KeyCode.Space);
        keys.Add(Key.Hide, KeyCode.X);
        keys.Add(Key.Attack, KeyCode.C);
        keys.Add(Key.Pause, KeyCode.Escape);
    }

    /// <summary>
    /// Simplifies vertical inputs into a single axis value
    /// </summary>
    /// <returns>A vertical axis value</returns>
    public float Vertical()
    {
        float axis = 0;
        if (Input.GetKey(keys[Key.Up]))
            axis += 1;
        if (Input.GetKey(keys[Key.Down]))
            axis -= 1;
        return axis;
    }

    /// <summary>
    /// Simplifies horizontal inputs into a single axis value
    /// </summary>
    /// <returns>A horizontal axis value</returns>
    public float Horizontal()
    {
        float axis = 0;
        if (Input.GetKey(keys[Key.Right]))
            axis += 1;
        if (Input.GetKey(keys[Key.Left]))
            axis -= 1;
        return axis;
    }

}

[System.Serializable]
public enum Key
{
    Up,
    Down,
    Left,
    Right,
    Jump,
    Hide,
    Attack,
    Pause
}