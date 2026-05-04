using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GoalUiManager : MonoBehaviour
{
    [SerializeField]
    private GameObject goalItemPrefab;
    [SerializeField]
    private List<Sprite> sprites;
    private GridLayoutGroup gridLayoutGroup;
    private List<GameObject> goalItems;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        goalItems = new List<GameObject>();
        gridLayoutGroup = GetComponent<GridLayoutGroup>();
        if(Board.Instance.RuntimeGoals.Count <= 4)
        {
            gridLayoutGroup.cellSize = new Vector2(50, 50);
            gridLayoutGroup.spacing = new Vector2(2, 0);
        }
        else if(Board.Instance.RuntimeGoals.Count >= 5)
        {
            gridLayoutGroup.cellSize = new Vector2(30, 30);
            gridLayoutGroup.spacing = new Vector2(12, 1);
        }
        SetUp();
        Board.Instance.OnGemDestroyed += UpdataeGoalUi;
    }

    private void UpdataeGoalUi(int obj)
    {
        UpdateTarget(obj);
    }

    private void OnDisable()
    {
        Board.Instance.OnGemDestroyed -= UpdataeGoalUi;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void SetUp()
    {
        GameObject tempGoal;
        Image image;
        TextMeshProUGUI text;
        foreach (var g in Board.Instance.RuntimeGoals)
        {
            tempGoal = Instantiate(goalItemPrefab, this.transform);
            image = tempGoal.transform.Find("ICon").GetComponentInChildren<Image>();
            text = tempGoal.GetComponentInChildren<TextMeshProUGUI>();
            goalItems.Add(tempGoal);
            if (g.GoalType == GoalType.Rope)
            {
                image.sprite = sprites[7];
            }
            else if (g.GoalType == GoalType.Crate)
            {
                image.sprite = sprites[6];
            }
            else
            {
                if(g.GemType == GemType.Blue)
                {
                    image.sprite = sprites[0];
                }
                else if (g.GemType == GemType.Green)
                {
                    image.sprite = sprites[1];
                }
                else if (g.GemType == GemType.Purple)
                {
                    image.sprite = sprites[2];
                }
                else if(g.GemType == GemType.Red)
                {
                    image.sprite = sprites[3];
                }
                else if(g.GemType == GemType.White)
                {
                    image.sprite = sprites[4];
                }
                else
                {
                    image.sprite= sprites[5];
                }
            }
            text.text = g.Target.ToString();
        }
    }

    void UpdateTarget(int indexChanged)
    {
        TextMeshProUGUI text = goalItems[indexChanged].GetComponentInChildren<TextMeshProUGUI>();
        int targetLeft = Board.Instance.RuntimeGoals[indexChanged].Target - Board.Instance.RuntimeGoals[indexChanged].Current;
        if(targetLeft <= 0)
        {
            if(text != null)
            {
                
                text.gameObject.SetActive(false);
                Image tickImage = goalItems[indexChanged].transform.Find("TickImage").GetComponentInChildren<Image>();
                tickImage.gameObject.SetActive(true);
                text.text = 0 + "";
            }
        }
        else
        {
            text.text = targetLeft.ToString();
        }
    }
}
