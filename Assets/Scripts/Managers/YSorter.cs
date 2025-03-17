using UnityEngine;

public class YSorter : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        YSorterManager sorterManager = FindAnyObjectByType<YSorterManager>();
        if (sorterManager != null)
        {
            sorterManager.RegisterRenderer(spriteRenderer);
        }
    }

    void OnDestroy()
    {
        YSorterManager sorterManager = FindAnyObjectByType<YSorterManager>();
        if (sorterManager != null)
        {
            sorterManager.UnregisterRenderer(spriteRenderer);
        }
    }
}
