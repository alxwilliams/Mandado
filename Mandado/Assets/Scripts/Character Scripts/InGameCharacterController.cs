using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InGameCharacterController : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _sprite;
    [SerializeField] private Animator _animator;
    
    [Header("Effect Box")]
    [SerializeField] private Animator _effectBoxAnimator;
    [SerializeField] private TMP_Text _effectBoxText;
    
    [Header("Action Animations")]
    [SerializeField] private AnimationClip _genericAttackClip;
    [SerializeField] private AnimationClip _bigAttackClip;
    

    private Sprite _frontSprite;
    private Sprite _backSprite;

    public void SetSprites(Sprite frontSprite, Sprite backSprite)
    {
        _frontSprite = frontSprite;
        _backSprite = backSprite;

        _sprite.sprite = _frontSprite;
    }

    public void TakeDamage(float damage)
    {
        _effectBoxText.text = $"{damage}";
        _effectBoxAnimator.SetTrigger("Damage");
    }

    public float PlayAttack()
    {
        _animator.SetTrigger("GenericAttack");
        return _genericAttackClip.length;
    }

    public float PlayBigAttack()
    {
        _animator.SetTrigger("BigAttack");
        return _bigAttackClip.length;
    }
}
