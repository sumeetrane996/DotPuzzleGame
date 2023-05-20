using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace PuzzleGame
{
    public enum GameState
    {
        HomeScene,
        LevelScene,
        GameScene
    }
    public class GameManager : MonoBehaviour
    {
        #region Variables
        [SerializeField] private GameState currentState;
        [SerializeField] private Canvas parentCanvas;
        [SerializeField] private GraphicRaycaster graphicRaycaster;
        [SerializeField] private DataManager dataManager;
        [SerializeField] private HomeScreenManager homeScreenManager;
        [SerializeField] private LevelSelectionManager levelSelectionManagerPrefab;
        [SerializeField] private PuzzleManager puzzleManagerPrefab;
        [SerializeField]private int currentLevelNum;
        
        #endregion

        #region Methods

        private void Awake()
        {
            #if !UNITY_EDITOR
            currentState = GameState.HomeScene;
            #endif
        }

        // Start is called before the first frame update
        void Start()
        {
            StateSwitch(currentState);
        }

        void StateSwitch(GameState state)
        {
            switch (state)
            {
                case GameState.HomeScene:
                    OnHomeSceneBegin();
                    break;
                case GameState.LevelScene:
                    OnLevelSceneBegin();
                    break;
                case GameState.GameScene:
                    OnGameStateBegin();
                    break;
            }
        }

        void EndState(GameState state)
        {
            switch (state)
            {
                case GameState.HomeScene:
                    currentState = GameState.LevelScene;
                    StateSwitch(currentState);
                    break;
                case GameState.LevelScene:
                    currentState = GameState.GameScene;
                    StateSwitch(currentState);
                    break;
                case GameState.GameScene:
                    currentState = GameState.LevelScene;
                    StateSwitch(currentState);
                    break;
            }
        }

        void OnHomeSceneBegin()
        {
            Debug.Log("Home State Begin");
            HomeScreenManager homeManager = Instantiate(homeScreenManager, parentCanvas.transform);
            homeManager.transform.SetSiblingIndex(1);

            homeManager.OnComplete += OnHomeSceneEnd;
            homeManager.Init();
        }
        
        void OnHomeSceneEnd(HomeScreenManager homeMan)
        {
            Debug.Log("Home State End");
            homeMan.OnComplete -= OnHomeSceneEnd;
            
            Destroy(homeMan.gameObject);
            
            EndState(currentState);
        }
        
        void OnLevelSceneBegin()
        {
            Debug.Log("Level State Begin");
            LevelSelectionManager levelSelectionManager = Instantiate(levelSelectionManagerPrefab, parentCanvas.transform);
            levelSelectionManager.transform.SetSiblingIndex(1);
            levelSelectionManager.OnLevelSelected += OnLevelSceneEnd;
            levelSelectionManager.Init(dataManager);
        }
        
        void OnLevelSceneEnd(int levelNum,LevelSelectionManager manager)
        {
            Debug.Log("Level State End");
            manager.OnLevelSelected -= OnLevelSceneEnd;

            currentLevelNum = levelNum;
            
            Destroy(manager.gameObject);
            EndState(currentState);
        }
        
        void OnGameStateBegin()
        {
            Debug.Log("Game State load");
            PuzzleManager puzzleMan = Instantiate(puzzleManagerPrefab,parentCanvas.transform);
            puzzleMan.transform.SetSiblingIndex(1);
            puzzleMan.OnPuzzleComplete += OnGameStateEnd;
            
            puzzleMan.Init(currentLevelNum,dataManager,graphicRaycaster);
        }

        void OnGameStateEnd(PuzzleManager manager)
        {
            Debug.Log("Game State Unload");
            manager.OnPuzzleComplete -= OnGameStateEnd;
            
            Destroy(manager.gameObject);
            
            EndState(currentState);
        }
        
        #endregion

    }
    
}
