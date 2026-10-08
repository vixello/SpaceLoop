using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MissionCenterController : MonoBehaviour
{
    [Header("UI Document Reference")]
    [SerializeField] private UIDocument uiDocument;

    // --- Tab & Page Management ---
    private Button tabDaily;
    private Button tabWeekly;
    private Button tabEvents;
    private Button tabBounties;

    private VisualElement pageDaily;
    private VisualElement pageWeekly;
    private VisualElement pageEvents;
    private VisualElement pageBounties;

    private readonly List<Button> tabs = new();
    private readonly List<VisualElement> pages = new();

    // --- Dynamic Elements ---
    private ScrollView listDaily;
    private Label freeRerollNote;
    private int freeRerollsRemaining = 1;

    private void OnEnable()
    {
        if (uiDocument == null)
            uiDocument = GetComponent<UIDocument>();

        var root = uiDocument.rootVisualElement;

        // Query Tabs
        tabDaily = root.Q<Button>("tab-daily");
        tabWeekly = root.Q<Button>("tab-weekly");
        tabEvents = root.Q<Button>("tab-events");
        tabBounties = root.Q<Button>("tab-bounties");

        // Query Pages
        pageDaily = root.Q<VisualElement>("page-daily");
        pageWeekly = root.Q<VisualElement>("page-weekly");
        pageEvents = root.Q<VisualElement>("page-events");
        pageBounties = root.Q<VisualElement>("page-bounties");

        // Register Tabs and Pages into lists
        RegisterTab(tabDaily, pageDaily);
        RegisterTab(tabWeekly, pageWeekly);
        RegisterTab(tabEvents, pageEvents);
        RegisterTab(tabBounties, pageBounties);

        // Bind Daily Mission List & Rerolls
        listDaily = root.Q<ScrollView>("list-daily");
        freeRerollNote = root.Q<Label>(className: "mnote");
        BindDailyMissions();

        // Bind Activity Crates
        BindActivityCrates(root);

        // Bind Bounty Cards
        BindBountyCards(root);

        // Bind Event CTA
        var btnPremium = root.Q<Button>("btn-premium");
        if (btnPremium != null)
            btnPremium.clicked += () => Debug.Log("[MissionCenter] Unlock Premium Event Track Clicked");
    }

    private void OnDisable()
    {
        foreach (var tab in tabs)
        {
            tab.UnregisterCallback<ClickEvent>(OnTabClicked);
        }
    }

    // ---------------------------------------------------------------------
    // TAB NAVIGATION
    // ---------------------------------------------------------------------
    private void RegisterTab(Button tabButton, VisualElement pageElement)
    {
        if (tabButton == null || pageElement == null) return;

        tabs.Add(tabButton);
        pages.Add(pageElement);

        tabButton.RegisterCallback<ClickEvent>(OnTabClicked);
    }

    private void OnTabClicked(ClickEvent evt)
    {
        if (evt.currentTarget is not Button clickedTab) return;

        int index = tabs.IndexOf(clickedTab);
        if (index < 0) return;

        for (int i = 0; i < tabs.Count; i++)
        {
            bool isActive = (i == index);

            // Toggle CSS classes for tab active highlight
            if (isActive)
                tabs[i].AddToClassList("tab--active");
            else
                tabs[i].RemoveFromClassList("tab--active");

            // Toggle visibility for page content
            if (isActive)
                pages[i].RemoveFromClassList("page--hidden");
            else
                pages[i].AddToClassList("page--hidden");
        }
    }

    // ---------------------------------------------------------------------
    // DAILY MISSIONS & REROLL SYSTEM
    // ---------------------------------------------------------------------
    private void BindDailyMissions()
    {
        if (listDaily == null) return;

        // Query all mission rows in the list
        var missionRows = listDaily.Query<VisualElement>(className: "mrow").ToList();

        foreach (var row in missionRows)
        {
            // Bind Claim Buttons
            var claimBtn = row.Q<Button>(className: "mrow__claim");
            if (claimBtn != null)
            {
                claimBtn.clicked += () => ClaimMissionReward(row);
            }

            // Bind Reroll Buttons
            var rerollBtn = row.Q<Button>(className: "mrow__reroll");
            if (rerollBtn != null)
            {
                rerollBtn.clicked += () => RerollMissionRow(row);
            }
        }
    }

    private void ClaimMissionReward(VisualElement row)
    {
        // Transition row style from claimable to done state
        row.RemoveFromClassList("mrow--claim");
        row.RemoveFromClassList("mrow--progress");
        row.AddToClassList("mrow--done");

        var claimBtn = row.Q<Button>(className: "mrow__claim");
        if (claimBtn != null) claimBtn.style.display = DisplayStyle.None;

        var countLabel = row.Q<Label>(className: "mrow__count");
        if (countLabel != null) countLabel.text = "COMPLETE";

        Debug.Log("[MissionCenter] Mission Reward Claimed!");
    }

    private void RerollMissionRow(VisualElement row)
    {
        if (freeRerollsRemaining <= 0)
        {
            Debug.Log("[MissionCenter] No free rerolls left!");
            return;
        }

        freeRerollsRemaining--;

        if (freeRerollNote != null)
            freeRerollNote.text = $"{freeRerollsRemaining} FREE REROLL LEFT";

        // Example dynamic text update on reroll
        var titleLabel = row.Q<Label>(className: "mrow__title");
        var descLabel = row.Q<Label>(className: "mrow__desc");
        var countLabel = row.Q<Label>(className: "mrow__count");

        if (titleLabel != null) titleLabel.text = "SPEED DEMON";
        if (descLabel != null) descLabel.text = "REACH 300 KM/H IN 2 RACES";
        if (countLabel != null) countLabel.text = "0 / 2";

        var pbarFill = row.Q<VisualElement>(className: "pbar__fill");
        if (pbarFill != null) pbarFill.style.width = Length.Percent(0);

        // Hide reroll button once used
        var rerollBtn = row.Q<Button>(className: "mrow__reroll");
        if (rerollBtn != null && freeRerollsRemaining <= 0)
        {
            rerollBtn.SetEnabled(false);
        }

        Debug.Log("[MissionCenter] Mission Rerolled!");
    }

    // ---------------------------------------------------------------------
    // ACTIVITY CHESTS
    // ---------------------------------------------------------------------
    private void BindActivityCrates(VisualElement root)
    {
        var crates = root.Query<Button>(className: "crate").ToList();

        foreach (var crate in crates)
        {
            crate.clicked += () =>
            {
                if (crate.ClassListContains("crate--ready"))
                {
                    crate.RemoveFromClassList("crate--ready");
                    crate.AddToClassList("crate--claimed");

                    var label = crate.Q<Label>(className: "crate__label");
                    if (label != null) label.text = "CLAIMED";

                    Debug.Log("[MissionCenter] Activity Chest Claimed!");
                }
            };
        }
    }

    // ---------------------------------------------------------------------
    // BOUNTIES BOARD
    // ---------------------------------------------------------------------
    private void BindBountyCards(VisualElement root)
    {
        var bountyCards = root.Query<VisualElement>(className: "wanted").ToList();

        foreach (var card in bountyCards)
        {
            var ctaBtn = card.Q<Button>(className: "wanted__cta");
            if (ctaBtn == null) continue;

            ctaBtn.clicked += () =>
            {
                if (card.ClassListContains("wanted--open"))
                {
                    card.RemoveFromClassList("wanted--open");
                    card.AddToClassList("wanted--active");

                    var label = ctaBtn.Q<Label>(className: "btn-cta__label");
                    if (label != null) label.text = "TRACKING";

                    Debug.Log("[MissionCenter] Bounty Accepted!");
                }
            };
        }
    }
}