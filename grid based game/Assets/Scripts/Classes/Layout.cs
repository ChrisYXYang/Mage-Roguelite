using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Direction { Up, Down, Left, Right};

[System.Serializable]
public class Layout
{
    public GameObject layout;
    public List<Direction> doorsNeeded = new();
}
