﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralGeneration : MonoBehaviour
{
    public static ProceduralGeneration instance = null;

    [Header("Track")]
    [SerializeField] private Transform center;
    [SerializeField] private Transform ballTransform;

    // Render parameters
    [Header("Renderer")]
    [SerializeField] private float renderRadius;
    [SerializeField] private float renderFOV;
    [SerializeField] private bool cutFOV;

    [Header("Pillars")]
    [SerializeField] private bool biomsON;
    [SerializeField] private GameObject pillarObj;
    [SerializeField] private GameObject trampolineObj;

    private Bioms bioms;
    private float defaultRenderRadius;
    private int prerenderRings;
    private bool prerendering;
    private int ring = 1;
    private int ballRing = 1;
    private float fixedR = 0;
    private float ballDistance = 0;
    private float oldPillarsRingStep;

    void Start()
    {
        // Singletone pattern
        if (instance == null)
        { 
            instance = this; 
        }
        else if (instance == this)
        { 
            Destroy(gameObject); 
        }

        // Prerender
        float pillarsRingStep = Bioms.GetInstance().GetPillarsRingStep();
        oldPillarsRingStep = pillarsRingStep;
        prerenderRings = Mathf.RoundToInt(renderRadius / pillarsRingStep);
        defaultRenderRadius = renderRadius;

        if (prerenderRings == 0)
        {
            prerenderRings = 1;
        }

        prerendering = true;
        for (int i = 1; i <= prerenderRings; i++)
        {
            GenerateRing(i);
            ring = i;
        }
        prerendering = false;
    }

    public float GetRing()
    {
        return ring;
    }
    public float GetBallRing()
    {
        return ballRing;
    }


    private void GenerateRing(int ring)
    {
        // Bioms
        if (biomsON)
        {
            Bioms.instance.CheckNewBiom(ballRing);
            if (renderRadius < Bioms.instance.GetPillarsRingStep()*Bioms.instance.GetCurrentBiom() + defaultRenderRadius)
            {
                //renderRadius += Bioms.instance.GetPillarsRingStep();
            }
        }

        // pillars param init zone
        float pillarsRingStep = Bioms.instance.GetPillarsRingStep();

        float pillarsFrequency = Bioms.instance.GetPillarsFrequency();
        Vector2 pillarsBodyHeight = Bioms.instance.GetPillarsBodyHeight();
        Vector2 pillarsFloorSize = Bioms.instance.GetPillarsFloorSize();

        // trampoline param init zone
        float trampolineSpawnChance = Bioms.instance.GetTrampolineSpawnChance();
        Vector2 trampolineBodyHeight = Bioms.instance.GetTrampolineBodyHeight();
        Vector2 trampolineFloorSize = Bioms.instance.GetTrampolineFloorSize();

        //float rPrev = pillarsRingStep * (ring - 1 - Bioms.instance.GetCurrentBiom()) + 10.0f;
        //float r = pillarsRingStep * (ring - Bioms.instance.GetCurrentBiom());
        float rPrev = fixedR + 10.0f;
        fixedR += pillarsRingStep;
        float r = fixedR;

        int numberOfPillars = 0;
        if (ring == 1)
        {
            numberOfPillars = Mathf.RoundToInt(pillarsFrequency);
        }
        else
        {
            numberOfPillars = Mathf.RoundToInt(pillarsFrequency * r/pillarsRingStep);
        }
        float angleStep = 360.0f / numberOfPillars;

        Vector2 ballPos = new Vector2(ballTransform.position.x, ballTransform.position.z);
        Vector2 centerPos = new Vector2(center.position.x, center.position.z);

        float ballMoveAngle = Mathf.Atan2(ballPos.y - centerPos.y, ballPos.x - centerPos.x) * (180.0f / Mathf.PI);
        if (ballMoveAngle < 0.0f)
        {
            ballMoveAngle += 360.0f;
        }

        // Cut FOV
        if (ring > 5 && cutFOV)
        {
            renderFOV /= (1.0f + 1.0f/(ring*pillarsRingStep*0.05f));
            // Debug.Log(renderFOV);
        }
        float thetaMin = ballMoveAngle - renderFOV/2;
        float thetaMax = thetaMin + renderFOV;


        Color ringColor = new Color(Random.value, Random.value, Random.value);

        // full circle thetaMin = 0; thetaMax = 360.0f
        for (float theta = thetaMin; theta < thetaMax; theta += angleStep)
        {

            float rDist = Random.Range(rPrev, r); //Mathf.Sqrt(Random.value)
            float x = center.position.x + rDist * Mathf.Cos(theta * (Mathf.PI / 180.0f));
            float z = center.position.z + rDist * Mathf.Sin(theta * (Mathf.PI / 180.0f));


            Vector2 bodyHeight;
            Vector2 floorSize;
            GameObject obj;

            // Default pillar
            if (Random.value > trampolineSpawnChance)
            {
                bodyHeight = pillarsBodyHeight;
                floorSize = pillarsFloorSize;
                obj = pillarObj;
            }
            // Trampoline pillar
            else
            {
                bodyHeight = trampolineBodyHeight;
                floorSize = trampolineFloorSize;
                obj = trampolineObj;
            }
            float h = Random.Range(bodyHeight.x, bodyHeight.y);
            float s = Random.Range(floorSize.x, floorSize.y);

            CreatePillar(x, z, s, h, ringColor, obj, !prerendering, ring);
        }
    }

    public bool IsRender()
    {
        return prerendering;
    }

    private GameObject CreatePillar(float x, float z, float s, float h, Color ringColor, GameObject obj, bool isAnimate, int ring)
    {
        Vector3 position = new Vector3(x, transform.position.y, z);
        GameObject pillar = Instantiate(obj, position, Quaternion.identity);
        pillar.GetComponent<Pillar>().SetRing(ring);

        // Animations
        if (isAnimate)
        { 
            pillar.GetComponent<Animator>().SetTrigger("Appear");
        }
        
        // Coloring
        Transform pillarModel = pillar.transform.GetChild(0);
        pillarModel.GetChild(0).GetComponent<Renderer>().material.color = ringColor;
        pillarModel.localScale = new Vector3(s, h, s);

        // Puddle
        if (Random.value <= Bioms.instance.GetPuddleSpawnChance() && obj.tag == "Bounce")
        {
            Puddle puddle = pillarModel.GetChild(0).gameObject.AddComponent(typeof(Puddle)) as Puddle;
            if (Random.value <= Bioms.instance.GetPuddleBoostChance())
            {
                puddle.SetPuddleType(Puddle.PuddleTypes.BOOST);
            }
            else
            {
                puddle.SetPuddleType(Puddle.PuddleTypes.SLOW);
            }
        }

        // Diamonds
        if (Random.value <= Bioms.instance.GetDiamondsSpawnChance() && obj.tag == "Bounce")
        {
            Debug.Log("SPAWN !!!");
            GameObject diamond = Diamond.SpawnDiamond(
                Bioms.instance.GetDiamondsVariety(), 
                Bioms.instance.GetDiamondsPrefabs(), 
                Bioms.instance.GetDiamondsProbabilities(), 
                pillarModel.GetChild(0));
            diamond.transform.parent = pillarModel;
        }

        return pillar;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        float pillarsRingStep = Bioms.GetInstance().GetPillarsRingStep();
        for (int i = 1; i <= ring; i++)
        {
           UnityEditor.Handles.DrawWireDisc(center.position, center.up, pillarsRingStep * i);
        }
    }

    void Update()
    {
        // new ring
        Vector2 ballPos = new Vector2(ballTransform.position.x, ballTransform.position.z);
        Vector2 centerPos = new Vector2(center.position.x, center.position.z);

        float pillarsRingStep = Bioms.instance.GetPillarsRingStep();
        Vector2 pillarsFloorSize = Bioms.instance.GetPillarsFloorSize();

        if (Vector2.Distance(ballPos, centerPos) >= ballDistance + oldPillarsRingStep)
        {
            ballDistance += oldPillarsRingStep;
            ballRing += 1;
            if (Bioms.instance.GetNewBiom())
            {
                oldPillarsRingStep = pillarsRingStep;
            }
            Debug.Log(ballRing);
        }
        if (Vector2.Distance(ballPos, centerPos) + renderRadius >= pillarsRingStep + fixedR)
        {
            ring += 1;
            GenerateRing(ring);
        }
    }
}
