using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// Harita katmanları (5000x3000 Display/Process/Line/Cats) Android'de sıkıştırılmadan yükleniyordu (~460 MB/harita).
// Bu araç sadece büyük harita katmanlarına Android için ASTC sıkıştırması uygular; masaüstü ayarlarına dokunmaz.
// Bölge parçaları SpriteAtlas üzerinden zaten sıkıştırıldığı için kapsam dışı (boyut eşiği ile ayrılıyor).
public static class MapTextureMobileCompression
{
    private static readonly string[] MapFolders = { "Assets/PaintRes", "Assets/Editor/Bekle" };

    // Bu piksel sayısından büyük görseller harita katmanı sayılır (bölge parçaları en fazla ~2 MP)
    private const int MinPixels = 4000000;

    private const string AndroidTarget = "Android";
    private const int MaxSize = 8192;
    private const TextureImporterFormat AndroidFormat = TextureImporterFormat.ASTC_4x4;

    [MenuItem("Tools/Mobil/Harita Görsellerini Android İçin Sıkıştır")]
    public static void CompressMapLayersForAndroid()
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", MapFolders);
        var changed = new List<string>();

        try
        {
            AssetDatabase.StartAssetEditing();

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (EditorUtility.DisplayCancelableProgressBar("Harita görselleri", path, (float)i / guids.Length)) break;

                if (!(AssetImporter.GetAtPath(path) is TextureImporter importer)) continue;

                importer.GetSourceTextureWidthAndHeight(out int width, out int height);
                if ((long)width * height < MinPixels) continue;

                TextureImporterPlatformSettings settings = importer.GetPlatformTextureSettings(AndroidTarget);
                if (settings.overridden && settings.format == AndroidFormat && settings.maxTextureSize == MaxSize) continue;

                settings.overridden = true;
                settings.maxTextureSize = MaxSize;
                settings.format = AndroidFormat;
                settings.compressionQuality = (int)TextureCompressionQuality.Normal;
                importer.SetPlatformTextureSettings(settings);
                importer.SaveAndReimport();
                changed.Add(path);
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            EditorUtility.ClearProgressBar();
        }

        foreach (string path in changed) Debug.Log("[Mobil] Android sıkıştırması uygulandı: " + path);
        EditorUtility.DisplayDialog("Harita Görselleri",
            changed.Count > 0
                ? changed.Count + " harita katmanı Android için sıkıştırıldı (ASTC 4x4, en fazla 8192 px)."
                : "Değiştirilecek görsel bulunamadı; hepsi zaten ayarlı.",
            "Tamam");
    }
}
