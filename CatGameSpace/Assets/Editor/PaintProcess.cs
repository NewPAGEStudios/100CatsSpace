using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PaintObject))]
public class PaintProcess : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PaintObject obj = (PaintObject)target;

        //if (GUILayout.Button("RegionCollect()"))
        //{
        //    obj.regionCollect();
        //    if (obj.regionTexture.Count <= 0)
        //    {
        //        Debug.Log("No region Data");
        //        return;
        //    }
        //    int i = 0;
        //    foreach (Texture2D region in obj.regionTexture)
        //    {
        //        SaveSpriteAsset(region, obj.id, i, obj.PPU);
        //        i++;
        //    }
        //    AssetDatabase.SaveAssets();  // Asset'leri kaydet
        //    AssetDatabase.Refresh();  // Asset listelerini yenile

        //    for (int c = 0; c < obj.regionTexture.Count; c++)
        //    {
        //        obj.regionSprites.Add(LoadSpriteAsset(obj.id, c));
        //    }


        //    EditorUtility.SetDirty(target);  // Deðiþiklikleri iþaretle
        //}
        if (GUILayout.Button("ColorCollect"))
        {
            obj.colorCollect();
            EditorUtility.SetDirty(target);  // Deðiþiklikleri iþaretle
            AssetDatabase.SaveAssets();  // Asset'leri kaydet
            AssetDatabase.Refresh();  // Asset listelerini yenile
        }
        else if (GUILayout.Button("RegionCollectColored"))
        {
            obj.regionCollectColored();
            if (obj.regionTexture.Count <= 0)
            {
                Debug.Log("No region Data");
                return;
            }
            int i = 0;
            foreach (Texture2D region in obj.regionTexture)
            {
                SaveSpriteAsset(region, obj.id, i, obj.PPU);
                i++;
            }
            AssetDatabase.SaveAssets();  // Asset'leri kaydet
            AssetDatabase.Refresh();  // Asset listelerini yenile

            for (int c = 0; c < obj.regionTexture.Count; c++)
            {
                obj.regionSprites.Add(LoadSpriteAsset(obj.id, c));
            }


            EditorUtility.SetDirty(target);  // Deðiþiklikleri iþaretle
        }
        else if (GUILayout.Button("RegionTxtSpawn()"))
        {
            obj.regionTxtSpawns();
            EditorUtility.SetDirty(target);  // Deðiþiklikleri iþaretle
            AssetDatabase.SaveAssets();  // Asset'leri kaydet
            AssetDatabase.Refresh();  // Asset listelerini yenile
        }
        else if (GUILayout.Button("AssignNames()"))
        {
            obj.AssignNames();
            EditorUtility.SetDirty(target);  // Deðiþiklikleri iþaretle
        }

    }
    private void DeleteAllSpriteAsset(string objectID)
    {
        string path = "Assets/Resources/GeneratedSprites/" + objectID;
        AssetDatabase.DeleteAsset(path);
    }
    private void SaveSpriteAsset(Texture2D tex, string objectID, int regionid, float ppu)
    {
        if (tex != null)
        {
            // Sprite'ý belirtilen bir path'e kaydet
            string path = "Assets/Resources/GeneratedSprites/" + objectID + "/spriteRegion" + regionid + ".png";
            string folderPath = "Assets/Resources/GeneratedSprites/" + objectID;

            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                AssetDatabase.CreateFolder("Assets/Resources/GeneratedSprites", objectID);
                Debug.Log("Yeni klasör oluþturuldu: " + folderPath);
            }

            // Yeni sprite'ý kaydet
            TextureSaver.SaveTextureAsPNG(tex, path, ppu);
        }
        else
        {
            Debug.LogError("Sprite oluþturulamadý!");
        }
    }
    private Sprite LoadSpriteAsset(string objectID, int id)
    {
        string path = "Assets/Resources/GeneratedSprites/" + objectID + "/spriteRegion" + id + ".png";
        Sprite loadedSpr = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        return loadedSpr;
    }
}
