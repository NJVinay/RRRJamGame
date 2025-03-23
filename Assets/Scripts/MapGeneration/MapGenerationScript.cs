

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum CurrentLevel
{
    Hotel,      // index 0
    Subway,     // index 1
    Hospital    // index 2
}

public enum CurrentVariant
{
    V1,         // index 0
    V2,         // index 1
    V3          // index 2
}

public class MapGenerationScript : MonoBehaviour
{
    // Erir's recommendations:
    // - same room count each time
    // i.e 2 item rooms, 1 boss room, 1 spawn room, 10 enemy rooms
    // also, make sure that item rooms and boss rooms are on the edges.
    // spawn room can be anywhere really
    // in terms of rooms, I think we already figured that much out - 2 item rooms, 1 boss room, 1 spawn room and 10 enemy rooms
    // enemies, I was thinking we have 6 spawn points per room, and there are scriptable objects that determine what enemies spawn
    // i.e a more hard encounter might only spawn 2 but harder enemies, and it'll spawn them as far away from the player upon room entry

    public List<Tilemap> dungeonTilemap = new List<Tilemap>();
    public List<GameObject> roomMarkers = new List<GameObject>();
    List<GameObject> markersPlaced = new List<GameObject>();
    List<GameObject> prefabRoomsPlaced = new List<GameObject>();

    List<List<List<TileBase>>> roomTiles = new List<List<List<TileBase>>>();

    public List<GameObject> hotelRooms = new List<GameObject>();
    List<List<TileBase>> hotelTiles = new List<List<TileBase>>();
    public List<TileBase> hotelV1Tiles = new List<TileBase>();

    public List<GameObject> subwayRooms = new List<GameObject>();
    List<List<TileBase>> subwayTiles = new List<List<TileBase>>();
    public List<TileBase> subwayV1Tiles = new List<TileBase>();
    public List<TileBase> subwayV2Tiles = new List<TileBase>();
    public List<TileBase> subwayV3Tiles = new List<TileBase>();

    public List<GameObject> hospitalRooms = new List<GameObject>();
    List<List<TileBase>> hospitalTiles = new List<List<TileBase>>();
    public List<TileBase> hospitalV1Tiles = new List<TileBase>();

    public CurrentLevel currentLevel = CurrentLevel.Hotel;
    public CurrentVariant currentVariant = CurrentVariant.V1;
    List<TileBase> currentTiles = new List<TileBase>();
    List<GameObject> currentLevelRooms = new List<GameObject>();

    List<Vector2Int[]> sizeBank = new List<Vector2Int[]>();

    // Room sizes
    Vector2Int[] hotelSizeBank = new Vector2Int[16] // --------------- HOTEL
    {
        new Vector2Int(24, 22), // BossRoom1 
        new Vector2Int(30, 22), // 2

        new Vector2Int(20, 20), // EnemyRoom1
        new Vector2Int(22, 18), // 2
        new Vector2Int(28, 22), // 3
        new Vector2Int(54, 20), // 4
        new Vector2Int(40, 38), // 5
        new Vector2Int(54, 30), // 6
        new Vector2Int(22, 15), // 7
        new Vector2Int(28, 16), // 8
        new Vector2Int(34, 22), // 9
        new Vector2Int(20, 14), // 10

        new Vector2Int(18, 14), // ItemRoom1
        new Vector2Int(18, 10), // 2
        new Vector2Int(18, 14), // 3
        new Vector2Int(18, 10)  // 4
    };

    Vector2Int[] subwaySizeBank = new Vector2Int[16] // --------------- SUBWAY
    {
        new Vector2Int(46, 72), // BossRoom1 
        new Vector2Int(58, 62), // 2

        new Vector2Int(40, 20), // EnemyRoom1
        new Vector2Int(20, 20), // 2
        new Vector2Int(38, 30), // 3
        new Vector2Int(38, 50), // 4
        new Vector2Int(38, 28), // 5
        new Vector2Int(26, 46), // 6
        new Vector2Int(38, 28), // 7
        new Vector2Int(38, 58), // 8
        new Vector2Int(68, 36), // 9
        new Vector2Int(30, 36), // 10

        new Vector2Int(30, 32), // ItemRoom1
        new Vector2Int(32, 56), // 2
        new Vector2Int(30, 32), // 3
        new Vector2Int(30, 32)  // 4
    };

    Vector2Int[] hospitalSizeBank = new Vector2Int[16] // --------------- HOSPITAL
    {
        new Vector2Int(0, 0), // BossRoom1 
        new Vector2Int(0, 0), // 2

        new Vector2Int(0, 0), // EnemyRoom1
        new Vector2Int(0, 0), // 2
        new Vector2Int(0, 0), // 3
        new Vector2Int(0, 0), // 4
        new Vector2Int(0, 0), // 5
        new Vector2Int(0, 0), // 6
        new Vector2Int(0, 0), // 7
        new Vector2Int(0, 0), // 8
        new Vector2Int(0, 0), // 9
        new Vector2Int(0, 0), // 10

        new Vector2Int(0, 0), // ItemRoom1
        new Vector2Int(0, 0), // 2
        new Vector2Int(0, 0), // 3
        new Vector2Int(0, 0)  // 4
    };


    List<Vector3Int[]> offsetbank = new List<Vector3Int[]>();

    // Offset banks
    Vector3Int[] hotelOffsetBank = new Vector3Int[16] // --------------- HOTEL
    {
        new Vector3Int(31, -37, 0), // BossRoom1 
        new Vector3Int(28, -35, 0), // 2

        new Vector3Int(5, -5, 0),   // EnemyRoom1
        new Vector3Int(5, -9, 0),   // 2
        new Vector3Int(13, -10, 0), // 3
        new Vector3Int(21, -34, 0), // 4
        new Vector3Int(25, -20, 0), // 5
        new Vector3Int(23, -33, 0), // 6
        new Vector3Int(33, -32, 0), // 7
        new Vector3Int(30, -31, 0), // 8
        new Vector3Int(27, -33, 0), // 9
        new Vector3Int(-20, -45, 0), // 10

        new Vector3Int(33, -41, 0), // ItemRoom1
        new Vector3Int(35, -45, 0), // 2
        new Vector3Int(35, -39, 0), // 3
        new Vector3Int(33, -35, 0)  // 4
    };

    Vector3Int[] subwayOffsetBank = new Vector3Int[16] // --------------- SUBWAY
    {
        new Vector3Int(28, -30, 0), // BossRoom1 
        new Vector3Int(38, -33, 0), // 2

        new Vector3Int(3, -5, 0),   // EnemyRoom1
        new Vector3Int(8, -9, 0),   // 2
        new Vector3Int(2, -9, 0), // 3
        new Vector3Int(2, -10, 0), // 4
        new Vector3Int(2, -9, 0), // 5
        new Vector3Int(-7, -10, 0), // 6
        new Vector3Int(2, -9, 0), // 7
        new Vector3Int(2, -24, 0), // 8
        new Vector3Int(17, -12, 0), // 9
        new Vector3Int(35, -12, 0), // 10

        new Vector3Int(35, -14, 0), // ItemRoom1
        new Vector3Int(35, -25, 0), // 2
        new Vector3Int(35, -14, 0), // 3
        new Vector3Int(35, -14, 0)  // 4
    };

    Vector3Int[] hospitalOffsetBank = new Vector3Int[16] // --------------- HOSPITAL
    {
        new Vector3Int(0, 0, 0), // BossRoom1 
        new Vector3Int(0, 0, 0), // 2

        new Vector3Int(0, 0, 0),   // EnemyRoom1
        new Vector3Int(0, 0, 0),   // 2
        new Vector3Int(0, 0, 0), // 3
        new Vector3Int(0, 0, 0), // 4
        new Vector3Int(0, 0, 0), // 5
        new Vector3Int(0, 0, 0), // 6
        new Vector3Int(0, 0, 0), // 7
        new Vector3Int(0, 0, 0), // 8
        new Vector3Int(0, 0, 0), // 9
        new Vector3Int(0, 0, 0), // 10

        new Vector3Int(0, 0, 0), // ItemRoom1
        new Vector3Int(0, 0, 0), // 2
        new Vector3Int(0, 0, 0), // 3
        new Vector3Int(0, 0, 0)  // 4
    };

    // Quick customization from the Inspector
    [Header("Map Generation Settings")]
    [Space(10)]
    [SerializeField]
    [Range(0, 10)]
    int enemyRooms = 5;
    [Space(10)]
    [SerializeField]
    [Range(0, 70)]
    int transitionRooms = 5;
    [SerializeField]
    [Range(5, 50)]
    int sizeTransitionRooms = 30;
    [SerializeField]
    [Range(0, 20)]
    int variationTranstitionRooms = 5;

    [Space(10)]
    [SerializeField]
    GameObject baseRoomPrefab;
    List<RoomManager> rectangles = new List<RoomManager>();
    Dictionary<int, List<int>> dungeonGraph = new Dictionary<int, List<int>>();
    List<int> importantNodes = new List<int>();
    bool tryAgain = true;
    int attemptNumber = 1;

    // Prefab rooms management
    List<int> prefabRoomIndexes = new List<int>();

    void Awake()
    {
        // Initialize the tilemaps
        hotelTiles.Add(hotelV1Tiles);

        subwayTiles.Add(subwayV1Tiles);
        subwayTiles.Add(subwayV2Tiles);
        subwayTiles.Add(subwayV3Tiles);

        hospitalTiles.Add(hospitalV1Tiles);

        roomTiles.Add(hotelTiles);
        roomTiles.Add(subwayTiles);
        roomTiles.Add(hospitalTiles);

        // Initialize the offset banks
        offsetbank.Add(hotelOffsetBank);
        offsetbank.Add(subwayOffsetBank);
        offsetbank.Add(hospitalOffsetBank);

        // Initialize the size banks
        sizeBank.Add(hotelSizeBank);
        sizeBank.Add(subwaySizeBank);
        sizeBank.Add(hospitalSizeBank);
    }

    void Update()
    {
        // For test purposes: regenerate the map when the Jump button is pressed
        if (Input.GetButtonDown("Jump"))
        {
            ClearMap();
            Debug.ClearDeveloperConsole();

            // Generate a new map
            GenerateMap(currentLevel, currentVariant);
        }
    }

    void ClearMap()
    {
        // Destroy all existing rectangles
        for (int i = 0; i < rectangles.Count; i++)
        {
            Destroy(rectangles[i].gameObject);
        }

        // Clear the lists and dictionary
        rectangles.Clear();
        dungeonGraph.Clear();
        importantNodes.Clear();
        prefabRoomIndexes.Clear();
        tryAgain = true;

        for (int i = 0; i < dungeonTilemap.Count; i++)
        {
            dungeonTilemap[i].ClearAllTiles();
        }

        for (int i = 0; i < prefabRoomsPlaced.Count; i++)
        {
            Destroy(prefabRoomsPlaced[i]);
        }

        for (int i = 0; i < markersPlaced.Count; i++)
        {
            Destroy(markersPlaced[i]);
        }

        markersPlaced.Clear();
    }

    public void GenerateMap(CurrentLevel currentLevel, CurrentVariant currentVariant)
    {
        currentTiles = roomTiles[(int)currentLevel][(int)currentVariant];

        switch (currentLevel)
        {
            case CurrentLevel.Hotel:
                currentLevelRooms = hotelRooms;
                break;

            case CurrentLevel.Subway:
                currentLevelRooms = subwayRooms;
                break;

            case CurrentLevel.Hospital:
                currentLevelRooms = hospitalRooms;
                break;
        }

        while (tryAgain)
        {
            PlaceRooms();

            // Separate overlapping rooms
            bool overlapping = true;
            int overlap_counter = 0;

            while (overlapping)
            {
                overlapping = false;
                overlap_counter += 1;

                // Check for overlapping rectangles and separate them
                for (int i = 0; i < rectangles.Count; i++)
                {
                    for (int j = 0; j < rectangles.Count; j++)
                    {
                        if (i != j)
                        {
                            if (Mathf.Abs(rectangles[i].transform.position.x - rectangles[j].transform.position.x) < (rectangles[i].width / 2 + rectangles[j].width / 2) &&
                                Mathf.Abs(rectangles[i].transform.position.y - rectangles[j].transform.position.y) < (rectangles[i].height / 2 + rectangles[j].height / 2))
                            {
                                SeparateRectangles(rectangles[i], rectangles[j]);
                                overlapping = true;
                            }
                        }
                    }
                }

                // Round the position of the rectangles to avoid floating point errors and make the map look better as a grid-like environment
                for (int i = 0; i < rectangles.Count; i++)
                {
                    rectangles[i].transform.position = new Vector3(Mathf.FloorToInt(rectangles[i].transform.position.x), Mathf.FloorToInt(rectangles[i].transform.position.y), 0);
                }

                // Safety measure to prevent the game from getting stuck on map generation
                if (overlap_counter > 1000)
                {
                    overlapping = false;
                }
            } 

            // Create a graph connecting all the rectangles if they are touching each other
            BuildGraph();

            if (AreAllImportantNodesConnected())
            {
                tryAgain = false;

                // Create a spanning tree from the graph
                ConnectImportantNodes();

                Debug.Log("Custom rooms : " + prefabRoomIndexes.Count);
                Debug.LogWarning("Map generated at attempt n. " + attemptNumber);
                attemptNumber = 1;
            }
            else
            {
                ClearMap();
                attemptNumber += 1;

                if (attemptNumber == 100)
                {
                    Debug.Log("Attempt n. 100 reached");
                    return;
                }
            }
        }

        // Placing tiles 
        PlaceFloorTiles();
        PlaceWallTiles();
        PlaceRoomPrefabs();
        PlacePlayerSpawnPoint();
    }

    void PlaceRooms()
    {
        // Placing 1 boss room and 2 items rooms
        int bossRoomIndex = Random.Range(0, 2);
        prefabRoomIndexes.Add(bossRoomIndex);

        // Picking 2 item rooms
        List<int> possibleindexes = new List<int> { 12, 13, 14, 15 };
        int firstPick = GetRandomAndRemove(possibleindexes);
        int secondPick = GetRandomAndRemove(possibleindexes);
        prefabRoomIndexes.Add(firstPick);
        prefabRoomIndexes.Add(secondPick);

        possibleindexes = new List<int> { 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };

        // Create enemy rooms
        for (int i = 0; i < enemyRooms; i++)
        {
            prefabRoomIndexes.Add(GetRandomAndRemove(possibleindexes));
        }

        for (int i = 0; i < prefabRoomIndexes.Count; i++)
        {
            PlacePrefabRectangle(prefabRoomIndexes[i]);
        }

        // Create middle rooms with random positions and sizes
        for (int i = 0; i < transitionRooms; i++)
        {
            float direction = Random.Range(0, Mathf.PI * 2);
            float distance = i *2;
            float x_pos = Mathf.Cos(direction) *distance;
            float y_pos = Mathf.Sin(direction) *distance;

            AddRectangle(new Vector2(x_pos, y_pos), 
            new Vector2Int(
                sizeTransitionRooms +Random.Range(0, variationTranstitionRooms),
                sizeTransitionRooms +Random.Range(0, variationTranstitionRooms)), 
            RoomType.Passageway);
        }
    }

    int GetRandomAndRemove(List<int> list)
    {
        int index = Random.Range(0, list.Count);
        int value = list[index];
        list.RemoveAt(index);
        return value;
    }

    void PlacePrefabRectangle(int index)
    {
        float direction;
        float distance = 30;

        switch (index)
        { //----------------------------------- HOTEL
            case 0: // BossRoom1
            direction = Mathf.PI / 2;
            break;

            case 1: // 2
            direction = Mathf.PI / 2;            
            break;

            case 12: // ItemRoom1
            direction = Mathf.PI / 2 + Mathf.PI *2 *rectangles.Count /3;            
            break;

            case 13: // 2
            direction = Mathf.PI / 2 + Mathf.PI *2 *rectangles.Count /3;            
            break;

            case 14: // 3
            direction = Mathf.PI / 2 + Mathf.PI *2 *rectangles.Count /3;            
            break;

            case 15: // 4
            direction = Mathf.PI / 2 + Mathf.PI *2 *rectangles.Count /3;            
            break;

            default:
            direction = Random.Range(0, Mathf.PI * 2);
            break;
        }

        
        RoomType roomtype = RoomType.Passageway;

        if (index < 2)
        {
            roomtype = RoomType.Boss;
        }
        else if (index >= 2 && index < 12)
        {
            roomtype = RoomType.Enemy;
        }
        else if (index >= 12)
        {
            roomtype = RoomType.Item;
        }

        float x_pos = Mathf.Cos(direction) *distance;
        float y_pos = Mathf.Sin(direction) *distance;

        AddRectangle(new Vector2(x_pos, y_pos), sizeBank[(int)currentLevel][index], roomtype);
        importantNodes.Add(rectangles.Count -1);
    }

    void PlaceRoomPrefabs()
    {
        for (int i = 0; i < prefabRoomIndexes.Count; i++)
        {
            GameObject tempRoom = Instantiate(currentLevelRooms[prefabRoomIndexes[i]], 
            rectangles[i].transform.position +offsetbank[(int)currentLevel][prefabRoomIndexes[i]], 
            Quaternion.identity);
            tempRoom.transform.parent = transform;
            rectangles[i].roomPrefab = tempRoom;
            prefabRoomsPlaced.Add(tempRoom);
            Tilemap sourceTilemap = tempRoom.GetComponentInChildren<Tilemap>();

            // Get the bounds of the source tilemap
            BoundsInt bounds = sourceTilemap.cellBounds;

            // Loop through each cell inside the bounds
            foreach (var position in bounds.allPositionsWithin)
            {
                // Check if there's a tile at this position in the source tilemap
                if (sourceTilemap.HasTile(position))
                {
                    Vector3Int currentPosition = position +new Vector3Int(
                        (int)rectangles[i].transform.position.x, 
                        (int)rectangles[i].transform.position.y, 
                        0) +offsetbank[(int)currentLevel][prefabRoomIndexes[i]];

                    // Place the tile in the same position in the target tilemap
                    dungeonTilemap[1].SetTile(currentPosition, currentTiles[1]); // Lower wall
                    dungeonTilemap[2].SetTile(currentPosition + Vector3Int.up, currentTiles[2]); // Upper wall
                    dungeonTilemap[3].SetTile(currentPosition + new Vector3Int(0, 2, 0), currentTiles[3]); // Roof
                }
            }

            dungeonTilemap[1].RefreshAllTiles();
            dungeonTilemap[2].RefreshAllTiles();
            dungeonTilemap[3].RefreshAllTiles();

            sourceTilemap.ClearAllTiles();

            // Debug: placing room markers
            /**GameObject markerToPlace = null;

            if (prefabRoomIndexes[i] < 2)
            {
                markerToPlace = roomMarkers[0];
            }
            else if (prefabRoomIndexes[i] >= 2 && prefabRoomIndexes[i] < 12)
            {
                markerToPlace = roomMarkers[2];
            }
            else if (prefabRoomIndexes[i] >= 12)
            {
                markerToPlace = roomMarkers[1];
            }

            if (markerToPlace != null)
            {
                GameObject marker = Instantiate(markerToPlace, 
                rectangles[i].transform.position, 
                Quaternion.identity);
                markersPlaced.Add(marker);
            }**/
        }
    }

    void PlaceFloorTiles()
    {
        HashSet<Vector3Int> floorPositions = new HashSet<Vector3Int>();

        foreach (var rect in rectangles)
        {
            int left = Mathf.FloorToInt(rect.transform.position.x - rect.width / 2);
            int right = Mathf.FloorToInt(rect.transform.position.x + rect.width / 2) - 1;
            int bottom = Mathf.FloorToInt(rect.transform.position.y - rect.height / 2);
            int top = Mathf.FloorToInt(rect.transform.position.y + rect.height / 2) - 1;

            for (int x = left; x <= right; x++)
            {
                for (int y = bottom; y <= top; y++)
                {
                    floorPositions.Add(new Vector3Int(x, y, 0)); // Floor Layer
                }
            }
        }

        // Apply floor tiles to the correct layer
        foreach (var pos in floorPositions)
        {
            dungeonTilemap[0].SetTile(pos, currentTiles[0]); // Floor Layer
        }
    }

    void PlaceWallTiles()
    {
        // Store each room's positions separately
        List<HashSet<Vector3Int>> roomPositionSets = new List<HashSet<Vector3Int>>();
        foreach (var rect in rectangles)
        {
            HashSet<Vector3Int> currentRoomPositions = new HashSet<Vector3Int>();
            int left = Mathf.FloorToInt(rect.transform.position.x - rect.width / 2);
            int right = Mathf.FloorToInt(rect.transform.position.x + rect.width / 2) - 1;
            int bottom = Mathf.FloorToInt(rect.transform.position.y - rect.height / 2);
            int top = Mathf.FloorToInt(rect.transform.position.y + rect.height / 2) - 1;

            for (int x = left; x <= right; x++)
            {
                for (int y = bottom; y <= top; y++)
                {
                    currentRoomPositions.Add(new Vector3Int(x, y, 0));
                }
            }
            roomPositionSets.Add(currentRoomPositions);
        }

        // Combine all room positions into one set
        HashSet<Vector3Int> allRoomPositions = new HashSet<Vector3Int>();
        foreach (var set in roomPositionSets)
            foreach (var pos in set)
                allRoomPositions.Add(pos);

        Vector3Int[] directions = { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right };

        // Step 1: Place walls on each room's inner border
        for (int i = 0; i < roomPositionSets.Count; i++)
        {
            HashSet<Vector3Int> thisRoomPositions = roomPositionSets[i];
            HashSet<Vector3Int> innerBorderPositions = new HashSet<Vector3Int>();

            foreach (var pos in thisRoomPositions)
            {
                foreach (var dir in directions)
                {
                    Vector3Int neighborPos = pos + dir;
                    if (!allRoomPositions.Contains(neighborPos) ||
                        (!thisRoomPositions.Contains(neighborPos) && allRoomPositions.Contains(neighborPos)))
                    {
                        innerBorderPositions.Add(pos);
                        break;
                    }
                }
            }

            foreach (var pos in innerBorderPositions)
            {
                dungeonTilemap[1].SetTile(pos, currentTiles[1]); // Lower wall
                dungeonTilemap[2].SetTile(pos + Vector3Int.up, currentTiles[2]); // Upper wall
                dungeonTilemap[3].SetTile(pos + new Vector3Int(0, 2, 0), currentTiles[3]); // Roof
            }
        }

        // Step 2: Create hallways between adjacent rooms
        for (int i = 0; i < rectangles.Count; i++)
        {
            for (int j = i + 1; j < rectangles.Count; j++)
            {
                if (AreAdjacent(rectangles[i], rectangles[j]))
                {
                    CreateHallwayBetween(rectangles[i], rectangles[j]);
                }
            }
        }

        //PlaceRoofBackgroundTiles(allRoomPositions);
    }

    void CreateHallwayBetween(RoomManager rect1, RoomManager rect2)
    {
        float x1 = rect1.transform.position.x;
        float y1 = rect1.transform.position.y;
        float x2 = rect2.transform.position.x;
        float y2 = rect2.transform.position.y;

        int left1 = Mathf.FloorToInt(x1 - rect1.width / 2);
        int right1 = Mathf.FloorToInt(x1 + rect1.width / 2) - 1;
        int bottom1 = Mathf.FloorToInt(y1 - rect1.height / 2);
        int top1 = Mathf.FloorToInt(y1 + rect1.height / 2) - 1;

        int left2 = Mathf.FloorToInt(x2 - rect2.width / 2);
        int right2 = Mathf.FloorToInt(x2 + rect2.width / 2) - 1;
        int bottom2 = Mathf.FloorToInt(y2 - rect2.height / 2);
        int top2 = Mathf.FloorToInt(y2 + rect2.height / 2) - 1;

        int hallwayWidth = 4;

        if (Mathf.Abs(x1 - x2) > Mathf.Abs(y1 - y2))
        {
            // Horizontal adjacency
            int overlapStart = Mathf.Max(bottom1, bottom2);
            int overlapEnd = Mathf.Min(top1, top2);
            int hallwayStartY = (overlapStart + overlapEnd + 1) / 2 - (hallwayWidth / 2);

            int wallX = (right1 < left2) ? right1 : right2; // border wall line

            for (int y = hallwayStartY; y < hallwayStartY + hallwayWidth; y++)
            {
                ClearTile(new Vector3Int(wallX, y, 0));     
                ClearTile(new Vector3Int(wallX + 1, y, 0)); 
                ClearTile(new Vector3Int(wallX + 2, y, 0)); 
                ClearTile(new Vector3Int(wallX + 3, y, 0)); 
            }
        }
        else
        {
            // Vertical adjacency
            int overlapStart = Mathf.Max(left1, left2);
            int overlapEnd = Mathf.Min(right1, right2);
            int hallwayStartX = (overlapStart + overlapEnd + 1) / 2 - (hallwayWidth / 2);

            int wallY = (top1 < bottom2) ? top1 : top2; // border wall line

            for (int x = hallwayStartX; x < hallwayStartX + hallwayWidth; x++)
            {
                ClearTile(new Vector3Int(x, wallY, 0));     
                ClearTile(new Vector3Int(x, wallY + 1, 0)); 
                ClearTile(new Vector3Int(x, wallY + 2, 0)); 
                ClearTile(new Vector3Int(x, wallY + 3, 0)); 
            }
        }
    }

    void ClearTile(Vector3Int pos)
    {
        // Clear existing tiles
        dungeonTilemap[1].SetTile(pos, null);
        dungeonTilemap[2].SetTile(pos + Vector3Int.up, null);
        dungeonTilemap[3].SetTile(pos + new Vector3Int(0, 2, 0), null);

        // Optionally: place a floor tile (if you have a floor layer, replace below)
        dungeonTilemap[0].SetTile(pos, currentTiles[0]);
    }

    void PlaceRoofBackgroundTiles(HashSet<Vector3Int> roomPositions)
    {
        // Calculate map bounds
        int minX = int.MaxValue, maxX = int.MinValue;
        int minY = int.MaxValue, maxY = int.MinValue;

        foreach (var pos in roomPositions)
        {
            if (pos.x < minX) minX = pos.x;
            if (pos.x > maxX) maxX = pos.x;
            if (pos.y < minY) minY = pos.y;
            if (pos.y > maxY) maxY = pos.y;
        }

        // Add some padding
        minX -= 15; maxX += 15;
        minY -= 15; maxY += 15;

        // Fill outside areas with roof tiles
        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                if (!roomPositions.Contains(pos))
                {
                    dungeonTilemap[3].SetTile(pos + new Vector3Int(0, 2, 0), currentTiles[3]); // place roof/background tile
                }
            }
        }
    }

    void PlacePlayerSpawnPoint()
    {
        RoomManager closestRect = null;
        float closestDistance = float.MaxValue;

        foreach (var rect in rectangles)
        {
            float distance = Vector2.Distance(rect.transform.position, Vector2.zero);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestRect = rect;
            }
        }

        if (closestRect == null)
        {
            Debug.LogWarning("Couldn't find a suitable rectangle to place Player Spawn Point.");
            return;
        }

        GameObject marker = Instantiate(roomMarkers[3], closestRect.transform.position, Quaternion.identity);
        markersPlaced.Add(marker);
    }

    void AddRectangle(Vector2 position, Vector2Int sizeVector, RoomType roomType)
    {
        // Instantiate a new rectangle and set its properties
        RoomManager RoomManager = Instantiate(baseRoomPrefab, new Vector3(position.x, position.y, 0), Quaternion.identity).GetComponent<RoomManager>();
        RoomManager.transform.parent = transform;
        RoomManager.width = sizeVector.x;
        RoomManager.height = sizeVector.y;
        RoomManager.roomType = roomType;
        RoomManager.GetComponent<BoxCollider2D>().size = sizeVector;
        rectangles.Add(RoomManager);        
    }

    void SeparateRectangles(RoomManager rectangle1, RoomManager rectangle2)
    {
        // Calculate the direction to separate the rectangles
        Vector2 direction = new Vector2(
            rectangle1.transform.position.x - rectangle2.transform.position.x,
            rectangle1.transform.position.y - rectangle2.transform.position.y
        ).normalized;

        // Calculate the overlap in the x and y directions
        float overlapX = rectangle1.width / 2 + rectangle2.width / 2 - Mathf.Abs(rectangle1.transform.position.x - rectangle2.transform.position.x);
        float overlapY = rectangle1.height / 2 + rectangle2.height / 2 - Mathf.Abs(rectangle1.transform.position.y - rectangle2.transform.position.y);

        // Separate the rectangles based on the smaller overlap
        if (overlapX < overlapY)
        {
            rectangle1.transform.position += new Vector3(direction.x * overlapX / 2, 0, 0);
            rectangle2.transform.position -= new Vector3(direction.x * overlapX / 2, 0, 0);
        }
        else
        {
            rectangle1.transform.position += new Vector3(0, direction.y * overlapY / 2, 0);
            rectangle2.transform.position -= new Vector3(0, direction.y * overlapY / 2, 0);
        }
    }

    void BuildGraph()
    {
        // Create a graph connecting all the rectangles if they are touching each other
        for (int i = 0; i < rectangles.Count; i++)
        {
            dungeonGraph[i] = new List<int>();

            for (int j = 0; j < rectangles.Count; j++)
            {
                // Check if the rectangles are touching each other
                if (i != j && AreAdjacent(rectangles[i], rectangles[j]))
                {
                    dungeonGraph[i].Add(j);
                }
            }
        }
    }

    bool AreAdjacent(RoomManager rectangle1, RoomManager rectangle2)
    {
        float deltaX = Mathf.Abs(rectangle1.transform.position.x - rectangle2.transform.position.x);
        float deltaY = Mathf.Abs(rectangle1.transform.position.y - rectangle2.transform.position.y);
        
        float halfWidth1 = rectangle1.width / 2;
        float halfWidth2 = rectangle2.width / 2;
        float halfHeight1 = rectangle1.height / 2;
        float halfHeight2 = rectangle2.height / 2;

        // Check if the adjacent segment is at least 6 units long        
        bool horizontallyAdjacent = deltaX == (halfWidth1 + halfWidth2) &&
                                    Mathf.Min(rectangle1.height, rectangle2.height) - deltaY >= 6;
        
        bool verticallyAdjacent = deltaY == (halfHeight1 + halfHeight2) &&
                                Mathf.Min(rectangle1.width, rectangle2.width) - deltaX >= 6;
        
        return horizontallyAdjacent || verticallyAdjacent;
    }

    private void ConnectImportantNodes()
    {
        var connectedPaths = new Dictionary<int, List<int>>();

        // Connect all important nodes using paths found in the graph
        foreach (int startNode in importantNodes)
        {
            foreach (int endNode in importantNodes)
            {
                if (startNode != endNode)
                {
                    List<int> path = FindPath(startNode, endNode);

                    if (path != null)
                    {
                        foreach (int node in path)
                        {
                            if (!connectedPaths.ContainsKey(node))
                            {
                                connectedPaths[node] = new List<int>();
                            }

                            foreach (int neighbor in path)
                            {
                                if (node != neighbor && !connectedPaths[node].Contains(neighbor))
                                {
                                    connectedPaths[node].Add(neighbor);
                                }
                            }
                        }
                    }
                }
            }
        }

        // Remove rectangles that are not in connectedPaths
        for (int i = rectangles.Count - 1; i >= 0; i--)
        {
            if (!connectedPaths.ContainsKey(i))
            {
                Destroy(rectangles[i].gameObject);
                rectangles.RemoveAt(i);
            }
        }
    }

    private List<int> FindPath(int startNode, int endNode)
    {
        var queue = new Queue<int>();
        var cameFrom = new Dictionary<int, int>();
        var visited = new HashSet<int>();

        queue.Enqueue(startNode);
        visited.Add(startNode);

        // Perform BFS to find the shortest path between startNode and endNode
        while (queue.Count > 0)
        {
            int current = queue.Dequeue();

            if (current == endNode)
            {
                return ReconstructPath(cameFrom, startNode, endNode);
            }

            foreach (int neighbor in dungeonGraph[current])
            {
                if (!visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    queue.Enqueue(neighbor);
                    cameFrom[neighbor] = current;
                }
            }
        }

        return null; // No path found
    }

    private List<int> ReconstructPath(Dictionary<int, int> cameFrom, int startNode, int endNode)
    {
        var path = new List<int>();
        int current = endNode;

        // Reconstruct the path from endNode to startNode
        while (current != startNode)
        {
            path.Add(current);
            current = cameFrom[current];
        }
        path.Add(startNode);
        path.Reverse();

        return path;
    }

    bool AreAllImportantNodesConnected()
    {
        if (importantNodes.Count == 0) return true;

        HashSet<int> visited = new HashSet<int>();
        Queue<int> queue = new Queue<int>();

        // Start BFS from the first important node
        queue.Enqueue(importantNodes[0]);
        visited.Add(importantNodes[0]);

        while (queue.Count > 0)
        {
            int current = queue.Dequeue();
            foreach (int neighbor in dungeonGraph[current])
            {
                if (!visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }
        }

        // Check if all important nodes are visited
        return importantNodes.All(node => visited.Contains(node));
    }
}
