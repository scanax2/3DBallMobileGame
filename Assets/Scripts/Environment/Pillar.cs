﻿using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Pillar : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private Trampoline trampoline;
    public Trampoline Trampoline { get => trampoline; }

    [SerializeField]
    private Puddle puddle;
    public Puddle Puddle { get => puddle; }

    [SerializeField]
    private GameObject model;
    public GameObject Model { get => model; }

    [SerializeField]
    private Transform diamondSpawnTransform;
    public Transform DiamondSpawnTransform { get => diamondSpawnTransform; }

    [SerializeField]
    private Animator animator;
    public Animator Animator { get => animator; }

    [Header("Construction")]
    [SerializeField]
    private Renderer bodyRenderer;
    public Renderer BodyRenderer { get => bodyRenderer; }

    [SerializeField]
    private Renderer floorRenderer;
    public Renderer FloorRenderer { get => floorRenderer; }

    private int lifetime;


    public void InitValues(int lifetime)
    {
        this.lifetime = lifetime;
    }

    public void TryDisable()
    {
        lifetime -= 1;

        if (lifetime <= 0)
        {
            if (ProceduralGeneration.Instance.IsEditorUsing)
            {
                gameObject.SetActive(false);
            }

            animator.SetTrigger("Disappear");
        }
    }
}
