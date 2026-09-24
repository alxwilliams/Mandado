using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class BattleSystem
{
    #region Sentinel Checks
    
    private int FocusSentinelCheckForFullPoints()
    {
        int highestIndexedSentinel = -1;

        for (int i = 0; i < _currentPlayerCharacters.Count; i++)
        {
            if (_currentPlayerCharacters[i].classType == ClassType.Sentinel && _currentPlayerCharacters[i].currentFocus == 3)
            {
                highestIndexedSentinel = i;
            }
        }

        return highestIndexedSentinel;
    }

    private void FocusSentinelCheckForHeals()
    {
        foreach (var character in _currentPlayerCharacters)
        {
            if (character.classType == ClassType.Sentinel && character.currentFocus > 0)
            {
                if (character.actionSet.focusValues.Count == 0)
                {
                    Debug.LogError("Sentinel focus values not properly set");
                }
                else
                {
                    HealPlayerUnit(character.currentIndex, character.actionSet.focusValues[0]);
                }
            }
        }
    }
    
    #endregion
}
