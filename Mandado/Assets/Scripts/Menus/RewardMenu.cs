using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardMenu : BaseMenu
{
    [SerializeField] private TMP_Text _rewardText;
    [SerializeField] private Button _continueButton;

    private Action _startNextbattle;
    
    
    public override void Initialize(MenuSystem menuSystem)
    {
        _startNextbattle = GameManager.Instance.StartNextIndexedBattle;
        _continueButton.onClick.AddListener(ContinueButtonClicked);
        base.Initialize(menuSystem);
    }

    public void UpdateRewardText(string text)
    {
        _rewardText.text = text;
    }

    private void ContinueButtonClicked()
    {
        Show(false);
        _startNextbattle?.Invoke();
    }

}
