using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleSystem : BaseSystem
{
    [SerializeField] private List<BaseCharacter> _fakePlayerData = new List<BaseCharacter>();
    [SerializeField] private List<BaseCharacter> _fakeEnemyData = new List<BaseCharacter>();
    [SerializeField] private FieldController _fieldController;
    
    private Action<string> UpdateBattleTextAction;

    public void Initialize(GameManager gameManager, Action<string> updateBattleText)
    {
        UpdateBattleTextAction = updateBattleText;
        Initialize(gameManager);
    }
    private void Start()
    {
        LoadCharacters(_fakePlayerData, _fakeEnemyData);
    }

    public void LoadCharacters(List<BaseCharacter> playerCharacters, List<BaseCharacter> enemyCharacters)
    {
        List<CharacterData> playerData = new List<CharacterData>(); 
        List<CharacterData> enemyData = new List<CharacterData>(); 
        
        foreach (var character in playerCharacters)
        {
            playerData.Add(character.GetFullHealthCharacterData());
        }
        
        _fieldController.LoadPlayerCharacters(playerData);
        
        foreach (var character in enemyCharacters)
        {
            enemyData.Add(character.GetFullHealthCharacterData());
        }
        
        _fieldController.LoadEnemyCharacters(enemyData);
    }
    
    
}
