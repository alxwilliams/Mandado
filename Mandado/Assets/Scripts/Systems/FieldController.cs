using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldController : MonoBehaviour
{
    [SerializeField] private Transform _playerStartLocation;
    [SerializeField] private Transform _enemyStartLocation;
    [SerializeField] private GameObject _characterPrefab;

    [SerializeField] private float _sideBySideBufferValue = 2;

    private InGameCharacterController _loadedController;

    private float _currentPlayerSpawnOffset = 0;
    private float _currentEnemySpawnOffset = 0;

    private Coroutine _playerLoadRoutine;
    private Coroutine _enemyLoadRoutine;

    private Dictionary<PlayerCharacterData, InGameCharacterController> _playerControllerDictionary = new Dictionary<PlayerCharacterData, InGameCharacterController>();
    private Dictionary<EnemyCharacterData, InGameCharacterController> _enemyControllerDictionary = new Dictionary<EnemyCharacterData, InGameCharacterController>();

    public void WipeCharacterDictionary()
    {
        _playerControllerDictionary = new Dictionary<PlayerCharacterData, InGameCharacterController>();
    }
    public void LoadPlayerCharacters(List<PlayerCharacterData> characters)
    {
        if (_playerLoadRoutine != null)
        {
            StopCoroutine(_playerLoadRoutine);
        }
        _playerLoadRoutine = StartCoroutine(LoadPlayerCharactersCoroutine(characters));
    }
    
    public void LoadEnemyCharacters(List<EnemyCharacterData> characters)
    {
        if (_enemyLoadRoutine != null)
        {
            StopCoroutine(_enemyLoadRoutine);
        }
        StartCoroutine(LoadEnemyCharactersCoroutine(characters));
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
            yield return CreateCharacterController(true, character.width, character.frontSprite,character.backSprite);
            _playerControllerDictionary.TryAdd(character, _loadedController);

            i++;
        }

        _playerLoadRoutine = null;
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
            yield return CreateCharacterController(false, character.width, character.frontSprite,character.backSprite);
            _enemyControllerDictionary.TryAdd(character, _loadedController);

            i++;
        }

        _enemyLoadRoutine = null;
    }

    private IEnumerator CreateCharacterController(bool player, float width, Sprite frontSprite, Sprite backSprite)
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
        controller.SetSprites(frontSprite, backSprite);

        _loadedController = controller;
    }

    public float EnemyAttack(EnemyCharacterData data)
    {
        return _enemyControllerDictionary[data].PlayAttack();
    }

    public float CharacterAttack(PlayerCharacterData data)
    {
        return _playerControllerDictionary[data].PlayAttack();
    }
    
    public float CharacterBigAttack(PlayerCharacterData data)
    {
        return _playerControllerDictionary[data].PlayBigAttack();
    }

    public void PlayerTakeDamage(PlayerCharacterData data, float num)
    {
        _playerControllerDictionary[data].TakeDamage(num);
    }
    
    public void EnemyTakeDamage(EnemyCharacterData data, float num)
    {
        _enemyControllerDictionary[data].TakeDamage(num);
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(_playerStartLocation.position, _playerStartLocation.position + 15 * Vector3.right);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(_enemyStartLocation.position, _enemyStartLocation.position + 15 * Vector3.left);
    }
}
