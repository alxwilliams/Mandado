using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Enemy Character", menuName = "Character/Enemy Character")]
public class EnemyCharacter : BaseCharacter
{
   [SerializeField] private List<EnemyAttackSet> _enemyActions;

   public EnemyCharacterData GetFullHealthCharacterData()
   {
      EnemyCharacterData data = new EnemyCharacterData();

      data.name = _name;
      data.currentHealth = _baseHealth;
      data.enemyActions = _enemyActions;
      data.maxHealth = _baseHealth;
      data.currentFocus = 0;

      data.statusEffects = new Dictionary<StatusEffects, float>();
      data.frontSprite = _frontSprite;
      data.backSprite = _backSprite;
      data.width = _width;

      return data;
   }
}

[Serializable]
public class EnemyCharacterData
{
   public string name;
   public float currentHealth;
   public float maxHealth;
   public float currentFocus;
   public float currentDamageMultiplier = 1;
   public List<EnemyAttackSet> enemyActions;
   public Dictionary<StatusEffects, float> statusEffects;
   public float width;
   public Sprite frontSprite;
   public Sprite backSprite;
}