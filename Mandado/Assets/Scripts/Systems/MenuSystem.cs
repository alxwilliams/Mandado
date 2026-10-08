using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuSystem : BaseSystem
{
    [SerializeField] private BattleMenu _battleMenu;
    [SerializeField] private TitleScreen _titleScreen;
    [SerializeField] private CharacterSelectMenu _characterSelectMenu;
    
    private BaseMenu _activeMenu;

    public void Initialize(GameManager gameManager, Action startNewGame, Action loadGame)
    {
        _titleScreen.Initialize(this,loadGame,ShowCharacterSelectScreen);
        _characterSelectMenu.Initialize(this,startNewGame,ShowTitleScreen);
        
        base.Initialize(gameManager);
    }

    [ContextMenu("show battle")]
    public void ShowBattleMenu()
    {
        _battleMenu.Show(true);
    }

    [ContextMenu("show title")]
    public void ShowTitleScreen()
    {
        _titleScreen.Show(true);
    }

    public void ShowCharacterSelectScreen()
    {
        _characterSelectMenu.Show(true);
    }

    public void SetNewMenu(BaseMenu newMenu)
    {
        if (_activeMenu != null && _activeMenu != newMenu && _activeMenu.Active)
        {
            _activeMenu.Show(false);
        }
        
        _activeMenu = newMenu;
    }
}
