using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Composition root for the HUD. It only holds the Inspector references and hands them to one
/// presenter per panel; each presenter owns exactly one piece of UI behaviour.
/// (Field names are unchanged so the references on the Player prefab survive.)
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("Read UI")]
    [SerializeField] private GameObject readUIPanel;
    [SerializeField] private TMP_Text readUIText;
    [SerializeField] private Button closeButton;

    [Header("Tips")]
    [SerializeField] private TMP_Text tipsText;

    [Header("Password Input")]
    [SerializeField] private GameObject passwordInputPanel;

    [SerializeField] private GameObject lightsOutEffect;

    [Header("Enemy State")]
    [SerializeField] private Transform enemyStateUIParent;
    [SerializeField] private GameObject enemyStateUIPrefab;
    [SerializeField] private Sprite seeSprite;
    [SerializeField] private Sprite hearSprite;
    [SerializeField] private Sprite walkSprite;
    [SerializeField] private Sprite chaseSprite;
    [SerializeField] private Sprite stunnedSprite;

    [Header("Game Over")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Button restartButton;

    [Header("Tutorial")]
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private TMP_Text tutorialHeader;
    [SerializeField] private TMP_Text tutorialBody;
    [SerializeField] private Image tutorialImg;

    [SerializeField] private Button nextButton;

    private IUIPresenter[] presenters;

    void Awake()
    {
        presenters = new IUIPresenter[]
        {
            new ReadPanelPresenter(readUIPanel, readUIText, closeButton),
            new TipPresenter(tipsText),
            new PasswordPanelPresenter(passwordInputPanel),
            new LightsOutPresenter(lightsOutEffect),
            new EnemyIndicatorPresenter(enemyStateUIParent, enemyStateUIPrefab, new EnemyIndicatorPresenter.Sprites
            {
                See = seeSprite,
                Hear = hearSprite,
                Walk = walkSprite,
                Chase = chaseSprite,
                Stunned = stunnedSprite,
            }),
            new GameOverPresenter(gameOverPanel, restartButton),
            new TutorialPresenter(tutorialPanel, tutorialHeader, tutorialBody, nextButton, tutorialImg)
        };
    }

    void OnEnable()
    {
        foreach (IUIPresenter presenter in presenters)
            presenter.Bind();
    }

    void OnDisable()
    {
        foreach (IUIPresenter presenter in presenters)
            presenter.Unbind();
    }
}
