using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GemBonusUI : MonoBehaviour
{
    [Serializable]
    private class GemSpriteMap
    {
        [SerializeField]
        private GemType type;
        [SerializeField]
        private Sprite sprite;
        public GemType Type => type;
        public Sprite Sprite => sprite;
    }
    [SerializeField]
    private GameObject GemBonusPrefab;
    [SerializeField]
    private List<GemSpriteMap> spriteMaps;
    private List<GemBonusItem> gemBonusItems;
    private List<GameObject> allBtn;
    private int verticalQuantity;
    private int horizontalQuantity;
    private int bombQuantity;
    private int colorQuantity;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        allBtn = new List<GameObject>();
        gemBonusItems = new List<GemBonusItem>();
        verticalQuantity = PlayerPrefs.GetInt(Board.KEY_VERTICAL, 1);
        gemBonusItems.Add(new GemBonusItem(GemType.Vertical, verticalQuantity));
        horizontalQuantity = PlayerPrefs.GetInt(Board.KEY_HORIZONTAL, 1);
        gemBonusItems.Add(new GemBonusItem(GemType.Horizontal, horizontalQuantity));
        bombQuantity = PlayerPrefs.GetInt(Board.KEY_BOMB, 1);
        gemBonusItems.Add(new GemBonusItem(GemType.SmallBomb, bombQuantity));
        colorQuantity = PlayerPrefs.GetInt(Board.KEY_COLOR, 1);
        gemBonusItems.Add(new GemBonusItem(GemType.Color, colorQuantity));
        Board.Instance.UpdateBonusItem += BoardUpdateBonusItem;
        UiManager.OnGemBonusChanged += UiManager_OnGemBonusChanged;
        Image tempImage;
        Image tempGemBonus;
        TextMeshProUGUI quantityText;
        foreach (var item in gemBonusItems)
        {
            GameObject tempGemBonusBtn = Instantiate(GemBonusPrefab, this.transform);
            allBtn.Add(tempGemBonusBtn);
            tempGemBonusBtn.GetComponent<Button>().onClick.AddListener(() => OnClick(item.GemType, tempGemBonusBtn.transform));
            tempGemBonus = tempGemBonusBtn.GetComponentInChildren<Image>();
            tempImage = tempGemBonus.transform.Find("GemBonusImage").GetComponentInChildren<Image>();
            quantityText = tempGemBonus.GetComponentInChildren<TextMeshProUGUI>();
            quantityText.text = item.Quantity.ToString();
            foreach(var item2 in spriteMaps)
            {
                if(item.GemType == item2.Type)
                {
                    tempImage.sprite = item2.Sprite;
                    break;
                }
            } 
        }
        CheckItemCanUse();
    }

    private void UiManager_OnGemBonusChanged(GemType arg1, int arg2)
    {
        for (int i = 0; i < gemBonusItems.Count; i++)
        {
            if (gemBonusItems[i].GemType == arg1)
            {
                gemBonusItems[i].Quantity = arg2;
                Image tempImage = allBtn[i].GetComponentInChildren<Image>();
                TextMeshProUGUI text = tempImage.GetComponentInChildren<TextMeshProUGUI>();
                text.text = gemBonusItems[i].Quantity.ToString();
                break;
            }
        }
        CheckItemCanUse();
    }

    private void BoardUpdateBonusItem(GemType obj)
    {
        ResetAll();
        for (int i = 0; i < gemBonusItems.Count; i++) 
        {
            if (gemBonusItems[i].GemType == obj)
            {
                gemBonusItems[i].Quantity--;
                if (GemType.Vertical == obj)
                {
                    PlayerPrefs.SetInt(Board.KEY_VERTICAL, gemBonusItems[i].Quantity);
                }
                else if(GemType.Horizontal == obj)
                {
                    PlayerPrefs.SetInt(Board.KEY_HORIZONTAL, gemBonusItems[i].Quantity);
                }
                else if(GemType.SmallBomb == obj)
                {
                    PlayerPrefs.SetInt(Board.KEY_BOMB, gemBonusItems[i].Quantity);
                }
                else
                {
                    PlayerPrefs.SetInt(Board.KEY_COLOR, gemBonusItems[i].Quantity);
                }
                Image tempImage = allBtn[i].GetComponentInChildren<Image>();
                TextMeshProUGUI text = tempImage.GetComponentInChildren<TextMeshProUGUI>();
                text.text = gemBonusItems[i].Quantity.ToString();
                if(gemBonusItems[i].Quantity == 0)
                {
                    allBtn[i].GetComponent<Button>().interactable = false;
                }
                break;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void CheckItemCanUse()
    {
        for (int i = 0; i < gemBonusItems.Count; i++)
        {
            if (gemBonusItems[i].Quantity <= 0)
            {
                allBtn[i].GetComponent <Button>().interactable = false;
            }
            else
            {
                allBtn[i].GetComponent<Button>().interactable = true;
            }

        }
    }

    void OnClick(GemType gemType, Transform transform)
    {
        ResetAll();
        if (gemType == Board.Instance.GemBonus) 
        {
            Board.Instance.GemBonus = GemType.Blue;
        }
        else
        {
            StartCoroutine(Scale(transform, true));
            AudioManager.Instance.OnClickGem();
            Board.Instance.GemBonus = gemType;
        }
    }

    void ResetAll()
    {
        foreach(var btn in allBtn)
        {
            StartCoroutine(Scale(btn.transform, false));
        }
    }

    IEnumerator Scale(Transform transform, bool up)
    {
        Vector3 start;
        Vector3 target;
        if (up)
        {
            start = transform.localScale;
            target = Vector3.one * 1.2f;
        }
        else
        {
            start = transform.localScale;
            target = Vector3.one;
        }
        float t = 0;

        while (t < 1)
        {
            t += Time.deltaTime * 5;
            float smoothT = t * t * (3f - 2f * t);
            transform.localScale = Vector3.Lerp(start, target, smoothT);
            yield return null;
        }
        transform.localScale = target;
    }
}
