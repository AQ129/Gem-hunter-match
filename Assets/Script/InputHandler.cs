using Match3;
using System.Linq;
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    private InputActions inputActions;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        inputActions = new InputActions();
    }

    private void OnEnable()
    {
        inputActions.Enable();
        inputActions.Player.Click.performed += Click_performed;
    }

    private void OnDisable()
    {
        inputActions.Disable();
        inputActions.Player.Click.performed -= Click_performed;
    }

    private void Click_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        Vector2 screenPos = inputActions.Player.Position.ReadValue<Vector2>();
        Vector3 worldPos3 = Camera.main.ScreenToWorldPoint(screenPos);
        Vector2 worldPos = new Vector2(worldPos3.x, worldPos3.y);
        Collider2D[] hits = Physics2D.OverlapPointAll(worldPos);
        if(hits.Count() > 0 && Board.Instance.CurrentState == BoardState.Idle)
        {
            Gem gem = null;
            Rope rope = null;
            foreach(var hit in hits)
            {
                if(hit.TryGetComponent(out Gem g))
                {
                    gem = g;
                }
                if(hit.TryGetComponent(out Rope r))
                {
                    rope = r;
                }
            }
            if (gem != null && rope == null)
            {
                if(Board.Instance.GemBonus != GemType.Blue && !gem.IsGemBonus)
                {
                    Board.Instance.SpawnGemBonusItem(gem.X, gem.Y);
                }
                else
                {
                    Board.Instance.SelectGem(gem);
                }
            }
            else
            {
                Board.Instance.SelectGem(null);
            }
        }
        else
        {
            //Debug.Log("Click outside");
            Board.Instance.SelectGem(null);
        }
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
