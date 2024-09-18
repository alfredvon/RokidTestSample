using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExpandUI : MonoBehaviour
{
    [SerializeField]
    private RectTransform layoutRoot;
    [SerializeField]
    private RectTransform content;
    [SerializeField]
    private float expandTime = 0.2f;
    private bool isExpand;

    public void TriggerExpand()
    {
        if (isExpand)
        {
            Shrink();
        }
        else
        {
            Expand();
        }
    }

    public void Expand()
    {
        isExpand = true;        
        RectTransform rectTransform = transform as RectTransform;
        Vector2 start = new Vector2(rectTransform.sizeDelta.x, 0);
        Vector2 end = new Vector2(rectTransform.sizeDelta.x, content.rect.height);
        StartCoroutine(ExpandCoroutine(rectTransform,start, end));
    }

    public void Shrink()
    {
        isExpand = false;        
        RectTransform rectTransform = transform as RectTransform;
        Vector2 start = new Vector2(rectTransform.sizeDelta.x, content.rect.height);
        Vector2 end = new Vector2(rectTransform.sizeDelta.x, 0);
        StartCoroutine(ExpandCoroutine(rectTransform, start, end));
    }

    private IEnumerator ExpandCoroutine(RectTransform rectTransform, Vector2 start,Vector2 end)
    {
        float currentTime = 0;
        rectTransform.sizeDelta = start; 

        while(currentTime < expandTime)
        {
            currentTime += Time.deltaTime;
            rectTransform.sizeDelta = Vector2.Lerp(rectTransform.sizeDelta,end, currentTime/expandTime);
            if(layoutRoot != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(layoutRoot);
            }            
            yield return null;
        }
    }
}
