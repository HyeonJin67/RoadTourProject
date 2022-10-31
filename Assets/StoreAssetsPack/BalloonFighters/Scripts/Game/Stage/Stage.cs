using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stage : MonoBehaviour
{
    int     _index;
    string  _name;
    Vector2 _worldSize;

    public int Index { get { return _index; } }
    public string Name{ get { return _name; } }
    public Vector2 WorldSize { get { return _worldSize; } }
}