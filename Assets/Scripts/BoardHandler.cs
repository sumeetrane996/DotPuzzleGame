using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PuzzleGame
{
    public class BoardHandler : MonoBehaviour
    {
        #region Variables
        
        //public List<GameObject> greenLineList, redLineList, orangeLineList, blueLineList, purpleLineList;//delete this

        GraphicRaycaster m_Raycaster;
        PointerEventData m_PointerEventData;
        EventSystem m_EventSystem;
        
        private UnityAction OnLevelWin;
        private UnityAction<int> OnScoreUpdate;
        private DataManager dataManager;
        private LevelData lvlData;
        
        [SerializeField] private GridLayoutGroup gridLayout;
        [SerializeField] private GameObject blockPrefab;
        [SerializeField] private GameObject nodePrefab;
        [SerializeField] private TextMeshProUGUI scoreTxt;
        [SerializeField] private Block currentBlock;
        [SerializeField] private Block lastBlock;
        [SerializeField] private ColorType currentDrawnColor;
        [SerializeField] private bool[] completeArr;

        private List<Block>[] lineDataArr;
        private Block[,] playMatrix;
        private GameObject clickedOn,oldClickedObj;

        private bool stopMouseDetect = false;
        
        #endregion

        #region Methods

        void Start()
        {
            currentDrawnColor=ColorType.White;
            //m_Raycaster = GameObject.Find("Canvas").GetComponent<GraphicRaycaster>();
            m_EventSystem = FindObjectOfType<EventSystem>();
        }
        public void Init(DataManager _data,GraphicRaycaster raycaster)
        {
            dataManager = _data;
            m_Raycaster = raycaster;
        }
        void Update()
        {
            if (Input.GetMouseButton(0)&&!stopMouseDetect)
            {
                clickedOn=RayCastFun("MatrixBlock");
                if(clickedOn!=null)
                    OnBlockInteract();
                    
            }

            if (Input.GetMouseButtonUp(0))
            {
                ResetFn();
            }
        }

        void ResetFn()
        {
            currentDrawnColor = ColorType.White;
            lastBlock = null;
            currentBlock = null;
            oldClickedObj = null;
            stopMouseDetect = false;
        }
        void OnBlockInteract()
        {
            if(clickedOn==null || clickedOn==oldClickedObj)
                return;
            
            oldClickedObj = clickedOn;
            currentBlock = clickedOn.GetComponent<Block>();
            
            if (currentDrawnColor == ColorType.White && (currentBlock.isIconBlock||currentBlock.isLineDrawn))//hand down either on icon or half drawn line
            {
                if (currentBlock.isIconBlock && CheckIfLineDrawInProgress(currentBlock.ctype))//if tapped on icon of color whose line is made or halfway->remove line
                {
                    ClearLineData(currentBlock.ctype);
                }
                else//if tapped on icon of color whose line is not made or halfway line of any color
                {
                    currentDrawnColor = currentBlock.ctype;
                    
                    lastBlock = currentBlock;

                    if(lastBlock.isLineDrawn)
                        TrimLineData(currentDrawnColor,lastBlock);
                    
                    //Debug.Log(lastBlock.gameObject.name+"$$"+currentBlock.isLineDrawn+"$$"+currentBlock.ctype);

                }
            }else if (currentDrawnColor != ColorType.White)//hand drag continue
            {
                if (!currentBlock.isIconBlock)//hand drag continue and move over non-icon block
                {
                    Debug.Log("SELF INTERSECT>>"+IsLineSelfIntersect(currentDrawnColor,currentBlock));
                    bool isSelfIntersect = IsLineSelfIntersect(currentDrawnColor, currentBlock);//Check if line intersect with self i.e traces back on same path
                    
                    if(currentBlock.isLineDrawn&&!isSelfIntersect)
                        TrimOverlappingLineData(currentBlock.ctype,currentBlock);//Trim if intersect with other color line in path
                    
                    if(!isSelfIntersect)
                        DrawLine(lastBlock,currentBlock);//Draw line as path is now clear
                    else
                    {
                        lastBlock = currentBlock;

                        TrimSelfLineData(currentDrawnColor, currentBlock);//Erase on-going line as user undoing path
                    }
                }
                else//hand drag continue and move over icon block
                {
                    if (currentBlock.ctype == currentDrawnColor)
                    {
                        
                        bool isSelfIntersect = IsLineSelfIntersect(currentDrawnColor, currentBlock);

                        if(!isSelfIntersect)//if reached another end of same color icon
                        {
                            Debug.Log("Correct");
                            stopMouseDetect = true;//dont allow mouse to detect next block as combination is completed
                            DrawLine(lastBlock, currentBlock);

                            MarkCombinationComplete(currentDrawnColor, true);//mark combination as complete

                            if (CheckIfLevelComplete())//check if all combination is complete
                            {
                                Debug.Log("Level Complete");
                                m_Raycaster = null;
                                OnLevelWin?.Invoke();
                            }
                        }
                        else
                        {
                            TrimSelfLineData(currentDrawnColor, currentBlock);//if line joins back to startIcon of a color, ease line and restart
                        }
                    }
                }
            }

        }

        
        void DrawLine(Block lBlock,Block curBlock)
        {
            if(lBlock==null||currentBlock==null)
                return;
            
            Debug.Log("DRAW Last>>"+lBlock.gameObject.name+" Cur>>"+curBlock.gameObject.name);
            
            int diff = 0;
            bool isAdjacentBlock = false;
            if ((int)lBlock.matrixPos.x == (int)curBlock.matrixPos.x)
            {
                Debug.Log("Same row");
                //same row
                diff = (int)(lBlock.matrixPos.y - curBlock.matrixPos.y);
                
                
                if (diff < -1 || diff > 1)
                {
                    return;
                }

                isAdjacentBlock = true;
                curBlock.SetLineColor(dataManager.colorDataList[(int)currentDrawnColor].color);
                curBlock.SetPoint(diff, true);

            }
            else if((int)lBlock.matrixPos.y == (int)curBlock.matrixPos.y)
            {
                Debug.Log("Same column");
                //same column
                diff = (int)(curBlock.matrixPos.x - lBlock.matrixPos.x);

                if (diff < -1 || diff > 1)
                {
                    return;
                }

                isAdjacentBlock = true;
                curBlock.SetLineColor(dataManager.colorDataList[(int)currentDrawnColor].color);
                curBlock.SetPoint(diff, false);
            }
            
            if(!isAdjacentBlock)
                return;
            
            Debug.Log("DIFF>>>"+diff);
            lBlock.isLineDrawn = true;
            lBlock.ctype = currentDrawnColor;
            curBlock.ctype = currentDrawnColor;
            
            UpdateLineData(lBlock);

            lastBlock = curBlock;

            UpdateLineData(lastBlock);

        }

        void UpdateLineData(Block lBlock)
        {
            //Debug.Log("Update>>"+lineDataArr[(int)currentDrawnColor].Count);
            if (!lineDataArr[(int)currentDrawnColor].Contains(lBlock))
            {
                lineDataArr[(int)currentDrawnColor].Add(lBlock);

                //UpdateTempArray(currentDrawnColor,lBlock);//delete this
            }
        }

        #region Line Trimming Methods
        
        void TrimLineData(ColorType c, Block block)
        {
            if(lineDataArr[(int)c].Count<=0)
                return;
            
            int index = 0;
            for (int i = 0; i < lineDataArr[(int)c].Count; i++)
            {
                if (lineDataArr[(int)c][i].matrixPos == block.matrixPos)
                {
                    index = i;
                }
            }

            for (int j = index+1; j < lineDataArr[(int)c].Count; j++)
            {
                lineDataArr[(int)c][j].ClearPoint();
            }
            
            Debug.Log("TRIM-LINE index="+ (index+1)+"Count="+ (lineDataArr[(int)c].Count - index-1)+" Length="+lineDataArr[(int)c].Count);
            lineDataArr[(int)c].RemoveRange(index+1,lineDataArr[(int)c].Count-index-1);
            //GetTempArray(c).RemoveRange(index+1,GetTempArray(c).Count-index-1);//delete this
            
            MarkCombinationComplete(c,false);

        }
        void TrimOverlappingLineData(ColorType c, Block block)
        {
            if(lineDataArr[(int)c].Count<=0)
                return;
            
            int index = 0;
            for (int i = 0; i < lineDataArr[(int)c].Count; i++)
            {
                if (lineDataArr[(int)c][i].matrixPos == block.matrixPos)
                {
                    index = i;
                }
            }

            for (int j = index+1; j < lineDataArr[(int)c].Count; j++)
            {
                lineDataArr[(int)c][j].ClearPoint();
            }
            
            Debug.Log("TRIM-OVERLAP index="+index+"Count="+ (lineDataArr[(int)c].Count - index-1)+" Length="+lineDataArr[(int)c].Count);
            lineDataArr[(int)c].RemoveRange(index,lineDataArr[(int)c].Count-index);
            //GetTempArray(c).RemoveRange(index,GetTempArray(c).Count-index);//delete this
            
            MarkCombinationComplete(c,false);

        }

        bool IsLineSelfIntersect(ColorType c,Block block)
        {
            //Debug.Log(block.gameObject.name+"<<SELF>>"+lineDataArr[(int)c].Count);
            
            if(lineDataArr[(int)c].Count<=0)
                return false;

            return lineDataArr[(int)c].Contains(block);
            
        }
        
        void TrimSelfLineData(ColorType c, Block block)
        {
            if(lineDataArr[(int)c].Count<=0)
                return;

            int index = 0;
            for (int i = 0; i < lineDataArr[(int)c].Count; i++)
            {
                if (lineDataArr[(int)c][i].matrixPos == block.matrixPos)
                {
                    index = i;
                }
            }

            for (int j = index+1; j < lineDataArr[(int)c].Count; j++)
            {
                lineDataArr[(int)c][j].ClearPoint();
            }
            
            Debug.Log("TRIM-SELF index="+ (index+1)+"Count="+ (lineDataArr[(int)c].Count - index-1)+" Length="+lineDataArr[(int)c].Count);

            if (index > 0)
            {
                lineDataArr[(int)c].RemoveRange(index+1,lineDataArr[(int)c].Count-index-1);
                //GetTempArray(c).RemoveRange(index+1,GetTempArray(c).Count-index-1);//delete this
            }
            else
            {
                lastBlock = lineDataArr[(int)c][0];//Set start Point of line as latestBlock if line intersect back to origin pt

                lineDataArr[(int)c].RemoveRange(index,lineDataArr[(int)c].Count-index);
                //GetTempArray(c).RemoveRange(index,GetTempArray(c).Count-index);//delete this
            }
        }
        
        void ClearLineData(ColorType type)
        {
            Debug.Log("Clear>>"+lineDataArr[(int)type].Count);
            if (lineDataArr[(int)type].Count>0)
            {
                lineDataArr[(int)type].ForEach(s=>s.ClearPoint());
                lineDataArr[(int)type].Clear();
            }
            
            //ClearTempArray(type);//delete this
            
            MarkCombinationComplete(type,false);
        }

        #endregion
        
        bool CheckIfLineDrawInProgress(ColorType colorType)
        {
            if (lineDataArr[(int)colorType].Count != 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        
        void MarkCombinationComplete(ColorType type,bool isComplete)
        {
            completeArr[(int)type] = isComplete;

            int cnt = 0;
            for (int i = 0; i < completeArr.Length; i++)
            {
                if (completeArr[i]) cnt++;
            }

            scoreTxt.text = "Flow: " + cnt + "/" + completeArr.Length;
        }

        bool CheckIfLevelComplete()
        {
            for (int i = 0; i < completeArr.Length; i++)
            {
                if (!completeArr[i])
                    return false;
            }

            return true;
        }
        
        private GameObject RayCastFun(string keyStr)
        {
            if (m_Raycaster == null || m_EventSystem==null)
                return null;
            
            m_PointerEventData = new PointerEventData(m_EventSystem);
            m_PointerEventData.position = Input.mousePosition;

            List<RaycastResult> results = new List<RaycastResult>();
            m_Raycaster.Raycast(m_PointerEventData, results);

            GameObject hitObj = null;
            //For every result returned, output the name of the GameObject on the Canvas hit by the Ray
            foreach (RaycastResult result in results)
            {
                //Debug.Log("Hit " + result.gameObject.name);
                if (result.gameObject.name.Contains(keyStr))
                {
                    hitObj = result.gameObject;
                    break;
                }
            }

            return hitObj;
        }
        
        public void SpawnLevel(LevelData levelData,UnityAction levelComplete)
        {
            lvlData = levelData;
            OnLevelWin = levelComplete;
            playMatrix = new Block[levelData.matrixSize, levelData.matrixSize];
            
            //----Block spawn
            for (int i = 0; i < levelData.matrixSize; i++)
            {
                for (int j = 0; j < levelData.matrixSize; j++)
                {
                    GameObject block = Instantiate(blockPrefab, transform);
                    block.name = "MatrixBlock_" + i +"_"+ j;

                    Block b = block.GetComponent<Block>();
                    playMatrix[i, j] = b;
                    b.matrixPos = new Vector2(i, j);
                }
            }
            
            //---Level Data Spawn
            lineDataArr= new List<Block>[levelData.nodeList.Count];
            completeArr = new bool[levelData.nodeList.Count];

            for (int k = 0; k < levelData.nodeList.Count; k++)
            {
                NodeData node = levelData.nodeList[k];

                Block block1 = playMatrix[node.startPos[0], node.startPos[1]];
                Block block2 = playMatrix[node.endPos[0], node.endPos[1]];
                
                GameObject startNode = Instantiate(nodePrefab,block1.transform);
                startNode.name = "Node_" + node.nodeKey;
                
                GameObject endNode = Instantiate(nodePrefab,block2.transform);
                endNode.name = "Node_" + node.nodeKey;

                Color color=Color.white;

                foreach (ColorData colorData in dataManager.colorDataList)
                {
                    if (colorData.colorKey.Equals(node.nodeKey))
                    {
                        color = colorData.color;
                        break;
                    }
                }
                
                // dataManager.colorDataList.Find(s => s.colorKey.Equals(endNode.nodeKey)).color;
                startNode.GetComponent<Image>().color =color;
                endNode.GetComponent<Image>().color = color;
                
                block1.Init(true,node.colorType);
                block2.Init(true,node.colorType);

                lineDataArr[k] = new List<Block>();
            }
            
            Debug.Log("COUNT>>"+lineDataArr.Length);
            scoreTxt.text = "Flow: " + 0 + "/" + completeArr.Length;

        }

        public void ClearPreviousLevel()
        {
            if(lvlData==null)
                return;
            
            for (int i = 0; i < lvlData.matrixSize; i++)
            {
                for (int j = 0; j < lvlData.matrixSize; j++)
                {
                    Destroy(playMatrix[i, j].gameObject);
                }
            }

            for (int i = 0; i < lineDataArr.Length; i++)
            {
                lineDataArr[i].Clear();
            }
            
            //-delete this
            /*greenLineList.Clear();
            redLineList.Clear();
            orangeLineList.Clear();
            blueLineList.Clear();
            purpleLineList.Clear();*/
        }
        
        #region Temp Methods
        //--delete this
        /*
        void UpdateTempArray(ColorType type,Block lBlock)
        {
            GetTempArray(type).Add(lBlock.gameObject);
        }

        void ClearTempArray(ColorType c)
        {
            GetTempArray(c).Clear();
        }

        List<GameObject> GetTempArray(ColorType c)
        {
            if (c == ColorType.Red)
            {
                return redLineList;
            }else if (c == ColorType.Blue)
            {
                return blueLineList;
            }else if (c == ColorType.Green)
            {
                return greenLineList;
            }else if (c == ColorType.Orange)
            {
                return orangeLineList;
            }else if (c == ColorType.Purple)
            {
                return purpleLineList;
            }

            return null;
        }
        */
        

        #endregion

        #endregion

    }

}
