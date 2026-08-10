using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InfoPopUpItem : MonoBehaviour
{
    public MapParent mapParent;
    [Space(25)]
    public TextMeshProUGUI mapName;
    [Space(25)]
    public GameObject NormalSection;
    public TextMeshProUGUI comingSoonTxt;
    public TextMeshProUGUI CorruptedInformer;

    [Space(25)]

    public TextMeshProUGUI statistic_normal_catfound;
    public TextMeshProUGUI statistic_normal_regionpainted;

    private void Awake()
    {
        if (mapParent.NormalID == "Sahte") 
        {
            comingSoonTxt.gameObject.SetActive(true);
            NormalSection.SetActive(false);
        }
    }




}