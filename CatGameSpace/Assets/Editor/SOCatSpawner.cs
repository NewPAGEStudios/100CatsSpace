using UnityEngine;
using UnityEditor;

public class SOCatSpawner : EditorWindow
{
    public PaintObject dataSO;
    public Transform catParent;
    public GameObject confetiPrefab;

    [MenuItem("Tools/SO Cat Spawner")]
    public static void ShowWindow()
    {
        GetWindow<SOCatSpawner>("SO Cat Spawner");
    }

    private void OnGUI()
    {
        GUILayout.Label("Kedi Spawn Referanslarý", EditorStyles.boldLabel);
        dataSO = (PaintObject)EditorGUILayout.ObjectField("Data SO (PaintObject)", dataSO, typeof(PaintObject), false);
        catParent = (Transform)EditorGUILayout.ObjectField("Cat Parent", catParent, typeof(Transform), true);
        confetiPrefab = (GameObject)EditorGUILayout.ObjectField("Confeti Prefab", confetiPrefab, typeof(GameObject), false);

        GUILayout.Space(20);
        if (GUILayout.Button("SO Ýçindeki Kedileri Spawnla", GUILayout.Height(30)))
        {
            if (dataSO != null && confetiPrefab != null)
            {
                SpawnCatsFromSO();
            }
            else
            {
                Debug.LogError("Lütfen PaintObject SO ve Confeti Prefab atamalarýný yapýn!");
            }
        }
    }

    private void SpawnCatsFromSO()
    {
        if (dataSO.catsSprites == null || dataSO.catsSprites.Length == 0)
        {
            Debug.LogWarning("PaintObject içinde catsSprites bulunamadý!");
            return;
        }

        // sheetPivot senin kodunda tanýmlý deðildi, bu yüzden ana resmin tam merkezini baz alarak hesapladým.
        Texture2D mainTex = dataSO.catsSprites[0].texture;
        Vector2 sheetPivot = new Vector2(mainTex.width * 0.5f, mainTex.height * 0.5f);

        int count = dataSO.catsSprites.Length;

        for (int i = 0; i < count; i++)
        {
            Sprite sprite = dataSO.catsSprites[i];
            Rect r = sprite.rect;
            Vector2 sliceCenter = new Vector2(r.x + r.width * 0.5f, r.y + r.height * 0.5f);

            // dataSO içindeki PPU deðerini çekiyoruz
            Vector2 localOffset = (sliceCenter - sheetPivot) / dataSO.PPU;

            string catName = "Bilinmeyen Kedi";
            if (dataSO.catsName != null && i < dataSO.catsName.Length)
            {
                catName = dataSO.catsName[i];
            }

            SpawnSprites(sprite, localOffset, i, catName);
        }

        Debug.Log(count + " adet kedi baþarýyla oluþturuldu!");
    }

    void SpawnSprites(Sprite sp, Vector2 wp, int id, string catName)
    {
        GameObject go = new GameObject("Cat" + id.ToString());
        Undo.RegisterCreatedObjectUndo(go, "Create Cat Object");

        if (catParent != null) go.transform.parent = catParent;

        // wp deðerin senin hesabýnda bir "local offset" olduðu için, 
        // objeyi parent'a atadýktan sonra localPosition olarak vermek Editör'de çok daha tutarlý çalýþýr.
        go.transform.localPosition = wp;

        // Confeti Prefab'ýný editör modunda baðýný koparmadan (mavi þekilde) Instantiate ediyoruz
        GameObject conf = (GameObject)PrefabUtility.InstantiatePrefab(confetiPrefab, go.transform);
        conf.transform.localPosition = Vector3.zero;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sp;
        sr.sortingOrder = 9999;
        sr.color = Color.white;

        PolygonCollider2D pol = go.AddComponent<PolygonCollider2D>();
        pol.isTrigger = true;

        // CatController bileþenini ekliyoruz
        CatController cc = go.AddComponent<CatController>();
        cc.catName = catName;
        cc.Confeti = conf;
        cc.catColor = Color.white; // Ýstediðin gibi varsayýlan renk beyaz býrakýldý
    }
}