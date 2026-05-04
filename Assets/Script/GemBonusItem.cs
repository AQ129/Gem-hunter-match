using UnityEngine;

public class GemBonusItem
{
    private GemType gemType;
    private int quantity;
    
    public GemType GemType => gemType;
    public int Quantity
    {
        get {  return quantity; }
        set 
        {
            quantity = value;
        }
    }

    public GemBonusItem(GemType gemType, int quantity)
    {
        this.gemType = gemType;
        this.quantity = quantity;
    }

}
