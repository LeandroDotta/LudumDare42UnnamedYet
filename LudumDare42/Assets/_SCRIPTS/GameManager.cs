﻿using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
	//public enum StateMachine { Title, Game, Pause, GameOver };
	//public StateMachine state = StateMachine.Title;

    [Header("UI Panels")]
    public GUIBar MessPanel;
    public RectTransform CenterPanel;
    public RectTransform GameOverPanel;
    [SerializeField] private Text _scoreText;
    [SerializeField] private Text _debrisText;
    [SerializeField] private Text _centerText;

    [Header("Waves ScriptableObjects")]
    public Wave[] Waves;

    [Header("Spawners")]
    public Spawner[] Spawners;

    private Wave _currentWave;
    private int _score = 0;
    private int _currentDebriCount = 0;    
    private int _currentMessValue = 0;
    private int _currentWaveIndex = -1;
    private int _maxMessValue = 25;
    private float _totalWaveMessValue = 25;

    public int MaxMessValue {
        get { return _maxMessValue; }
        set { _maxMessValue = value; }
    }

    public float totalWaveMessValue
    {
        get { return _totalWaveMessValue; }
        set { _totalWaveMessValue = value; }
    }

    private static GameManager _instance;

    public static GameManager Instance { get { return _instance; } }

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);

        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }

        Assert.IsNotNull(_debrisText);
        Assert.IsNotNull(_scoreText);
        Assert.IsNotNull(_centerText);
        Assert.IsNotNull(GameOverPanel);        

        Init();
    }

    public void Init()
    {        
        StartNextWave();
        MessPanel.MaxRawValue = _maxMessValue;        
        MessPanel.UpdateMess(_currentMessValue);
        UpdateDebriText();

        // Disabling panels, just in case
        GameOverPanel.gameObject.SetActive(false);
        CenterPanel.gameObject.SetActive(false);
    }

    private void UpdateDebriText()
    {
        _debrisText.text = _currentDebriCount + "/" + _currentWave.DebrisTotal;
    }

	public void AddScore(int value)
	{
		_score += value;
		_scoreText.text = _score.ToString();
	}

    public void UpdateMessValue(int value)
    {        
        _currentMessValue += value;
        _currentDebriCount++;

        if (_currentMessValue < 0) { _currentMessValue = 0; }

        if (IsGameOver())
        {
            GameOver();
            return;
        }

        if (HasWavedEnded())
        {
            StartNextWave();
            return;
        }
        
        MessPanel.UpdateMess((float)_currentMessValue);
        UpdateDebriText();
    }

    private void GameOver()
    {
        Debug.Log("GAME OVER!");
        StartCoroutine(ShowCenterPanelForSeconds("GAME OVER!", 5f));
        Time.timeScale = 0f;
        GameOverPanel.gameObject.SetActive(true);

        // TODO show UI GameOver with Restart and Quit buttons
    }

    bool IsGameOver()
    {
        return _currentMessValue >= _currentWave.MaxMessValue;
    }

    bool HasWavedEnded()
    {
        return _currentDebriCount > _currentWave.DebrisTotal;
    }

    public void StartNextWave()
    {
        // Check if we are in the last Wave, or the first one
        _currentWaveIndex++;
        if (_currentWaveIndex == Waves.Length) { GameOverWin(); return; }   
        
        // Set Wave values
        _currentWave = Waves[_currentWaveIndex];        
        _totalWaveMessValue = _currentWave.MaxMessValue;
        MessPanel.MaxRawValue = (int)_totalWaveMessValue;

        _currentMessValue = 0;
        _currentDebriCount = 0;               

        MessPanel.SetBarToZero();

        StartCoroutine(ShowCenterPanelForSeconds("Wave " + (_currentWaveIndex + 1)) );
    }

    /// <summary>
    /// Shows the CenterPanel for a couple seconds, default is 3s
    /// </summary>
    public IEnumerator ShowCenterPanelForSeconds(string textToShow, float timeToShow = 3f)
    {
        _centerText.text = "";
        _centerText.text = textToShow;
        CenterPanel.gameObject.SetActive(true);
        yield return new WaitForSeconds(timeToShow);
        CenterPanel.gameObject.SetActive(false);
    }

    private void HideWaveUI()
    {
        CenterPanel.gameObject.SetActive(false);
    }

    private void GameOverWin()
    {
        StartCoroutine(ShowCenterPanelForSeconds("YOU WIN!", 5f));
        Time.timeScale = 0f;
        GameOverPanel.gameObject.SetActive(true);
        // TODO if Reach last end show YOU WIN Screen!        
    }

    public void RestartLevel()
    {        
        SceneManager.LoadScene(SceneManager.GetActiveScene().ToString());        
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
