using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class BattleSystem
{
    #region Sentinel Checks
    
    private int FocusSentinelCheckForFullPoints()
    {
        int highestIndexedSentinel = -1;

        for (int i = 0; i < _currentSaveState.playerCharacters.Count; i++)
        {
            if (_currentSaveState.playerCharacters[i].classType == ClassType.Sentinel && _currentSaveState.playerCharacters[i].currentFocus == 3)
            {
                highestIndexedSentinel = i;
            }
        }

        return highestIndexedSentinel;
    }

    private void FocusSentinelCheckForHeals()
    {
        foreach (var character in _currentSaveState.playerCharacters)
        {
            if (character.classType == ClassType.Sentinel && character.currentFocus > 0)
            {
                if (character.actionSet.focusValues.Count == 0)
                {
                    Debug.LogError("Sentinel focus values not properly set");
                }
                else
                {
                    HealPlayerUnit(character.currentIndex, character.actionSet.focusValues[0] * character.currentFocus);
                }
            }
        }
    }
    
    #endregion

    #region Pilgrim Checks

    private void FocusPilgrimCheckForOrderTokens()
    {
        foreach (var character in _currentSaveState.playerCharacters)
        {
            if (character.classType == ClassType.Pilgrim && character.currentFocus == 3)
            {
                AddOrder(2);
            }
        }
    }

    #endregion

    #region Warrior Checks

    private void FocusWarriorCheckCounterDamage(PlayerCharacterData data)
    {
        PlayerAttackEnemy(data.currentFocus * data.actionSet.focusValues[0]);
    }

    #endregion

    #region EnemyChecks

    private void TragosFocusAttack()
    {
        List<PlayerCharacterData> bleedList = new List<PlayerCharacterData>();
        int playerIndex;
        
        foreach (var data in _currentSaveState.playerCharacters)
        {
            if (data.statusEffects.ContainsKey(StatusEffects.Bleed) && data.statusEffects[StatusEffects.Bleed] > 0)
            {
                bleedList.Add(data);
            }
        }

        if (bleedList.Count > 0)
        {
            playerIndex = _gameManager.GetNewTargetRandom(0, bleedList.Count);
            EnemyAttackPlayer(playerIndex, bleedList[playerIndex].statusEffects[StatusEffects.Bleed]);
        }
        else
        {
            playerIndex = GetNewPlayerIndex();
            EnemyAttackPlayer(playerIndex,0);
        }
    }

    #endregion
}
