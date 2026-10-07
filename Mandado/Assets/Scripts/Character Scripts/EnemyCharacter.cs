using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Enemy Character", menuName = "Character/Enemy Character")]
public class EnemyCharacter : BaseCharacter
{
   [SerializeField] private List<EnemyAttackSet> _enemyActions;
   [SerializeField] private EnemyFocusAbility _enemyFocusAbility;

   public EnemyCharacterData GetFullHealthCharacterData()
   {
      EnemyCharacterData data = new EnemyCharacterData();

      data.name = _name;
      data.currentHealth = _baseHealth;
      data.enemyFocusAbility = _enemyFocusAbility;
      data.enemyActions = _enemyActions;
      data.maxHealth = _baseHealth;
      data.currentFocus = 0;

      data.statusEffects = new Dictionary<StatusEffects, float>();
      data.characterSpriteData = _characterSpriteData;
      data.width = _width;

      return data;
   }
}

[Serializable]
public class EnemyCharacterData
{
   public string name;
   public EnemyFocusAbility enemyFocusAbility;
   public float currentHealth;
   public float maxHealth;
   public float currentFocus;
   public float currentDamageMultiplier = 1;
   public List<EnemyAttackSet> enemyActions;
   public SerializableStatusDictionary statusEffectsSerialized;
   public Dictionary<StatusEffects, float> statusEffects;
   public float width;
   public CharacterSpriteData characterSpriteData;
}

public enum EnemyFocusAbility
{
   Tragos
}