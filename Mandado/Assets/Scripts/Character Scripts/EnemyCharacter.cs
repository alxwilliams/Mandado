using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Enemy Character", menuName = "Character/Enemy Character")]
public class EnemyCharacter : BaseCharacter
{
   [SerializeField] private List<CompoundDiceActionSet> _enemyAttacks;


   public EnemyCharacterData GetFullHealthCharacterData()
   {
      EnemyCharacterData data = new EnemyCharacterData();

      data.name = _name;
      data.currentHealth = _baseHealth;
      data.actionSet = _enemyAttacks;

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
   public List<CompoundDiceActionSet> actionSet;
   public float width;
   public Sprite frontSprite;
   public Sprite backSprite;
}