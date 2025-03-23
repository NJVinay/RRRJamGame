

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

public class MapGenerationScript : MonoBehaviour
{
    // Erir's recommendations:
    // - Player room in the center with 2 weapon spawn markers
    // - Just one marker per door
    // - Doors for enemies and item rooms OK

    [Header("Tilemaps & Markers")]
    public List<Tilemap> dungeonTilemap = new List<Tilemap>();
    public List<GameObject> roomMarkers = new List<GameObject>(); // 0: player, 1: door, 2: item
    private List<GameObject> markersPlaced = new List<GameObject>();
    private List<GameObject> prefabRoomsPlaced = new List<GameObject>();

    [Header("Room Tile Data")]
    private List<List<TileBase>> roomTiles = new List<List<TileBase>>();

    [Header("Hotel")]
    public List<GameObject> hotelRooms = new List<GameObject>();
    public List<TileBase> hotelTiles = new List<TileBase>();

    [Header("Subway")]
    public List<GameObject> subwayRooms = new List<GameObject>();
    public List<TileBase> subwayTiles = new List<TileBase>();

    [Header("Hospital")]
    public List<GameObject> hospitalRooms = new List<GameObject>();
    public List<TileBase> hospitalTiles = new List<TileBase>();

    [Header("Current Level Data")]
    public CurrentLevel currentLevel = CurrentLevel.Hotel;
    private List<TileBase> currentTiles = new List<TileBase>();
    private List<GameObject> currentLevelRooms = new List<GameObject>();

    [Header("Size Banks")]
    private List<Vector2Int[]> sizeBank = new List<Vector2Int[]>();

    private Vector2Int[] hotelSizeBank = new Vector2Int[]
    {
        new Vector2Int(24, 22), new Vector2Int(30, 22),
        new Vector2Int(20, 20), new Vector2Int(22, 18), 
        new Vector2Int(28, 22), new Vector2Int(54, 20),
        new Vector2Int(40, 38), new Vector2Int(54, 30), 
        new Vector2Int(22, 15), new Vector2Int(28, 16),
        new Vector2Int(34, 22), new Vector2Int(20, 14),
        new Vector2Int(18, 14), new Vector2Int(18, 10), 
        new Vector2Int(18, 14), new Vector2Int(18, 10)
    };

    private Vector2Int[] subwaySizeBank = new Vector2Int[]
    {
        new Vector2Int(46, 72), new Vector2Int(58, 62),
        new Vector2Int(40, 20), new Vector2Int(20, 20), 
        new Vector2Int(38, 30), new Vector2Int(38, 50),
        new Vector2Int(38, 28), new Vector2Int(26, 46), 
        new Vector2Int(38, 28), new Vector2Int(38, 58),
        new Vector2Int(68, 36), new Vector2Int(30, 36),
        new Vector2Int(30, 32), new Vector2Int(32, 56), 
        new Vector2Int(30, 32), new Vector2Int(30, 32)
    };

    private Vector2Int[] hospitalSizeBank = new Vector2Int[16]
    {
        new Vector2Int(0, 0), new Vector2Int(0, 0),
        new Vector2Int(0, 0), new Vector2Int(0, 0), 
        new Vector2Int(0, 0), new Vector2Int(0, 0),
        new Vector2Int(0, 0), new Vector2Int(0, 0), 
        new Vector2Int(0, 0), new Vector2Int(0, 0),
        new Vector2Int(0, 0), new Vector2Int(0, 0),
        new Vector2Int(0, 0), new Vector2Int(0, 0), 
        new Vector2Int(0, 0), new Vector2Int(0, 0)
    };

    [Header("Offset Banks")]
    private List<Vector3Int[]> offsetbank = new List<Vector3Int[]>();

    private Vector3Int[] hotelOffsetBank = new Vector3Int[]
    {
        new Vector3Int(31, -37, 0), new Vector3Int(28, -35, 0),
        new Vector3Int(5, -5, 0), new Vector3Int(5, -9, 0), 
        new Vector3Int(10, -10, 0), new Vector3Int(21, -34, 0),
        new Vector3Int(25, -20, 0), new Vector3Int(23, -33, 0), 
        new Vector3Int(33, -32, 0), new Vector3Int(30, -31, 0),
        new Vector3Int(27, -33, 0), new Vector3Int(33, -43, 0),
        new Vector3Int(33, -41, 0), new Vector3Int(35, -45, 0), 
        new Vector3Int(35, -39, 0), new Vector3Int(33, -35, 0)
    };

    private Vector3Int[] subwayOffsetBank = new Vector3Int[]
    {
        new Vector3Int(28, -30, 0), new Vector3Int(38, -33, 0),
        new Vector3Int(3, -5, 0), new Vector3Int(8, -9, 0), 
        new Vector3Int(2, -9, 0), new Vector3Int(2, -10, 0),
        new Vector3Int(2, -9, 0), new Vector3Int(-7, -10, 0), 
        new Vector3Int(2, -9, 0), new Vector3Int(2, -24, 0),
        new Vector3Int(17, -12, 0), new Vector3Int(35, -12, 0),
        new Vector3Int(35, -14, 0), new Vector3Int(35, -25, 0), 
        new Vector3Int(35, -14, 0), new Vector3Int(35, -14, 0)
    };

    private Vector3Int[] hospitalOffsetBank = new Vector3Int[16]
    {
        new Vector3Int(0, 0, 0), 
        new Vector3Int(0, 0, 0),
        new Vector3Int(0, 0, 0), 
        new Vector3Int(0, 0, 0), 
        new Vector3Int(0, 0, 0), 
        new Vector3Int(0, 0, 0),
        new Vector3Int(0, 0, 0), 
        new Vector3Int(0, 0, 0), 
        new Vector3Int(0, 0, 0), 
        new Vector3Int(0, 0, 0),
        new Vector3Int(0, 0, 0), 
        new Vector3Int(0, 0, 0),
        new Vector3Int(0, 0, 0), 
        new Vector3Int(0, 0, 0), 
        new Vector3Int(0, 0, 0), 
        new Vector3Int(0, 0, 0)
    };

    [Header("Map Generation Settings")]
    [Range(0, 10)] public int enemyRooms = 5;
    [Range(0, 70)] public int transitionRooms = 5;
    [Range(5, 50)] public int sizeTransitionRooms = 30;
    [Range(0, 20)] public int variationTranstitionRooms = 5;

    [Space(10)]
    [SerializeField] private GameObject baseRoomPrefab;

    // Room generation tracking
    private List<RoomManager> rectangles = new List<RoomManager>();
    private Dictionary<int, List<int>> dungeonGraph = new Dictionary<int, List<int>>();
    private List<int> importantNodes = new List<int>();
    private int attemptNumber = 1;

    // Prefab room indexes
    private List<int> prefabRoomIndexes = new List<int>();

    void Awake()
    {
        roomTiles.AddRange(new List<List<TileBase>> { hotelTiles, subwayTiles, hospitalTiles });

        offsetbank.AddRange(new[]
        {
            hotelOffsetBank,
            subwayOffsetBank,
            hospitalOffsetBank
        });
        
        sizeBank.AddRange(new[]
        {
            hotelSizeBank,
            subwaySizeBank,
            hospitalSizeBank
        });
    }

    void Update()
    {
        if (Input.GetButtonDown("Jump"))
        {
            ClearMap();
            Debug.ClearDeveloperConsole();
            GenerateMap(currentLevel);
        }
    }

    void ClearMap()
    {
        ClearAndDestroy(rectangles.Select(r => r.gameObject));
        rectangles.Clear();

        dungeonGraph.Clear();
        importantNodes.Clear();
        prefabRoomIndexes.Clear();

        dungeonTilemap.ForEach(tilemap => tilemap.ClearAllTiles());

        ClearAndDestroy(prefabRoomsPlaced);
        ClearAndDestroy(markersPlaced);
    }

    private void ClearAndDestroy(IEnumerable<GameObject> objects)
    {
        foreach (var obj in objects)
        {
            Destroy(obj);
        }
    }

    public void GenerateMap(CurrentLevel currentLevel)
    {
        currentTiles = roomTiles[(int)currentLevel];
        currentLevelRooms = currentLevel switch
        {
            CurrentLevel.Hotel => hotelRooms,
            CurrentLevel.Subway => subwayRooms,
            CurrentLevel.Hospital => hospitalRooms,
            _ => throw new System.ArgumentOutOfRangeException(nameof(currentLevel), currentLevel, null)
        };

        for (attemptNumber = 1; attemptNumber <= 100; attemptNumber++)
        {
            PlaceRooms();
            if (!SeparateOverlappingRooms()) continue;

            BuildGraph();

            if (AreAllImportantNodesConnected())
            {
                ConnectImportantNodes(); // This is now in the correct place

                PlaceFloorTiles();
                PlaceWallTiles();

                if (true) //IsWallLoopClosed()
                {
                    PlaceRoomPrefabs();
                    PlacePlayerSpawnPoint();
                    Debug.LogWarning($"Map generated at attempt n. {attemptNumber}");
                    return;
                }
            }

            ClearMap(); // Moved outside the inner if to ensure clearing even if nodes aren't connected
        }

        Debug.Log("Attempt n. 100 reached");
    }

    private bool SeparateOverlappingRooms()
    {
        for (int overlap_counter = 0; overlap_counter < 1000; overlap_counter++)
        {
            bool overlapping = false;

            // Cache rectangle positions and sizes
            var cachedPositions = rectangles.Select(r => r.transform.position).ToList();
            var cachedWidths = rectangles.Select(r => r.width / 2).ToList();
            var cachedHeights = rectangles.Select(r => r.height / 2).ToList();

            for (int i = 0; i < rectangles.Count; i++)
            {
                for (int j = i + 1; j < rectangles.Count; j++) // Start j from i + 1 to avoid redundant checks
                {
                    if (Mathf.Abs(cachedPositions[i].x - cachedPositions[j].x) < (cachedWidths[i] + cachedWidths[j]) &&
                        Mathf.Abs(cachedPositions[i].y - cachedPositions[j].y) < (cachedHeights[i] + cachedHeights[j]))
                    {
                        SeparateRectangles(rectangles[i], rectangles[j]);
                        overlapping = true;
                    }
                }
            }

            for (int i = 0; i < rectangles.Count; i++)
            {
                rectangles[i].transform.position = new Vector3(
                    Mathf.FloorToInt(rectangles[i].transform.position.x),
                    Mathf.FloorToInt(rectangles[i].transform.position.y),
                    0
                );
            }

            if (!overlapping) return true; // No more overlaps
        }

        return false; // Overlap separation failed after 1000 attempts
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
                    CreateHallwayBetween(rectangles[i], rectangles[j],
                    rectangles[i].roomType != RoomType.Passageway || rectangles[j].roomType != RoomType.Passageway);
                }
            }
        }

        PlaceRoofBackgroundTiles(allRoomPositions);
    }

    void CreateHallwayBetween(RoomManager rect1, RoomManager rect2, bool placeDoor)
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

        Vector3 doorPosition = Vector3.zero;
        bool isHorizontal = true;

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

            // Place door marker at center of the carved hallway
            doorPosition = new Vector3(wallX + 1f, hallwayStartY + (hallwayWidth / 2f), 0f);
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

            // Place door marker at center of the carved hallway
            doorPosition = new Vector3(hallwayStartX + (hallwayWidth / 2f), wallY + 1f, 0f);
            isHorizontal = false;
        }

        if (doorPosition != Vector3.zero && placeDoor)
        {
            GameObject marker = Instantiate(roomMarkers[1], doorPosition, Quaternion.identity);
            DoorManager doorManager = marker.GetComponent<DoorManager>();
            doorManager.neighbouringRooms.Add(rect1);
            doorManager.neighbouringRooms.Add(rect2);
            doorManager.isHorizontal = isHorizontal;
            markersPlaced.Add(marker);
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

        GameObject marker = Instantiate(roomMarkers[0], closestRect.transform.position, Quaternion.identity);
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

    bool IsWallLoopClosed()
    {
        HashSet<Vector3Int> wallPositions = new HashSet<Vector3Int>();

        // Step 1: Collect all wall tile positions
        foreach (var pos in dungeonTilemap[1].cellBounds.allPositionsWithin)
        {
            if (dungeonTilemap[1].HasTile(pos))
            {
                wallPositions.Add(pos);
            }
        }

        if (wallPositions.Count == 0)
        {
            Debug.LogWarning("No wall tiles found.");
            return false;
        }

        // Step 2: Start DFS from any wall tile
        Stack<Vector3Int> stack = new Stack<Vector3Int>();
        HashSet<Vector3Int> visited = new HashSet<Vector3Int>();
        Vector3Int start = wallPositions.First();
        stack.Push(start);

        Vector3Int[] directions = new Vector3Int[]
        {
            Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right
        };

        while (stack.Count > 0)
        {
            Vector3Int current = stack.Pop();
            if (!visited.Add(current))
                continue;

            // Count how many neighbors are wall tiles
            int wallNeighborCount = 0;
            foreach (var dir in directions)
            {
                Vector3Int neighbor = current + dir;
                if (wallPositions.Contains(neighbor))
                {
                    wallNeighborCount++;
                    if (!visited.Contains(neighbor))
                        stack.Push(neighbor);
                }
            }

            // If a wall tile has less than 2 neighbors, it’s likely an opening
            if (wallNeighborCount < 2)
            {
                return false;
            }
        }

        // Step 3: Check if all wall positions were visited (no isolated fragments)
        if (visited.Count != wallPositions.Count)
        {
            return false;
        }

        return true;
    }
}