using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectMenu : BaseMenu
{
    [SerializeField] private Button _startGameButton;
    [SerializeField] private Button _backButton;

    [SerializeField] private List<PlayerCharacter> _allUsableCharacters;
    [SerializeField] private List<TMP_Dropdown> _dropdowns;

    private List<PlayerCharacter> _characters = new List<PlayerCharacter>();
    private Action _startGameAction;
    private Action _backToTitleAction;
    private BattleSystem _battleSystem;

    public void Initialize(MenuSystem menuSystem, Action startGame, Action backToTitle)
    {
        _startGameAction = startGame;
        _backToTitleAction = backToTitle;
        
        _startGameButton.onClick.AddListener(StartGame);
        _backButton.onClick.AddListener(BackButtonClicked);

        _battleSystem = GameManager.Instance.BattleSystem;

        foreach (var dropdown in _dropdowns)
        {
            dropdown.options = new List<TMP_Dropdown.OptionData>();
            
            foreach (var character in _allUsableCharacters)
            {
                TMP_Dropdown.OptionData option = new TMP_Dropdown.OptionData();
                option.text = character.name;
                dropdown.options.Add(option);
            }
        }
        
        base.Initialize(menuSystem);
    }

    private void PopulateList()
    {
        _characters = new List<PlayerCharacter>();
        
        foreach (var dropdown in _dropdowns)
        {
            foreach (var character in _allUsableCharacters)
            {
                int index = dropdown.value;

                if (dropdown.options[index].text == character.name)
                {
                    _characters.Add(character);
                    break;
                }
            }
        }
    }
    
    private bool CheckListHasACharacter()
    {
        foreach (var character in _characters)
        {
            if (character.GetClassType() != ClassType.Empty)
            {
                return true;
            }
        }

        return false;
    }

    private void StartGame()
    {
        PopulateList();
        
        if(CheckListHasACharacter())
        {
            _battleSystem.SetNewGamePlayerCharacters(_characters);
            _startGameAction?.Invoke();
        }
    }

    private void BackButtonClicked()
    {
        _backToTitleAction?.Invoke();
    }
}
