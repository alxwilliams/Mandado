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

    public void LoadPlayerCharacters(List<CharacterData> characters)
    {
        StartCoroutine(LoadCharactersCoroutine(characters,_playerStartLocation));
    }
    
    public void LoadEnemyCharacters(List<CharacterData> characters)
    {
        StartCoroutine(LoadCharactersCoroutine(characters, _enemyStartLocation));
    }
    
    public IEnumerator LoadCharactersCoroutine(List<CharacterData> characters, Transform startLocation)
    {
        int i = 0;
        float currentSpawnOffset = 0;
        
        foreach (Transform child in startLocation)
        {
            Destroy(child.gameObject);
        }

        yield return null;
        
        foreach (var character in characters)
        {
            GameObject obj = Instantiate(_characterPrefab, startLocation);
            
            yield return null;
            
            
            obj.transform.position += currentSpawnOffset * Vector3.right;
            currentSpawnOffset += character.width + _sideBySideBufferValue;

            InGameCharacterController controller = obj.GetComponent<InGameCharacterController>();
            
            yield return null;
            controller.SetSprites(character.frontSprite,character.backSprite);
            
            

            i++;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(_playerStartLocation.position, _playerStartLocation.position + 15 * Vector3.right);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(_enemyStartLocation.position, _enemyStartLocation.position + 15 * Vector3.right);
    }
}
