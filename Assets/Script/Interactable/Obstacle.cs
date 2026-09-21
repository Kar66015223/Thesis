using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Obstacle : Interactable
{
    [SerializeField] private List<ObstacleSide> sides = new();
    [SerializeField] private ObstacleSide currentSide;
    [SerializeField] private ObstacleSide oppositeSide;

    void Update()
    {
        currentSide = sides.FirstOrDefault(s => s.isPlayerOnThisSide == true);

        
    }

    public void MoveCharacterToOtherSide(
        Transform charTransform, ObstacleSide currentSide, ObstacleSide oppositeSide)
    {
        
    }
}