using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LevelSelectionManager : MonoBehaviour
{
    #region Variables

    public UnityAction<int,LevelSelectionManager> OnLevelSelected;

    [SerializeField] private GridLayoutGroup gridLayout;
    [SerializeField] private Transform content;
    [SerializeField] private LevelItem itemPrefab;

    private List<LevelItem> itemList;
    private DataManager dataManager;
    
    #endregion
    
    #region Methods
    
    // Start is called before the first frame update
    void Start()
    {
        itemList = new List<LevelItem>();
    }

    public void Init(DataManager dataMan)
    {
        dataManager = dataMan;
        
        SpawnLevelItems();
    }

    void SpawnLevelItems()
    {
        for (int i = 0; i < dataManager.GetLevelData().Count; i++)
        {
            LevelItem item = Instantiate(itemPrefab, content);
            item.OnItemSelected += OnLevelClicked;
            item.Init(i);
        }

        float blockSize = (gridLayout.cellSize.x + gridLayout.spacing.x);
        content.GetComponent<RectTransform>().sizeDelta = new Vector2( blockSize* gridLayout.constraintCount,blockSize * (dataManager.GetLevelData().Count/gridLayout.constraintCount));
    }
    
    void OnLevelClicked(LevelItem lvlItem,int levelNum)
    {
        lvlItem.OnItemSelected -= OnLevelClicked;
        
        itemList.ForEach(x=>x.OnItemSelected-=OnLevelClicked);
        
        OnLevelSelected?.Invoke(levelNum,this);
    }
    

    #endregion

}
