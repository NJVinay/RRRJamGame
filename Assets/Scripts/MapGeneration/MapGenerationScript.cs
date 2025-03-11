using System.Collections.Generic;
using UnityEditor.EditorTools;
using UnityEngine;

public class MapGenerationScript : MonoBehaviour
{
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
    [Range(0, 50)]
    int middleRooms = 5;
    [SerializeField]
    [Range(5, 50)]
    int sizeMiddle = 30;
    [SerializeField]
    [Range(0, 20)]
    int sizeVariationMiddle = 5;
    [Space(10)]
    [SerializeField]
    [Range(0, 50)]
    int smallRooms = 5;
    [SerializeField]
    [Range(5, 50)]
    int sizeSmall = 30;
    [SerializeField]
    [Range(0, 20)]
    int sizeVariationSmall = 5;

    [Space(10)]
    [SerializeField]
    GameObject rectanglePrefab;
    List<MG_Rectangle> rectangles = new List<MG_Rectangle>();
    Dictionary<int, List<int>> dungeonGraph = new Dictionary<int, List<int>>();
    List<int> importantNodes = new List<int>();

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
            // Destroy all existing rectangles
            for (int i = 0; i < rectangles.Count; i++)
            {
                Destroy(rectangles[i].gameObject);
            }

            // Clear the lists and dictionary
            rectangles.Clear();
            dungeonGraph.Clear();
            importantNodes.Clear();

            // Generate a new map
            GenerateMap();
        }
    }

    void GenerateMap()
    {
        // Create main rooms with random positions and sizes
        for (int i = 0; i < mainRooms; i++)
        {
            AddRectangle(new Vector2(Random.Range(-40, 40), Random.Range(-40, 40)), 
            Mathf.Clamp(Random.Range(sizeMain - sizeVariationMain, sizeMain + sizeVariationMain), 5, 50), 
            Mathf.Clamp(Random.Range(sizeMain - sizeVariationMain, sizeMain + sizeVariationMain), 5, 50), Color.grey);
            importantNodes.Add(i);
        }

        // Create middle rooms with random positions and sizes
        for (int i = 0; i < middleRooms; i++)
        {
            AddRectangle(new Vector2(Random.Range(-40, 40), Random.Range(-40, 40)), 
            Mathf.Clamp(Random.Range(sizeMiddle - sizeVariationMiddle, sizeMiddle + sizeVariationMiddle), 5, 50), 
            Mathf.Clamp(Random.Range(sizeMiddle - sizeVariationMiddle, sizeMiddle + sizeVariationMiddle), 5, 50), Color.grey);
        }

        // Create small rooms with random positions and sizes
        for (int i = 0; i < smallRooms; i++)
        {
            AddRectangle(new Vector2(Random.Range(-40, 40), Random.Range(-40, 40)), 
            Mathf.Clamp(Random.Range(sizeSmall - sizeVariationSmall, sizeSmall + sizeVariationSmall), 5, 50), 
            Mathf.Clamp(Random.Range(sizeSmall - sizeVariationSmall, sizeSmall + sizeVariationSmall), 5, 50), Color.grey);
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

        // Create a spanning tree from the graph
        ConnectImportantNodes();

        // Debug: color the main rooms green
        for (int i = 0; i < mainRooms; i++)
        {
            rectangles[i].color = Color.green;
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
        // Check if the rectangles are adjacent (touching each other)
        return Mathf.Abs(rectangle1.transform.position.x - rectangle2.transform.position.x) <= (rectangle1.width / 2 + rectangle2.width / 2) &&
               Mathf.Abs(rectangle1.transform.position.y - rectangle2.transform.position.y) <= (rectangle1.height / 2 + rectangle2.height / 2);
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
}
