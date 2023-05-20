using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace PuzzleGame
{
    public class PuzzleManager : MonoBehaviour
    {
        #region Variables
 
        public UnityAction<PuzzleManager> OnPuzzleComplete;
        
        [SerializeField] private BoardHandler boardHandler;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private Button backBtn;
        [SerializeField] private GameObject victoryScreen;
        [SerializeField] private Button nextLvlBtn;
        
        private GraphicRaycaster raycaster;
        private DataManager dataManager;
        private LevelData levelData;
        private int levelNumber;
        
        #endregion
        
        #region Methods

        // Start is called before the first frame update
        void Start()
        {
            victoryScreen.SetActive(false);
            nextLvlBtn.onClick.AddListener(LoadNextLevel);
        }

        public void Init(int _level,DataManager _data,GraphicRaycaster ray)
        {
            levelNumber = _level;
            dataManager = _data;
            raycaster = ray;
            
            levelData = _data.GetLevelData()[levelNumber];

            levelText.text = "Level " + levelNumber;
            
            backBtn.onClick.AddListener(OnBackClick);
            LoadLevel();
        }

        private void LoadLevel()
        {
            boardHandler.Init(dataManager,raycaster);
            boardHandler.SpawnLevel(levelData,(() =>
            {
                StartCoroutine(OnLevelComplete());
            }));
        }

        private IEnumerator OnLevelComplete()
        {
            yield return new WaitForSeconds(2f);
            victoryScreen.SetActive(true);
        }

        private void LoadNextLevel()
        {
            boardHandler.ClearPreviousLevel();
            
            levelNumber++;

            if (levelNumber >= dataManager.GetLevelData().Count)
                levelNumber = 0;
            
            levelData = dataManager.GetLevelData()[levelNumber];

            levelText.text = "Level " + levelNumber;
            LoadLevel();
            
            victoryScreen.SetActive(false);
        }
        
        void OnBackClick()
        {
            backBtn.onClick.RemoveListener(OnBackClick);
            
            OnPuzzleComplete?.Invoke(this);
        }

        private void OnDisable()
        {
            nextLvlBtn.onClick.RemoveListener(LoadNextLevel);
            backBtn.onClick.RemoveListener(OnBackClick);

        }

        #endregion

    }

}
