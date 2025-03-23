using System.Collections.Generic;
using UnityEngine;

public class DoorManager : MonoBehaviour
{
    public bool isHorizontal = true;
    public List<RoomManager> neighbouringRooms = new List<RoomManager>();
}
