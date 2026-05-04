using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Level", menuName = "Match3/Level")]
public class LevelData : ScriptableObject
{
    [SerializeField]
    private int width;
    [SerializeField] 
    private int height;
    [SerializeField]
    private string[] map;
    [SerializeField]
    private List<Goal> goal = new List<Goal>();
    [SerializeField]
    private int movesLeft;

    public int Movesleft => movesLeft;
    public int Width => width;
    public int Height => height;
    public string[] Map => map;

    public List<Goal> Goals => goal; 
    //public void SetMapValue(int x, int y, char value)
    //{
    //    char[] row = map[y].ToCharArray();
    //    row[x] = value;
    //    map[y] = new string(row);
    //}
}
