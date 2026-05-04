using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class WoodenCrate : MonoBehaviour
{
    [SerializeField]
    private List<Sprite> crateState;
    private int x;
    private int y;
    private int crateHealth;
    private SpriteRenderer spriteRenderer;

    public int X => x;
    public int Y => y;
    public int CrateHealth
    {
        get { return crateHealth; }
        set 
        {
            //Debug.Log("OK");
            AudioManager.Instance.OnCrateRopBreak();
            crateHealth = value;
            if(crateHealth == 2)
            {
                spriteRenderer.sprite = crateState[1];
            }
            else if(crateHealth == 1)
            {
                spriteRenderer.sprite = crateState[2];
            }
            else if(crateHealth == 0)
            {
                Board.Instance.RemoveCrate(x, y);
            }
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
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
        crateHealth = 3;
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = crateState[0];
    }
}
