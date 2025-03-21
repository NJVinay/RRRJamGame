using UnityEngine;

public class RoomManager : MonoBehaviour
{
    public int width;
    public int height;
    BoxCollider2D boxCollider;

    void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
    }
}
