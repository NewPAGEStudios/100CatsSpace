using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.U2D;


[CreateAssetMenu(menuName = "PaintOBJ")]
public class PaintObject : ScriptableObject
{
    public string id;
    public string PaintName;
    [Header("Assignables")]
    public Sprite[] catsSprites;
    public Sprite lineSprite;
    public Sprite colorSpriteBase;
    public Sprite colorSprite;
    public Sprite whiteBGSprite;
    public Color ColoredSectionBG = Color.white;
    public SpriteAtlas atlas;
    public TextAsset catsNameTxtFile;


    [Header("Cats")]
    public string[] catsName;
    public AudioClip[] catsSounds;

    public void AssignNames()
    {
        if (catsNameTxtFile.text == null)
        {
            Debug.Log("No Text File Couldn't find");
        }
        catsName = catsNameTxtFile.text.Split(new char[] { '\n' });
        Debug.Log(catsName.Length);
        catsSounds = new AudioClip[catsName.Length];

    }

    public List<Sprite> regionSprites;
    public List<Vector2Int> mins;

    public List<Color> regionColor;
    public List<Vector2> regionTextPosCollector;
    public List<float> regionTextSquareSizeLocal;

    public List<Texture2D> regionTexture;
    public List<Color> regionColorUnique;
    public List<int> regionColorUniqueMax;


    [Tooltip("to Ignore max square width or height size")]
    public float sprite_height_width_Tolerance;
    [Tooltip("to Ignore px of sprite width or height size")]
    public int tolerance;
    [Tooltip("Applying Pixel Per Unit")]
    public float PPU = 100;

    #region NotUsing
    //public void regionCollect()
    //{
    //    mins.Clear();
    //    regionTexture.Clear();
    //    regionSprites.Clear();


    //    List<List<Vector2Int>> regions = FindFillableRegions(colorSprite.texture, Color.white);


    //    foreach (List<Vector2Int> region in regions)
    //    {
    //        if (region.Count < 50) continue;

    //        int minX = region.Min(p => p.x);
    //        int minY = region.Min(p => p.y);
    //        mins.Add(new Vector2Int(minX, minY));
    //        Texture2D newTex = CreateTextureFromRegion(region, colorSprite.texture);
    //        regionTexture.Add(newTex);
    //    }

    //}

    #endregion
    public void colorCollect()
    {
        regionColor.Clear();
        regionColorUnique.Clear();
        regionColorUniqueMax.Clear();

        if (regionSprites.Count <= 0)
        {
            Debug.Log("No Region");
        }
        for (int c = 0; c < regionSprites.Count; c++)
        {
            if (regionColor.Count != c)
            {
                regionColor.Add(Color.white);
            }
            List<Color> colorsInRegion = new List<Color>();

            //bool breakit = false;
            for (int y = 0; y < regionSprites[c].texture.height; y++)
            {
                for (int x = 0; x < regionSprites[c].texture.width; x++)
                {
                    if (regionSprites[c].texture.GetPixel(x, y).a <= .95f) continue;

                    Color col = colorSprite.texture.GetPixel(mins[c].x + x, mins[c].y + y);

                    if (col.a < .95) continue;

                    if (col == Color.black)
                    {
                        continue;
                    }
                    colorsInRegion.Add(col);



                    //breakit = true;
                    //break;
                }
                //if (breakit) break;
            }
            var mostColor = colorsInRegion.GroupBy(r => r).OrderByDescending(g => g.Count()).First().Key;

            if (!regionColorUnique.Contains(mostColor))
            {
                regionColorUnique.Add(mostColor);
                regionColorUniqueMax.Add(0);
            }

            regionColorUniqueMax[regionColorUnique.IndexOf(mostColor)]++;
            regionColor.Add(mostColor);

        }
    }
    public void regionTxtSpawns()
    {
        regionTextPosCollector.Clear();
        regionTextSquareSizeLocal.Clear();
        foreach (Sprite sprite in regionSprites)
        {
            var res = GiveTxtPos(sprite.texture, sprite.textureRect);
            regionTextPosCollector.Add(res.Item1);
            regionTextSquareSizeLocal.Add(res.Item2);
        }
    }
    public void regionCollectColored()
    {
        mins.Clear();
        regionTexture.Clear();
        regionSprites.Clear();

        regionColor.Clear();
        regionColorUnique.Clear();
        regionColorUniqueMax.Clear();

        List<List<Vector2Int>> regions = FindColorRegions(colorSprite.texture);


        int c = 0;
        foreach (List<Vector2Int> region in regions)
        {
            if (region.Count < 50) continue;

            int minX = region.Min(p => p.x);
            int minY = region.Min(p => p.y);
            mins.Add(new Vector2Int(minX, minY));
            Texture2D newTex = CreateTextureFromRegion(region, colorSprite.texture);
            regionTexture.Add(newTex);
            c++;
        }
    }


    #region interalFunctions
    private (Vector2, float) GiveTxtPos(Texture2D tex, Rect rect)
    {

        int x0 = (int)rect.x, y0 = (int)rect.y;
        int w = (int)rect.width, h = (int)rect.height;
        Color32[] pixels = tex.GetPixels32();
        bool[,] mask = new bool[w, h];
        for (int yy = 0; yy < h; yy++)
            for (int xx = 0; xx < w; xx++)
                mask[xx, yy] = pixels[(y0 + yy) * tex.width + (x0 + xx)].a > 0;

        // 2) Eðer çok küçük sprite ise toleransý atla
        bool isTiny = (w < sprite_height_width_Tolerance || h < sprite_height_width_Tolerance);
        if (!isTiny)
        {
            // yatay doldurma
            ApplyTolerance(mask, w, h, tolerance, true);
            // düþey doldurma
            ApplyTolerance(mask, w, h, tolerance, false);
        }

        // 3) DP ile en büyük kareyi bul
        int[,] dp = new int[w, h];
        int maxSize = 0, maxX = 0, maxY = 0;
        for (int yy = 0; yy < h; yy++)
            for (int xx = 0; xx < w; xx++)
                if (mask[xx, yy])
                {
                    dp[xx, yy] = (xx > 0 && yy > 0)
                        ? Mathf.Min(dp[xx - 1, yy], Mathf.Min(dp[xx, yy - 1], dp[xx - 1, yy - 1])) + 1
                        : 1;
                    if (dp[xx, yy] > maxSize)
                    {
                        maxSize = dp[xx, yy];
                        maxX = xx; maxY = yy;
                    }
                }

        if (maxSize <= 0)
        {
            Debug.LogWarning("No fitting square found!");
            return (Vector2.zero, 0);
        }

        int squareStartX, squareStartY, squareSize;
        Vector3 localSquareOrigin;
        float squareSizeLocal;

        // 4) Kareyi local-space'e dönüþtür
        squareSize = maxSize;
        squareStartX = maxX - squareSize + 1;
        squareStartY = maxY - squareSize + 1;
        squareSizeLocal = squareSize / PPU;
        localSquareOrigin = new Vector3(squareStartX / PPU, squareStartY / PPU, 0f);
        Vector3 localCenter = new Vector3(
            (squareStartX + (squareSize - 1) * 0.5f) / PPU,
            (squareStartY + (squareSize - 1) * 0.5f) / PPU,
            0f
        );
        return (localCenter, squareSizeLocal);
    }

    void ApplyTolerance(bool[,] mask, int w, int h, int tol, bool horizontal)
    {
        if (horizontal)
        {
            for (int y = 0; y < h; y++)
            {
                int runStart = -1, runLen = 0;
                for (int x = 0; x < w; x++)
                {
                    if (!mask[x, y]) { if (runLen++ == 0) runStart = x; }
                    else if (runLen > 0)
                    {
                        if (runLen <= tol)
                            for (int k = runStart; k < runStart + runLen; k++)
                                mask[k, y] = true;
                        runLen = 0;
                    }
                }
                if (runLen > 0 && runLen <= tol)
                    for (int k = runStart; k < w; k++)
                        mask[k, y] = true;
            }
        }
        else
        {
            for (int x = 0; x < w; x++)
            {
                int runStart = -1, runLen = 0;
                for (int y = 0; y < h; y++)
                {
                    if (!mask[x, y]) { if (runLen++ == 0) runStart = y; }
                    else if (runLen > 0)
                    {
                        if (runLen <= tol)
                            for (int k = runStart; k < runStart + runLen; k++)
                                mask[x, k] = true;
                        runLen = 0;
                    }
                }
                if (runLen > 0 && runLen <= tol)
                    for (int k = runStart; k < h; k++)
                        mask[x, k] = true;
            }
        }
    }
    List<List<Vector2Int>> FindFillableRegions(Texture2D tex, Color fillableColor)
    {
        int width = tex.width;
        int height = tex.height;
        bool[,] visited = new bool[width, height];
        List<List<Vector2Int>> regions = new List<List<Vector2Int>>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Color pixelColor = tex.GetPixel(x, y);
                if (!visited[x, y] && pixelColor.r >= .5f && pixelColor.g >= .5f && pixelColor.b >= .5f)
                {
                    List<Vector2Int> region = new List<Vector2Int>();
                    Queue<Vector2Int> queue = new Queue<Vector2Int>();
                    queue.Enqueue(new Vector2Int(x, y));

                    while (queue.Count > 0)
                    {
                        Vector2Int p = queue.Dequeue();
                        if (p.x < 0 || p.y < 0 || p.x >= width || p.y >= height)
                            continue;

                        if (visited[p.x, p.y])
                            continue;

                        if (tex.GetPixel(p.x, p.y) != fillableColor)
                            continue;

                        visited[p.x, p.y] = true;
                        region.Add(p);

                        queue.Enqueue(new Vector2Int(p.x + 1, p.y));
                        queue.Enqueue(new Vector2Int(p.x - 1, p.y));
                        queue.Enqueue(new Vector2Int(p.x, p.y + 1));
                        queue.Enqueue(new Vector2Int(p.x, p.y - 1));
                    }

                    regions.Add(region);
                }
            }
        }

        return regions;
    }
    List<List<Vector2Int>> FindColorRegions(Texture2D tex, float tolerance = 0.1f)
    {
        int width = tex.width;
        int height = tex.height;

        bool[,] visited = new bool[width, height];
        List<List<Vector2Int>> regions = new List<List<Vector2Int>>();

        // Tüm pikselleri (GetPixels) bir kerede çekmek GetPixel(x,y)'den çok daha hýzlýdýr.
        // Ancak mantýðý bozmamak için senin yapýný koruyarak düzeltiyorum:

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (visited[x, y]) continue;

                Color targetColor = tex.GetPixel(x, y);

                // DÜZELTME 1: Alpha kontrolünü esnettik. 
                // Sadece çok þeffaf olanlarý (0.1'in altý) iþleme alma.
                if (targetColor.a < 0.1f)
                {
                    visited[x, y] = true;
                    continue;
                }

                List<Vector2Int> region = new List<Vector2Int>();
                Queue<Vector2Int> queue = new Queue<Vector2Int>();

                queue.Enqueue(new Vector2Int(x, y));
                visited[x, y] = true; // DÜZELTME 2: Kuyruða ekler eklemez visited yapmalýsýn!

                while (queue.Count > 0)
                {
                    Vector2Int p = queue.Dequeue();
                    region.Add(p);

                    // Komþulara bak (Sað, Sol, Yukarý, Aþaðý)
                    Vector2Int[] neighbors = new Vector2Int[]
                    {
                    new Vector2Int(p.x + 1, p.y),
                    new Vector2Int(p.x - 1, p.y),
                    new Vector2Int(p.x, p.y + 1),
                    new Vector2Int(p.x, p.y - 1)
                    };

                    foreach (Vector2Int n in neighbors)
                    {
                        // Sýnýr kontrolü
                        if (n.x < 0 || n.y < 0 || n.x >= width || n.y >= height)
                            continue;

                        // Zaten ziyaret edildiyse atla
                        if (visited[n.x, n.y])
                            continue;

                        Color currentColor = tex.GetPixel(n.x, n.y);

                        // Renk benzerse ve þeffaf deðilse ekle
                        if (currentColor.a >= 0.5f && ColorsAreSimilar(currentColor, targetColor, tolerance))
                        {
                            visited[n.x, n.y] = true; // Tekrar eklenmemesi için hemen iþaretle
                            queue.Enqueue(n);
                        }
                    }
                }

                if (region.Count > 0)
                {
                    regions.Add(region);
                }
            }
        }

        return regions;
    }
    bool ColorsAreSimilar(Color a, Color b, float tolerance)
    {
        float diff = Mathf.Abs(a.r - b.r) + Mathf.Abs(a.g - b.g) + Mathf.Abs(a.b - b.b) + Mathf.Abs(a.a - b.a);
        return diff <= tolerance;
    }
    Texture2D CreateTextureFromRegion(List<Vector2Int> region, Texture2D originalTex)
    {
        // Bölge sýnýrlarýný bul
        int minX = region.Min(p => p.x);
        int maxX = region.Max(p => p.x);
        int minY = region.Min(p => p.y);
        int maxY = region.Max(p => p.y);

        int width = maxX - minX + 1;
        int height = maxY - minY + 1;

        Texture2D newTex = new Texture2D(width, height);
        newTex.filterMode = FilterMode.Point;

        // Tüm pikselleri transparent yap
        Color[] clearPixels = Enumerable.Repeat(new Color(0, 0, 0, 0), width * height).ToArray();
        newTex.SetPixels(clearPixels);

        // Bölge piksellerini kopyala
        foreach (var p in region)
        {
            Color c = originalTex.GetPixel(p.x, p.y);
            float alpha = c.a; // orijinalin alpha deðeri
            Color whiteWithAlpha;
            if (alpha != 1f)
            {
                whiteWithAlpha = new Color(1f, 1f, 1f, 0);
            }
            else
            {
                whiteWithAlpha = new Color(1f, 1f, 1f, 1);
            }
            newTex.SetPixel(p.x - minX, p.y - minY, whiteWithAlpha);
        }

        newTex.Apply();
        return newTex;
    }
    #endregion
}