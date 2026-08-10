using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteCentroidWithHoles
{
    public static bool TryGetCentroid(
           SpriteRenderer sr,
           out Vector2 centroid)
    {
        Sprite sprite = sr.sprite;
        int shapeCount = sprite.GetPhysicsShapeCount();

        // Alt-poligonlarý ve alanlarýný toplayacaðýmýz listeler
        var allPaths = new List<Vector2[]>();
        var signedAreas = new List<float>();

        // 1) Tüm physics-shape path’lerini oku
        for (int i = 0; i < shapeCount; i++)
        {
            var path = new List<Vector2>();
            sprite.GetPhysicsShape(i, path);
            if (path.Count < 3) continue;

            float area = SignedArea(path);
            allPaths.Add(path.ToArray());
            signedAreas.Add(area);
        }

        if (allPaths.Count == 0)
        {
            centroid = Vector2.zero;
            return false;
        }

        // 2) Alt-poligon centroid’lerini hesapla ve alanla aðýrlýklandýr
        Vector2 weightedSum = Vector2.zero;
        float totalArea = 0f;

        for (int i = 0; i < allPaths.Count; i++)
        {
            var poly = allPaths[i];
            float A = signedAreas[i];
            Vector2 C = ComputeCentroid(poly);

            weightedSum += C * A;
            totalArea += A;
        }

        if (Mathf.Approximately(totalArea, 0f))
        {
            centroid = Vector2.zero;
            return false;
        }

        // 3) Bileþik centroid (local uzayda)
        Vector2 localCentroid = weightedSum / totalArea;

        // 4) Dünya uzayýna dönüþtür
        centroid = sr.transform.TransformPoint(localCentroid);

        // 5) Opsiyonel: içeride kalýp kalmadýðýný test et
        // if (!IsPointInCompositePolygon(centroid, allPaths, sr))
        //     centroid = FindNearestInteriorPoint(centroid, allPaths, sr);

        return true;
    }

    // Shoelace formülü ile imzalý alan
    private static float SignedArea(IList<Vector2> verts)
    {
        float area = 0f;
        for (int i = 0, j = verts.Count - 1; i < verts.Count; j = i++)
            area += verts[j].x * verts[i].y - verts[i].x * verts[j].y;
        return area * 0.5f;
    }

    // Çokgen centroid hesaplamasý
    private static Vector2 ComputeCentroid(Vector2[] verts)
    {
        float signedArea = 0f, cx = 0f, cy = 0f;
        for (int i = 0, j = verts.Length - 1; i < verts.Length; j = i++)
        {
            Vector2 v0 = verts[j], v1 = verts[i];
            float a = v0.x * v1.y - v1.x * v0.y;
            signedArea += a;
            cx += (v0.x + v1.x) * a;
            cy += (v0.y + v1.y) * a;
        }
        signedArea *= 0.5f;
        cx /= (6f * signedArea);
        cy /= (6f * signedArea);
        return new Vector2(cx, cy);
    }

    // (Ýhtiyaç halinde) Bir noktanýn çoklu path’lerden oluþan bileþik þeklin içinde olup olmadýðýný
    // test etmek için her alt-poligon için ray-casting testi yapýlabilir.
}
