using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class SaveSystem : MonoBehaviour
{
    private GameManager _gameManager;
    private SaveData _saveData;
    
    protected bool _initialized = false;
    public Action InitializedAction;
    private string _savePath;
    
    private bool _loadedGameFound = false;
    public bool LoadedGameFound => _loadedGameFound;

    public async void Initialize(GameManager gameManager)
    {
        _gameManager = gameManager;
        
        _savePath = Path.Combine(Application.persistentDataPath, "save.json");
        
        await LoadAsync();
    }

    public void SaveGame(SaveData data)
    {
        _saveData = data;
        WriteToFile();
    }
    
    public async Task LoadAsync()
    {
        try
        {
            if (!File.Exists(_savePath))
            {
                throw new Exception("No save file");
            }

            string json = await File.ReadAllTextAsync(_savePath);
            var data = JsonUtility.FromJson<SaveData>(json);

            if (data == null)
            {
                throw new Exception("Invalid save format");
            }

            _saveData = data;
            _loadedGameFound = true;

            Debug.Log("Game loaded from " + _savePath);
        }
        catch
        {
            Debug.Log("Save invalid or missing.");
            _loadedGameFound = false;
            //NewSave();
        }

        _initialized = true;
        InitializedAction?.Invoke();
    }

    public SaveData LoadGame()
    {
        return _saveData;
    }
    
    private void NewSave()
    {
        WriteToFile();
    }
    
    private void WriteToFile()
    {
        var data = _saveData;
        string json = JsonUtility.ToJson(data);
        File.WriteAllText(_savePath, json);
        Debug.Log("Game saved to " + _savePath);
    }

}

[Serializable]
public class SaveData
{
    public string mainSeed;
    public int mainRandomCalls;
    public int targetRandomCalls;
    public int miscRandomCalls;
    public int version;
    public BattleSystemState battleState;
}

[Serializable]
public class BattleSystemState
{
    public int turnNumber = 0;
    public float  currentOrderTokens =0;
    public int[] activeDiceRolls = new int[6];
    public int amountOfRerolls = 3;
    public int diceInCharacterTrays = 0;
    public bool playerHasHealed = false;
    public bool canAttack = false;
    public List<PlayerCharacterData> playerCharacters;
    public List<EnemyCharacterData> enemyCharacters;
}

[System.Serializable]
public class SerializableStatusDictionary
{
    public List<StatusEffects> keys = new List<StatusEffects>();
    public List<float> values = new List<float>();
        
    public SerializableStatusDictionary() { }
        
    public SerializableStatusDictionary(Dictionary<StatusEffects, float> dictionary)
    {
        foreach (var kvp in dictionary)
        {
            keys.Add(kvp.Key);
            values.Add(kvp.Value);
        }
    }

    public Dictionary<StatusEffects, float> ToDictionary()
    {
        var dictionary = new Dictionary<StatusEffects, float>();
        
        for (int i = 0; i < keys.Count; i++)
        {
            dictionary[keys[i]] = values[i];
        }
        return dictionary;
    }
}
