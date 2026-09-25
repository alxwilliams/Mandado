using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterUI : MonoBehaviour
{
    
    [SerializeField] protected GameObject _uiParent;
    [SerializeField] protected Image _healthBarFill;
    [SerializeField] protected GameObject _focusStar1;
    [SerializeField] protected GameObject _focusStar2;
    [SerializeField] protected GameObject _focusStar3;
    
    
    [Header("Status Effects")] 
    [SerializeField] protected GameObject _guardSymbol;
    [SerializeField] protected TMP_Text _guardText;
    
    [SerializeField] protected GameObject _bleedSymbol;
    [SerializeField] protected TMP_Text _bleedText;
    
    protected float _currentGuard = -1;
    protected float _currentBleed = -1;
    
    public void SetFocus(float amount)
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

    public void UpdateGuardUI(float num)
    {
        if (num != _currentGuard)
        {
            _currentGuard = num;

            if (_currentGuard > 0)
            {
                _guardSymbol.SetActive(true);
                _guardText.text = $"{num}";
            }
            else
            {
                _guardSymbol.SetActive(false);
            }
        }
    }

    public void UpdateBleedUI(float num)
    {
        if (num != _currentBleed)
        {
            _currentBleed = num;
            
            if (_currentBleed > 0)
            {
                _bleedSymbol.SetActive(true);
                _bleedText.text = $"{num}";
            }
            else
            {
                _bleedSymbol.SetActive(false);
            }
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
