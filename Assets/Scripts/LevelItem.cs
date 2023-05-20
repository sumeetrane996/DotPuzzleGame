using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LevelItem : MonoBehaviour
{
    #region Variables

    public UnityAction<LevelItem,int> OnItemSelected;
    [SerializeField] private Button itemBtn;
    [SerializeField] private TextMeshProUGUI levelTxt;

    private int levelNum;
    
    #endregion
    
    #region Methods
    
    public void Init(int num)
    {
        levelNum = num;
        levelTxt.text = "Level " + (levelNum + 1);
        itemBtn.onClick.AddListener(OnItemClicked);    
    }

    void OnItemClicked()
    {
        itemBtn.onClick.RemoveListener(OnItemClicked);
        
        OnItemSelected?.Invoke(this,levelNum);
    }
    
    #endregion

}
