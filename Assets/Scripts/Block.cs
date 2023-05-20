using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    #region Variables

    [SerializeField] private BoxCollider2D boxCollider;
    [SerializeField] private LineRenderer lineRenderer;
    
    public ColorType ctype;
    public bool isIconBlock;
    public bool isLineDrawn;
    public Vector2 matrixPos;
    private int valBuff = 45;
    private int valLength = 245;    

    #endregion

    #region Methods
    public void Init(bool isIcon,ColorType type=ColorType.White)
    {
        isIconBlock = isIcon;
        ctype=type;
    }

    public void SetLineColor(Color _color)
    {
        lineRenderer.startColor=_color;
        lineRenderer.endColor=_color;
    }
    
    public void SetPoint(int val,bool isHorizontal)
    {
        if (isHorizontal)
        {
            lineRenderer.SetPosition(0,new Vector3(-(val * valBuff),0,0));
            lineRenderer.SetPosition(1,new Vector3(val*valLength,0,0));
        }
        else
        {
            lineRenderer.SetPosition(0,new Vector3(0, -(val * valBuff),0));
            lineRenderer.SetPosition(1,new Vector3(0,val*valLength,0));
        }

        isLineDrawn = true;
    }

    public void ClearPoint()
    {
        //Debug.Log("Clearing>>"+gameObject.name);
        lineRenderer.SetPosition(0,new Vector3(0,0,0));
        lineRenderer.SetPosition(1,new Vector3(0,0,0));

        isLineDrawn = false;
        
        if(!isIconBlock)
            ctype = ColorType.White;
    }
    
    #endregion

}
