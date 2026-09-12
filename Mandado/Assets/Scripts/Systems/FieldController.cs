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

    private float currentSpawnOffset = 0;

    private Dictionary<PlayerCharacterData, InGameCharacterController> _playerControllerDictionary = new Dictionary<PlayerCharacterData, InGameCharacterController>();
    private Dictionary<EnemyCharacterData, InGameCharacterController> _enemyControllerDictionary = new Dictionary<EnemyCharacterData, InGameCharacterController>();

    public void WipeCharacterDictionary()
    {
        _playerControllerDictionary = new Dictionary<PlayerCharacterData, InGameCharacterController>();
    }
    public void LoadPlayerCharacters(List<PlayerCharacterData> characters)
    {
        StartCoroutine(LoadPlayerCharactersCoroutine(characters));
    }
    
    public void LoadEnemyCharacters(List<EnemyCharacterData> characters)
    {
        StartCoroutine(LoadEnemyCharactersCoroutine(characters));
    }
    
    public IEnumerator LoadPlayerCharactersCoroutine(List<PlayerCharacterData> characters)
    {
        int i = 0;
        float currentSpawnOffset = 0;
        
        foreach (Transform child in _playerStartLocation)
        {
            Destroy(child.gameObject);
        }

        yield return null;
        
        foreach (var character in characters)
        {
            yield return CreateCharacterController(_playerStartLocation, character.width, character.frontSprite,character.backSprite);
            _playerControllerDictionary.TryAdd(character, _loadedController);

            i++;
        }
    }
    
    public IEnumerator LoadEnemyCharactersCoroutine(List<EnemyCharacterData> characters)
    {
        int i = 0;
        float currentSpawnOffset = 0;
        
        foreach (Transform child in _enemyStartLocation)
        {
            Destroy(child.gameObject);
        }

        yield return null;
        
        foreach (var character in characters)
        {
            yield return CreateCharacterController(_enemyStartLocation, character.width, character.frontSprite,character.backSprite);
            _enemyControllerDictionary.TryAdd(character, _loadedController);

            i++;
        }
    }

    private IEnumerator CreateCharacterController(Transform startLocation, float width, Sprite frontSprite, Sprite backSprite)
    {
        GameObject obj = Instantiate(_characterPrefab, startLocation);

        yield return null;

        obj.transform.position += currentSpawnOffset * Vector3.right;
        currentSpawnOffset += width + _sideBySideBufferValue;

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

    public void CharacterTakeDamage(PlayerCharacterData data, float num)
    {
        _playerControllerDictionary[data].TakeDamage(num);
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(_playerStartLocation.position, _playerStartLocation.position + 15 * Vector3.right);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(_enemyStartLocation.position, _enemyStartLocation.position + 15 * Vector3.left);
    }
}
