using TMPro;
using UnityEngine;

public class MovesLeftUi : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI movesLeftText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        movesLeftText.text = Board.Instance.MovesLeft.ToString();
        Board.Instance.OnMovesChanged += UpdateMovesLeftUi;
    }

    private void UpdateMovesLeftUi(int obj)
    {
        movesLeftText.text = obj.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDestroy()
    {
        Board.Instance.OnMovesChanged -= UpdateMovesLeftUi;
    }
}
