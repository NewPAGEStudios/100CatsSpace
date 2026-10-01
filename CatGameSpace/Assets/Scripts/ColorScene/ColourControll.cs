using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ColourControll : MonoBehaviour
{
    private static ColourControll _instance;
    public static ColourControll Instance
    {
        get { return _instance; }
    }


    public GameObject canvasOBJ;

    [SerializeField]
    private TextMeshProUGUI percentageOfGame_txt;

    public Material targetMat;
    [Header("GamePreferances")]
    [SerializeField]
    private SpriteRenderer colored_Sprite;
    [SerializeField]
    private SpriteRenderer lined_Sprite;
    [SerializeField]
    public Transform regionParent;
    [SerializeField]
    private GameObject maskObje;
    [SerializeField]
    private AudioSource paintFeedBack;
    public int colorToFillRemain { get; private set; }

    public bool colourMenuOpenned { get; private set; }
    private int[] remainingUniqueColourOnSprite;

    public Material mat_openRegion;
    private Material mat_NotOpenRegion;
    [Header("ClickActionExtras")]
    public float circleRadius;
    [Header("TxtPreferances")]
    public GameObject txtPrefab;
    [Header("UniqueColorPreferances")]
    public GameObject buttonPrefab;
    public Transform uniqueColorsParent;
    public GameObject uniqueColorSelectionPanel;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }
    private void Start()
    {
        regionParent.gameObject.SetActive(true);
        lined_Sprite.gameObject.SetActive(true);
    }

    public void StartTheGame()
    {
        mat_NotOpenRegion = targetMat;

        colored_Sprite.sprite = GameController.Instance.selectedPaint.colorSpriteBase;
        lined_Sprite.sprite = GameController.Instance.selectedPaint.lineSprite;

        remainingUniqueColourOnSprite = GameController.Instance.selectedPaint.regionColorUniqueMax.ToArray();

        Debug.Log(GameController.Instance.selectedPaint.atlas.spriteCount);
        for (int i = 0; i < GameController.Instance.selectedPaint.regionSprites.Count; i++)
        {
            SpawnSprites(
                GameController.Instance.selectedPaint.atlas.GetSprite("spriteRegion" + i),
                WorldPosFromPixelCoords(GameController.Instance.selectedPaint.mins[i].x, GameController.Instance.selectedPaint.mins[i].y, colored_Sprite),
                i,
                GameController.Instance.selectedPaint.regionColor[i],
                GameController.Instance.selectedPaint.regionColorUnique.IndexOf(GameController.Instance.selectedPaint.regionColor[i]),
                GameController.Instance.selectedPaint.regionTextPosCollector[i],
                GameController.Instance.selectedPaint.regionTextSquareSizeLocal[i]
                );
        }

        for (int k = 0; k < GameController.Instance.selectedPaint.regionColorUnique.Count; k++)
        {
            int butID = k;
            GameObject goo = Instantiate(buttonPrefab, uniqueColorsParent);
            goo.name = "ColorID:" + butID;



            goo.GetComponent<Image>().color = GameController.Instance.selectedPaint.regionColorUnique[butID];

            goo.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = butID.ToString();
            goo.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = remainingUniqueColourOnSprite[butID].ToString();

            if (IsColorDark(goo.GetComponent<Image>().color))
            {
                goo.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = Color.white;
                goo.transform.GetChild(1).GetComponent<TextMeshProUGUI>().color = Color.white;
            }
            else
            {
                goo.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = Color.black;
                goo.transform.GetChild(1).GetComponent<TextMeshProUGUI>().color = Color.black;
            }

            goo.GetComponent<Button>().onClick.AddListener(() => openToOpenAll(butID, goo));
        }

        uniqueColorSelectionPanel.GetComponent<ColorsMenuHandle>().SetColorMenuProperties();


        colorToFillRemain = regionParent.childCount;
        float percentage = ((float)regionParent.childCount - (float)colorToFillRemain) / (float)regionParent.childCount;
        percentage *= 100;
        percentageOfGame_txt.text = Mathf.FloorToInt(percentage) + "%";
    }
    public void LoadData(bool[] data)
    {   
        for (int i = 0; i < data.Length; i++)
        {
            if (data[i]) regionParent.GetChild(i).GetComponent<RegionController>().LoadIt();
        }
        GameController.Instance.colourControllerReady = true;
    }


    #region InspectorReach

    #endregion

    public void coloringFeedBackButton(int id)
    {
        colorToFillRemain--;


        float percentage = ((float)regionParent.childCount - (float)colorToFillRemain) / (float)regionParent.childCount;
        percentage *= 100;
        percentageOfGame_txt.text = Mathf.FloorToInt(percentage).ToString() + "%";

        if (colorToFillRemain <= 0 && GameController.Instance.gameMode != StateManager.GameMode.findCat)
        {
            GameController.Instance.EndGame();
        }

        remainingUniqueColourOnSprite[id]--;
        uniqueColorSelectionPanel.GetComponent<ColorsMenuHandle>().UpdateColors(id, remainingUniqueColourOnSprite[id].ToString());
        if (remainingUniqueColourOnSprite[id] <= 0)
        {
            //gameGeneralFeedBack
        }
    }

    public void isGameEnded()
    {
        if (colorToFillRemain <= 0)
        {
            GameController.Instance.EndGame();
        }
    }

    public void ActivateThis()
    {
        canvasOBJ.SetActive(true);
        EventSystem.current.GetComponent<EventSystemController>().SetPanel("RegionColorPanel");

        GameController.Instance.OpenColourMenu();

        float percentage = ((float)regionParent.childCount - (float)colorToFillRemain) / (float)regionParent.childCount;
        percentage *= 100;
        percentageOfGame_txt.text = Mathf.FloorToInt(percentage) + "%";
    }


    public void openToOpenAll(int id, GameObject go)
    {
        //UI Booom
        for(int c = 0; c < uniqueColorsParent.transform.childCount; c++)
        {
            uniqueColorsParent.transform.GetChild(c).GetComponent<Outline>().enabled = false;
        }
        go.GetComponent<Outline>().enabled = true;


        //Function
        closeToOpenAll();
        uniqueColorSelectionPanel.GetComponent<ColorsMenuHandle>().opennedColor = go.transform.GetSiblingIndex();
        for (int i = 0;i<regionParent.transform.childCount;i++)
        {
            if (regionParent.transform.GetChild(i).GetComponent<RegionController>().colorID == id)
            {
                regionParent.transform.GetChild(i).GetComponent<RegionController>().openedToOpen = true;
                regionParent.transform.GetChild(i).GetComponent<SpriteRenderer>().material = mat_openRegion;
            }
        }
        uniqueColorSelectionPanel.GetComponent<ColorsMenuHandle>().UpdatePage();
    }

    public void closeToOpenAll()
    {
        for (int i = 0; i < regionParent.transform.childCount; i++)
        {
            regionParent.transform.GetChild(i).GetComponent<RegionController>().openedToOpen = false;
            regionParent.transform.GetChild(i).GetComponent<SpriteRenderer>().material = mat_NotOpenRegion;
        }
    }

    public void OpenRegion(Vector2 wp)
    {
        Collider2D[] hits = Physics2D.OverlapPointAll(wp);
        if (hits == null) return;
        foreach (Collider2D hit in hits)
        {

            if (hit.TryGetComponent<RegionController>(out RegionController rc) && !rc.opened && !rc.openedToOpen)
            {
                if (!isPointAlpha(rc.GetComponent<SpriteRenderer>(), wp)) continue;

                GameObject colorElement = uniqueColorSelectionPanel.GetComponent<ColorsMenuHandle>().GetColorElement(rc.colorID);
                if (colorElement == null)
                {
                    return;
                }
                else
                {
                    openToOpenAll(rc.colorID, colorElement);
                }
                break;
            }
        }

    }
    private int hintedRegion = -1;
    public void HintRegion()
    {

        if (hintedRegion != -1)
        {
            CameraZoom.Instance.Hint(regionParent.GetChild(hintedRegion).position);
//            regionParent.GetChild(hintedRegion).GetComponent<RegionController>().HintIt();
        }
        else if (hintedRegion == -1)
        {
            if (GameDataTracker.paint_hint > 0)
            {
                List<RegionController> avaibleCats = new();

                for (int i = 0; i < regionParent.childCount; i++)
                {
                    if (!regionParent.GetChild(i).GetComponent<RegionController>().opened)
                    {
                        avaibleCats.Add(regionParent.GetChild(i).GetComponent<RegionController>());
                    }
                }

                hintedRegion = avaibleCats[Random.Range(0, avaibleCats.Count)].transform.GetSiblingIndex();

                CameraZoom.Instance.Hint(regionParent.GetChild(hintedRegion).position);
                regionParent.GetChild(hintedRegion).GetComponent<RegionController>().HintIt();
                GameDataTracker.paint_hint -= 1;
                GameController.Instance.HintTxtUpdate();
            }
            //if (GameController.Instance.isOnline && GameController.Instance.LocalPlayerOnlinePlayer.connectionID != 0)
            //{
            //    if (Online_HintRoutine != null)
            //    {
            //        Debug.Log("CallEvent is not over yet");
            //        return;
            //    }
            //    Online_HintRoutine = StartCoroutine(HintActionRoutine());
            //}
            //else if(GameDataTracker.paint_hint > 0)
            //{
            //    List<RegionController> avaibleCats = new();

            //    for (int i = 0; i < regionParent.childCount; i++)
            //    {
            //        if (!regionParent.GetChild(i).GetComponent<RegionController>().opened)
            //        {
            //            avaibleCats.Add(regionParent.GetChild(i).GetComponent<RegionController>());
            //        }
            //    }

            //    hintedRegion = avaibleCats[Random.Range(0, avaibleCats.Count)].transform.GetSiblingIndex();

            //    CameraZoom.Instance.Hint(regionParent.GetChild(hintedRegion).position);
            //    regionParent.GetChild(hintedRegion).GetComponent<RegionController>().HintIt();
            //    GameDataTracker.paint_hint -= 1;
            //    GameController.Instance.HintTxtUpdate();

            //    if (GameController.Instance.isOnline)
            //    {
            //        GameController.Instance.LocalPlayerOnlinePlayer.CmdCallHintDataChange();
            //    }
            //}
            else if (GameDataTracker.paint_hint <= 0)
            {
                Debug.Log("NoHint");
            }

        }
    }
    [HideInInspector]
    public bool HintDataGot = false;
    [HideInInspector]
    public bool HintPermission = false;
    private Coroutine Online_HintRoutine = null;
    private IEnumerator HintActionRoutine()
    {
        HintDataGot = false;
        GameController.Instance.LocalPlayerOnlinePlayer.CmdHintCall();

        while (true)
        {
            if (HintDataGot) break;
            yield return null;
        }
        if (!HintPermission) yield break;

        List<RegionController> avaibleCats = new();

        for (int i = 0; i < regionParent.childCount; i++)
        {
            if (!regionParent.GetChild(i).GetComponent<RegionController>().opened)
            {
                avaibleCats.Add(regionParent.GetChild(i).GetComponent<RegionController>());
            }
        }

        hintedRegion = avaibleCats[Random.Range(0, avaibleCats.Count)].transform.GetSiblingIndex();

        CameraZoom.Instance.Hint(regionParent.GetChild(hintedRegion).position);
        regionParent.GetChild(hintedRegion).GetComponent<RegionController>().HintIt();
        GameDataTracker.paint_hint -= 1;
        GameController.Instance.HintTxtUpdate();

        Online_HintRoutine = null;
    }

    public void AutoRegion()
    {
        for (int i = 0; i < regionParent.childCount; i++)
        {
            if (!regionParent.GetChild(i).GetComponent<RegionController>().opened)
            {
                GameObject colorElement = uniqueColorSelectionPanel.GetComponent<ColorsMenuHandle>().GetColorElement(regionParent.GetChild(i).GetComponent<RegionController>().colorID);
                openToOpenAll(regionParent.GetChild(i).GetComponent<RegionController>().colorID, colorElement);

                ReplayController.Instance.RuntimeSaveReplay(regionParent.GetChild(i).transform.position, regionParent.GetChild(i).GetComponent<RegionController>().transform.GetSiblingIndex());
                GameDataTracker.paint_regionFillData[regionParent.GetChild(i).GetComponent<RegionController>().transform.GetSiblingIndex()] = true;

                regionParent.GetChild(i).GetComponent<RegionController>().ActivateMask(regionParent.GetChild(i).transform.position);
                return;
            }
        }

    }
    public void PaintRegion(Vector2 wp)
    {
        o = wp;
        Collider2D[] hits = Physics2D.OverlapPointAll(wp);

        if (hits == null) return;
        foreach (Collider2D hit in hits)
        {
            if (hit.TryGetComponent<RegionController>(out RegionController rc))//!rc.opened && rc.openedToOpen
            {
                if (!isPointAlpha(rc.GetComponent<SpriteRenderer>(), wp))continue;

                //if (colourMenuOpenned)
                //{
                //    CloseColourMenu();
                //}

                if(!rc.opened && rc.openedToOpen)
                {

                    ActivateSound();
                    SteamAchivementController.Instance.TryUnlockAchivement("ACH_02");


                    if (GameController.Instance.isOnline && GameController.Instance.LocalPlayerOnlinePlayer.connectionID == 0)
                    {
                        ReplayController.Instance.RuntimeSaveReplay(wp,rc.transform.GetSiblingIndex());
                        GameDataTracker.paint_regionFillData[rc.transform.GetSiblingIndex()] = true;
                    }
                    else if (!GameController.Instance.isOnline)
                    {
                        ReplayController.Instance.RuntimeSaveReplay(wp, rc.transform.GetSiblingIndex());
                        GameDataTracker.paint_regionFillData[rc.transform.GetSiblingIndex()] = true;
                    }

                    if (hintedRegion == rc.transform.GetSiblingIndex()) hintedRegion = -1;



                    rc.ActivateMask(wp);

                    if (GameController.Instance.isOnline)
                    {
                        GameController.Instance.LocalPlayerOnlinePlayer.CmdClickAction(rc.transform.GetSiblingIndex(), (int)StateManager.GameMode.fillColor,wp);
                    }

                    break;
                }
            }
        }
    }


    public void ForcePaintRegion(int id,Vector2 wp)//Online
    {
        RegionController rc = regionParent.transform.GetChild(id).GetComponent<RegionController>();

        if (!rc.opened)
        {
            ActivateSound();
            if (GameController.Instance.LocalPlayerOnlinePlayer.connectionID == 0)
            {
                ReplayController.Instance.RuntimeSaveReplay(wp,rc.transform.GetSiblingIndex());
                GameDataTracker.paint_regionFillData[rc.transform.GetSiblingIndex()] = true;
            }

            if (hintedRegion == rc.transform.GetSiblingIndex()) hintedRegion = -1;

            rc.ActivateMask(wp);

        }
    }



    public void ActivateSound()
    {
//        paintFeedBack.Play();
    }
    private Vector2 o;
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        Gizmos.DrawWireSphere(o, circleRadius);
    }
    #region internalFunctions


    void SpawnSprites(Sprite spr, Vector2 wp,int id,Color realColora, int colorID, Vector2 localPosOfTXT, float squareSizeLocal)
    {
        GameObject go = new GameObject("RegionSprite" + id.ToString());
        go.transform.position = wp;
        go.transform.parent = regionParent;

        Instantiate(maskObje, go.transform);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.color = GameController.Instance.selectedPaint.ColoredSectionBG;
//        Debug.Log(spr.name);
        sr.sprite = spr;
        sr.sortingOrder = (id * 2) + 10;
        sr.maskInteraction = SpriteMaskInteraction.None;

        BoxCollider2D box = go.AddComponent<BoxCollider2D>();
        box.isTrigger = true;
        box.offset = spr.bounds.size / 2;
        box.size = spr.bounds.size;


        RegionController rc = go.AddComponent<RegionController>();
        rc.realColor = realColora;
        rc.colorID = colorID;

        GameObject txtObject = Instantiate(txtPrefab, go.transform);
        txtObject.name = "FittedTMP" + id.ToString();
        txtObject.transform.localPosition = localPosOfTXT;
        txtObject.GetComponent<TextMeshPro>().text = colorID.ToString();
        txtObject.GetComponent<TextMeshPro>().alignment = TextAlignmentOptions.Center;
        txtObject.GetComponent<TextMeshPro>().color = GameDataTracker.themeColor;
        txtObject.GetComponent<TextMeshPro>().color =
            new Color(txtObject.GetComponent<TextMeshPro>().color.r - 0.3921f, txtObject.GetComponent<TextMeshPro>().color.g - 0.3921f, txtObject.GetComponent<TextMeshPro>().color.b - 0.3921f);

        txtObject.GetComponent<TextMeshPro>().ForceMeshUpdate();
        Vector2 textSize = txtObject.GetComponent<TextMeshPro>().GetRenderedValues(false);
        if (textSize.x > 0 && textSize.y > 0)
        {
            float wF = squareSizeLocal / textSize.x;
            float hF = squareSizeLocal / textSize.y;
            txtObject.GetComponent<TextMeshPro>().fontSize *= Mathf.Min(wF, hF);
        }
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
    bool IsColorDark(Color color)
    {
        // W3C'ye göre algılanan parlaklık (Relative Luminance) hesaplaması
        double luminance = (0.2126 * color.r) + (0.7152 * color.g) + (0.0722 * color.b);

        // Eşik değeri 0.5, daha küçükse koyu, büyükse açık renk olarak kabul edilir
        return luminance < 0.5;
    }

    bool isPointAlpha(SpriteRenderer sr, Vector2 wp)
    {
        Sprite sprite = sr.sprite;
        Texture2D tex = sprite.texture;

        // Dünya koordinatından lokal sprite koordinatına geç
        Vector2 local = sr.transform.InverseTransformPoint(wp);

        // Sprite'ın ölçüsüne göre texture pozisyonu (pivot sol alt!)
        float px = local.x * sprite.pixelsPerUnit + sprite.textureRect.x;
        float py = local.y * sprite.pixelsPerUnit + sprite.textureRect.y;

        int x = Mathf.FloorToInt(px);
        int y = Mathf.FloorToInt(py);

        // Güvenlik kontrolü
        if (x < 0 || y < 0 || x >= tex.width || y >= tex.height)
            return false;

        Color pixel = tex.GetPixel(x, y); // Texture readable olmalı!
        return pixel.a == 1f;
    }
    #endregion
}