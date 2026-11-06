using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCharacterDiceButton : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private Image _image;
    [SerializeField] private Button _button;

    public Button Button => _button;
    
    public Image Image => _image;

    public Animator Animator => _animator;
}
