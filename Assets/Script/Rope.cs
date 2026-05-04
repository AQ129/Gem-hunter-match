using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class Rope : MonoBehaviour
{
    [SerializeField]
    private List<Sprite> lockState;
    private int x;
    private int y;
    private int ropeState;
    private SpriteRenderer sprite;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public int X => x;
    public int Y => y;

    public int RopeState
    {
        get
        {   
            return ropeState;
        }
        set
        {
            AudioManager.Instance.OnCrateRopBreak();
            ropeState = value;
            if(ropeState == 1)
            {
                sprite.sprite = lockState[0];
            }
            else if(ropeState == 0)
            {
                Board.Instance.RemoveRope(X, Y);
            }
        }
    }
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Initialize(int x, int y)
    {
        this.x = x;
        this.y = y;
        ropeState = 2;
        sprite = GetComponent<SpriteRenderer>();
        sprite.sprite = lockState[1];
    }
}
