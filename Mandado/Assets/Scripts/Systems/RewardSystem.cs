using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardSystem : BaseSystem
{
    [Header("Coin Values")]
    [SerializeField] private int _enemyKilledWithOneAttack = 4;
    [SerializeField] private int _perFiveOfAKindUsed = 3;
    [SerializeField] private int _perStraightUsed = 3;
    [SerializeField] private int _winningOnTurn1 = 3;
    [SerializeField] private int _winningOnTurn2 = 2;
    [SerializeField] private int _winningOnTurn3 = 1;
    [SerializeField] private int _oneUnitAtEndOfBattle = 3;
    [SerializeField] private int _allUnitsAbove75Percent = 2;
    [SerializeField] private int _doingAtleast50PercentWithOneAttack = 2;
    [SerializeField] private int _killingEnemyOnTheirTurn = 2;
    [SerializeField] private int _EachTurnWithoutReroll = 1;
    [SerializeField] private int _allUnitsBelow25Percent = 1;

    private BattleSystem _battleSystem;
     //serialize values
    
    public override void Initialize(GameManager gameManager)
    {
        _battleSystem = gameManager.BattleSystem;
        base.Initialize(gameManager);
    }

    public string GetRewards()
    {
        var battleResults = _battleSystem.GetBattleSystemState();
        int coinCount = 0;
        int val;
        string resultString = "";
        
        int aliveCount = 0;
        bool allAbove75 = true;
        bool allBelow25 = true;
        
        foreach (var character in battleResults.playerCharacters)
        {
            if (character.classType != ClassType.Empty)
            {
                if(character.IsAlive)
                {
                    aliveCount++;
                }
                else
                {
                    allAbove75 = false;
                    allBelow25 = false;
                }

                if (allAbove75 && (character.currentHealth / character.maxHealth < .75f))
                {
                    allAbove75 = false;
                }else if (allBelow25 && (character.currentHealth / character.maxHealth > .25f))
                {
                    allBelow25 = false;
                }
            }
        }

        if (battleResults.rewardData.enemyKilledWithOneAttack)
        {
            coinCount += _enemyKilledWithOneAttack;
            resultString += $"Killing an enemy with one attack: {_enemyKilledWithOneAttack}\n";
        }
        
        if (battleResults.rewardData.fiveOfAKindsUsed > 0)
        {
            val = battleResults.rewardData.fiveOfAKindsUsed * _perFiveOfAKindUsed;
            coinCount += val;
            resultString += $"Five of a kind used {battleResults.rewardData.fiveOfAKindsUsed} times: {val}\n";
        }
        
        if (battleResults.rewardData.straightsUsed > 0)
        {
            val = battleResults.rewardData.straightsUsed * _perStraightUsed;
            coinCount += val;
            resultString += $"Straight used {battleResults.rewardData.straightsUsed} times: {val}\n";
        }
        
        if (battleResults.turnNumber == 1)
        {
            coinCount += _winningOnTurn1;
            resultString += $"Winning on turn 1: {_winningOnTurn1}\n";
        }else if (battleResults.turnNumber == 2)
        {
            coinCount += _winningOnTurn2;
            resultString += $"Winning on turn 2: {_winningOnTurn2}\n";
        }else if (battleResults.turnNumber == 3)
        {
            coinCount += _winningOnTurn3;
            resultString += $"Winning on turn 3: {_winningOnTurn3}\n";
        }

        if (aliveCount == 1)
        {
            coinCount += _oneUnitAtEndOfBattle;
            resultString += $"One unit alive at the end of battle: {_oneUnitAtEndOfBattle}\n";
        }

        if (allAbove75)
        {
            coinCount += _allUnitsAbove75Percent;
            resultString += $"All units above 75 percent health: {_allUnitsAbove75Percent}\n";
        }
        
        if (allBelow25)
        {
            coinCount += _allUnitsBelow25Percent;
            resultString += $"All units below 25 percent health: {_allUnitsBelow25Percent}\n";
        }

        if (battleResults.rewardData.fiftyPercentDamageOneAttack)
        {
            coinCount += _doingAtleast50PercentWithOneAttack;
            resultString += $"50% of enemy health gone with one attack: {_doingAtleast50PercentWithOneAttack}\n";
        }
        
        if (battleResults.rewardData.enemyKilledOnTheirTurn)
        {
            coinCount += _killingEnemyOnTheirTurn;
            resultString += $"Enemy killed on their turn: {_killingEnemyOnTheirTurn}\n";
        }
        
        if (battleResults.rewardData.noRerollCount > 0)
        {
            val = battleResults.rewardData.noRerollCount * _EachTurnWithoutReroll;
            coinCount += val;
            resultString += $"Reroll not used on {battleResults.rewardData.noRerollCount} turns: {val}\n";
        }
        
        _gameManager.AddCoins(coinCount);
        
        return resultString;
    }

}
