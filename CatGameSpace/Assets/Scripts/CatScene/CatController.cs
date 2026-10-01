using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CatController : MonoBehaviour
{
    public bool found = false;
    public string catName;
    public GameObject Confeti;

    public Color catColor;

    private bool hintInterrupt = false;
    public void FindIt()
    {
        CatFindController.Instance.FindingFeedBack();
        hintInterrupt = true;
        found = true;
        GetComponent<SpriteRenderer>().color = catColor;
        StartCoroutine(FindCatRoutine());
    }

    public void ReplayIt()
    {
        found = true;
        GetComponent<SpriteRenderer>().color = catColor;
        StartCoroutine(FindCatRoutine());
    }

    public void LoadIt()
    {
        CatFindController.Instance.FindingFeedBack();
        found = true;
        GetComponent<SpriteRenderer>().color = catColor;
    }

    IEnumerator FindCatRoutine()//Animation
    {
        yield return null;
        GetComponent<SpriteRenderer>().color = catColor;
        Confeti.GetComponent<ParticleSystem>().Play();
    }

    public void DefaultIt()
    {
        found = false;
        GetComponent<SpriteRenderer>().color = Color.white;
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
            if(timer < .75f)
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

    }

}
