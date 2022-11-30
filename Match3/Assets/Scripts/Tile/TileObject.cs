using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
public class TileObject : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    [SerializeField]
    private TileData tileData;

    public bool IsSelect = false;
    public TileData TileData { 
        get { 
            return tileData; 
        } 
        set { 
            tileData = value;
        } 
    }


    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }
    private void Setup()
    {

    }
    public void Setup(TileData tileData)
    {
        this.tileData = tileData;

    }

    public void Boom()
    {
        animator.Play("boom");
        Helper.Wait(this, 0.5f, AfterBoom);
    }

    public void AfterBoom()
    {
        spriteRenderer.sprite = BaseResourcesSupplirs.SpriteSupplier.GetObjectForID("Cell");
    }

    private void OnMouseDown()
    {
        var gridController = GameObject.FindGameObjectWithTag("GridController").GetComponent<GridController>();
        if (gridController.turnInProcess) return;
        if (IsSelect)
        {
            Deselect();
            gridController.Deselect(this);
        } else {
            Select();
            gridController.Select(this);
        }
    }

    public void Select()
    {
        spriteRenderer.color = Color.gray;
        IsSelect= true;
    }
    public void Deselect()
    {
        spriteRenderer.color = Color.white;
        IsSelect= false;
    }
}
