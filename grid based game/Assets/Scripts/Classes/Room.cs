using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum DoorState { Present, NotPresent};
public enum RoomType { Normal, Starting, Loot, Shop, Boss };

public class Room
{
    public RoomType roomType;
    public int gridX, gridY, distanceFromStart;
    public GameObject layout;
    public bool roomCleared;
    public List<DoorState> doors = new();

    public Room(RoomType _roomType, int _gridX, int _gridY)
    {
        roomType = _roomType;
        gridX = _gridX;
        gridY = _gridY;
        distanceFromStart = 0;
        roomCleared = false;

        for (int i = 0; i < 4; i++)
        {
            doors.Add(DoorState.NotPresent);
        }
    }
}
