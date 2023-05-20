using System;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using UnityEngine.Serialization;

public class DataManager : MonoBehaviour
{
    #region Variables

    [SerializeField] private TextAsset jsonTxtFile;
    public List<ColorData> colorDataList;
    [SerializeField] private List<LevelData> levelData = new List<LevelData>();
    
    #endregion
    
    #region Methods

    private void Awake()
    {
        ParseJsonData();
    }

    void ParseJsonData()
    {
        JSONNode jsonNode = JSON.Parse(jsonTxtFile.text);
        var arrData = jsonNode["level_data"].AsArray;
        
        foreach (JSONNode data in arrData)
        {
            LevelData obj = new LevelData();
            obj.matrixSize = data["matrixSize"];

            obj.nodeList = new List<NodeData>();

            for (int i = 0; i < colorDataList.Count; i++)
            {
                //Debug.Log(colorDataList[i].colorKey+"%%");
                string key = colorDataList[i].colorKey;
                if (data.HasKey(key))
                {
                    NodeData node = new NodeData();
                    node.nodeKey = key;
                    node.colorType = GetType(node.nodeKey);
                    node.startPos =Array.ConvertAll(data[key][0].Value.Split(','),s=>int.Parse(s)) ;
                    node.endPos =Array.ConvertAll(data[key][1].Value.Split(','),s=>int.Parse(s)) ;
                    
                    obj.nodeList.Add(node);
                }
            }
            
            levelData.Add(obj);
        }
    }

    public List<LevelData> GetLevelData()
    {
        return levelData;
    }

    ColorType GetType(string key)
    {
        ColorType type=ColorType.Red;
        switch (key)
        {
            case "red": type = ColorType.Red;
                break;
            case "blue": type = ColorType.Blue;
                break;
            case "green": type = ColorType.Green;
                break;
            case "orange": type = ColorType.Orange;
                break;
            case "purple": type = ColorType.Purple;
                break;
        }

        return type;
    }    

    #endregion
    
}


[Serializable]
public class LevelData
{
    public int matrixSize;
    public List<NodeData> nodeList;
}

[Serializable]
public struct NodeData
{
    public string nodeKey;
    public ColorType colorType;
    public int[] startPos;
    public int[] endPos;
}

[Serializable]
public struct ColorData
{
    public string colorKey;
    public ColorType type;
    public Color color;
}

public enum ColorType
{
    Red=0,
    Blue=1,
    Green=2,
    Orange=3,
    Purple=4,
    White=5
}