using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CatFindController : MonoBehaviour
{
    private static CatFindController _instance;
    public static CatFindController Instance
    {
        get { return _instance; }
    }

    public GameObject canvasOBJ;
    [SerializeField]
    private TextMeshProUGUI percentageOfGame_txt;

    [Header("GamePreferances")]
    [SerializeField]
    private SpriteRenderer WhiteBG_Sprite;
    public Transform catParent;
    [SerializeField]
    private Transform catFindFeedBack;
    [SerializeField]
    private Transform catFindFeedBackNotNull;

    [Header("Prefab")]
    [SerializeField]
    private GameObject ConfetiPrefab;
    private int catRemain;

    public Color desiredCatColor;

//    public int selectedPaint { get; private set; } = 0;
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




    void Start()
    {
        catParent.gameObject.SetActive(true);
    }
    public void StartTheGame()
    {
        desiredCatColor = GameDataTracker.themeColor;

        WhiteBG_Sprite.sprite = GameController.Instance.selectedPaint.lineSprite;
        //Sprite Creation

        float texWidth = GameController.Instance.selectedPaint.catsSprites[0].texture.width;
        float texHeight = GameController.Instance.selectedPaint.catsSprites[0].texture.height;

        Debug.Log("texW: " + texWidth);

        Vector2 sheetPivot = new Vector2(texWidth * 0.5f, texHeight * 0.5f);

        for (int i = 0; i < GameController.Instance.selectedPaint.catsSprites.Length; i++)
        {
            Sprite sprite = GameController.Instance.selectedPaint.catsSprites[i];
            Rect r = sprite.rect;
            Vector2 sliceCenter = new Vector2(r.x + r.width * 0.5f, r.y + r.height * 0.5f);
            Vector2 localOffset = (sliceCenter - sheetPivot) / GameController.Instance.selectedPaint.PPU;
            SpawnSprites(sprite, localOffset, i, GameController.Instance.selectedPaint.catsName[i]);
        }
        catRemain = 0;
        percentageOfGame_txt.text = catRemain.ToString() + " / " + catParent.childCount.ToString();

    }

    public void FindingFeedBack()
    {
        catRemain++;
        percentageOfGame_txt.text = catRemain.ToString() + " / " + catParent.childCount.ToString();

        if (GameController.Instance.isOnline && catRemain >= catParent.childCount) GameController.Instance.EndCatFindSection();
        else if (catRemain >= catParent.childCount && !GameController.Instance.isOnline) GameController.Instance.EndCatFindSection();
    }


    public void ActivateThis()
    {
        percentageOfGame_txt.text = catRemain.ToString() + " / " + catParent.childCount.ToString();
    }


    public void LoadData(bool[] data)
    {

        Debug.Log("LoadingSTART.");

        for (int i = 0; i < data.Length; i++)
        {
            if (data[i]) catParent.GetChild(i).GetComponent<CatController>().LoadIt();
        }

        Debug.Log("Loading END.data number: " + data.Length);

        GameController.Instance.catFinderReady = true;
    }

    private int hintedCat = -1;
    public void HintCat()
    {
        if (hintedCat != -1)
        {
            CameraZoom.Instance.Hint(catParent.GetChild(hintedCat).position);
//            catParent.GetChild(hintedCat).GetComponent<CatController>().HintIt();
        }
        else if (hintedCat == -1)
        {
            if (GameDataTracker.paint_hint > 0)
            {
                List<CatController> avaibleCats = new();

                for (int i = 0; i < catParent.childCount; i++)
                {
                    if (!catParent.GetChild(i).GetComponent<CatController>().found)
                    {
                        avaibleCats.Add(catParent.GetChild(i).GetComponent<CatController>());
                    }
                }

                hintedCat = avaibleCats[UnityEngine.Random.Range(0, avaibleCats.Count)].transform.GetSiblingIndex();

                CameraZoom.Instance.Hint(catParent.GetChild(hintedCat).position);
                catParent.GetChild(hintedCat).GetComponent<CatController>().HintIt();
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
            //else if (GameDataTracker.paint_hint > 0)
            //{
            //    List<CatController> avaibleCats = new();

            //    for (int i = 0; i < catParent.childCount; i++)
            //    {
            //        if (!catParent.GetChild(i).GetComponent<CatController>().found)
            //        {
            //            avaibleCats.Add(catParent.GetChild(i).GetComponent<CatController>());
            //        }
            //    }

            //    hintedCat = avaibleCats[UnityEngine.Random.Range(0, avaibleCats.Count)].transform.GetSiblingIndex();

            //    CameraZoom.Instance.Hint(catParent.GetChild(hintedCat).position);
            //    catParent.GetChild(hintedCat).GetComponent<CatController>().HintIt();
            //    GameDataTracker.paint_hint -= 1;
            //    GameController.Instance.HintTxtUpdate();

            //    if (GameController.Instance.isOnline)
            //    {
            //        GameController.Instance.LocalPlayerOnlinePlayer.CmdCallHintDataChange();
            //    }

            //}
        }
        else if(GameDataTracker.paint_hint <= 0)
        {
            Debug.Log("NoHint");
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

        for (int i = 0; i < catParent.childCount; i++)
        {
            if (!catParent.GetChild(i).GetComponent<RegionController>().opened)
            {
                avaibleCats.Add(catParent.GetChild(i).GetComponent<RegionController>());
            }
        }

        hintedCat = avaibleCats[UnityEngine.Random.Range(0, avaibleCats.Count)].transform.GetSiblingIndex();

        CameraZoom.Instance.Hint(catParent.GetChild(hintedCat).position);
        catParent.GetChild(hintedCat).GetComponent<RegionController>().HintIt();

        Online_HintRoutine = null;
    }

    public void AutoCat()
    {
        for (int i = 0; i < catParent.childCount; i++)
        {
            if (!catParent.GetChild(i).GetComponent<CatController>().found)
            {
                GameDataTracker.paint_findCatData[i] = true;

                catParent.GetChild(i).GetComponent<CatController>().FindIt();
                return;
            }
        }
    }

    public void FindCat(Vector2 wp)
    {
        Collider2D[] hits = Physics2D.OverlapPointAll(wp);
        if (hits != null)
        {
            foreach (Collider2D hit in hits)
            {
                if (hit.TryGetComponent<CatController>(out CatController cc) && !cc.found)
                {
                    SoundActivate(cc);

                    if (GameController.Instance.isOnline && GameController.Instance.LocalPlayerOnlinePlayer.connectionID == 0)
                    {
                        GameDataTracker.paint_findCatData[cc.transform.GetSiblingIndex()] = true;
                        Debug.Log("Cat Find: " + cc.transform.GetSiblingIndex());
                    }
                    else if (!GameController.Instance.isOnline)
                    {
                        GameDataTracker.paint_findCatData[cc.transform.GetSiblingIndex()] = true;
                        Debug.Log("Cat Find: " + cc.transform.GetSiblingIndex());
                    }


                    #region SteamAchivements
                    SteamAchivementController.Instance.TryUnlockAchivement("ACH_00");
                    #endregion


                    if (cc.transform.GetSiblingIndex() == hintedCat) hintedCat = -1;


                    cc.FindIt();

                    if (GameController.Instance.isOnline)
                    {
                        GameController.Instance.LocalPlayerOnlinePlayer.CmdClickAction(cc.transform.GetSiblingIndex(), (int)StateManager.GameMode.findCat,wp);
                        Debug.Log("Cat Find: " + cc.transform.GetSiblingIndex());
                    }

                    break;
                }
            }
        }

    }

    public void ForceFindCat(int id)
    {
        CatController cc = catParent.transform.GetChild(id).GetComponent<CatController>();

        if (!cc.found)
        {
            SoundActivate(cc);
            if (GameController.Instance.LocalPlayerOnlinePlayer.connectionID == 0)
            {
                GameDataTracker.paint_findCatData[cc.transform.GetSiblingIndex()] = true;
                Debug.Log("Cat Force Find: " + cc.transform.GetSiblingIndex());
            }

            if (cc.transform.GetSiblingIndex() == hintedCat) hintedCat = -1;

            cc.FindIt();

        }
    }


    public void SoundActivate(CatController cc)
    {
        if (GameController.Instance.selectedPaint.catsSounds.Length <= cc.transform.GetSiblingIndex() && GameController.Instance.selectedPaint.catsSounds[cc.transform.GetSiblingIndex()] != null)
        {
            catFindFeedBackNotNull.GetComponent<AudioSource>().clip = GameController.Instance.selectedPaint.catsSounds[cc.transform.GetSiblingIndex()];
            catFindFeedBackNotNull.GetComponent<AudioSource>().Play();
        }
        else
        {
            catFindFeedBack.GetChild(UnityEngine.Random.Range(0, catFindFeedBack.childCount)).GetComponent<AudioSource>().Play();
        }
    }


    #region internal
    void SpawnSprites(Sprite sp, Vector2 wp, int id, string catName)
    {
        GameObject go = new GameObject("Cat" + id.ToString());
        go.transform.position = wp;
        go.transform.parent = catParent;
        GameObject conf = Instantiate(ConfetiPrefab, go.transform);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sp;
        sr.sortingOrder = 9999;
        sr.color = Color.white;

        PolygonCollider2D pol = go.AddComponent<PolygonCollider2D>();
        pol.isTrigger = true;

        CatController cc = go.AddComponent<CatController>();
        cc.catName = catName;
        cc.Confeti = conf;
        cc.catColor = desiredCatColor;
    }



    #endregion


}
