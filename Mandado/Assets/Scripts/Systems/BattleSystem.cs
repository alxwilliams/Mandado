using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleSystem : BaseSystem
{
    [SerializeField] private List<BaseCharacter> _fakePlayerData = new List<BaseCharacter>();
    [SerializeField] private List<BaseCharacter> _fakeEnemyData = new List<BaseCharacter>();
    [SerializeField] private FieldController _fieldController;
    
    private void Start()
    {
        LoadCharacters(_fakePlayerData, _fakeEnemyData);
    }

    public void LoadCharacters(List<BaseCharacter> playerCharacters, List<BaseCharacter> enemyCharacters)
    {
        List<CharacterData> data = new List<CharacterData>(); 
        
        foreach (var character in playerCharacters)
        {
            data.Add(character.GetFullHealthCharacterData());
        }
        
        _fieldController.LoadPlayerCharacters(data);
        
        foreach (var character in enemyCharacters)
        {
            data.Add(character.GetFullHealthCharacterData());
        }
        
        _fieldController.LoadEnemyCharacters(data);
    }
}
