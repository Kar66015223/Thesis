using System.Collections.Generic;
using UnityEngine;

public class EnemyIndicatorPresenter : IUIPresenter
{
    public struct Sprites
    {
        public Sprite See;
        public Sprite Hear;
        public Sprite Walk;
        public Sprite Chase;
        public Sprite Stunned;
        public Sprite[] ConfirmLevels;
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

        GetSprites(state, out Sprite main, out Sprite sub, out Color color);
        ui.SetIcons(main, sub, color);
    }

    private void HandleConfirmProgress(Transform enemy, float progress)
    {
        if (enemy != null &&
            activeUIs.TryGetValue(enemy, out EnemyStateUI ui) && ui != null &&
            currentStates.TryGetValue(enemy, out EnemyState state) && state == EnemyState.Confirming)
        {
            ui.SetIcons(GetConfirmSprite(progress), null, Color.white);
        }
    }

    private Sprite GetConfirmSprite(float progress)
    {
        Sprite[] levels = sprites.ConfirmLevels;
        if (levels == null || levels.Length == 0)
            return sprites.See;

        int level = Mathf.Min(Mathf.FloorToInt(progress * levels.Length), levels.Length - 1);
        return levels[level];
    }

    private void GetSprites(EnemyState state, out Sprite main, out Sprite sub, out Color color)
    {
        switch (state)
        {
            case EnemyState.DistractedLook:
                main = sprites.Hear;
                sub = sprites.See;
                color = Color.white;
                break;

            case EnemyState.DistractedMove:
                main = sprites.Hear;
                sub = sprites.Walk; 
                color = Color.white;
                break;

            case EnemyState.Confirming:
                main = GetConfirmSprite(0f);
                sub = null; 
                color = Color.white;
                break;

            case EnemyState.Chase:
                main = sprites.Chase;
                sub = null; 
                color = Color.red;
                break;

            case EnemyState.Stunned:
                main = sprites.Stunned;
                sub = null; 
                color = Color.white;
                break;

            default: main = null; sub = null; color = Color.white; break;
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

        if (instance.TryGetComponent(out EnemyIndicatorUI indicator))
            indicator.SetTarget(anchor != null ? anchor : enemy);

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
