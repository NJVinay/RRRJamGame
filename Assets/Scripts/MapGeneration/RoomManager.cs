using UnityEngine;

public enum RoomType
{
    Boss,
    Enemy,
    Item,
    Passageway,
}

public class RoomManager : MonoBehaviour
{
    public RoomType roomType;
    public int width;
    public int height;
    BoxCollider2D boxCollider;
    public GameObject roomPrefab;

    void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
    }
}
