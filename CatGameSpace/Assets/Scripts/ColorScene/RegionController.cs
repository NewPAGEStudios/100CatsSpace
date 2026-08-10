using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RegionController : MonoBehaviour
{
    public bool opened = false;
    public bool openedToOpen = false;
    public Color realColor;
    public int colorID;
    GameObject mask;

    Material defaultMaterial;

    private bool hintInterrupt = false;
    private void Start()
    {
        defaultMaterial = ColourControll.Instance.targetMat;
        GetComponent<SpriteRenderer>().material = defaultMaterial;

        mask = transform.GetChild(0).gameObject;
        mask.GetComponent<SpriteMask>().frontSortingOrder = GetComponent<SpriteRenderer>().sortingOrder + 1;
        mask.GetComponent<SpriteMask>().backSortingOrder = GetComponent<SpriteRenderer>().sortingOrder - 1;
        mask.SetActive(false);

    }



    public void ActivateMask(Vector3 worldPos)
    {

        hintInterrupt = true;

        opened = true;

        GetComponent<SpriteRenderer>().material = defaultMaterial;

        transform.GetChild(0).position = worldPos;

        ColourControll.Instance.coloringFeedBackButton(colorID);

        StartCoroutine(MaskOpenRoutine());
    }

    public void LoadIt()
    {
        opened = true;

        GetComponent<SpriteRenderer>().material = defaultMaterial;

        ColourControll.Instance.coloringFeedBackButton(colorID);

        gameObject.SetActive(false);
    }


    public void replayMask(Vector2 wp)
    {
        opened = true;

        GetComponent<SpriteRenderer>().material = defaultMaterial;

        transform.GetChild(0).position = wp;

        StartCoroutine(MaskOpenRoutine());
    }

    public void ForceMask()
    {
        opened = true;

        GetComponent<SpriteRenderer>().material = defaultMaterial;

        gameObject.SetActive(false);
    }


    public void defaultIt()
    {
        opened = false;
        openedToOpen = false;

        transform.GetChild(0).position = Vector3.zero;
        transform.GetChild(0).GetComponent<SpriteMask>().enabled = false;
        transform.GetChild(0).localScale = new Vector3(0, 0, 1);
        transform.GetChild(0).gameObject.SetActive(false);
        transform.GetChild(1).gameObject.SetActive(true);


        gameObject.SetActive(true);
    }

    public void HintIt()
    {
        hintInterrupt = false;
        StartCoroutine(HintAction());
    }


    IEnumerator HintAction()
    {
        float timer = 0f;
        while (!hintInterrupt)
        {
            yield return null;
            if (timer < .75f)
            {
                timer += Time.deltaTime;
            }
            else
            {
                if (GetComponent<SpriteRenderer>().color == Color.yellow)
                {
                    GetComponent<SpriteRenderer>().color = Color.white;
                }
                else
                {
                    GetComponent<SpriteRenderer>().color = Color.yellow;
                }
                timer = 0f;
            }
        }
        GetComponent<SpriteRenderer>().color = Color.white;

    }
    IEnumerator MaskOpenRoutine()
    {
        transform.GetChild(1).gameObject.SetActive(false);

        GetComponent<SpriteRenderer>().maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;

        transform.GetChild(0).localScale = new Vector3(0, 0, 1);

        transform.GetChild(0).gameObject.SetActive(true);
        transform.GetChild(0).GetComponent<SpriteMask>().enabled = true;

        Vector3 target = Vector3.zero;
        if (GetComponent<SpriteRenderer>().sprite.bounds.size.x > GetComponent<SpriteRenderer>().sprite.bounds.size.y)
        {

             target = new Vector3(GetComponent<SpriteRenderer>().sprite.bounds.size.x * 2, GetComponent<SpriteRenderer>().sprite.bounds.size.x * 2, 1);
        }
        else
        {

            target = new Vector3(GetComponent<SpriteRenderer>().sprite.bounds.size.y * 2, GetComponent<SpriteRenderer>().sprite.bounds.size.y * 2, 1);
        }

        float speed = Mathf.Lerp(5f, 45f, target.magnitude / new Vector3(55f, 55f, 1f).magnitude);
        while (true)
        {
            transform.GetChild(0).localScale = Vector3.MoveTowards(transform.GetChild(0).localScale, target, Time.deltaTime * speed);

            if (transform.GetChild(0).localScale == target) break;
            yield return null;
        }

        gameObject.SetActive(false);
    }


}
