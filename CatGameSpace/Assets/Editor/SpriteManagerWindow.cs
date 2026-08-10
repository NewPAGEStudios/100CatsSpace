using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
// SceneManager yerine artýk EditorBuildSettings kullanýyoruz
using UnityEditor.SceneManagement;

public class SpriteManagerWindow : EditorWindow
{
    MonoScript targetSOScript;

    [MenuItem("Tools/Sprite & Asset Manager")]
    public static void ShowWindow()
    {
        GetWindow<SpriteManagerWindow>("Sprite Manager");
    }

    void OnGUI()
    {
        GUILayout.Label("1. Kullanýlmayan Sprite Analizi", EditorStyles.boldLabel);
        GUILayout.Label("Build Settings'deki tüm sahneleri ve\nseçili SO'larý tarar.", EditorStyles.miniLabel);

        targetSOScript = (MonoScript)EditorGUILayout.ObjectField("Hedef SO Tipi:", targetSOScript, typeof(MonoScript), false);

        if (GUILayout.Button("Analiz Et (Build Scenes + SO)"))
        {
            FindUnusedSprites();
        }

        EditorGUILayout.Space();
        GUILayout.Label("------------------------------------------------", EditorStyles.centeredGreyMiniLabel);
        EditorGUILayout.Space();

        GUILayout.Label("2. Max Size Optimizasyonu", EditorStyles.boldLabel);

        if (GUILayout.Button("Tüm Sprite Boyutlarýný Düzelt"))
        {
            if (EditorUtility.DisplayDialog("Onay", "Bu iþlem Assets klasöründeki tüm Texture'larýn Max Size ayarýný güncelleyecek. Devam edilsin mi?", "Evet", "Ýptal"))
            {
                OptimizeTextureSizes();
            }
        }
    }

    // --- GÖREV 1: KULLANILMAYANLARI BULMA (GÜNCELLENDÝ) ---
    void FindUnusedSprites()
    {
        // 1. Tüm Texture'larý Bul
        string[] allTextureGUIDs = AssetDatabase.FindAssets("t:Texture2D");
        HashSet<string> usedPaths = new HashSet<string>();
        List<string> pathsToScan = new List<string>();

        // 2. Build Settings'deki AKTÝF Sahneleri Bul
        // Sadece "tikli" olan sahneleri listeye ekliyoruz.
        foreach (var scene in EditorBuildSettings.scenes)
        {
            if (scene.enabled)
            {
                pathsToScan.Add(scene.path);
            }
        }
        Debug.Log($"Build Settings'de {pathsToScan.Count} adet aktif sahne bulundu.");

        // 3. Hedef SO Tipindeki Dosyalarý Bul
        if (targetSOScript != null)
        {
            System.Type soType = targetSOScript.GetClass();
            if (soType != null && soType.IsSubclassOf(typeof(ScriptableObject)))
            {
                string[] allSOGuids = AssetDatabase.FindAssets("t:" + soType.Name);
                foreach (var guid in allSOGuids)
                {
                    pathsToScan.Add(AssetDatabase.GUIDToAssetPath(guid));
                }
                Debug.Log($"{soType.Name} tipinde {allSOGuids.Length} adet ScriptableObject eklendi.");
            }
            else
            {
                Debug.LogWarning("Uyarý: Geçerli bir ScriptableObject scripti seçmediniz. Sadece sahneler taranacak.");
            }
        }

        // 4. Toplu Baðýmlýlýk Taramasý
        // Unity'nin bu fonksiyonu, verilen tüm path'lerin içindeki her þeyi (recursive) bulur.
        string[] allDependencies = AssetDatabase.GetDependencies(pathsToScan.ToArray(), true);

        foreach (var path in allDependencies)
        {
            usedPaths.Add(path);
        }

        // 5. Karþýlaþtýrma ve Raporlama
        int unusedCount = 0;
        foreach (var guid in allTextureGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            // Editor klasörleri, Paketler ve Resources klasörü (özel yükleme olduðu için) hariç tutulabilir
            if (path.Contains("/Editor/") || path.Contains("Packages/")) continue;

            // Eðer kullanýlanlar listesinde yoksa
            if (!usedPaths.Contains(path))
            {
                // Konsola týklanabilir obje olarak bas
                Debug.LogWarning($"[KULLANILMIYOR]: {path}", AssetDatabase.LoadAssetAtPath<Object>(path));
                unusedCount++;
            }
        }

        if (unusedCount == 0) Debug.Log("Harika! Build'e giren her þey kullanýlýyor görünüyor.");
        else Debug.LogError($"Toplam {unusedCount} adet kullanýlmayan sprite bulundu.");
    }

    // --- GÖREV 2: BOYUT OPTÝMÝZASYONU (AYNI KALDI) ---
    void OptimizeTextureSizes()
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture2D");
        int processed = 0;

        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.Contains("Packages/") || path.Contains("/Editor/")) continue;

            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) continue;

            int width = 0, height = 0;
            GetSourceSize(importer, out width, out height); // Helper metod aþaðýda

            if (width == 0 || height == 0) continue;

            int maxDimension = Mathf.Max(width, height);
            int bestSize = GetNextPowerOfTwo(maxDimension);

            if (importer.maxTextureSize != bestSize)
            {
                importer.maxTextureSize = bestSize;
                importer.SaveAndReimport();
                processed++;
            }
        }
        Debug.Log($"Ýþlem Tamamlandý! {processed} adet texture yeniden ayarlandý.");
    }

    int GetNextPowerOfTwo(int value)
    {
        int[] potentialSizes = { 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192 };
        foreach (int size in potentialSizes)
        {
            if (size >= value) return size;
        }
        return 8192;
    }

    void GetSourceSize(TextureImporter importer, out int width, out int height)
    {
#if UNITY_2022_2_OR_NEWER
        importer.GetSourceTextureWidthAndHeight(out width, out height);
#else
        object[] args = new object[2] { 0, 0 };
        var method = typeof(TextureImporter).GetMethod("GetWidthAndHeight", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if(method != null) { method.Invoke(importer, args); width = (int)args[0]; height = (int)args[1]; }
        else { Texture2D t = AssetDatabase.LoadAssetAtPath<Texture2D>(importer.assetPath); width = t.width; height = t.height; }
#endif
    }
}