using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class HomeScreenManager : MonoBehaviour
{
    #region Variables
    
    public UnityAction<HomeScreenManager> OnComplete;
    [SerializeField] private Button homeBtn;
    
    #endregion
    
    #region Methods
   
    public void Init()
    {
        homeBtn.onClick.AddListener(OnHomeBtnClick);
    }

    void OnHomeBtnClick()
    {
        homeBtn.onClick.RemoveListener(OnHomeBtnClick);
        
        OnComplete?.Invoke(this);
    }
    
    #endregion
    

}
