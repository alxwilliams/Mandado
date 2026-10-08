using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _healthText;
    [SerializeField] protected GameObject _uiParent;
    [SerializeField] protected RectTransform _healthBarFill;
    [SerializeField] protected GameObject _focusStar1;
    [SerializeField] protected GameObject _focusStar2;
    [SerializeField] protected GameObject _focusStar3;
    [SerializeField] protected Image _labelImage;
    
    [Header("Health Bar Data")]
    [SerializeField] private float _healthBarFullWidth = 675.4f;
    [SerializeField] private float _healthBarEmptyWidth = 37.7f;

    [Header("Status Effects")] 
    [SerializeField] protected GameObject _empowerSymbol;
    [SerializeField] protected GameObject _guardSymbol;
    [SerializeField] protected TMP_Text _guardText;

    [SerializeField] protected TMP_Text _empowerText;
    [SerializeField] protected GameObject _bleedSymbol;
    [SerializeField] protected TMP_Text _bleedText;
    
    protected float _currentGuard = -1;
    protected float _currentBleed = -1;
    protected float _currentEmpower = -1;
    protected float _currentFocus = -1;
    protected float _currentHealth = -1;
    protected float _maxHealth = -1;

    protected float _healthPercentage;
    protected float _healthDifference;

    public float CurrentFocus => _currentFocus;
    public float CurrentBleed => _currentBleed;
    public float CurrentGuard => _currentGuard;
    public float CurrentEmpower => _currentEmpower;

    public float CurrentHealth
    {
        get => _currentHealth;
        set => _currentHealth = value;
    }

    public float MaxHealth
    {
        get => _maxHealth;
        set => _maxHealth = value;
    }

    public float HealthPercentage
    {
        get => _healthPercentage;
        set => _healthPercentage = value;
    }
    public float HealthDifference
    {
        get => _healthDifference;
        set => _healthDifference = value;
    }

    public GameObject UIParent => _uiParent;

    public Image LabelImage
    {
        get => _labelImage;
        set => _labelImage = value;
    }

    private void Awake()
    {
        _healthDifference = _healthBarFullWidth - _healthBarEmptyWidth;
    }

    public void SetLabel(Sprite labelImage)
    {
        _labelImage.sprite = labelImage;
    }
    
    public void SetFocus(float amount)
    {
        if (amount > 3 || amount < 0)
        {
            Debug.LogError($"amount is too low or too high, I don't care which one something is wrong: {amount}");
        }
        else
        {
            _currentFocus = amount;
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
    
    public void UpdateEmpowerUI(float num)
    {
        if (num != _currentEmpower)
        {
            _currentEmpower = num;

            if (_currentEmpower > 0)
            {
                _empowerSymbol.SetActive(true);
                _empowerText.text = $"{num}%";
            }
            else
            {
                _empowerSymbol.SetActive(false);
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

    public void UpdateHealth(float currentHealth, float maxHealth)
    {
        _currentHealth = currentHealth;
        _maxHealth = maxHealth;
        _healthText.text = $"{currentHealth}/{maxHealth}";
        _healthPercentage = currentHealth/maxHealth;
        
        _healthBarFill.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,_healthDifference * _healthPercentage + _healthBarEmptyWidth);
    }

    public void SetActive(bool on)
    {
        _uiParent.SetActive(on);
    }
}
