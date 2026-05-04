using System;
using UnityEngine;

[Serializable]
public class Goal
{
    [SerializeField]
    private GoalType goalType;
    [Tooltip("Only use that if goalType is Gem!")]
    [SerializeField]
    private GemType gemType;
    [SerializeField]
    private int target;

    private int current;

    public Goal(GoalType goalType, GemType gemType, int target)
    {
        this .goalType = goalType;
        this .gemType = gemType;
        this .target = target;
        this .current = 0;
    }

    public GoalType GoalType => goalType;
    public GemType GemType => gemType;
    public int Target => target;

    public int Current
    {
        get {  return current; }
        set 
        {
            current = value;
        }
    }
}
