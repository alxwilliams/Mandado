using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCharacterUI : MonoBehaviour
{
    [SerializeField] private GameObject _uiParent;
    [SerializeField] private Image _healthBarFill;
    [SerializeField] private GameObject _focusStar1;
    [SerializeField] private GameObject _focusStar2;
    [SerializeField] private GameObject _focusStar3;

    public void UpdateFocus(float amount)
    {
        if (amount > 3 || amount < 0)
        {
            Debug.LogError($"amount is too low or too high, I don't care which one something is wrong: {amount}");
        }
        else
        {
            _focusStar1.SetActive(amount >= 1);
            _focusStar2.SetActive(amount >= 2);
            _focusStar3.SetActive(amount >= 3);
        }
    }

    public void UpdateHealth(float healthPercentage)
    {
        _healthBarFill.fillAmount = healthPercentage;
    }

    public void SetActive(bool on)
    {
        _uiParent.SetActive(on);
    }
    
}
