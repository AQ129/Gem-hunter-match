using System.Collections;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Gem : MonoBehaviour
{
    private int x;
    private int y;
    [SerializeField]
    private GemType types;
    private bool isMoving;
    private SpriteRenderer spriteRenderer;
    private bool isTriggered;
    private bool isGemBonus;
    private GemSelectEffect selectEffect;
    public bool IsTriggered
    {
        get => isTriggered;
        set
        {
            isTriggered = value;
        }
    }
    public bool IsGemBonus
    {
        get
        {
            if (types == GemType.Vertical || types == GemType.Horizontal || types == GemType.Color || types == GemType.LargeBomb || types == GemType.SmallBomb)
            {
                return true;
            }
            else return false;
        }
    }
    public bool IsMoving
    {
        get;
        private set;
    }

    public void Initialize(int x, int y)
    {
        this.x = x;
        this.y = y;
        IsMoving = false;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        selectEffect = GetComponent<GemSelectEffect>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Select()
    {
        selectEffect.Select();
    }

    public void DeSelect()
    {
        selectEffect.Deselect();
    }

    public void SetCoordinates(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public int X => x;
    public int Y => y;

    public GemType Types
    {
        get => types;
        set
        {
            this.types = value;
            if((int)value >= Board.Instance.GemPrefabs.Count())
            {
                int index = (int)value - Board.Instance.GemPrefabs.Count();
                spriteRenderer.sprite = Board.Instance.GemBonusPrefabs[index].GetComponent<SpriteRenderer>().sprite;
            }
        }
    }
public IEnumerator MoveTo(Vector2 targetPos)
    {
        IsMoving = true;
        Vector2 startPos = transform.position;
        float distance = Vector2.Distance(startPos, targetPos);
        float duration = 0.1f * distance;
        float time = 0f;
        while(time < duration)
        {
            float t = time / duration;
            t = t * t * (3f - 2f * t);
            transform.position = Vector2.Lerp(startPos, targetPos, t);
            time += Time.deltaTime;
            yield return null;
        }
        transform.position = targetPos;
        IsMoving = false;
    }

}
