using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GroupedCollisionManager : MonoBehaviour
{
    public static GroupedCollisionManager Instance;

    // Seguimiento actual de colliders por región
    private Dictionary<BodyRegion, HashSet<Collider>> regionColliders = new Dictionary<BodyRegion, HashSet<Collider>>();
    private Dictionary<BodyRegion, int> regionColliderCounts = new Dictionary<BodyRegion, int>();

    // Conteo acumulado total de colisiones por región
    private Dictionary<BodyRegion, int> totalRegionHits = new Dictionary<BodyRegion, int>();

    public int CurrentGroupedCollisions => regionColliderCounts.Count;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // Devuelve las regiones que están actualmente en colisión.
    public List<BodyRegion> GetActiveRegions()
    {
        return regionColliderCounts.Keys.ToList();
    }

    // Devuelve las regiones que han colisionado en cualquier momento.
    public List<BodyRegion> GetAllTouchedRegions()
    {
        return totalRegionHits.Keys.ToList();
    }

    public int GetTotalHitsForRegion(BodyRegion region)
    {
        return totalRegionHits.ContainsKey(region) ? totalRegionHits[region] : 0;
    }

    // Devuelve el total acumulado de colisiones agrupadas entre todas las extremidades.
    public int GetTotalGroupedCollisions()
    {
        return totalRegionHits.Values.Sum();
    }

    public void RegisterCollision(BodyRegion region, Collider collider)
    {
        if (!regionColliders.ContainsKey(region))
        {
            regionColliders[region] = new HashSet<Collider>();
        }

        if (regionColliders[region].Add(collider))
        {
            if (!regionColliderCounts.ContainsKey(region))
                regionColliderCounts[region] = 0;

            regionColliderCounts[region]++;

            // Incrementar el total acumulado
            if (!totalRegionHits.ContainsKey(region))
                totalRegionHits[region] = 0;

            totalRegionHits[region]++;

            Debug.Log("Colisión iniciada en extremidad: " + region);
        }
    }

    public void UnregisterCollision(BodyRegion region, Collider collider)
    {
        if (regionColliders.ContainsKey(region) && regionColliders[region].Remove(collider))
        {
            regionColliderCounts[region]--;

            if (regionColliderCounts[region] <= 0)
            {
                regionColliderCounts.Remove(region);
                regionColliders.Remove(region);

                Debug.Log("Colisión finalizada en extremidad: " + region);
            }
        }
    }

    void OnGUI()
    {
        GUIStyle style = new GUIStyle
        {
            fontSize = 60,
            normal = { textColor = Color.cyan }
        };

        GUI.Label(new Rect(40, 60, 1000, 80), "Total de colisiones agrupadas: " + GetTotalGroupedCollisions(), style);
        GUI.Label(new Rect(40, 100, 1000, 80), "Colisiones agrupadas activas: " + CurrentGroupedCollisions, style);

        style.fontSize = 40;
        style.normal.textColor = Color.white;

        int y = 180;
        GUI.Label(new Rect(40, y, 1000, 40), "<b>Extremidades en colisión actual:</b>", style);
        y += 40;
        foreach (var region in GetActiveRegions())
        {
            GUI.Label(new Rect(60, y, 1000, 40), "-->" + region.ToString(), style);
            y += 40;
        }

        y += 30;
        style.normal.textColor = Color.yellow;
        GUI.Label(new Rect(40, y, 1000, 40), "<b>Total acumulado por extremidad:</b>", style);
        y += 40;
        foreach (var region in GetAllTouchedRegions())
        {
            GUI.Label(new Rect(60, y, 1000, 40), region + ": " + totalRegionHits[region], style);
            y += 40;
        }
    }
}