
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using static Define;

public class GameManager
{

    #region Action
    public event Action<Vector2> OnMoveDirChanged;
    public event Action<Define.EJoystickState> OnJoystickStateChanged;
    public event Action<bool> OnInputModeChanged;
    #endregion


    #region Hero
    private Vector2 _moveDir;
    public Vector2 MoveDir
    {
        get { return _moveDir; }
        set
        {
            _moveDir = value;
            OnMoveDirChanged?.Invoke(value);
        }
    }

    private Define.EJoystickState _joystickState;
    public Define.EJoystickState JoystickState
    {
        get { return _joystickState; }
        set
        {
            _joystickState = value;
            OnJoystickStateChanged?.Invoke(_joystickState);
        }
    }
    
    // 클릭 모드 상태
    private bool _isClickMode = true;
    public bool IsClickMode
    {
        get { return _isClickMode; }
        set 
        { 
            _isClickMode = value;
            // UI 컨트롤러에게 모드 변경 알림
            if (OnInputModeChanged != null)
                OnInputModeChanged.Invoke(_isClickMode);
        }
    }
    
    // 플레이어 위치 반환 (클릭 방향 계산에 사용)
    public Vector3 GetPlayerPosition()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
            return player.transform.position;
        return Vector3.zero;
    }
    #endregion


}

