using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
public class TileObject : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private ParticleSystem particle;

    [SerializeField]
    private TileData tileData;

    private Color tileColor;

    public bool IsSelected = false;
    public TileData TileData { 
        get { 
            return tileData; 
        } 
        set { 
            tileData = value;
        } 
    }

    public string Type
    {
        get
        {
            return tileData.Type;
        }
    }

    public int X
    {
        get
        {
            return tileData.x;
        }
    }

    public int Y
    {
        get
        {
            return tileData.y;
        }
    }


    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        particle = GetComponent<ParticleSystem>();
        tileColor = spriteRenderer.color;
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
        particle.Play();
    }


    private void OnMouseDown()
    {
        var gridController = GameObject.FindGameObjectWithTag("GridController").GetComponent<GridController>();
        if (gridController.turnInProcess) return;
        if (IsSelected)
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
        spriteRenderer.color = new Color(tileColor.r, tileColor.g, tileColor.b, 0.5f);
        IsSelected= true;
    }
    public void Deselect()
    {
        spriteRenderer.color = tileColor;
        IsSelected= false;
    }
}
