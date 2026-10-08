using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldController : MonoBehaviour
{
    [SerializeField] private Transform _playerStartLocation;
    [SerializeField] private Transform _enemyStartLocation;
    [SerializeField] private GameObject _characterPrefab;
    [SerializeField] private float _enemyAttackAngle = 220;

    [SerializeField] private float _sideBySideBufferValue = 2;

    private InGameCharacterController _loadedController;

    private float _currentPlayerSpawnOffset = 0;
    private float _currentEnemySpawnOffset = 0;


    private Dictionary<PlayerCharacterData, InGameCharacterController> _playerControllerDictionary = new Dictionary<PlayerCharacterData, InGameCharacterController>();
    private Dictionary<EnemyCharacterData, InGameCharacterController> _enemyControllerDictionary = new Dictionary<EnemyCharacterData, InGameCharacterController>();

    private BattleSystem _battleSystem;

    public void Init(BattleSystem system, BattleSystemState state)
    {
        _battleSystem = system;
    }
    public void WipeCharacterDictionary()
    {
        _playerControllerDictionary = new Dictionary<PlayerCharacterData, InGameCharacterController>();
        _enemyControllerDictionary = new Dictionary<EnemyCharacterData, InGameCharacterController>();
    }

    public void DisableAllCurrentCharacters()
    {
        foreach (var pair in _playerControllerDictionary)
        {
            pair.Value.gameObject.SetActive(false);
        }
        
        foreach (var pair in _enemyControllerDictionary)
        {
            pair.Value.gameObject.SetActive(false);
        }
    }

    public void UpdatePlayerStates(BattleSystemState state)
    {
        PlayerCharacterData data;
        
        for (int i = 0; i < state.playerCharacters.Count; i++)
        {
            data = state.playerCharacters[i];
            
            InGameCharacterController controller = _playerControllerDictionary[data];
            List<PlayerCharacterAction> actionSet = data.actionSet.GetActionSetFromRollNumber(state.activeDiceRolls[i]);

            if (controller.CurrentCharacterIntents != actionSet)
            {
                controller.ChangeCharacterIntent(actionSet);
            }
        }
    }

    public void SwapCharacterPlaces(PlayerCharacterData character1, PlayerCharacterData character2)
    {
        (_playerControllerDictionary[character1].gameObject.transform.position,
            _playerControllerDictionary[character2].gameObject.transform.position) = (
            _playerControllerDictionary[character2].gameObject.transform.position,
            _playerControllerDictionary[character1].gameObject.transform.position);
    }

    public IEnumerator LoadPlayerCharactersCoroutine(List<PlayerCharacterData> characters)
    {
        int i = 0;
        _currentPlayerSpawnOffset = 0;
        
        foreach (Transform child in _playerStartLocation)
        {
            DestroyImmediate(child.gameObject);
        }

        yield return null;
        
        foreach (var character in characters)
        {
            yield return CreateCharacterController(true, character.width, character.characterSpriteData);
            _playerControllerDictionary.TryAdd(character, _loadedController);

            if (character.classType == ClassType.Empty || !character.IsAlive)
            {
                _loadedController.gameObject.SetActive(false);
            }
            else
            {
                _loadedController.gameObject.SetActive(true);
            }

            i++;
        }
    }
    
    public IEnumerator LoadEnemyCharactersCoroutine(List<EnemyCharacterData> characters)
    {
        int i = 0;
        _currentEnemySpawnOffset = 0;
        
        foreach (Transform child in _enemyStartLocation)
        {
            DestroyImmediate(child.gameObject);
        }

        yield return null;
        
        foreach (var character in characters)
        {
            yield return CreateCharacterController(false, character.width, character.characterSpriteData);
            _enemyControllerDictionary.TryAdd(character, _loadedController);
            _loadedController.TrueFacingDirection = _enemyAttackAngle;

            i++;
        }
    }

    private IEnumerator CreateCharacterController(bool player, float width, CharacterSpriteData spriteData)
    {
        GameObject obj = Instantiate(_characterPrefab, player?_playerStartLocation:_enemyStartLocation);

        yield return null;

        if(player)
        {
            obj.transform.position += _currentPlayerSpawnOffset * Vector3.right;
            _currentPlayerSpawnOffset += width + _sideBySideBufferValue;
        }
        else
        {
            obj.transform.position += _currentEnemySpawnOffset * Vector3.right;
            _currentEnemySpawnOffset += width + _sideBySideBufferValue;
        }

        InGameCharacterController controller = obj.GetComponent<InGameCharacterController>();

        yield return null;
        controller.Init(spriteData,_battleSystem);

        _loadedController = controller;
    }

    public void WipePlayerCharacterIntent(PlayerCharacterData data)
    {
        _playerControllerDictionary[data].WipeIntent();
    }

    public void PlayerCharacterGetHealed(PlayerCharacterData data, float num)
    {
        _playerControllerDictionary[data].GetHealed(num);
    }
    
    public void EnemyCharacterGetHealed(EnemyCharacterData data, float num)
    {
        _enemyControllerDictionary[data].GetHealed(num);
    }
    
    public float EnemyCharacterMoveForward(EnemyCharacterData data)
    {
        return _enemyControllerDictionary[data].MoveForward();
    }

    public float PlayerCharacterMoveForward(PlayerCharacterData data)
    {
        return _playerControllerDictionary[data].MoveForward();
    }
    
    public float PlayerCharacterMoveForwardFullDiceRoll(PlayerCharacterData data)
    {
        return _playerControllerDictionary[data].MoveForwardFullDiceRoll();
    }

    public void PlayerTakeDamage(PlayerCharacterData data, float num)
    {
        _playerControllerDictionary[data].TakeDamage(num);
    }
    
    public void EnemyTakeDamage(EnemyCharacterData data, float num)
    {
        _enemyControllerDictionary[data].TakeDamage(num);
    }

    public void KillPlayer(PlayerCharacterData data)
    {
        _playerControllerDictionary[data].KillPlayer();
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(_playerStartLocation.position, _playerStartLocation.position + 15 * Vector3.right);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(_enemyStartLocation.position, _enemyStartLocation.position + 15 * Vector3.left);
    }
}
