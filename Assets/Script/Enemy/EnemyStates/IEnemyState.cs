using UnityEngine;

public interface IEnemyState
{
    /// <summary>Icon the UI shows above the enemy while it is in this state.</summary>
    EnemyState Indicator { get; }

    void Enter();
    void Update();
    void Exit();

    void OnHearSound(SoundSignal sound);
    void OnSeenTarget(Transform target);
}
