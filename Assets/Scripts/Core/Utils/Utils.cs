using Assets.Scripts.Data.Titles;
using Data;
using System;
using UnityEngine;
using Assets.Scripts.Contracts;

namespace Assets.Scripts.Core
{
    public struct StringConstants
    {
        public const string SaveFileName = "save";
        public const string LoggingIn = "Logging in...";
    }

    public struct ErrorMessages
    {
        public const string NoInternet = "Internet not reachable.";
        public const string LoginFailed = "Login failed.";
        public const string TransferFailed = "Transfer failed.";
    }

    public struct SceneNames
    {
        public const string LoginScene = "Login";
        public const string MenuScene = "MainMenu";
        public const string Bootstrap = "Bootstrap";
    }

    public struct Colors
    {
        public const string AchievementUnlockedColor = "#7D359B";
        public const string AchievementLockedColor = "#310055";
        public const string AchievementUnlockDate = "#FF8EF2";
        public const string AchievementLockedDate = "#846F82";
    }

    public struct PinnedAchievContext
    {
        public string Id;
        public Texture2D Texture;
        public int PinOrder;
    }

    public struct AchievementDetailContext
    {
        public bool IsUnlocked;
        public bool IsRewardClaimed;
        public DateTimeOffset? UnlockDate;
        public string Title;
        public string Detail;
        public int Progress;
        public int MaxProgress;
        public Texture2D Texture;
    }
    public struct UIAchievementContext
    {
        public string Id;
        public string Title;
        public bool IsUnlocked;
        public bool IsRewardClaimed;

        public int Progress;
        public int MaxProgress;

        public bool IsPinned;
        public int PinOrder;

        public Reward Reward;
        public Texture2D Icon;
    }

    public struct LevelProgress
    {
        public int Level;
        public int CurrentXp;
        public int RequiredXp;
        public float Normalized;
    }

    public class RuntimeTitle
    {
        public TitleConfig Config;
        public TitleSaveData Save;
    }

    public enum UsernameErrorType
    {
        Empty,
        TooShort,
        TooLong,
        ContainsSpaces,
        ReservedWord,
        BrandProtected,
        Offensive,
        Government,
        Financial,
        SymbolOnly,
        StartsWithSymbol,
        ForbiddenGeneric,
        Allowed
    }
    public class UsernameValidationResult
    {
        public UsernameErrorType ErrorType;
        public string Message;
    }

    public static class Utils
    {
        public static uint ExpectedAchievementsCount = 0;

        public static Vector3 GenerateNewStartPosition(float offsetX, float offsetY, float depth, float startYpoint, float minDistanceInBetween)
        {
            return Vector3.zero;
        }
    }
}
