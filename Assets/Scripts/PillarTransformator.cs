using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PillarType
{
    PILLAR,
    TRAMPOLINE
}

public class PillarTransformator
{
    public static void ReshapePillar(GameObject pillarObj, BiomData biomData)
    {
        float drawnTrampolineChance = Random.value;

        Pillar pillar = pillarObj.GetComponent<Pillar>();
        pillar.Trampoline.enabled = false;

        pillar.Puddle.enabled = false;
        pillar.Puddle.gameObject.SetActive(false);

        if (drawnTrampolineChance > biomData.TrampolineSpawnChance)
        {
            ReshapeToCommonPillar(pillar, biomData);

            float drawnPuddleChance = Random.value;
            if (drawnPuddleChance <= biomData.PuddleSpawnChance)
            {
                AddPuddle(pillar, biomData);
            }
        }
        else
        {
            ReshapeToTrampoline(pillar, biomData);
        }
    }

    private static void ReshapeToCommonPillar(Pillar pillar, BiomData biomData)
    {
        ReshapeTo(pillar,
                  Random.Range(biomData.PillarsBodyHeightRange.x, biomData.PillarsBodyHeightRange.y),
                  Random.Range(biomData.PillarsFloorSizeRange.x, biomData.PillarsFloorSizeRange.y),
                  biomData.PillarsBodyColor,
                  biomData.PillarsFloorColor);
    }

    private static void AddPuddle(Pillar pillar, BiomData biomData)
    {
        Puddle puddle = pillar.Puddle;
        puddle.enabled = true;
        puddle.gameObject.SetActive(true);

        if (Random.value <= biomData.PuddleBoostChance)
        {
            puddle.SlowPuddle.SetActive(false);
            puddle.BoostPuddle.SetActive(true);

            puddle.PuddleText.text = (int)(biomData.PuddleBoostPower * 100) + "%";
            puddle.BoostPower = biomData.PuddleBoostPower;
            puddle.CurrentType = Puddle.PuddleTypes.BOOST;
        }
        else
        {
            puddle.SlowPuddle.SetActive(true);
            puddle.BoostPuddle.SetActive(false);

            puddle.PuddleText.text = (int)(biomData.PuddleSlowPower * 100) + "%";
            puddle.BoostPower = biomData.PuddleSlowPower * -1;
            puddle.CurrentType = Puddle.PuddleTypes.SLOW;
        }
    }

    private static void ReshapeToTrampoline(Pillar pillar, BiomData biomData)
    {
        Trampoline trampoline = pillar.Trampoline;
        trampoline.enabled = true;

        ReshapeTo(pillar, 
                  Random.Range(biomData.TrampolineBodyHeightRange.x, biomData.TrampolineBodyHeightRange.y),
                  Random.Range(biomData.TrampolineFloorSizeRange.x, biomData.TrampolineFloorSizeRange.y),
                  trampoline.BodyColor, 
                  trampoline.FloorColor);
    }

    private static void ReshapeTo(Pillar pillar, float h, float s, Color body, Color floor)
    {
        // Set size
        pillar.Model.transform.localScale = new Vector3(s, h, s);

        // Set coloring
        pillar.Body.transform.GetComponent<Renderer>().material.color = body;
        pillar.Floor.transform.GetComponent<Renderer>().material.color = floor;
    }
}