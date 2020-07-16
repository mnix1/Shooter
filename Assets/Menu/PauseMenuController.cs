using System;
using System.Collections;
using System.Collections.Generic;
using Game;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    private static List<Action<bool>> _onShowHideActions = new List<Action<bool>>();

    public static void AddOnShowHideAction(Action<bool> action)
    {
        _onShowHideActions.Add(action);
    }

    public static void RemoveOnShowHideAction(Action<bool> action)
    {
        _onShowHideActions.Add(action);
    }

    private Canvas _canvas;
    private Transform _menuButtons;
    private Transform _optionsButtons;

    void Start()
    {
        _canvas = GetComponent<Canvas>();
    }

    private void OnEnable()
    {
        _menuButtons = transform.Find("Panel/MenuButtons");
        _optionsButtons = transform.Find("Panel/OptionsButtons");
    }

    void Show()
    {
        _menuButtons.gameObject.SetActive(true);
        _optionsButtons.gameObject.SetActive(false);
        _canvas.enabled = true;
        foreach (var showAction in _onShowHideActions)
        {
            showAction.Invoke(true);
        }
    }

    public void Hide()
    {
        _canvas.enabled = false;
        foreach (var hideAction in _onShowHideActions)
        {
            hideAction.Invoke(false);
        }
    }

    public void Options()
    {
        _menuButtons.gameObject.SetActive(false);
        _optionsButtons.gameObject.SetActive(true);
    }

    public void LeaveMatch()
    {
        if (GameManager.instance.networkEnabled)
        {
            PhotonNetwork.LeaveRoom();
        }
    }

    public void Exit()
    {
        Application.Quit();
    }
    
    void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.Escape) && !_canvas.enabled)
        {
            Show();
        }

        if (_canvas.enabled)
        {
        }
    }
}