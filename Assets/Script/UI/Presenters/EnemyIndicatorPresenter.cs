using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Floating "!" / "?" icons over enemies. Enemies only report their indicator; this class owns
/// the sprite mapping, so the enemy code no longer knows which icon is used for what.
/// </summary>
public class EnemyIndicatorPresenter : IUIPresenter
{
    public struct Sprites
    {
        public Sprite See;
        public Sprite Hear;
        public Sprite Walk;
        public Sprite Chase;
        public Sprite Stunned;
    }

    private readonly Transform parent;
    private readonly GameObject prefab;
    private readonly Sprites sprites;

    private readonly Dictionary<Transform, EnemyStateUI> activeUIs = new();
    private readonly Dictionary<Transform, EnemyState> currentStates = new();

    public EnemyIndicatorPresenter(Transform parent, GameObject prefab, Sprites sprites)
    {
        this.parent = parent;
        this.prefab = prefab;
        this.sprites = sprites;
    }

    public void Bind()
    {
        EnemyEvents.IndicatorChanged += HandleIndicatorChanged;
        EnemyEvents.ConfirmProgressChanged += HandleConfirmProgress;
    }

    public void Unbind()
    {
        EnemyEvents.IndicatorChanged -= HandleIndicatorChanged;
        EnemyEvents.ConfirmProgressChanged -= HandleConfirmProgress;
    }

    private void HandleIndicatorChanged(Transform enemy, Transform anchor, EnemyState state)
    {
        if (enemy == null)
            return;

        currentStates[enemy] = state;

        if (state == EnemyState.None)
        {
            Remove(enemy);
            return;
        }

        EnemyStateUI ui = GetOrCreate(enemy, anchor);
        if (ui == null)
            return;

        GetSprites(state, out Sprite main, out Sprite sub);
        ui.SetIcons(main, sub);
        ui.SetFill(state == EnemyState.Confirming ? 0f : 1f);
    }

    private void HandleConfirmProgress(Transform enemy, float progress)
    {
        // Only the "?" icon fills up; every other icon stays full.
        if (enemy != null &&
            activeUIs.TryGetValue(enemy, out EnemyStateUI ui) && ui != null &&
            currentStates.TryGetValue(enemy, out EnemyState state) && state == EnemyState.Confirming)
        {
            ui.SetFill(progress);
        }
    }

    private void GetSprites(EnemyState state, out Sprite main, out Sprite sub)
    {
        switch (state)
        {
            case EnemyState.DistractedLook: main = sprites.Hear; sub = sprites.See; break;
            case EnemyState.DistractedMove: main = sprites.Hear; sub = sprites.Walk; break;
            case EnemyState.Confirming: main = sprites.See; sub = null; break;
            case EnemyState.Chase: main = sprites.Chase; sub = null; break;
            case EnemyState.Stunned: main = sprites.Stunned; sub = null; break;
            default: main = null; sub = null; break;
        }
    }

    private EnemyStateUI GetOrCreate(Transform enemy, Transform anchor)
    {
        if (activeUIs.TryGetValue(enemy, out EnemyStateUI existing) && existing != null)
            return existing;

        if (prefab == null)
            return null;

        GameObject instance = Object.Instantiate(prefab, parent);
        if (!instance.TryGetComponent(out EnemyStateUI ui))
        {
            Debug.LogError("Enemy state UI prefab needs an EnemyStateUI component.", prefab);
            Object.Destroy(instance);
            return null;
        }

        if (instance.TryGetComponent(out UITargetTracking tracking))
            tracking.SetTarget(anchor != null ? anchor : enemy);

        activeUIs[enemy] = ui;
        return ui;
    }

    private void Remove(Transform enemy)
    {
        if (activeUIs.TryGetValue(enemy, out EnemyStateUI ui))
        {
            if (ui != null)
                Object.Destroy(ui.gameObject);

            activeUIs.Remove(enemy);
        }
    }
}
