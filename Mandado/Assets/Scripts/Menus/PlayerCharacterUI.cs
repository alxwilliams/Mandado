using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCharacterUI : MonoBehaviour
{
    [SerializeField] private Animator _trayAnimator;
    [SerializeField] private GameObject _uiParent;
    [SerializeField] private Image _healthBarFill;
    [SerializeField] private GameObject _focusStar1;
    [SerializeField] private GameObject _focusStar2;
    [SerializeField] private GameObject _focusStar3;
    

    [Header("Dice Images/Sprites")] 
    
    [SerializeField] private Image _firstDice;
    [SerializeField] private Image _secondDice;
    [SerializeField] private Image _thirdDice;
    [SerializeField] private Image _fourDice;
    [SerializeField] private Image _fiveDice;

    [Header("Status Effects")] 
    [SerializeField] private GameObject _guardSymbol;
    [SerializeField] private TMP_Text _guardText;

    private float _currentGuard = -1;


    public void SetAnimationSpeed(float animatorSpeed)
    {
        _trayAnimator.speed = animatorSpeed;
    }
    
    public void OpenTray()
    {
        _trayAnimator.SetBool("Opened", true);
    }

    public void CloseTray()
    {
        _trayAnimator.SetBool("Opened", false);
    }

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

    public void SetActiveDice(float amount)
    {
        if (amount > 5 || amount < 0)
        {
            Debug.LogError($"amount is too low or too high, I don't care which one something is wrong: {amount}");
        }
        else
        {
            _firstDice.gameObject.SetActive(amount >= 1);
            _secondDice.gameObject.SetActive(amount >= 2);
            _thirdDice.gameObject.SetActive(amount >= 3);
            _fourDice.gameObject.SetActive(amount >= 4);
            _fiveDice.gameObject.SetActive(amount >= 5);
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
