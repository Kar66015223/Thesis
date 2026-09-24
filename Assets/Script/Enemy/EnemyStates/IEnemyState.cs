using UnityEngine;

public interface IEnemyState
{
    void Enter();
    void Update();
    void Exit();

    void OnHearSound(SoundSignal sound);
    void OnSeenTarget(Transform target);
}