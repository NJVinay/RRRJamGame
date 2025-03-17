using System.Collections.Generic;
using UnityEngine;

public class YSorterManager : MonoBehaviour
{
    private List<SpriteRenderer> renderers = new List<SpriteRenderer>();
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void FixedUpdate()
    {
        UpdateSortingOrders();
    }

    public void RegisterRenderer(SpriteRenderer renderer)
    {
        if (!renderers.Contains(renderer))
        {
            renderers.Add(renderer);
        }
    }

    public void UnregisterRenderer(SpriteRenderer renderer)
    {
        if (renderers.Contains(renderer))
        {
            renderers.Remove(renderer);
        }
    }

    private void UpdateSortingOrders()
    {
        if (mainCamera == null) return;

        float camBottomY = mainCamera.transform.position.y - mainCamera.orthographicSize;
        float camTopY = mainCamera.transform.position.y + mainCamera.orthographicSize;

        foreach (var renderer in renderers)
        {
            float relativeY = Mathf.InverseLerp(camTopY, camBottomY, renderer.transform.position.y);
            int sortingOrder = Mathf.RoundToInt(Mathf.Lerp(2, 99, relativeY));
            renderer.sortingOrder = sortingOrder;
        }
    }
}
