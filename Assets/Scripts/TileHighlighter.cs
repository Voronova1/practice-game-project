using UnityEngine;
using System.Collections.Generic;

public class TileHighlighter : MonoBehaviour
{
    public static TileHighlighter Instance;
    public Material highlightMaterial;
    private List<GameObject> highlightedTiles = new List<GameObject>();
    private Unit selectedUnit;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetSelectedUnit(Unit unit)
    {
        selectedUnit = unit;
    }

    public void HighlightTiles(Vector3 unitPos, int range)
    {
        ClearHighlights();

        // Получаем точные координаты центра текущей клетки
        int currentX = Mathf.RoundToInt(unitPos.x / 5) * 5;
        int currentZ = Mathf.RoundToInt(unitPos.z / 5) * 5;

        Unit[] allUnits = FindObjectsOfType<Unit>();

        // Перебираем все клетки в квадратном радиусе
        for (int x = currentX - range * 5; x <= currentX + range * 5; x += 5)
        {
            for (int z = currentZ - range * 5; z <= currentZ + range * 5; z += 5)
            {
                // Пропускаем центральную клетку
                if (x == currentX && z == currentZ) continue;

                string tileName = $"Tile_{x}_{z}";
                GameObject tile = GameObject.Find(tileName);

                if (tile != null)
                {
                    bool isOccupied = false;
                    Vector3 tileCenter = new Vector3(x, 0, z);

                    // Проверяем занятость клетки
                    foreach (Unit unit in allUnits)
                    {
                        if (unit != selectedUnit &&
                            Vector3.Distance(unit.transform.position, tileCenter) < 2.5f)
                        {
                            isOccupied = true;
                            break;
                        }
                    }

                    if (!isOccupied)
                    {
                        // Подсвечиваем все клетки в квадратном радиусе
                        tile.GetComponent<Renderer>().material = highlightMaterial;
                        highlightedTiles.Add(tile);
                    }
                }
            }
        }
    }

    public void ClearHighlights()
    {
        foreach (var tile in highlightedTiles)
        {
            if (tile != null)
            {
                tile.GetComponent<Renderer>().material = Resources.Load<Material>("DefaultTileMaterial");
            }
        }
        highlightedTiles.Clear();
    }
}