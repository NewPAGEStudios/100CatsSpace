using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class TextureSaver
{
    public static void SaveTextureAsPNG(Texture2D texture, string path,float ppu)
    {
        byte[] bytes = texture.EncodeToPNG();

        // Klasör yoksa oluþtur
        string dir = Path.GetDirectoryName(path);
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        File.WriteAllBytes(path, bytes);

        // Unity'ye bildir
        AssetDatabase.ImportAsset(path);

        // Texture ayarlarýný düzelt (örneðin filter mode)
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.filterMode = FilterMode.Point;
            importer.isReadable = true;
            importer.spritePixelsPerUnit = ppu;

            importer.textureCompression = TextureImporterCompression.Uncompressed;

            if (texture.width <= 2048 && texture.height <= 2048) importer.maxTextureSize = 2048;
            else if (texture.width <= 4096 && texture.height <= 4096) importer.maxTextureSize = 4096;
            else if (texture.width <= 8192 && texture.height <= 8192) importer.maxTextureSize = 8192;

            TextureImporterSettings settings = new();
            importer.ReadTextureSettings(settings);

            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.spriteAlignment = (int)SpriteAlignment.BottomLeft;
            settings.spritePivot = new Vector2(0,0);

            importer.SetTextureSettings(settings);

            importer.SaveAndReimport();

        }
    }
}
