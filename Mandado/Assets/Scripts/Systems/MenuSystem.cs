using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuSystem : BaseSystem
{
    [SerializeField] private BattleMenu _battleMenu;
    private BaseMenu _activeMenu;
    public override void Initialize(GameManager gameManager)
    {
        base.Initialize(gameManager);
    }

    public void UpdateBattleMenuText(string text)
    {
        _battleMenu.UpdateDebugText(text);
    }
    
    public void ShowBattleMenu()
    {
        if (_activeMenu != _battleMenu && _activeMenu.Active)
        {
            _activeMenu.Show(false);
        }

        _activeMenu = _battleMenu;
        
        _battleMenu.Show(true);
    }
}
