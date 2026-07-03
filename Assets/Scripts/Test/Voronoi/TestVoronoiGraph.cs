using System.Collections.Generic;
using UnityEngine;

public class TestVoronoiGraph : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        List<Vector2> boundaryPoints = new List<Vector2>()
        {
            new Vector2(-10, -10),
            new Vector2(10, -10),
            new Vector2(10, 10),
            new Vector2(-10, 10)
        };
        List<Vector2> voronoiPoints = new List<Vector2>();
        int seed = Random.Range(0, int.MaxValue);
        voronoiPoints = DeterministicRandom.GeneratePointsInsideBounds(seed,Rand.SeededRange(10,30,seed),-10,10,-10,10);
        
        VoronoiGraph graph = this.gameObject.AddComponent<VoronoiGraph>();
        graph.voronoiGraphPoints = boundaryPoints;
        graph.dynamicPoints = new List<DynamicVoronoiPoint>();

        foreach(Vector2 point in voronoiPoints)
        {
            GameObject pointObj = new GameObject("Point");
            pointObj.transform.position = new Vector3(point.x, point.y, 0);
            pointObj.transform.localScale = new Vector3(0.3f, 0.3f, 1);
            SpriteRenderer pointRenderer = pointObj.AddComponent<SpriteRenderer>();
            pointRenderer.sprite = LocalRefDefaultRS.GetSpriteByName("Circle");

            DynamicVoronoiPoint dynamicPoint = pointObj.AddComponent<DynamicVoronoiPoint>();
            graph.dynamicPoints.Add(dynamicPoint);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
