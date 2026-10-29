using UnityEngine;

public interface IEnemyState
{
    EnemyState Indicator { get; }
    
    void Enter();
    void Update();
    void Exit();

    void OnHearSound(SoundSignal sound);
    void OnSeenTarget(Transform target);
}