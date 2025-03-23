using UnityEngine;

[CreateAssetMenu(fileName = "NewEncounter", menuName = "Scriptable Objects/EncounterScrObj", order = 1)]
public class EncounterScrObj : ScriptableObject
{
    public GameObject[] enemyPrefabs = new GameObject[6];
}
