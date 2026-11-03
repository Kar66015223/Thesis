#if UNITY_EDITOR
using System;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class InventoryLayoutBuilder
{
    private const string UndoName = "Build Inventory UI";
    private const string PlayerPrefabPath = "Assets/Prefab/Character/Player.prefab";
    private const string SpriteFolder = "Assets/Sprite/Inventory/";
    private const string PanelBGSprite = SpriteFolder + "Inventory_Panel_BG.png";
    private const string CornerSprite = SpriteFolder + "Inventory_Panel_Corner.png";
    private const string DividerSprite = SpriteFolder + "Inventory_Header_Divider.png";
    private const string ScrollbarSprite = SpriteFolder + "Inventory_Scrollbar.png";
    private const string ScrollbarHandleSprite = SpriteFolder + "Inventory_Scrollbar_Handle.png";

    private const string InventoryPanelName = "InventoryPanel";
    private const string SlotPanelName = "SlotPanel";
    private const string SlotPanelCornerName = "SlotPanel_Corner";
    private const string GridName = "SlotScrollView";
    private const string ViewportName = "Viewport";
    private const string ContentName = "Content";
    private const string HeaderName = "InventoryHeader";
    private const string ScrollLineName = "ScrollLine";
    private const string ScrollbarName = "SlotScrollbar";
    private const string DetailPanelName = "ItemDetailPanel";
    private const string QuickSlotPanelName = "QuickSlotPanel";

    private const float CornerBorder = 104f;
    private static readonly Vector4 ScrollbarBorder = new(0f, 14f, 0f, 8f);
    private const float ScrollbarWidth = 26f;
    private const float ScrollbarTrackWidth = 12f;
    private const float ScrollSensitivity = 20f;
    private const int GridColumns = 5;
    private static readonly Vector2 GridSpacing = new(30f, 30f);
    private const float ColumnGap = 30f;
    private const float DetailHeightShare = 0.56f;
    private const int ActionButtonCount = 4;
    private const int QuickSlotCount = 3;

    private static readonly Color TextColor = new(0.93f, 0.93f, 0.93f, 1f);
    private static readonly Color SubTextColor = new(0.78f, 0.78f, 0.78f, 1f);
    private static readonly Color LineColor = new(0.95f, 0.95f, 0.95f, 1f);
    private static readonly Color SlotColor = new(0.11f, 0.11f, 0.11f, 0.95f);
    private static readonly Color SlotOutlineColor = new(0.32f, 0.32f, 0.32f, 1f);
    private static readonly Color BadgeColor = new(0.08f, 0.08f, 0.08f, 1f);
    private static readonly Color ButtonColor = new(0.42f, 0.42f, 0.42f, 1f);

    private static Sprite panelBG;
    private static Sprite corner;
    private static Sprite divider;
    private static Sprite scrollbarBody;
    private static Sprite scrollbarHandle;
    private static Sprite uiSprite;
    private static Sprite knob;

    [MenuItem("Tools/For Your Tomorrow/Build Inventory Layout")]
    private static void BuildLayout() => EditInventory("layout", (root, inventoryPanel, slotPanel) =>
    {
        BuildHeader(slotPanel);
        BuildRightColumn(inventoryPanel, slotPanel);
        return true;
    });

    [MenuItem("Tools/For Your Tomorrow/Build Inventory Scroll View")]
    private static void BuildScrollView() => EditInventory("scroll view", BuildSlotScrollView);

    private static void EditInventory(string part, Func<GameObject, RectTransform, RectTransform, bool> build)
    {
        PrefabStage stage = PrefabStageUtility.GetCurrentPrefabStage();
        bool isPrefabOpen = stage != null && stage.assetPath == PlayerPrefabPath;
        GameObject root = isPrefabOpen ? stage.prefabContentsRoot : PrefabUtility.LoadPrefabContents(PlayerPrefabPath);
        bool built = false;

        try
        {
            RectTransform inventoryPanel = FindDeep(root.transform, InventoryPanelName) as RectTransform;
            RectTransform slotPanel = inventoryPanel != null ? FindDeep(inventoryPanel, SlotPanelName) as RectTransform : null;
            if (slotPanel == null)
            {
                EditorUtility.DisplayDialog(UndoName, $"Couldn't find {InventoryPanelName}/{SlotPanelName} in the Player prefab.", "OK");
                return;
            }

            LoadSprites();
            built = build(root, inventoryPanel, slotPanel);
            if (!built)
                return;

            if (isPrefabOpen)
                EditorSceneManager.MarkSceneDirty(stage.scene);
            else
                PrefabUtility.SaveAsPrefabAsset(root, PlayerPrefabPath);
        }
        finally
        {
            if (!isPrefabOpen)
                PrefabUtility.UnloadPrefabContents(root);
        }

        if (built)
        {
            Debug.Log(isPrefabOpen
                ? $"Inventory {part} built in the open Player prefab. Save the prefab to keep it."
                : $"Inventory {part} built and saved to the Player prefab.");
        }
    }

    private static void LoadSprites()
    {
        SetBorderIfMissing(CornerSprite, new Vector4(CornerBorder, CornerBorder, CornerBorder, CornerBorder));
        SetBorderIfMissing(ScrollbarSprite, ScrollbarBorder);

        panelBG = LoadSprite(PanelBGSprite);
        corner = LoadSprite(CornerSprite);
        divider = LoadSprite(DividerSprite);
        scrollbarBody = LoadSprite(ScrollbarSprite);
        scrollbarHandle = LoadSprite(ScrollbarHandleSprite);
        uiSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
    }

    private static void SetBorderIfMissing(string path, Vector4 border)
    {
        if (AssetImporter.GetAtPath(path) is TextureImporter importer && importer.spriteBorder == Vector4.zero)
        {
            importer.spriteBorder = border;
            importer.SaveAndReimport();
        }
    }

    private static Sprite LoadSprite(string path)
    {
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite == null)
            Debug.LogWarning($"{UndoName}: sprite not found at {path}.");

        return sprite;
    }

    private static void BuildHeader(RectTransform slotPanel)
    {
        DestroyChild(slotPanel, HeaderName);

        RectTransform header = CreateRect(HeaderName, slotPanel);
        SetOffsets(header, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(70f, -110f), new Vector2(-90f, -30f));

        RectTransform icon = CreateRect("TitleIcon", header);
        SetAnchored(icon, new Vector2(0f, 0.5f), new Vector2(20f, 0f), new Vector2(18f, 18f));
        icon.localRotation = Quaternion.Euler(0f, 0f, 45f);
        AddImage(icon, null, LineColor, false);

        RectTransform title = CreateRect("Title", header);
        SetAnchored(title, new Vector2(0f, 0.5f), new Vector2(45f + 160f, 0f), new Vector2(320f, 80f));
        AddText(title, "INVENTORY", 46f, TextAlignmentOptions.MidlineLeft, FontStyles.Bold, 8f, TextColor);

        RectTransform line = CreateRect("TitleDivider", header);
        SetOffsets(line, new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), new Vector2(380f, -3f), new Vector2(0f, 3f));
        AddImage(line, divider, LineColor, false);
    }

    private static bool BuildSlotScrollView(GameObject root, RectTransform inventoryPanel, RectTransform slotPanel)
    {
        RectTransform view = FindDeep(slotPanel, GridName) as RectTransform;
        if (view == null)
        {
            EditorUtility.DisplayDialog(UndoName, $"Couldn't find {GridName} under {SlotPanelName}.", "OK");
            return false;
        }

        DestroyChild(slotPanel, ScrollLineName);
        DestroyChild(slotPanel, ScrollbarName);
        StopBlockingRaycasts(inventoryPanel.Find(SlotPanelCornerName));

        RectTransform viewport = view.Find(ViewportName) as RectTransform;
        if (viewport == null)
        {
            viewport = CreateRect(ViewportName, view);
            Stretch(viewport);
            Undo.AddComponent<RectMask2D>(viewport.gameObject);
            AddImage(viewport, null, Color.clear, true);
        }

        RectTransform content = viewport.Find(ContentName) as RectTransform;
        if (content == null)
        {
            content = CreateRect(ContentName, viewport);
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.anchoredPosition = Vector2.zero;
            content.sizeDelta = Vector2.zero;
        }

        MoveGridToContent(view, content);

        if (!content.TryGetComponent(out ContentSizeFitter fitter))
            fitter = Undo.AddComponent<ContentSizeFitter>(content.gameObject);

        Undo.RecordObject(fitter, UndoName);
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        if (!view.TryGetComponent(out ScrollRect scrollRect))
            scrollRect = Undo.AddComponent<ScrollRect>(view.gameObject);

        Undo.RecordObject(scrollRect, UndoName);
        scrollRect.content = content;
        scrollRect.viewport = viewport;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.scrollSensitivity = ScrollSensitivity;
        scrollRect.verticalScrollbar = null;

        BuildScrollbar(slotPanel, view, scrollRect);
        PointInventoryAt(root, content);
        return true;
    }

    private static void StopBlockingRaycasts(Transform overlay)
    {
        if (overlay == null || !overlay.TryGetComponent(out Image image) || !image.raycastTarget)
            return;

        Undo.RecordObject(image, UndoName);
        image.raycastTarget = false;
    }

    private static void MoveGridToContent(RectTransform view, RectTransform content)
    {
        if (content.GetComponent<GridLayoutGroup>() != null)
            return;

        GridLayoutGroup oldGrid = view.GetComponent<GridLayoutGroup>();
        GridLayoutGroup grid = Undo.AddComponent<GridLayoutGroup>(content.gameObject);
        grid.cellSize = oldGrid != null ? oldGrid.cellSize : new Vector2(120f, 120f);
        grid.spacing = GridSpacing;
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = GridColumns;
        grid.childAlignment = TextAnchor.UpperCenter;
        grid.padding = new RectOffset(0, 0, 6, 6);

        if (oldGrid != null)
            Undo.DestroyObjectImmediate(oldGrid);
    }

    private static void BuildScrollbar(RectTransform slotPanel, RectTransform view, ScrollRect scrollRect)
    {
        Rect viewBounds = BoundsInParent(view);
        float x = (slotPanel.rect.xMin + viewBounds.xMin) / 2f;
        float handleHeight = scrollbarHandle != null ? scrollbarHandle.rect.height * (100f / scrollbarHandle.pixelsPerUnit) : 25f;

        RectTransform bar = CreateRect(ScrollbarName, slotPanel);
        SetAnchored(bar, new Vector2(0.5f, 0.5f), new Vector2(x, viewBounds.center.y), new Vector2(ScrollbarWidth, viewBounds.height));
        AddImage(bar, null, Color.clear, true);

        RectTransform track = CreateRect("Track", bar);
        SetOffsets(track, new Vector2(0.5f, 0f), new Vector2(0.5f, 1f),
            new Vector2(-ScrollbarTrackWidth / 2f, 0f), new Vector2(ScrollbarTrackWidth / 2f, 0f));
        Image trackImage = AddImage(track, scrollbarBody, Color.white, false);
        trackImage.type = Image.Type.Sliced;

        RectTransform slidingArea = CreateRect("Sliding Area", bar);
        Stretch(slidingArea);

        RectTransform handle = CreateRect("Handle", slidingArea);
        SetOffsets(handle, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        Image handleImage = AddImage(handle, scrollbarHandle, Color.white, true);
        handleImage.preserveAspect = true;

        Scrollbar scrollbar = Undo.AddComponent<Scrollbar>(bar.gameObject);
        scrollbar.handleRect = handle;
        scrollbar.targetGraphic = handleImage;
        scrollbar.direction = Scrollbar.Direction.BottomToTop;
        scrollbar.navigation = new Navigation { mode = Navigation.Mode.None };
        scrollbar.size = Mathf.Clamp01(handleHeight / Mathf.Max(1f, viewBounds.height));
        scrollbar.value = 1f;

        FixedHandleScrollbar follower = Undo.AddComponent<FixedHandleScrollbar>(bar.gameObject);
        SerializedObject serialized = new(follower);
        serialized.FindProperty("scrollRect").objectReferenceValue = scrollRect;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void PointInventoryAt(GameObject root, RectTransform content)
    {
        Inventory inventory = root.GetComponentInChildren<Inventory>(true);
        if (inventory == null)
        {
            Debug.LogWarning($"{UndoName}: no Inventory component in the Player prefab, so slots won't spawn into the scroll view.");
            return;
        }

        SerializedObject serialized = new(inventory);
        serialized.FindProperty("inventoryPanel").objectReferenceValue = content;
        serialized.ApplyModifiedProperties();
    }

    private static void BuildRightColumn(RectTransform inventoryPanel, RectTransform slotPanel)
    {
        DestroyChild(inventoryPanel, DetailPanelName);
        DestroyChild(inventoryPanel, QuickSlotPanelName);

        Rect left = BoundsInParent(slotPanel);
        Rect parent = inventoryPanel.rect;
        float sideMargin = left.xMin - parent.xMin;

        float xMin = left.xMax + ColumnGap;
        float xMax = parent.xMax - sideMargin;
        float detailHeight = (left.height - ColumnGap) * DetailHeightShare;
        float quickHeight = left.height - ColumnGap - detailHeight;

        Rect detailRect = Rect.MinMaxRect(xMin, left.yMax - detailHeight, xMax, left.yMax);
        Rect quickRect = Rect.MinMaxRect(xMin, left.yMin, xMax, left.yMin + quickHeight);

        BuildDetailPanel(CreatePanel(DetailPanelName, inventoryPanel, detailRect));
        BuildQuickSlotPanel(CreatePanel(QuickSlotPanelName, inventoryPanel, quickRect));
    }

    private static RectTransform CreatePanel(string name, RectTransform parent, Rect area)
    {
        RectTransform panel = CreateRect(name, parent);
        SetAnchored(panel, new Vector2(0.5f, 0.5f), area.center, area.size);
        AddImage(panel, panelBG, Color.white, false);

        RectTransform frame = CreateRect("CornerFrame", panel);
        Stretch(frame);
        Image frameImage = AddImage(frame, corner, Color.white, false);
        frameImage.type = Image.Type.Sliced;
        frameImage.fillCenter = false;

        return panel;
    }

    private static void BuildDetailPanel(RectTransform panel)
    {
        RectTransform name = CreateRect("ItemNameText", panel);
        SetOffsets(name, new Vector2(0f, 1f), new Vector2(0.7f, 1f), new Vector2(45f, -120f), new Vector2(0f, -40f));
        AddText(name, "NAME ITEM", 46f, TextAlignmentOptions.MidlineLeft, FontStyles.Bold, 6f, TextColor);

        RectTransform amount = CreateRect("ItemAmountText", panel);
        SetOffsets(amount, new Vector2(0.7f, 1f), new Vector2(1f, 1f), new Vector2(0f, -120f), new Vector2(-45f, -40f));
        AddText(amount, "x8", 32f, TextAlignmentOptions.MidlineRight, FontStyles.Normal, 2f, TextColor);

        RectTransform dividerRow = CreateRect("Divider", panel);
        SetOffsets(dividerRow, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(45f, -146f), new Vector2(-45f, -134f));

        RectTransform leftLine = CreateRect("LineLeft", dividerRow);
        SetOffsets(leftLine, new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -3f), new Vector2(-14f, 3f));
        AddImage(leftLine, divider, LineColor, false);

        RectTransform dot = CreateRect("Dot", dividerRow);
        SetAnchored(dot, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(10f, 10f));
        AddImage(dot, knob, LineColor, false);

        RectTransform rightLine = CreateRect("LineRight", dividerRow);
        SetOffsets(rightLine, new Vector2(0.5f, 0.5f), new Vector2(1f, 0.5f), new Vector2(14f, -3f), new Vector2(0f, 3f));
        rightLine.localScale = new Vector3(-1f, 1f, 1f);
        AddImage(rightLine, divider, LineColor, false);

        RectTransform details = CreateRect("ItemDetailsText", panel);
        SetOffsets(details, Vector2.zero, Vector2.one, new Vector2(50f, 120f), new Vector2(-50f, -170f));
        TextMeshProUGUI detailsText = AddText(details, "Details...................................................................", 28f,
            TextAlignmentOptions.TopLeft, FontStyles.Normal, 1f, SubTextColor);
        detailsText.textWrappingMode = TextWrappingModes.Normal;

        RectTransform buttons = CreateRect("ActionButtons", panel);
        SetOffsets(buttons, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(50f, 45f), new Vector2(-50f, 105f));
        HorizontalLayoutGroup row = Undo.AddComponent<HorizontalLayoutGroup>(buttons.gameObject);
        row.spacing = 28f;
        row.childAlignment = TextAnchor.MiddleCenter;
        row.childControlWidth = true;
        row.childControlHeight = true;
        row.childForceExpandWidth = true;
        row.childForceExpandHeight = true;

        for (int i = 1; i <= ActionButtonCount; i++)
        {
            RectTransform button = CreateRect($"ActionButton{i}", buttons);
            Image image = AddImage(button, uiSprite, ButtonColor, true);
            image.type = Image.Type.Sliced;
            Undo.AddComponent<Button>(button.gameObject).targetGraphic = image;

            RectTransform label = CreateRect("Label", button);
            Stretch(label);
            AddText(label, "", 26f, TextAlignmentOptions.Center, FontStyles.Bold, 2f, TextColor);
        }
    }

    private static void BuildQuickSlotPanel(RectTransform panel)
    {
        RectTransform header = CreateRect("Header", panel);
        SetOffsets(header, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(25f, -105f), new Vector2(-25f, -35f));

        const float titleWidth = 360f;
        RectTransform title = CreateRect("Title", header);
        SetAnchored(title, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(titleWidth, 70f));
        AddText(title, "QUICK SLOT", 44f, TextAlignmentOptions.Center, FontStyles.Bold, 6f, TextColor);

        RectTransform leftLine = CreateRect("LineLeft", header);
        SetOffsets(leftLine, new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -3f), new Vector2(-titleWidth / 2f, 3f));
        AddImage(leftLine, divider, LineColor, false);

        RectTransform rightLine = CreateRect("LineRight", header);
        SetOffsets(rightLine, new Vector2(0.5f, 0.5f), new Vector2(1f, 0.5f), new Vector2(titleWidth / 2f, -3f), new Vector2(0f, 3f));
        rightLine.localScale = new Vector3(-1f, 1f, 1f);
        AddImage(rightLine, divider, LineColor, false);

        RectTransform slots = CreateRect("Slots", panel);
        SetOffsets(slots, Vector2.zero, Vector2.one, new Vector2(60f, 45f), new Vector2(-60f, -115f));
        HorizontalLayoutGroup row = Undo.AddComponent<HorizontalLayoutGroup>(slots.gameObject);
        row.childAlignment = TextAnchor.MiddleCenter;
        row.childControlWidth = false;
        row.childControlHeight = false;
        row.childForceExpandWidth = true;
        row.childForceExpandHeight = false;

        float slotSize = Mathf.Min(slots.rect.height - 20f, 160f);

        for (int i = 1; i <= QuickSlotCount; i++)
        {
            RectTransform slot = CreateRect($"QuickSlot{i}", slots);
            slot.sizeDelta = new Vector2(slotSize, slotSize);
            AddImage(slot, null, SlotColor, true);
            AddOutline(slot, SlotOutlineColor);

            RectTransform badge = CreateRect("Number", slot);
            SetAnchored(badge, new Vector2(0.5f, 0f), Vector2.zero, new Vector2(34f, 34f));
            AddImage(badge, null, BadgeColor, false);
            AddOutline(badge, LineColor);

            RectTransform digit = CreateRect("Text", badge);
            Stretch(digit);
            AddText(digit, i.ToString(), 24f, TextAlignmentOptions.Center, FontStyles.Bold, 0f, TextColor);
        }
    }

    private static Rect BoundsInParent(RectTransform rt)
    {
        Rect r = rt.rect;
        Vector2 position = rt.localPosition;
        return new Rect(position + r.position, r.size);
    }

    private static Transform FindDeep(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
                return child;

            Transform found = FindDeep(child, name);
            if (found != null)
                return found;
        }

        return null;
    }

    private static void DestroyChild(Transform parent, string name)
    {
        Transform existing = parent.Find(name);
        if (existing != null)
            Undo.DestroyObjectImmediate(existing.gameObject);
    }

    private static RectTransform CreateRect(string name, Transform parent)
    {
        GameObject go = new(name, typeof(RectTransform));
        go.layer = parent.gameObject.layer;
        Undo.RegisterCreatedObjectUndo(go, UndoName);

        RectTransform rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        return rt;
    }

    private static void SetAnchored(RectTransform rt, Vector2 anchor, Vector2 position, Vector2 size)
    {
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = position;
        rt.sizeDelta = size;
    }

    private static void SetOffsets(RectTransform rt, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = offsetMin;
        rt.offsetMax = offsetMax;
    }

    private static void Stretch(RectTransform rt) => SetOffsets(rt, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

    private static Image AddImage(RectTransform rt, Sprite sprite, Color color, bool raycastTarget)
    {
        Image image = Undo.AddComponent<Image>(rt.gameObject);
        image.sprite = sprite;
        image.color = color;
        image.raycastTarget = raycastTarget;
        return image;
    }

    private static void AddOutline(RectTransform rt, Color color)
    {
        Outline outline = Undo.AddComponent<Outline>(rt.gameObject);
        outline.effectColor = color;
        outline.effectDistance = new Vector2(2f, -2f);
    }

    private static TextMeshProUGUI AddText(RectTransform rt, string text, float size, TextAlignmentOptions alignment,
        FontStyles style, float characterSpacing, Color color)
    {
        TextMeshProUGUI tmp = Undo.AddComponent<TextMeshProUGUI>(rt.gameObject);
        if (TMP_Settings.defaultFontAsset != null)
            tmp.font = TMP_Settings.defaultFontAsset;

        tmp.text = text;
        tmp.fontSize = size;
        tmp.alignment = alignment;
        tmp.fontStyle = style;
        tmp.characterSpacing = characterSpacing;
        tmp.color = color;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        tmp.raycastTarget = false;
        return tmp;
    }
}
#endif
