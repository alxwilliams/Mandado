using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardSystem : BaseSystem
{
    [SerializeField] private GameObject _rewardWindow;
    [SerializeField] private TMP_Text _rewardText;
    [SerializeField] private Button _continueButton;
    
    private BattleSystem _battleSystem;
    public override void Initialize(GameManager gameManager)
    {
        _battleSystem = gameManager.BattleSystem;
        
        //subscribe to battlesystem events
        
        base.Initialize(gameManager);
    }

    public void ShowMenu()
    {
        
    }
}
