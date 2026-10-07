using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InGameCharacterController : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _idleSpriteRenderer1;
    [SerializeField] private SpriteRenderer _idleSpriteRenderer2;
    [SerializeField] private SpriteRenderer _attackSpriteRenderer1;
    [SerializeField] private SpriteRenderer _attackSpriteRenderer2;
    [SerializeField] private SpriteRenderer _attackSpriteRenderer3;
    [SerializeField] private SpriteBillboard _spriteBillboard;
    [SerializeField] private Animator _animator;
    
    [Header("Effect Box")]
    [SerializeField] private Animator _effectBoxAnimator;
    [SerializeField] private TMP_Text _effectBoxText;
    
    [Header("Action Animations")]
    [SerializeField] private AnimationClip _genericAttackClip;
    [SerializeField] private AnimationClip _bigAttackClip;

    private CharacterSpriteData _characterSpriteData;

    private List<PlayerCharacterAction> _currentCharacterIntents = new List<PlayerCharacterAction>();

    public List<PlayerCharacterAction> CurrentCharacterIntents => _currentCharacterIntents;

    public float TrueFacingDirection
    {
        set
        {
            _spriteBillboard.TrueLookDirection = value;
            _spriteBillboard.ForceBillboardUpdate();
        }
    }

    public void SetSprites(CharacterSpriteData spriteData)
    {
        _characterSpriteData = spriteData;

        /*_spriteBillboard.FrontSprite = frontSprite;
        _spriteBillboard.BackSprite = backSprite;*/

        
        //_spriteBillboard.ForceBillboardUpdate();
        
        _idleSpriteRenderer1.sprite = spriteData._frontIdleSprite1;
        _idleSpriteRenderer2.sprite = spriteData._frontIdleSprite2;
        _attackSpriteRenderer1.sprite = spriteData._frontAttackSprite1;
        _attackSpriteRenderer2.sprite = spriteData._frontAttackSprite2;
        _attackSpriteRenderer3.sprite = spriteData._frontAttackSprite3;
    }

    public void ChangeCharacterIntent(List<PlayerCharacterAction> newIntents)
    {
        
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
