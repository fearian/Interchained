using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    private enum GameState
    {
        WaitingToStart,
        GamePlaying,
        GameOver,
    }

    public event EventHandler OnStateChanged;

    private GameState state;
    private float waitingToStartTimer = 1f;
    private float GamePlayingTimer = 3600f;

    private void Awake()
    {
        Instance = this;
        state = GameState.WaitingToStart;
    }

    private void Update()
    {
        switch (state)
        {
            case GameState.WaitingToStart:
                waitingToStartTimer -= Time.deltaTime;
                if (waitingToStartTimer < 0f)
                {
                    state = GameState.GamePlaying;
                    OnStateChanged?.Invoke(this, EventArgs.Empty);
                }

                break;
            case GameState.GamePlaying:
                GamePlayingTimer -= Time.deltaTime;
                break;
        }
    }

    public bool IsGamePlaying()
    {
        return state == GameState.GamePlaying;
    }

    public bool IsGameOver()
    {
        return state == GameState.GameOver;
    }

    public void TriggerGameOver()
    {
        if (state == GameState.GameOver) return;
        state = GameState.GameOver;
        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }

    public void ResetToPlaying()
    {
        GamePlayingTimer = 3600f;
        state = GameState.GamePlaying;
        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }
}