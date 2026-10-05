using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InGameCharacterController : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _idleSpriteRenderer;
    [SerializeField] private SpriteRenderer _attackSpriteRenderer;
    [SerializeField] private SpriteBillboard _spriteBillboard;
    [SerializeField] private Animator _animator;
    
    [Header("Effect Box")]
    [SerializeField] private Animator _effectBoxAnimator;
    [SerializeField] private TMP_Text _effectBoxText;
    
    [Header("Action Animations")]
    [SerializeField] private AnimationClip _genericAttackClip;
    [SerializeField] private AnimationClip _bigAttackClip;
    

    private Sprite _frontSprite;
    private Sprite _backSprite;

    public float TrueFacingDirection
    {
        set
        {
            _spriteBillboard.TrueLookDirection = value;
            _spriteBillboard.ForceBillboardUpdate();
        }
    }

    public void SetSprites(Sprite frontSprite, Sprite backSprite, Sprite attackSprite)
    {
        _frontSprite = frontSprite;
        _backSprite = backSprite;

        _spriteBillboard.FrontSprite = frontSprite;
        _spriteBillboard.BackSprite = backSprite;

        _attackSpriteRenderer.sprite = attackSprite;
        _spriteBillboard.ForceBillboardUpdate();
    }

    public void TakeDamage(float damage)
    {
        _effectBoxText.text = $"{damage}";
        _effectBoxAnimator.SetTrigger("Damage");
    }

    public void GetHealed(float amount)
    {
        _effectBoxText.text = $"{amount}";
        _effectBoxAnimator.SetTrigger("Healed");
    }

    public float MoveForward()
    {
        _animator.SetTrigger("GenericAttack");
        return _genericAttackClip.length;
    }

    public float MoveForwardFullDiceRoll()
    {
        _animator.SetTrigger("BigAttack");
        return _bigAttackClip.length;
    }
}
