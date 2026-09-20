using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSystem : BaseSystem
{
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private Animator _animator;

    public Camera MainCamera
    {
        get => _mainCamera;
        set => _mainCamera = value;
    }

    public void SwitchToPlayerView()
    {
        _animator.SetTrigger("PlayerView");
    }

    public void SwitchToEnemyView()
    {
        _animator.SetTrigger("EnemyView");
    }
}
