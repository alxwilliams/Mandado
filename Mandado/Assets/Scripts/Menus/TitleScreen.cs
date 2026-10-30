using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TitleScreen : BaseMenu
{
    [SerializeField] private Button _newGameButton;
    [SerializeField] private Button _loadGameButton;
    [SerializeField] private TMP_Text _coinText;

    private Action _openCharacterSelectAction;
    private Action _loadGameAction;
    private GameManager _gameManager;
    

    public void Initialize(MenuSystem menuSystem, Action loadGame, Action characterSelectScreen)
    {
        _loadGameAction = loadGame;
        _openCharacterSelectAction = characterSelectScreen;
        _gameManager = GameManager.Instance;

        _newGameButton.onClick.AddListener(OpenCharacterSelect);
        _loadGameButton.onClick.AddListener(LoadGame);
        base.Initialize(menuSystem);
    }

    public override void Show(bool show)
    {
        if (show)
        {
            UpdateCoinText();
        }
        
        base.Show(show);
    }

    private void UpdateCoinText()
    {
        _coinText.text = $"Coins: {_gameManager.GetCoinData()}";
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
