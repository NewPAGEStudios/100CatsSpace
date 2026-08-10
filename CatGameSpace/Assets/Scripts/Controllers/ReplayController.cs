using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReplayController : MonoBehaviour
{
    private static ReplayController _instance;
    public static ReplayController Instance
    {
        get { return _instance; }
    }
    public GameObject CanvasObj;

    public float replayPerSec = 0.005f;
    public bool hostInterrupt { get; private set; } = false;
    public Slider replaySpeedSlier;
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
        //ChangeSpeedOfReplay();
    }
    public void StartReplay(Vector2[] wp, int[] itemID)
    {
        GameController.Instance.FindCatParentOBJCollection.transform.GetChild(0).gameObject.SetActive(false);

        for (int i = 0;i < ColourControll.Instance.regionParent.childCount;i++)
        {
            ColourControll.Instance.regionParent.GetChild(i).GetComponent<RegionController>().defaultIt();
        }

        ColourControll.Instance.closeToOpenAll();

        hostInterrupt = false;
        StartCoroutine(ReplayRoutine());

        Debug.Log("Gamedatatracker replay count: " + GameDataTracker.replay_itemID.Count);
    }

    //public void ChangeSpeedOfReplay()
    //{
    //    string repl = Mathf.Lerp(replaySpeedSlier.minValue, replaySpeedSlier.maxValue, (replaySpeedSlier.maxValue - replaySpeedSlier.value) / (replaySpeedSlier.maxValue - replaySpeedSlier.minValue)).ToString();

    //    replayPerSec = replaySpeedSlier.value;
    //}

    IEnumerator ReplayRoutine()
    {
        int counter = 0;
        while (true)
        {
            if (hostInterrupt)
            {
                yield break;
            }
            ColourControll.Instance.ActivateSound();
            ColourControll.Instance.regionParent.GetChild(GameDataTracker.replay_itemID[counter]).GetComponent<RegionController>().replayMask(GameDataTracker.replay_paintPos[counter]);
            counter++;
            yield return new WaitForSeconds(replayPerSec);

            if (counter >= GameDataTracker.replay_itemID.Count)
            {
                break;
            }
        }
        if (hostInterrupt)
        {
            yield break;
        }
        while (true)
        {
            if (hostInterrupt)
            {
                yield break;
            }
            for (int i = 0;i < ColourControll.Instance.regionParent.childCount; i++)
            {
                if (ColourControll.Instance.regionParent.GetChild(i).gameObject.activeSelf)
                {
                    yield return null;
                    i = 0;
                    continue;
                }
            }
            break;
        }
        StopReplay();

    }


    public void StopReplay()
    {
        hostInterrupt = true;
        StartCoroutine(bumba());
    }

    IEnumerator bumba()
    {
        yield return null;
        for(int i = 0; i < ColourControll.Instance.regionParent.childCount; i++)
        {
            ColourControll.Instance.regionParent.GetChild(i).GetComponent<RegionController>().ForceMask();
        }
        CanvasObj.SetActive(false);
        GameController.Instance.fromReplay();
    }


    public void RuntimeSaveReplay(Vector2 wp,int itemID)
    {

        GameDataTracker.replay_paintPos.Add(wp);
        GameDataTracker.replay_itemID.Add(itemID);
    }



}
