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
    private HashSet<Vector3Int> floorPositions;

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

                if (AreWallsClosed(dungeonTilemap[1], floorPositions))
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
        // Place boss room
        prefabRoomIndexes.Add(Random.Range(0, 2));

        // Place 2 item rooms from fixed pool
        AddRandomRoomsFromPool(new List<int> { 12, 13, 14, 15 }, 2);

        // Place enemy rooms from larger pool
        AddRandomRoomsFromPool(new List<int> { 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, enemyRooms);

        // Instantiate all chosen room prefabs
        foreach (int index in prefabRoomIndexes)
        {
            PlacePrefabRectangle(index);
        }

        // Player spawn room in center
        AddRectangle(Vector2.zero, new Vector2Int(14, 14), RoomType.Passageway);
        importantNodes.Add(rectangles.Count - 1);

        // Place transition rooms around center in a spiral-like pattern
        for (int i = 0; i < transitionRooms; i++)
        {
            float angle = Random.Range(0, Mathf.PI * 2);
            float radius = i * 2f;
            Vector2 position = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;

            var roomSize = new Vector2Int(
                sizeTransitionRooms + Random.Range(0, variationTranstitionRooms),
                sizeTransitionRooms + Random.Range(0, variationTranstitionRooms)
            );

            AddRectangle(position, roomSize, RoomType.Passageway);
        }
    }

    void AddRandomRoomsFromPool(List<int> pool, int count)
    {
        for (int i = 0; i < count; i++)
        {
            int index = Random.Range(0, pool.Count);
            prefabRoomIndexes.Add(pool[index]);
            pool.RemoveAt(index);
        }
    }

    void PlacePrefabRectangle(int index)
    {
        float halfPi = Mathf.PI / 2f;
        float twoPi = Mathf.PI * 2f;
        float angleOffset = halfPi + twoPi * rectangles.Count / 3f;
        float distance = 30f;

        float direction = index switch
        {
            0 or 1 => halfPi,               // Boss room
            >= 12 and <= 15 => angleOffset, // Item rooms
            _ => Random.Range(0f, twoPi)    // Enemy rooms
        };

        RoomType roomType = index switch
        {
            < 2 => RoomType.Boss,
            < 12 => RoomType.Enemy,
            _ => RoomType.Item
        };

        Vector2 position = new Vector2(Mathf.Cos(direction), Mathf.Sin(direction)) * distance;

        AddRectangle(position, sizeBank[(int)currentLevel][index], roomType);
        importantNodes.Add(rectangles.Count - 1);
    }

    void PlaceRoomPrefabs()
    {
        var levelOffsetBank = offsetbank[(int)currentLevel];
        int count = prefabRoomIndexes.Count;

        for (int i = 0; i < count; i++)
        {
            int roomIndex = prefabRoomIndexes[i];
            var rect = rectangles[i];
            Vector3 positionOffset = rect.transform.position + levelOffsetBank[roomIndex];

            GameObject tempRoom = Instantiate(currentLevelRooms[roomIndex], positionOffset, Quaternion.identity, transform);
            rect.roomPrefab = tempRoom;
            prefabRoomsPlaced.Add(tempRoom);

            Tilemap sourceTilemap = tempRoom.GetComponentInChildren<Tilemap>();
            Vector3Int roomOffset = new Vector3Int((int)rect.transform.position.x, (int)rect.transform.position.y, 0) 
                                    + Vector3Int.FloorToInt(levelOffsetBank[roomIndex]);

            BoundsInt bounds = sourceTilemap.cellBounds;
            var lowerWallTilemap = dungeonTilemap[1];
            var upperWallTilemap = dungeonTilemap[2];
            var roofTilemap = dungeonTilemap[3];

            foreach (var position in bounds.allPositionsWithin)
            {
                if (sourceTilemap.HasTile(position))
                {
                    Vector3Int tilePos = position + roomOffset;
                    lowerWallTilemap.SetTile(tilePos, currentTiles[1]);
                    upperWallTilemap.SetTile(tilePos + Vector3Int.up, currentTiles[2]);
                    roofTilemap.SetTile(tilePos + new Vector3Int(0, 2, 0), currentTiles[3]);
                }
            }

            sourceTilemap.ClearAllTiles();
        }

        dungeonTilemap[1].RefreshAllTiles();
        dungeonTilemap[2].RefreshAllTiles();
        dungeonTilemap[3].RefreshAllTiles();
    }

    void PlaceFloorTiles()
    {
        floorPositions = new HashSet<Vector3Int>();
        Vector3Int tilePosition = new Vector3Int(0, 0, 0);

        foreach (var rect in rectangles)
        {
            Vector3 rectPos = rect.transform.position;
            int left = Mathf.FloorToInt(rectPos.x - rect.width * 0.5f);
            int right = Mathf.FloorToInt(rectPos.x + rect.width * 0.5f) - 1;
            int bottom = Mathf.FloorToInt(rectPos.y - rect.height * 0.5f);
            int top = Mathf.FloorToInt(rectPos.y + rect.height * 0.5f) - 1;

            for (int x = left; x <= right; x++)
            {
                tilePosition.x = x;
                for (int y = bottom; y <= top; y++)
                {
                    tilePosition.y = y;
                    floorPositions.Add(tilePosition);
                }
            }
        }

        var tilemap = dungeonTilemap[0];
        var floorTile = currentTiles[0];

        foreach (var pos in floorPositions)
        {
            tilemap.SetTile(pos, floorTile);
        }
    }

    void PlaceWallTiles()
    {
        HashSet<Vector3Int> allRoomPositions = new HashSet<Vector3Int>();
        List<HashSet<Vector3Int>> roomPositionSets = new List<HashSet<Vector3Int>>(rectangles.Count);

        // Precompute room positions and add to global set
        foreach (var rect in rectangles)
        {
            HashSet<Vector3Int> currentRoomPositions = new HashSet<Vector3Int>();
            Vector3 rectPos = rect.transform.position;
            int left = Mathf.FloorToInt(rectPos.x - rect.width * 0.5f);
            int right = Mathf.FloorToInt(rectPos.x + rect.width * 0.5f) - 1;
            int bottom = Mathf.FloorToInt(rectPos.y - rect.height * 0.5f);
            int top = Mathf.FloorToInt(rectPos.y + rect.height * 0.5f) - 1;

            for (int x = left; x <= right; x++)
            {
                for (int y = bottom; y <= top; y++)
                {
                    Vector3Int pos = new Vector3Int(x, y, 0);
                    currentRoomPositions.Add(pos);
                    allRoomPositions.Add(pos);
                }
            }
            roomPositionSets.Add(currentRoomPositions);
        }

        Vector3Int[] directions = { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right };

        // Step 1: Place wall and roof tiles along inner borders
        for (int i = 0; i < roomPositionSets.Count; i++)
        {
            HashSet<Vector3Int> roomPositions = roomPositionSets[i];
            foreach (var pos in roomPositions)
            {
                foreach (var dir in directions)
                {
                    Vector3Int neighborPos = pos + dir;
                    if (!roomPositions.Contains(neighborPos)) // Only check local borders
                    {
                        dungeonTilemap[1].SetTile(pos, currentTiles[1]); // Wall lower
                        dungeonTilemap[2].SetTile(pos + Vector3Int.up, currentTiles[2]); // Wall upper
                        dungeonTilemap[3].SetTile(pos + new Vector3Int(0, 2, 0), currentTiles[3]); // Roof
                        break;
                    }
                }
            }
        }

        // Step 2: Create hallways between adjacent rooms
        int rectCount = rectangles.Count;
        for (int i = 0; i < rectCount; i++)
        {
            var rectA = rectangles[i];
            for (int j = i + 1; j < rectCount; j++)
            {
                var rectB = rectangles[j];
                if (AreAdjacent(
                    (int)rectA.transform.position.x, (int)rectA.transform.position.y,
                    rectA.width / 2, rectA.height / 2, rectA.width, rectA.height,
                    (int)rectB.transform.position.x, (int)rectB.transform.position.y,
                    rectB.width / 2, rectB.height / 2, rectB.width, rectB.height))
                {
                    CreateHallwayBetween(rectA, rectB,
                        rectA.roomType != RoomType.Passageway || rectB.roomType != RoomType.Passageway);
                }
            }
        }

        PlaceRoofBackgroundTiles(allRoomPositions);
    }

    void CreateHallwayBetween(RoomManager rect1, RoomManager rect2, bool placeDoor)
    {
        Vector2 pos1 = rect1.transform.position;
        Vector2 pos2 = rect2.transform.position;

        int left1 = Mathf.FloorToInt(pos1.x - rect1.width / 2);
        int right1 = Mathf.FloorToInt(pos1.x + rect1.width / 2) - 1;
        int bottom1 = Mathf.FloorToInt(pos1.y - rect1.height / 2);
        int top1 = Mathf.FloorToInt(pos1.y + rect1.height / 2) - 1;

        int left2 = Mathf.FloorToInt(pos2.x - rect2.width / 2);
        int right2 = Mathf.FloorToInt(pos2.x + rect2.width / 2) - 1;
        int bottom2 = Mathf.FloorToInt(pos2.y - rect2.height / 2);
        int top2 = Mathf.FloorToInt(pos2.y + rect2.height / 2) - 1;

        int hallwayWidth = 4;
        Vector3 doorPosition = Vector3.zero;
        bool isHorizontal = true;

        void ClearHallwayTiles(int fixedCoord, int startCoord, bool horizontal)
        {
            for (int offset = 0; offset < hallwayWidth; offset++)
            {
                for (int lineOffset = 0; lineOffset < hallwayWidth; lineOffset++)
                {
                    Vector3Int tilePos = horizontal
                        ? new Vector3Int(fixedCoord + lineOffset, startCoord + offset, 0)
                        : new Vector3Int(startCoord + offset, fixedCoord + lineOffset, 0);

                    ClearTile(tilePos);
                }
            }
        }

        if (Mathf.Abs(pos1.x - pos2.x) > Mathf.Abs(pos1.y - pos2.y))
        {
            // Horizontal hallway
            int overlapStart = Mathf.Max(bottom1, bottom2);
            int overlapEnd = Mathf.Min(top1, top2);
            int hallwayStartY = (overlapStart + overlapEnd + 1) / 2 - (hallwayWidth / 2);
            int wallX = (right1 < left2) ? right1 : right2;

            ClearHallwayTiles(wallX, hallwayStartY, true);
            doorPosition = new Vector3(wallX + 1f, hallwayStartY + hallwayWidth / 2f, 0f);
        }
        else
        {
            // Vertical hallway
            int overlapStart = Mathf.Max(left1, left2);
            int overlapEnd = Mathf.Min(right1, right2);
            int hallwayStartX = (overlapStart + overlapEnd + 1) / 2 - (hallwayWidth / 2);
            int wallY = (top1 < bottom2) ? top1 : top2;

            ClearHallwayTiles(wallY, hallwayStartX, false);
            doorPosition = new Vector3(hallwayStartX + hallwayWidth / 2f, wallY + 1f, 0f);
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

    private static readonly Vector3Int OffsetDoubleUp = new Vector3Int(0, 2, 0);

    void ClearTile(Vector3Int pos)
    {
        var roofLayer = dungeonTilemap[3];
        var midRoofLayer = dungeonTilemap[2];
        var wallLayer = dungeonTilemap[1];
        var floorLayer = dungeonTilemap[0];

        wallLayer.SetTile(pos, null);
        midRoofLayer.SetTile(pos + Vector3Int.up, null);
        roofLayer.SetTile(pos + OffsetDoubleUp, null);

        floorLayer.SetTile(pos, currentTiles[0]);
    }

    void PlaceRoofBackgroundTiles(HashSet<Vector3Int> roomPositions)
    {
        if (roomPositions == null || roomPositions.Count == 0) return;

        int minX = int.MaxValue, maxX = int.MinValue;
        int minY = int.MaxValue, maxY = int.MinValue;

        foreach (var pos in roomPositions)
        {
            if (pos.x < minX) minX = pos.x;
            if (pos.x > maxX) maxX = pos.x;
            if (pos.y < minY) minY = pos.y;
            if (pos.y > maxY) maxY = pos.y;
        }

        BoundsInt bounds = new BoundsInt(minX - 15, minY - 15, 0, (maxX - minX) + 31, (maxY - minY) + 31, 1);
        var roofLayer = dungeonTilemap[3];
        var roofTile = currentTiles[3];
        Vector3Int offset = OffsetDoubleUp;

        foreach (var pos in bounds.allPositionsWithin)
        {
            if (!roomPositions.Contains(pos))
            {
                roofLayer.SetTile(pos + offset, roofTile);
            }
        }
    }

    void PlacePlayerSpawnPoint()
    {
        Vector3 centerPosition = rectangles[prefabRoomIndexes.Count].transform.position;
        Quaternion identity = Quaternion.identity;

        // Player spawn marker (center offset down)
        markersPlaced.Add(Instantiate(roomMarkers[0], centerPosition + Vector3.down * 3, identity));

        float radius = 3f;
        float[] angles = { Mathf.PI / 6f, 5f * Mathf.PI / 6f };

        foreach (float angle in angles)
        {
            Vector3 offset = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f);
            markersPlaced.Add(Instantiate(roomMarkers[2], centerPosition + offset, identity));
        }
    }

    void AddRectangle(Vector2 position, Vector2Int sizeVector, RoomType roomType)
    {
        var roomGO = Instantiate(baseRoomPrefab, new Vector3(position.x, position.y, 0f), Quaternion.identity, transform);
        var roomManager = roomGO.GetComponent<RoomManager>();
        
        roomManager.width = sizeVector.x;
        roomManager.height = sizeVector.y;
        roomManager.roomType = roomType;

        var collider = roomManager.GetComponent<BoxCollider2D>();
        if (collider != null)
        {
            collider.size = sizeVector;
        }

        rectangles.Add(roomManager);
    }

    void SeparateRectangles(RoomManager rectangle1, RoomManager rectangle2)
    {
        Vector2 pos1 = rectangle1.transform.position;
        Vector2 pos2 = rectangle2.transform.position;
        Vector2 delta = pos1 - pos2;

        float overlapX = rectangle1.width / 2f + rectangle2.width / 2f - Mathf.Abs(delta.x);
        float overlapY = rectangle1.height / 2f + rectangle2.height / 2f - Mathf.Abs(delta.y);

        // Fallback direction if exactly overlapping
        if (delta == Vector2.zero)
        {
            delta = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
        }

        if (overlapX < overlapY)
        {
            float moveX = overlapX / 2f * Mathf.Sign(delta.x);
            rectangle1.transform.position += new Vector3(moveX, 0f, 0f);
            rectangle2.transform.position -= new Vector3(moveX, 0f, 0f);
        }
        else
        {
            float moveY = overlapY / 2f * Mathf.Sign(delta.y);
            rectangle1.transform.position += new Vector3(0f, moveY, 0f);
            rectangle2.transform.position -= new Vector3(0f, moveY, 0f);
        }
    }

    void BuildGraph()
    {
        int count = rectangles.Count;
        for (int i = 0; i < count; i++)
        {
            dungeonGraph[i] = new List<int>();

            var rect1 = rectangles[i];
            int x1 = (int)rect1.transform.position.x;
            int y1 = (int)rect1.transform.position.y;
            int halfWidth1 = rect1.width >> 1;
            int halfHeight1 = rect1.height >> 1;
            int height1 = rect1.height;
            int width1 = rect1.width;

            for (int j = 0; j < count; j++)
            {
                if (i == j) continue;

                var rect2 = rectangles[j];
                int x2 = (int)rect2.transform.position.x;
                int y2 = (int)rect2.transform.position.y;

                if (AreAdjacent(
                        x1, y1, halfWidth1, halfHeight1, width1, height1,
                        x2, y2, rect2.width >> 1, rect2.height >> 1, rect2.width, rect2.height))
                {
                    dungeonGraph[i].Add(j);
                }
            }
        }
    }

    bool AreAdjacent(int x1, int y1, int halfWidth1, int halfHeight1, int width1, int height1,
                    int x2, int y2, int halfWidth2, int halfHeight2, int width2, int height2)
    {
        int deltaX = System.Math.Abs(x1 - x2);
        int deltaY = System.Math.Abs(y1 - y2);

        int minHeight = (height1 < height2) ? height1 : height2;
        int minWidth = (width1 < width2) ? width1 : width2;

        return (deltaX == (halfWidth1 + halfWidth2) && minHeight - deltaY >= 6) ||
            (deltaY == (halfHeight1 + halfHeight2) && minWidth - deltaX >= 6);
    }

    private void ConnectImportantNodes()
    {
        var connectedPaths = new Dictionary<int, HashSet<int>>();

        // Connect all important nodes using paths found in the graph
        foreach (int startNode in importantNodes)
        {
            foreach (int endNode in importantNodes)
            {
                if (startNode == endNode) continue;

                List<int> path = FindPath(startNode, endNode);
                if (path == null) continue;

                // Add all-to-all connections within the path
                int pathCount = path.Count;
                for (int i = 0; i < pathCount; i++)
                {
                    int node = path[i];
                    if (!connectedPaths.TryGetValue(node, out var neighbors))
                    {
                        neighbors = new HashSet<int>();
                        connectedPaths[node] = neighbors;
                    }

                    for (int j = 0; j < pathCount; j++)
                    {
                        if (i != j)
                            neighbors.Add(path[j]);
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
        var cameFrom = new Dictionary<int, int>(dungeonGraph.Count);
        var visited = new HashSet<int>();

        queue.Enqueue(startNode);
        visited.Add(startNode);

        // BFS for shortest path
        while (queue.Count > 0)
        {
            int current = queue.Dequeue();

            if (current == endNode)
            {
                return ReconstructPath(cameFrom, startNode, endNode);
            }

            foreach (int neighbor in dungeonGraph[current])
            {
                // Add() returns true if added, false if already present — faster check
                if (visited.Add(neighbor))
                {
                    queue.Enqueue(neighbor);
                    cameFrom[neighbor] = current;
                }
            }
        }

        return null; // No path found
    }

    private List<int> ReconstructPath(Dictionary<int, int> cameFrom, int startNode, int endNode)
    {
        var path = new List<int>(cameFrom.Count + 1);
        int current = endNode;

        // Reconstruct from endNode to startNode
        while (current != startNode)
        {
            path.Add(current);
            current = cameFrom[current];
        }
        path.Add(startNode);
        path.Reverse();

        return path;
    }

    private bool AreAllImportantNodesConnected()
    {
        int count = importantNodes.Count;
        if (count == 0) return true;

        var visited = new HashSet<int>();
        var queue = new Queue<int>();

        int start = importantNodes[0];
        queue.Enqueue(start);
        visited.Add(start);

        while (queue.Count > 0)
        {
            int current = queue.Dequeue();
            foreach (int neighbor in dungeonGraph[current])
            {
                if (visited.Add(neighbor))
                {
                    queue.Enqueue(neighbor);
                }
            }
        }

        // Check if all important nodes are visited using a simple loop
        for (int i = 0; i < count; i++)
        {
            if (!visited.Contains(importantNodes[i]))
                return false;
        }

        return true;
    }

    bool AreWallsClosed(Tilemap wallTilemap, HashSet<Vector3Int> floorPositions, int searchRadius = 100)
    {
        Vector3Int startOutsidePoint = new Vector3Int(wallTilemap.cellBounds.xMin - 10, wallTilemap.cellBounds.yMin - 10, 0);

        // We assume startOutsidePoint is a position *outside* your dungeon.  
        // If unsure, pick something far away like Vector3Int(1000, 1000, 0)
        
        HashSet<Vector3Int> visited = new HashSet<Vector3Int>();
        Queue<Vector3Int> toCheck = new Queue<Vector3Int>();
        toCheck.Enqueue(startOutsidePoint);

        Vector3Int[] directions = { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right };

        while (toCheck.Count > 0)
        {
            Vector3Int current = toCheck.Dequeue();

            if (visited.Contains(current))
                continue;

            visited.Add(current);

            // If we reached a floor position from the outside, that means walls are broken!
            if (floorPositions.Contains(current))
            {
                Debug.LogWarning("Broken walls detected! Outside flood reached floor tile at: " + current);
                return false;
            }

            // Limit search radius for performance
            if (Vector3Int.Distance(current, startOutsidePoint) > searchRadius)
                continue;

            foreach (var dir in directions)
            {
                Vector3Int neighbor = current + dir;

                // If there's no wall at the neighbor, that means we can move there
                if (wallTilemap.GetTile(neighbor) == null)
                {
                    toCheck.Enqueue(neighbor);
                }
            }
        }

        return true;
    }
}