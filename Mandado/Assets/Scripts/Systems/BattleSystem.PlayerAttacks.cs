using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class BattleSystem
{
    private IEnumerator PlayerDiceActions()
    {
        for (int i = 0; i < _currentSaveState.playerCharacters.Count; i++)
        {
            if (_currentSaveState.activeDiceRolls[i] > 0 && _currentSaveState.playerCharacters[i].IsAlive && _currentSaveState.playerCharacters[i].classType != ClassType.Empty)
            {
                yield return DealWithPlayerAction(_currentSaveState.playerCharacters[i].actionSet.GetActionSetFromRollNumber(_currentSaveState.activeDiceRolls[i]),
                    _currentSaveState.playerCharacters[i],i,_currentSaveState.activeDiceRolls[i]);

                if (_enemyDied)
                {
                    break;
                }
                
                ResetIndexedPlayerFocus(i);
                
                UpdateUI();
                
                yield return new WaitForSeconds(_waitTimeBetweenAttacks);
            }
            else if(_currentSaveState.playerCharacters[i].currentFocus < 3 && _currentSaveState.playerCharacters[i].currentHealth > 0)
            {
                _currentSaveState.playerCharacters[i].currentFocus++;
                CheckForPassiveFocusAbilities(i);
                UpdateUI();
            }
        }
    }
    
    private IEnumerator DealWithPlayerAction(List<PlayerCharacterAction> actions, PlayerCharacterData data, int castingUnitIndex, int rolledNumber)
    {
        bool maxRoll = rolledNumber == 5;

        if (maxRoll)
        {
            _currentSaveState.rewardData.fiveOfAKindsUsed++;
        }
        
        yield return new WaitForSeconds(_fieldController.PlayerCharacterMoveForward(data));

        foreach (var action in actions)
        {
            if (action.type == PlayerActionType.Damage)
            {

                float damage = action.value;
                
                if (data.statusEffects.ContainsKey(StatusEffects.Empower) &&
                    data.statusEffects[StatusEffects.Empower] > 0)
                {
                    damage += data.statusEffects[StatusEffects.Empower];

                    data.statusEffects[StatusEffects.Empower] = 0;
                    _battleMenu.UpdatePlayerCharacterEmpowerUI(castingUnitIndex, 0);
                }
                
                if(!maxRoll)
                {
                    PlayerAttackEnemy(damage);
                }
                else
                {
                    PlayerAttackEnemy(damage);
                }


                if (CheckEnemyHealth())
                {
                    _enemyDied = true;
                    break;
                }
                
            }

            if (action.type == PlayerActionType.Bleed)
            {
                ApplyEnemyBleed(action.value);
            }


            if (action.type == PlayerActionType.Heal)
            {

                for (int i = 0; i < 6;i++) 
                {
                    HealPlayerUnit(i, action.value);
                }
                    
                _currentSaveState.playerHasHealed = true;
            }
            

            if (action.type == PlayerActionType.Guard)
            {
                for (int i = 0; i < 6;i++) 
                {
                    GuardPlayerUnit(i, action.value);
                }
                
            }

            if (action.type == PlayerActionType.Order)
            {
                AddOrder(action.value);
            }

            if (action.type == PlayerActionType.Empower)
            {
                for (int i = 0; i < 6;i++) 
                {
                    EmpowerPlayerUnit(i, action.value);
                }
                
            }

            if (action.type == PlayerActionType.Inspire)
            {
                for (int i = 0; i < 6;i++) 
                {
                    _currentSaveState.playerCharacters[i].currentFocus++;
                    _battleMenu.UpdatePlayerCharacterFocus(_currentSaveState.playerCharacters[i]);
                }
                
            }

            yield return new WaitForSeconds(0.1f);
        }

        if(!_enemyDied)
        {
            _fieldController.WipePlayerCharacterIntent(data);
            _battleMenu.ResetDice(castingUnitIndex);
        }
    }

    #region Player Actions
    
    private void PlayerTakeDamage(int index, float num, StatusEffects effect = StatusEffects.Guard)
    {
        _currentSaveState.playerCharacters[index].currentHealth -= num;

        if (effect == StatusEffects.Bleed)
        {
            _fieldController.PlayerTakeBleedDamage(_currentSaveState.playerCharacters[index], num);
        }
        else
        {
            _fieldController.PlayerTakeDamageDamage(_currentSaveState.playerCharacters[index], num);
        }
        
        _battleMenu.UpdatePlayerCharacterHealth(_currentSaveState.playerCharacters[index]);

        if (!_currentSaveState.playerCharacters[index].IsAlive)
        {
            PlayerDead(index);
        }
        
    }
    
    private void PlayerAttackEnemy(float num)
    {
        int enemyIndex = 0;
        float newDamageNum = CheckStatusEffectsForGuard(ref _currentSaveState.enemyCharacters[enemyIndex].statusEffects, num);

        if (num != newDamageNum)
        {
            _battleMenu.UpdateEnemyGuardUI(_currentSaveState.enemyCharacters[0].statusEffects[StatusEffects.Guard]);
        }

        EnemyTakeDamage(newDamageNum);
    }

    private bool DealWithPlayerBleedDamage()
    {
        bool playerBled = false;
        foreach (var character in _currentSaveState.playerCharacters)
        {
            if (character.statusEffects.ContainsKey(StatusEffects.Bleed) && character.statusEffects[StatusEffects.Bleed] > 0)
            {
                PlayerTakeDamage(character.currentIndex,character.statusEffects[StatusEffects.Bleed], StatusEffects.Bleed);
                character.statusEffects[StatusEffects.Bleed]--;
                
                _battleMenu.UpdatePlayerCharacterBleedUI(character.currentIndex, character.statusEffects[StatusEffects.Bleed]);
                playerBled = true;
            }
        }

        return playerBled;
    }
    
    private void HealPlayerUnit(int unitIndex, float amount)
    {
        if (_currentSaveState.playerCharacters[unitIndex].classType == ClassType.Empty || !_currentSaveState.playerCharacters[unitIndex].IsAlive)
        {
            return;
        }
        
        if (_currentSaveState.playerCharacters[unitIndex].currentHealth + amount >=
            _currentSaveState.playerCharacters[unitIndex].maxHealth)
        {
            amount = _currentSaveState.playerCharacters[unitIndex].maxHealth -
                     _currentSaveState.playerCharacters[unitIndex].currentHealth;
        }
        
        _currentSaveState.playerCharacters[unitIndex].currentHealth += amount;
        _fieldController.PlayerCharacterGetHealed(_currentSaveState.playerCharacters[unitIndex], amount);
    }

    #endregion

    #region Status Effects
    
    private void GuardPlayerUnit(int unitIndex, float amount)
    {
        if (_currentSaveState.playerCharacters[unitIndex].classType == ClassType.Empty || !_currentSaveState.playerCharacters[unitIndex].IsAlive)
        {
            return;
        }
        
        if (_currentSaveState.playerCharacters[unitIndex].statusEffects.ContainsKey(StatusEffects.Guard))
        {
            _currentSaveState.playerCharacters[unitIndex].statusEffects[StatusEffects.Guard] += amount;
        }
        else
        {
            _currentSaveState.playerCharacters[unitIndex].statusEffects[StatusEffects.Guard] = amount;
        }
        
        _battleMenu.UpdatePlayerCharacterGuardUI(unitIndex,_currentSaveState.playerCharacters[unitIndex].statusEffects[StatusEffects.Guard]);
    }
    
    private void ApplyPlayerBleed(int index, float amount)
    {
        if (_currentSaveState.playerCharacters[index].classType == ClassType.Empty || !_currentSaveState.playerCharacters[index].IsAlive)
        {
            return;
        }
        
        if(_currentSaveState.playerCharacters[index].statusEffects.ContainsKey(StatusEffects.Bleed))
        {
            _currentSaveState.playerCharacters[index].statusEffects[StatusEffects.Bleed] += amount;
        }
        else
        {
            _currentSaveState.playerCharacters[index].statusEffects.Add(StatusEffects.Bleed,amount);
        }
        
        
        _fieldController.PlayerGainBleedDebuff(_currentSaveState.playerCharacters[index]);
        _battleMenu.UpdatePlayerCharacterBleedUI(index,_currentSaveState.playerCharacters[index].statusEffects[StatusEffects.Bleed]);
    }
    
    private void EmpowerPlayerUnit(int unitIndex, float amount)
    {
        if (_currentSaveState.playerCharacters[unitIndex].classType == ClassType.Empty || !_currentSaveState.playerCharacters[unitIndex].IsAlive)
        {
            return;
        }
        
        if (_currentSaveState.playerCharacters[unitIndex].statusEffects.ContainsKey(StatusEffects.Empower))
        {
            _currentSaveState.playerCharacters[unitIndex].statusEffects[StatusEffects.Empower] += amount;
        }
        else
        {
            _currentSaveState.playerCharacters[unitIndex].statusEffects[StatusEffects.Empower] = amount;
        }
        
        
        _battleMenu.UpdatePlayerCharacterEmpowerUI(unitIndex,
            (_currentSaveState.playerCharacters[unitIndex].statusEffects[StatusEffects.Empower]));
    }

    private void ResetPlayerGuard()
    {
        for (int i = 0; i < _currentSaveState.playerCharacters.Count; i++)
        {
            if (_currentSaveState.playerCharacters[i].statusEffects.ContainsKey(StatusEffects.Guard) && _currentSaveState.playerCharacters[i].statusEffects[StatusEffects.Guard] != 0 )
            {
                _currentSaveState.playerCharacters[i].statusEffects[StatusEffects.Guard] = 0;
                _battleMenu.UpdatePlayerCharacterGuardUI(i,0);
            }
        }
    }

    #endregion
    
    
}
