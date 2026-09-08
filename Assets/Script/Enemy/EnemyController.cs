using System.Runtime.CompilerServices;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public EnemyState CurrentState { get; private set; }

    private EnemyPatrol patrol;

    void Update()
    {
        switch(CurrentState)
        {
            case EnemyState.Patrol:
                UpdatePatrol();
                break;
        }
    }

    void UpdatePatrol()
    {
        
    }

    void UpdateChase()
    {

    }
    
    void UpdateStrangling()
    {
        
    }
}
