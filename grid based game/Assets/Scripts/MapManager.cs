using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MapManager : MonoBehaviour
{
    public static MapManager mapManager;
    public static Room currentRoom;

    [Header("Level Generation")]
    [SerializeField] private int rooms;
    [SerializeField] private GameObject roomIconPrefab, minimap;

    [SerializeField] private List<Layout> layouts = new();

    [Header("Icons")]
    [SerializeField] private Sprite normalIcon;
    [SerializeField] private Sprite bossIcon, lootIcon, shopIcon;

    private Room[,] roomGrid;
    private List<Room> roomLayout = new();
    private List<GameObject> roomIcons = new();

    private void Awake()
    {
        if (mapManager == null)
        {
            mapManager = this;
            GenerateLevel();
        }
        else
            Destroy(gameObject);
    }

    public void SwitchRoom(Vector2Int direction)
    {
        if (direction.y == 1)
        {
            if (currentRoom.doors[0] != DoorState.Present)
                return;

            GameManager.gameManager.NewRoom(GameManager.player.standingNode.gridX, 0);
        }
        else if (direction.y == -1)
        {
            if (currentRoom.doors[1] != DoorState.Present)
                return;

            GameManager.gameManager.NewRoom(GameManager.player.standingNode.gridX, 9);
        }
        else if (direction.x == -1)
        {
            if (currentRoom.doors[2] != DoorState.Present)
                return;

            GameManager.gameManager.NewRoom(19, GameManager.player.standingNode.gridY);
        }
        else if (direction.x == 1)
        {
            if (currentRoom.doors[3] != DoorState.Present)
                return;

            GameManager.gameManager.NewRoom(0, GameManager.player.standingNode.gridY);
        }
        
        currentRoom = roomGrid[currentRoom.gridX + direction.x, currentRoom.gridY + direction.y];
        SetMapPosition(currentRoom);
        SceneManager.LoadScene("First Floor", LoadSceneMode.Single);
    }
    
    private void GenerateLevel()
    {
        List<Room> openRooms = new();

        roomLayout.Clear();
        roomGrid = new Room[rooms * 2 + 1, rooms * 2 + 1];

        for (int x = 0; x < roomGrid.GetLength(0); x++)
        {
            for (int y = 0; y < roomGrid.GetLength(1); y++)
            {
                roomGrid[x, y] = new Room(RoomType.Normal, x, y);
            }
        }

        currentRoom = roomGrid[rooms, rooms];
        currentRoom.roomType = RoomType.Starting;
        roomLayout.Add(roomGrid[rooms, rooms]);

        for (int i = 1; i < rooms; i++)
        {
            List<Room> neighbors = GetNeighbors(roomLayout[Random.Range(0, roomLayout.Count)]);

            if (neighbors.Count > 0)
            {
                int randRoom = Random.Range(0, neighbors.Count);
                roomLayout.Add(neighbors[randRoom]);
                openRooms.Add(neighbors[randRoom]);
            }
            else
                i--;
        }

        foreach(Room room in roomLayout)
        {
            //calculate room distance
            room.distanceFromStart = Mathf.Abs(room.gridX - currentRoom.gridX) + Mathf.Abs(room.gridY - currentRoom.gridY);

            //find a suitable room layout
            List<Layout> possibleLayouts = new();
            List<Layout> layoutsToBeRemoved = new();

            foreach(Layout layout in layouts)
            {
                possibleLayouts.Add(layout);
            }
            
            GameObject roomIcon = Instantiate(roomIconPrefab, minimap.transform);
            roomIcon.GetComponent<RectTransform>().localPosition = new Vector2((room.gridX - rooms) * 40, (room.gridY - rooms) * 40);
            roomIcons.Add(roomIcon);

            if (room.roomType != RoomType.Starting)
                roomIcon.transform.GetChild(1).GetComponent<Image>().sprite = normalIcon;
            else
                roomIcon.transform.GetChild(1).GetComponent<Image>().enabled = false;

            if (roomLayout.Contains(roomGrid[room.gridX, room.gridY + 1]))
            {
                room.doors[0] = DoorState.Present;
                roomIcon.transform.GetChild(0).GetChild(0).gameObject.SetActive(false);
            }
            else
            {
                foreach(Layout layout in possibleLayouts)
                {
                    if (layout.doorsNeeded.Contains(Direction.Up))
                    {
                        layoutsToBeRemoved.Add(layout);
                    }
                }

                foreach(Layout layout in layoutsToBeRemoved)
                {
                    possibleLayouts.Remove(layout);
                }

                layoutsToBeRemoved.Clear();
            }

            if (roomLayout.Contains(roomGrid[room.gridX, room.gridY - 1]))
            {
                room.doors[1] = DoorState.Present;
                roomIcon.transform.GetChild(0).GetChild(1).gameObject.SetActive(false);
            }
            else
            {
                foreach (Layout layout in possibleLayouts)
                {
                    if (layout.doorsNeeded.Contains(Direction.Down))
                    {
                        layoutsToBeRemoved.Add(layout);
                    }
                }

                foreach (Layout layout in layoutsToBeRemoved)
                {
                    possibleLayouts.Remove(layout);
                }

                layoutsToBeRemoved.Clear();
            }

            if (roomLayout.Contains(roomGrid[room.gridX - 1, room.gridY]))
            {
                room.doors[2] = DoorState.Present;
                roomIcon.transform.GetChild(0).GetChild(2).gameObject.SetActive(false);
            }
            else
            {
                foreach (Layout layout in possibleLayouts)
                {
                    if (layout.doorsNeeded.Contains(Direction.Left))
                    {
                        layoutsToBeRemoved.Add(layout);
                    }
                }

                foreach (Layout layout in layoutsToBeRemoved)
                {
                    possibleLayouts.Remove(layout);
                }

                layoutsToBeRemoved.Clear();
            }

            if (roomLayout.Contains(roomGrid[room.gridX + 1, room.gridY]))
            {
                room.doors[3] = DoorState.Present;
                roomIcon.transform.GetChild(0).GetChild(3).gameObject.SetActive(false);
            }
            else
            {
                foreach (Layout layout in possibleLayouts)
                {
                    if (layout.doorsNeeded.Contains(Direction.Right))
                    {
                        layoutsToBeRemoved.Add(layout);
                    }
                }

                foreach (Layout layout in layoutsToBeRemoved)
                {
                    possibleLayouts.Remove(layout);
                }

                layoutsToBeRemoved.Clear();
            }

            if (possibleLayouts.Count > 0)
                room.layout = possibleLayouts[Random.Range(0, possibleLayouts.Count)].layout;
        }

        if (rooms > 1)
        {
            //determine boss room
            Room farthestRoom = null;
            int maxDistance = 0;

            foreach (Room room in openRooms)
            {
                if (room.distanceFromStart > maxDistance)
                {
                    farthestRoom = room;
                    maxDistance = room.distanceFromStart;
                }
            }

            farthestRoom.roomType = RoomType.Boss;
            roomIcons[roomLayout.IndexOf(farthestRoom)].transform.GetChild(1).GetComponent<Image>().sprite = bossIcon;
            openRooms.Remove(farthestRoom);


            //determine other special
            Room lootRoom = openRooms[Random.Range(0, openRooms.Count)];
            lootRoom.roomType = RoomType.Loot;
            roomIcons[roomLayout.IndexOf(lootRoom)].transform.GetChild(1).GetComponent<Image>().sprite = lootIcon;
            openRooms.Remove(lootRoom);

            Room shopRoom = openRooms[Random.Range(0, openRooms.Count)];
            shopRoom.roomType = RoomType.Shop;
            roomIcons[roomLayout.IndexOf(shopRoom)].transform.GetChild(1).GetComponent<Image>().sprite = shopIcon;
            openRooms.Remove(shopRoom);
        }
    }

    private void SetMapPosition(Room room)
    {
        minimap.GetComponent<RectTransform>().localPosition = new Vector2(-(room.gridX - rooms) * 40, -(room.gridY - rooms) * 40);
    }

    private List<Room> GetNeighbors(Room room)
    {
        List<Room> neighbors = new List<Room>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if ((x == 0 && y == 0) || (x == -1 && y == 1) || (x == 1 && y == 1) || (x == 1 && y == -1) || (x == -1 && y == -1))
                    continue;

                if (roomLayout.Contains(roomGrid[x + room.gridX, y + room.gridY]))
                    continue;

                neighbors.Add(roomGrid[x + room.gridX, y + room.gridY]);
            }
        }

        return neighbors;
    }
}
