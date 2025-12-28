using System;
using UnityEngine;

public class Door : MonoBehaviour
{
    public bool isOpen = false;
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void Set()
    {
        if (isOpen)
        {
            animator.SetBool("Open", true);
        }
        else
        {
            animator.SetBool("Open", false);
        }
    }
}
