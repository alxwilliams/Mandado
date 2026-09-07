using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameCharacterController : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _sprite;

    private Sprite _frontSprite;
    private Sprite _backSprite;

    public void SetSprites(Sprite frontSprite, Sprite backSprite)
    {
        _frontSprite = frontSprite;
        _backSprite = backSprite;

        _sprite.sprite = _frontSprite;
    }

    public void SetHealth(string health)
    {

    }
}
