using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TitleScreen : BaseMenu
{
    [SerializeField] private Button _newGameButton;
    [SerializeField] private Button _loadGameButton;

    private Action _openCharacterSelectAction;
    private Action _loadGameAction;

    public void Initialize(MenuSystem menuSystem, Action loadGame, Action characterSelectScreen)
    {
        _loadGameAction = loadGame;
        _openCharacterSelectAction = characterSelectScreen;
        
        _newGameButton.onClick.AddListener(OpenCharacterSelect);
        _loadGameButton.onClick.AddListener(LoadGame);
        base.Initialize(menuSystem);
    }

    private void LoadGame()
    {
        _loadGameAction?.Invoke();
    }

    private void OpenCharacterSelect()
    {
        _openCharacterSelectAction?.Invoke();
    }
}
