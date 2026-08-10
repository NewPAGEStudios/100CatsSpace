using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonAnimation : MonoBehaviour
{
    private float targetBlend = 0;
    private void Start()
    {
        CloseAnim();
    }
    public void OpenAnim()
    {
        targetBlend = 1;
    }

    public void CloseAnim()
    {
        targetBlend = 0;
    }

    private void Update()
    {
        GetComponent<Animator>().SetFloat("Blend", Mathf.Lerp(GetComponent<Animator>().GetFloat("Blend"), targetBlend, Time.deltaTime * 10f));
    }
}
