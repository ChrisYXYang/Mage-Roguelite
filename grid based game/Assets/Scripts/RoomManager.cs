using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    public static RoomManager roomManager;

    [SerializeField] private Sprite open, closed;
    
    [SerializeField] private GameObject doors;

    private void Awake()
    {
        roomManager = this;
        
        if (GameManager.canSpawnRoom)
        {
            if (MapManager.currentRoom.layout != null && !MapManager.currentRoom.roomCleared 
                && MapManager.currentRoom.roomType != RoomType.Starting && MapManager.currentRoom.roomType != RoomType.Loot && MapManager.currentRoom.roomType != RoomType.Boss && MapManager.currentRoom.roomType != RoomType.Shop)
                Instantiate(MapManager.currentRoom.layout, Vector2.zero, Quaternion.identity, transform);

            GameManager.gameManager.NewRoomPt2();
        }
    }

    private void Start()
    {
        for (int i = 0; i < 4; i++)
        {
            if (MapManager.currentRoom.doors[i] == DoorState.Present)
            {
                doors.transform.GetChild(i).gameObject.SetActive(true);
            }
        }

        GameManager.gameManager.NewRoomPt3();
    }

    //****** DESTROY TOMMMORW IMMEDIATELY!!!!!!!
    private void Update()
    {
        for (int i = 0; i < 4; i++)
        {
            if (MapManager.currentRoom.roomCleared)
            {
                doors.transform.GetChild(i).GetComponent<SpriteRenderer>().sprite = open;
            }
            else
            {
                doors.transform.GetChild(i).GetComponent<SpriteRenderer>().sprite = closed;
            }
        }
    }
}
