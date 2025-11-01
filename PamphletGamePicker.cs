using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Entities.UniversalDelegates;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public class PamphletGamePicker : MonoBehaviour
{
    [Header("Animation Details")]
    public Animator animator;
    public AnimationClip clip;
    public float timeBetweenOpenOrClose;
    private float lastTime;

    [Header("Rendering")]
    public SkinnedMeshRenderer pamphletRenderer;

    [Header("Cursor and Colliders")]
    public GameObject cursor;
    public List<GameObject> colliders = new List<GameObject>();

    [Header("VR Input")]
    public InputControl VRInput;
    private bool lastMenuButton;

    [Header("Selection Options")]
    public List<string> gameOptions = new List<string>();
    public TMP_Text selectionText;
    private bool showText = false;

    //Private variable that keeps track of options
    private int selection = 0;

    // Start is called before the first frame update
    void Start()
    {
        animator.Play("Idle");
        pamphletRenderer.enabled = false;
        cursor.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (VRInput.getMenuButton() && !lastMenuButton)
        {
            if (Time.time >= lastTime + timeBetweenOpenOrClose)
            {
                if (!pamphletRenderer.enabled)
                {
                    for (int i = 0; i < colliders.Count; i++)
                    {
                        colliders[i].SetActive(true);
                    }
                    Invoke("SetText", clip.length);
                    pamphletRenderer.enabled = true;
                    cursor.SetActive(true);
                    animator.Play("Open");
                    lastTime = Time.time;
                }
                else
                {
                    for (int i = 0; i < colliders.Count; i++)
                    {
                        colliders[i].SetActive(false);
                    }
                    showText = false;
                    Invoke("turnOffRenderer", clip.length);
                    cursor.SetActive(false);
                    animator.Play("Close");
                    lastTime = Time.time;
                }
            }
        }
        if(showText)
        {
            selectionText.text = gameOptions[selection];
        }
        else
        {
            selectionText.text = "";
        }
        lastMenuButton = VRInput.getMenuButton();
    }

    //Turns off renderer
    private void turnOffRenderer()
    {
        pamphletRenderer.enabled = false;
    }

    //Increment the selection
    public void IncrementSelection()
    {
        if(selection >= gameOptions.Count - 1)
        {
            selection = 0;
        }
        else
        {
            selection++;
        }
    }

    //Reduce the selection
    public void ReduceSelection()
    {
        if(selection == 0)
        {
            selection = gameOptions.Count - 1;
        }
        else
        {
            selection--;
        }
    }

    //Play the given selection
    public void PlaySelection()
    {

    }

    //Sets the text
    private void SetText()
    {
        showText = true;
    }
}
