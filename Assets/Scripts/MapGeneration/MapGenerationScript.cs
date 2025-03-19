using System.Collections.Generic;
using System.Linq;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapGenerationScript : MonoBehaviour
{
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

        // Placing wall tiles 
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
        HashSet<Vector3Int> wallPositions = new HashSet<Vector3Int>(); 
        HashSet<Vector3Int> roomPositions = new HashSet<Vector3Int>(); 
        int minX = int.MaxValue, maxX = int.MinValue, minY = int.MaxValue, maxY = int.MinValue;

        // Store all room positions to avoid placing walls inside
        for (int i = 0; i < rectangles.Count; i++)
        {
            var rect = rectangles[i];

            int left = Mathf.FloorToInt(rect.transform.position.x - rect.width / 2);
            int right = Mathf.FloorToInt(rect.transform.position.x + rect.width / 2) - 1;
            int bottom = Mathf.FloorToInt(rect.transform.position.y - rect.height / 2);
            int top = Mathf.FloorToInt(rect.transform.position.y + rect.height / 2) - 1;

            for (int x = left; x <= right; x++)
            {
                for (int y = bottom; y <= top; y++)
                {
                    roomPositions.Add(new Vector3Int(x, y, 0)); // Store room positions
                }
            }
        }

        // Find the exterior border of each rectangle and add walls
        for (int i = 0; i < rectangles.Count; i++)
        {
            var rect = rectangles[i];
            bool isMainRoom = i < mainRooms; // Check if the room is one of the mainRooms

            int left = Mathf.FloorToInt(rect.transform.position.x - rect.width / 2);
            int right = Mathf.FloorToInt(rect.transform.position.x + rect.width / 2) - 1;
            int bottom = Mathf.FloorToInt(rect.transform.position.y - rect.height / 2);
            int top = Mathf.FloorToInt(rect.transform.position.y + rect.height / 2) - 1;

            minX = Mathf.Min(minX, left - 15);
            maxX = Mathf.Max(maxX, right + 15);
            minY = Mathf.Min(minY, bottom - 15);
            maxY = Mathf.Max(maxY, top + 15);

            // Horizontal Borders (Above & Below the Room)
            for (int x = left - 1; x <= right + 1; x++)
            {
                Vector3Int topPos = new Vector3Int(x, top + 1, 0);
                Vector3Int bottomPos = new Vector3Int(x, bottom - 1, 0);
                
                if (isMainRoom || !roomPositions.Contains(topPos)) wallPositions.Add(topPos);
                if (isMainRoom || !roomPositions.Contains(bottomPos)) wallPositions.Add(bottomPos);
            }

            // Vertical Borders (Left & Right of the Room)
            for (int y = bottom - 1; y <= top + 1; y++)
            {
                Vector3Int leftPos = new Vector3Int(left - 1, y, 0);
                Vector3Int rightPos = new Vector3Int(right + 1, y, 0);
                
                if (isMainRoom || !roomPositions.Contains(leftPos)) wallPositions.Add(leftPos);
                if (isMainRoom || !roomPositions.Contains(rightPos)) wallPositions.Add(rightPos);
            }
        }

        // Apply wall tiles to the correct tilemap layer
        foreach (var pos in wallPositions)
        {
            dungeonTilemap[1].SetTile(pos, roomTiles[1]); // Lower Wall Layer
            dungeonTilemap[2].SetTile(pos + new Vector3Int(0, 1, 0), roomTiles[2]); // Upper Wall Layer
            dungeonTilemap[3].SetTile(pos + new Vector3Int(0, 2, 0), roomTiles[3]); // Roof Layer
        }

        // Fill outside areas with roof tiles offset by +2 on the Y axis
        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                if (!roomPositions.Contains(pos))
                {
                    dungeonTilemap[3].SetTile(pos + new Vector3Int(0, 2, 0), roomTiles[3]);
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

        // Check if the adjacent segment is at least 5 units long        
        bool horizontallyAdjacent = deltaX == (halfWidth1 + halfWidth2) &&
                                    Mathf.Min(rectangle1.height, rectangle2.height) - deltaY > 2;
        
        bool verticallyAdjacent = deltaY == (halfHeight1 + halfHeight2) &&
                                Mathf.Min(rectangle1.width, rectangle2.width) - deltaX > 2;
        
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
