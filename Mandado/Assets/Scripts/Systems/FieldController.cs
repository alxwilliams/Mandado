using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldController : MonoBehaviour
{
    [SerializeField] private Transform _playerStartLocation;
    [SerializeField] private Transform _enemyStartLocation;
    [SerializeField] private GameObject _characterPrefab;

    [SerializeField] private float _sideBySideBufferValue = 2;

    private float _currentSpawnOffset = 0;
    public void LoadPlayerCharacters(List<CharacterData> characters)
    {
        StartCoroutine(LoadCharactersCoroutine(characters));
    }
    
    public void LoadEnemyCharacters(List<CharacterData> characters)
    {
        StartCoroutine(LoadCharactersCoroutine(characters));
    }
    
    public IEnumerator LoadCharactersCoroutine(List<CharacterData> characters)
    {
        int i = 0;
        _currentSpawnOffset = 0;
        
        foreach (var character in characters)
        {
            if(i == 0)
            {
                _currentSpawnOffset += character.width / 2 + _sideBySideBufferValue;
            }
            else
            {
                _currentSpawnOffset += character.width + _sideBySideBufferValue;
            }
            
            GameObject obj = Instantiate(_characterPrefab, _playerStartLocation);
            
            yield return null;
            
            if(i > 0)
            {
                obj.transform.position += _currentSpawnOffset * Vector3.right;
            }
            
            InGameCharacterController controller = obj.GetComponent<InGameCharacterController>();
            
            yield return null;
            controller.SetSprites(character.frontSprite,character.backSprite);
            
            

            i++;
        }
    }

    
}
