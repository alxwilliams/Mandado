using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class BattleSystem
{
    private void GenerateEnemyActions()
    {
        if (_currentEnemyAttackSet == null ||
            _currentEnemyAttackSet.sequencedAttacks == null
            || _currentEnemyAttackIndex >= _currentEnemyAttackSet.sequencedAttacks.Count)
        {
            List<EnemyAttackSet> listOfAttacks = GetPotentialEnemyAttacks();

            _currentEnemyAttackIndex = 0;
            _currentEnemyAttackSet = listOfAttacks[_gameManager.GetNewMainRandom(0,listOfAttacks.Count)];
        }

        foreach (var action in _currentEnemyAttackSet.sequencedAttacks[_currentEnemyAttackIndex].actionSet)
        {
            if (action.type == EnemyActionType.DamageRandom ||
                action.type == EnemyActionType.BleedRandom ||
                action.type == EnemyActionType.DamageAndDamageNearby)
            {
                action.targetIndex = GetNewPlayerIndex();
            }
            
        }
        
        _battleMenu.ShowEnemyIntent(_currentEnemyAttackSet.sequencedAttacks[_currentEnemyAttackIndex].actionSet);
    }
    
    private IEnumerator EnemyAttackActions()
    {
        yield return DealWithEnemyAction(_currentEnemyAttackSet.sequencedAttacks[_currentEnemyAttackIndex].actionSet,
            _currentSaveState.enemyCharacters[0]);
        
        _currentEnemyAttackIndex++;
    }
    
    private List<EnemyAttackSet> GetPotentialEnemyAttacks()
    {
        var currentEnemy = _currentSaveState.enemyCharacters[0];
        List<EnemyAttackSet> _potentialAttackSets = new List<EnemyAttackSet>();

        foreach (var attackSet in currentEnemy.enemyActions)
        {
            bool conditionPass = true;
            
            foreach (var condition in attackSet.conditionList)
            {
                if (condition.condition == EnemyAttackCondition.HealthLowerThan && currentEnemy.currentHealth > condition.value)
                {
                    conditionPass = false;
                    break;
                }
                
                if (condition.condition == EnemyAttackCondition.HealthGreaterThan && currentEnemy.currentHealth < condition.value)
                {
                    conditionPass = false;
                    break;
                }

                if (condition.condition == EnemyAttackCondition.TurnCountEqualTo && _currentSaveState.turnNumber != condition.value)
                {
                    conditionPass = false;
                    break;
                }
                
                if (condition.condition == EnemyAttackCondition.TurnCountGreaterThan && _currentSaveState.turnNumber < condition.value)
                {
                    conditionPass = false;
                    break;
                }
                
                if (condition.condition == EnemyAttackCondition.TurnCountLessThan && _currentSaveState.turnNumber > condition.value)
                {
                    conditionPass = false;
                    break;
                }
                
                if (condition.condition == EnemyAttackCondition.PlayerHasHealed && !_currentSaveState.playerHasHealed)
                {
                    conditionPass = false;
                    break;
                }
                
                if (condition.condition == EnemyAttackCondition.PlayerHasNotHealed && _currentSaveState.playerHasHealed)
                {
                    conditionPass = false;
                    break;
                }
            }

            if (conditionPass)
            {
                _potentialAttackSets.Add(attackSet);
            }
        }

        return _potentialAttackSets;
    }

    private IEnumerator DealWithEnemyAction(List<EnemyCharacterAction> actions, EnemyCharacterData data, bool maxRoll = false)
    {
        foreach (var action in actions)
        {                    
            yield return new WaitForSeconds(_fieldController.EnemyCharacterMoveForward(data));

            if (action.type == EnemyActionType.DamageRandom)
            {
                if(!maxRoll)
                {
                    EnemyAttackPlayer(action.targetIndex,action.value);
                }
                else
                {
                    //yield return new WaitForSeconds(_fieldController.CharacterBigAttack(data));
                    EnemyAttackPlayer(action.targetIndex,action.value);
                }
            }
            
            if (action.type == EnemyActionType.DamageAndDamageNearby)
            {
                if(!maxRoll)
                {
                    EnemyAttackPlayer(action.targetIndex,action.value);
                    EnemyAttackPlayer(action.targetIndex-1,action.value);
                    EnemyAttackPlayer(action.targetIndex+1,action.value);
                }
                else
                {
                    //yield return new WaitForSeconds(_fieldController.CharacterBigAttack(data));
                    EnemyAttackPlayer(action.targetIndex,action.value);
                    EnemyAttackPlayer(action.targetIndex-1,action.value);
                    EnemyAttackPlayer(action.targetIndex+1,action.value);
                }
            }

            if (action.type == EnemyActionType.Cripple)
            {
                PlayerGainCripple(action.value);
            }


            if (action.type == EnemyActionType.Guard)
            {
                GuardEnemyUnit(action.value);
            }

            if (action.type == EnemyActionType.Heal)
            {
                HealEnemyUnit(action.value);
            }
            
            if (action.type == EnemyActionType.BleedRandom)
            {
                ApplyPlayerBleed(action.targetIndex, action.value);
            }

            if (action.type == EnemyActionType.BleedAll)
            {
                for (int i = 0; i < _currentSaveState.playerCharacters.Count; i++)
                {
                    ApplyPlayerBleed(i,action.value);
                }
            }

            if (action.type == EnemyActionType.Focus)
            {
                if (data.currentFocus < 3)
                {
                    data.currentFocus+=action.value;

                    if (data.currentFocus > 3)
                    {
                        data.currentFocus = 3;
                    }
                    
                    _battleMenu.UpdateEnemyUI(data);
                }
            }

            yield return new WaitForSeconds(_waitTimeBetweenAttacks);
        }

        _battleMenu.ResetTargetIndicators();

        if (data.currentFocus >= 3)
        {
            if (data.enemyFocusAbility == EnemyFocusAbility.Tragos)
            {
                yield return new WaitForSeconds(_fieldController.EnemyCharacterMoveForward(data));
                TragosFocusAttack();
            }

            if (data.enemyFocusAbility == EnemyFocusAbility.Cabra)
            {
                yield return new WaitForSeconds(_fieldController.EnemyCharacterMoveForward(data));
                CabraFocusAttack();
            }

            data.currentFocus = 0;
            _battleMenu.UpdateEnemyUI(data);
        }
    }
    
    #region Enemy Actions
    
    private void EnemyTakeDamage(float num, StatusEffects effect = StatusEffects.Guard)
    {
        _currentSaveState.enemyCharacters[0].currentHealth -= num;

        if (effect == StatusEffects.Bleed)
        {
            _fieldController.EnemyTakeBleedDamage(_currentSaveState.enemyCharacters[0], num);
        }
        else
        {
            _fieldController.EnemyTakeDamageDamage(_currentSaveState.enemyCharacters[0], num);
        }

        EnemyDamagedRewardChecks(num);
        
        _battleMenu.UpdateEnemyUI(_currentSaveState.enemyCharacters[0]);
        
    }
    
    private void EnemyAttackPlayer(int playerIndex, float num)
    {
        if (playerIndex < 0 || playerIndex >= 6)
        {
            return;
        }
        
        
        if (_currentSaveState.playerCharacters[playerIndex].classType == ClassType.Warrior &&
            _currentSaveState.playerCharacters[playerIndex].currentFocus > 0)
        {
            FocusWarriorCheckCounterDamage(_currentSaveState.playerCharacters[playerIndex]);
        }

        float newDamageNum = CheckStatusEffectsForGuard(ref _currentSaveState.playerCharacters[playerIndex].statusEffects, num);

        if (num != newDamageNum)
        {
            _battleMenu.UpdatePlayerCharacterGuardUI(playerIndex,_currentSaveState.playerCharacters[playerIndex].statusEffects[StatusEffects.Guard]);
        }
        
        PlayerTakeDamage(playerIndex, newDamageNum);
    }
    
    private void HealEnemyUnit(float amount)
    {
        if (_currentSaveState.enemyCharacters[0].currentHealth + amount >
            _currentSaveState.enemyCharacters[0].maxHealth)
        {
            amount = _currentSaveState.enemyCharacters[0].maxHealth -
                     _currentSaveState.enemyCharacters[0].currentHealth;
        }
        
        _currentSaveState.enemyCharacters[0].currentHealth += amount;
        _fieldController.EnemyCharacterGetHealed(_currentSaveState.enemyCharacters[0],amount);
    }

    private void ApplyEnemyBleed(float amount)
    {
        if (_currentSaveState.enemyCharacters[0].statusEffects.ContainsKey(StatusEffects.Bleed))
        {
            _currentSaveState.enemyCharacters[0].statusEffects[StatusEffects.Bleed] += amount;
        }
        else
        {
            _currentSaveState.enemyCharacters[0].statusEffects.Add(StatusEffects.Bleed,amount);
        }
        
        _fieldController.EnemyGainBleedDebuff(_currentSaveState.enemyCharacters[0]);
        _battleMenu.UpdateEnemyBleedUI(_currentSaveState.enemyCharacters[0].statusEffects[StatusEffects.Bleed]);
    }
    
    private void DealWithEnemyBleedDamage()
    {
        foreach (var character in _currentSaveState.enemyCharacters)
        {
            if (character.statusEffects.ContainsKey(StatusEffects.Bleed) && character.statusEffects[StatusEffects.Bleed] > 0)
            {
                EnemyTakeDamage(character.statusEffects[StatusEffects.Bleed], StatusEffects.Bleed);
                character.statusEffects[StatusEffects.Bleed]--;
                
                _battleMenu.UpdateEnemyBleedUI(character.statusEffects[StatusEffects.Bleed]);
            }
        }
    }

    #endregion
    
    #region Status Effects
    
    private void GuardEnemyUnit(float amount)
    {

        if (_currentSaveState.enemyCharacters[0].statusEffects.ContainsKey(StatusEffects.Guard))
        {
            _currentSaveState.enemyCharacters[0].statusEffects[StatusEffects.Guard] += amount;
        }
        else
        {
            _currentSaveState.enemyCharacters[0].statusEffects[StatusEffects.Guard] = amount;
        }
        
        _battleMenu.UpdateEnemyGuardUI(_currentSaveState.enemyCharacters[0].statusEffects[StatusEffects.Guard]);
    }
    
    private void ResetEnemyGuard()
    {
        if (_currentSaveState.enemyCharacters[0].statusEffects.ContainsKey(StatusEffects.Guard) &&
            _currentSaveState.enemyCharacters[0].statusEffects[StatusEffects.Guard] != 0)
        {
            _currentSaveState.enemyCharacters[0].statusEffects[StatusEffects.Guard] = 0;
            _battleMenu.UpdateEnemyGuardUI(0);
        }
    }
    
    #endregion
}
