using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapGenerationScript : MonoBehaviour
{
    // Erir's recommendations:
    // - same room count each time
    // i.e 2 item rooms, 1 boss room, 1 spawn room, 10 enemy rooms
    // also, make sure that item rooms and boss rooms are on the edges.
    // spawn room can be anywhere really

    public List<Tilemap> dungeonTilemap = new List<Tilemap>();
    public List<TileBase> roomTiles = new List<TileBase>();

    // Quick customization from the Inspector
    [Header("Map Generation Settings")]
    [Space(10)]
    [SerializeField]
    [Range(4, 30)]
    int mainRooms = 5;
    [SerializeField]
    [Range(5, 50)]
    int sizeMain = 30;
    [SerializeField]
    [Range(0, 20)]
    int sizeVariationMain = 5;
    [Space(10)]
    [SerializeField]
    [Range(0, 70)]
    int middleRooms = 5;
    [SerializeField]
    [Range(5, 50)]
    int sizeMiddle = 30;
    [SerializeField]
    [Range(0, 20)]
    int sizeVariationMiddle = 5;

    [Space(10)]
    [SerializeField]
    GameObject rectanglePrefab;
    List<MG_Rectangle> rectangles = new List<MG_Rectangle>();
    Dictionary<int, List<int>> dungeonGraph = new Dictionary<int, List<int>>();
    List<int> importantNodes = new List<int>();
    bool tryAgain = true;
    int attemptNumber = 1;

    void Start()
    {
        // Generate the map when the game starts
        GenerateMap();
    }

    void Update()
    {
        // For test purposes: regenerate the map when the Jump button is pressed
        if (Input.GetButtonDown("Jump"))
        {
            ClearMap();
            Debug.ClearDeveloperConsole();

            // Generate a new map
            GenerateMap();
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
        tryAgain = true;

        for (int i = 0; i < dungeonTilemap.Count; i++)
        {
            dungeonTilemap[i].ClearAllTiles();
        }
    }

    void GenerateMap()
    {
        while (tryAgain)
        {
            // Create main rooms with random positions and sizes
            for (int i = 0; i < mainRooms; i++)
            {
                float direction = i *2 *Mathf.PI / mainRooms;
                float x_pos = 0;
                float y_pos = 0;

                if (i < mainRooms /2)
                {
                    x_pos = Mathf.Cos(direction) *sizeMain /1.5f;
                    y_pos = Mathf.Sin(direction) *sizeMain /1.5f;
                }

                AddRectangle(new Vector2(x_pos, y_pos), 
                sizeMain +Random.Range(0, sizeVariationMain), 
                sizeMain +Random.Range(0, sizeVariationMain), Color.grey);
                importantNodes.Add(i);
            }

            // Create middle rooms with random positions and sizes
            for (int i = 0; i < middleRooms; i++)
            {
                float x_pos = Random.Range(-sizeMiddle, sizeMiddle);
                float y_pos = Random.Range(-sizeMiddle, sizeMiddle);

                AddRectangle(new Vector2(x_pos, y_pos), 
                sizeMiddle +Random.Range(0, sizeVariationMiddle), 
                sizeMiddle +Random.Range(0, sizeVariationMiddle), Color.grey);
            }

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

                // Debug: color the main rooms green
                for (int i = 0; i < mainRooms; i++)
                {
                    rectangles[i].color = Color.green;
                }
            }
            else
            {
                ClearMap();
                Debug.LogWarning("Attempt n. " + attemptNumber + " failed. MG algorithm run again.");
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
            dungeonTilemap[0].SetTile(pos, roomTiles[0]); // Floor Layer
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
                dungeonTilemap[1].SetTile(pos, roomTiles[1]); // Lower wall
                dungeonTilemap[2].SetTile(pos + Vector3Int.up, roomTiles[2]); // Upper wall
                dungeonTilemap[3].SetTile(pos + new Vector3Int(0, 2, 0), roomTiles[3]); // Roof
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

        PlaceRoofBackgroundTiles(allRoomPositions);
    }

    void CreateHallwayBetween(MG_Rectangle rect1, MG_Rectangle rect2)
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
                ClearAndFloorTile(new Vector3Int(wallX, y, 0));     
                ClearAndFloorTile(new Vector3Int(wallX + 1, y, 0)); 
                ClearAndFloorTile(new Vector3Int(wallX + 2, y, 0)); 
                ClearAndFloorTile(new Vector3Int(wallX + 3, y, 0)); 
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
                ClearAndFloorTile(new Vector3Int(x, wallY, 0));     
                ClearAndFloorTile(new Vector3Int(x, wallY + 1, 0)); 
                ClearAndFloorTile(new Vector3Int(x, wallY + 2, 0)); 
                ClearAndFloorTile(new Vector3Int(x, wallY + 3, 0)); 
            }
        }
    }

    void ClearAndFloorTile(Vector3Int pos)
    {
        // Clear existing tiles
        dungeonTilemap[1].SetTile(pos, null);
        dungeonTilemap[2].SetTile(pos + Vector3Int.up, null);
        dungeonTilemap[3].SetTile(pos + new Vector3Int(0, 2, 0), null);

        // Optionally: place a floor tile (if you have a floor layer, replace below)
        dungeonTilemap[0].SetTile(pos, roomTiles[0]);
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
                    dungeonTilemap[3].SetTile(pos + new Vector3Int(0, 2, 0), roomTiles[3]); // place roof/background tile
                }
            }
        }
    }

    void AddRectangle(Vector2 position, int width, int height, Color color)
    {
        // Instantiate a new rectangle and set its properties
        MG_Rectangle temp_mG_Rectangle = Instantiate(rectanglePrefab, new Vector3(position.x, position.y, 0), Quaternion.identity).GetComponent<MG_Rectangle>();
        temp_mG_Rectangle.transform.parent = transform;
        temp_mG_Rectangle.width = width;
        temp_mG_Rectangle.height = height;
        temp_mG_Rectangle.color = color;
        rectangles.Add(temp_mG_Rectangle);        
    }

    void SeparateRectangles(MG_Rectangle rectangle1, MG_Rectangle rectangle2)
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

    bool AreAdjacent(MG_Rectangle rectangle1, MG_Rectangle rectangle2)
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
            else
            {
                rectangles[i].color = Color.yellow;
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
