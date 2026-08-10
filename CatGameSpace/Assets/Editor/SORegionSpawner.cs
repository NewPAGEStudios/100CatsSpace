using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.U2D;

public class SORegionSpawner : EditorWindow
{
    public PaintObject dataSO;
    public SpriteRenderer colored_Sprite;
    public Transform regionParent;
    public GameObject maskObje;
    public GameObject txtPrefab;

    // Yeni eklenen Materyal alaný
    public Material customMaterial;

    public Color themeColor = Color.black;

    [MenuItem("Tools/SO Region Spawner")]
    public static void ShowWindow()
    {
        GetWindow<SORegionSpawner>("Region Spawner");
    }

    private void OnGUI()
    {
        GUILayout.Label("Referanslar", EditorStyles.boldLabel);
        dataSO = (PaintObject)EditorGUILayout.ObjectField("Data SO (PaintObject)", dataSO, typeof(PaintObject), false);
        colored_Sprite = (SpriteRenderer)EditorGUILayout.ObjectField("Colored Sprite (Ana Obje)", colored_Sprite, typeof(SpriteRenderer), true);
        regionParent = (Transform)EditorGUILayout.ObjectField("Region Parent", regionParent, typeof(Transform), true);

        GUILayout.Space(10);
        maskObje = (GameObject)EditorGUILayout.ObjectField("Mask Prefab", maskObje, typeof(GameObject), false);
        txtPrefab = (GameObject)EditorGUILayout.ObjectField("Text (TMP) Prefab", txtPrefab, typeof(GameObject), false);

        // Materyal için arayüz alaný
        customMaterial = (Material)EditorGUILayout.ObjectField("Custom Material", customMaterial, typeof(Material), false);

        GUILayout.Space(10);
        themeColor = EditorGUILayout.ColorField("Theme Color (Text)", themeColor);

        GUILayout.Space(20);
        if (GUILayout.Button("Regionlarý Oluþtur", GUILayout.Height(30)))
        {
            if (dataSO != null && colored_Sprite != null && maskObje != null && txtPrefab != null)
            {
                SpawnAllFromSO();
            }
            else
            {
                Debug.LogError("Lütfen tüm referans alanlarýný doldurun!");
            }
        }
    }

    private void SpawnAllFromSO()
    {
        for (int i = 0; i < dataSO.regionSprites.Count; i++)
        {
            Sprite spr = null;
            if (dataSO.atlas != null)
            {
                spr = dataSO.atlas.GetSprite("spriteRegion" + i);
            }
            if (spr == null)
            {
                spr = dataSO.regionSprites[i];
            }

            Vector2 wp = WorldPosFromPixelCoords(dataSO.mins[i].x, dataSO.mins[i].y, colored_Sprite);
            Color realColor = dataSO.regionColor[i];

            int colorID = 0;
            if (dataSO.regionColorUnique != null && dataSO.regionColorUnique.Count > 0)
            {
                colorID = dataSO.regionColorUnique.IndexOf(dataSO.regionColor[i]);
            }

            Vector2 localPosOfTXT = dataSO.regionTextPosCollector[i];
            float squareSizeLocal = dataSO.regionTextSquareSizeLocal[i];

            SpawnSprites(spr, wp, i, realColor, colorID, localPosOfTXT, squareSizeLocal);
        }

        Debug.Log("Regionlar materyalleriyle birlikte baþarýyla oluþturuldu!");
    }

    Vector2 WorldPosFromPixelCoords(int minX, int minY, SpriteRenderer originalSprite)
    {
        Sprite sprite = originalSprite.sprite;
        float ppu = sprite.pixelsPerUnit;

        Vector2 localOffset = new Vector2(minX / ppu, minY / ppu);
        Vector2 pivotOffset = sprite.pivot / ppu;

        Vector2 worldPos = (Vector2)originalSprite.transform.position - pivotOffset + localOffset;

        return worldPos;
    }

    void SpawnSprites(Sprite spr, Vector2 wp, int id, Color realColora, int colorID, Vector2 localPosOfTXT, float squareSizeLocal)
    {
        GameObject go = new GameObject("RegionSprite" + id.ToString());
        Undo.RegisterCreatedObjectUndo(go, "Create Region Object");

        go.transform.position = wp;
        if (regionParent != null) go.transform.parent = regionParent;

        GameObject maskInstance = (GameObject)PrefabUtility.InstantiatePrefab(maskObje, go.transform);
        maskInstance.transform.localPosition = Vector3.zero;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.color = dataSO.ColoredSectionBG;
        sr.sprite = spr;
        sr.sortingOrder = (id * 2) + 10;
        sr.maskInteraction = SpriteMaskInteraction.None;

        // Materyal atamasý burada yapýlýyor
        if (customMaterial != null)
        {
            sr.sharedMaterial = customMaterial;
        }

        BoxCollider2D box = go.AddComponent<BoxCollider2D>();
        box.isTrigger = true;
        box.offset = spr.bounds.size / 2;
        box.size = spr.bounds.size;

        RegionController rc = go.AddComponent<RegionController>();
        rc.realColor = realColora;
        rc.colorID = colorID;

        GameObject txtObject = (GameObject)PrefabUtility.InstantiatePrefab(txtPrefab, go.transform);
        txtObject.name = "FittedTMP" + id.ToString();
        txtObject.transform.localPosition = localPosOfTXT;

        TextMeshPro tmp = txtObject.GetComponent<TextMeshPro>();
        if (tmp != null)
        {
            tmp.text = colorID.ToString();
            tmp.alignment = TextAlignmentOptions.Center;

            Color calculatedColor = new Color(themeColor.r - 0.3921f, themeColor.g - 0.3921f, themeColor.b - 0.3921f);
            tmp.color = calculatedColor;

            tmp.ForceMeshUpdate();
            Vector2 textSize = tmp.GetRenderedValues(false);

            if (textSize.x > 0 && textSize.y > 0)
            {
                float wF = squareSizeLocal / textSize.x;
                float hF = squareSizeLocal / textSize.y;
                tmp.fontSize *= Mathf.Min(wF, hF);
            }
        }
    }
}