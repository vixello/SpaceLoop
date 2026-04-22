using UnityEngine;

namespace Services.Google 
{
    using System;
    using System.IO;
    using GoogleMobileAds.Api;
    //using UnityEditor.PackageManager;
    using UnityEngine;
    using UnityEngine.UI;

    public class AdManager : MonoBehaviour
    {

        // Singleton instence
        public static AdManager Instance;

        // Ad Unit IDs (use test IDs for development)
#if UNITY_ANDROID
        private string bannerAdUnitId = "ca-app-pub-3940256099942544/6300978111";
        private string interestitialAdUnitId = "ca-app-pub-3940256099942544/1033173712";
        private string rewardedAdUnitId = "ca-app-pub-3940256099942544/5224354917";
        private string nativeAdUnitId = "ca-app-pub-3940256099942544/2247696110";
#elif UNITY_IPHONE
    private string bannerAdUnitId = "ca-app-pub-3940256099942544/2934735716";
    private string interestitialAdUnitId = "ca-app-pub-3940256099942544/4411468910";
    private string rewardedAdUnitId = "ca-app-pub-3940256099942544/1712485313";
    private string nativeAdUnitId = "ca-app-pub-3940256099942544/3986624511";
#else
    private string bannerAdUnitId = "unused";
    private string interestitialAdUnitId = "unused";
    private string rewardedAdUnitId = "unused";
    private string nativeAdUnitId = "unused";
    
#endif


        private BannerView bannerView;
        private InterstitialAd interstitialAd;
        private RewardedAd rewardedAd;
        private NativeOverlayAd nativeOverlayAd;


        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }


        }


        void Start()
        {
            // Initialize the google mobile ads SDK
            MobileAds.Initialize(initStatus =>
            {
                Debug.Log("Admob SDK Initilized");
                //Load ads after initialization
                LoadBannerAd();
                LoadInterstitialAd();
                LoadRewardedAd();

            });
        }

        private void LoadBannerAd()
        {
            // Create a banner view at the bottom of the screen
            bannerView = new BannerView(bannerAdUnitId, AdSize.Banner, AdPosition.Top);

            // Create an empty ad request
            AdRequest request = new AdRequest();

            // Register event handlers for the banner ad
            bannerView.OnBannerAdLoaded += () =>
            {
                Debug.Log("Banner ad loaded.");
            };

            bannerView.OnBannerAdLoadFailed += (LoadAdError error) =>
            {
                Debug.Log("Banner ad failed to load: " + error.GetMessage());
            };

            // Load the banner ad
            bannerView.LoadAd(request);


        }

        private void LoadInterstitialAd()
        {

            // Clean up any existing interstitial ad
            if (interstitialAd != null)
            {
                interstitialAd.Destroy();
                interstitialAd = null;
            }

            // Load a new interstitial ad
            InterstitialAd.Load(interestitialAdUnitId, new AdRequest(), (InterstitialAd ad, LoadAdError error) => {

                if (error != null || ad == null)
                {
                    Debug.LogError("Interstitial ad failed to load: " + error?.GetMessage());
                    return;
                };

                interstitialAd = ad;
                Debug.Log("Interstitial ad loaded");

                // Register ad events
                interstitialAd.OnAdFullScreenContentClosed += () =>
                {
                    Debug.Log("Interstitial ad closed");
                    LoadInterstitialAd(); // Preload the next ad
                };

                interstitialAd.OnAdFullScreenContentFailed += (error) =>
                {
                    Debug.Log("Interstitial ad failed to show: " + error.GetMessage());
                };
            });

        }


        public void ShowInterstialAd()
        {
            if (interstitialAd != null && interstitialAd.CanShowAd())
            {
                interstitialAd.Show();
            }
            else
            {
                Debug.Log("Interstitial ad not ready");
            }
        }

        private void LoadRewardedAd()
        {

            // Clean up any existing rewarded ad
            if (rewardedAd != null)
            {
                rewardedAd.Destroy();
                rewardedAd = null;

            }

            // Load a new rewarded ad
            RewardedAd.Load(rewardedAdUnitId, new AdRequest(), (RewardedAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
                    Debug.LogError("Rewarded ad failed to load:" + error?.GetMessage());
                    return;
                }

                rewardedAd = ad;
                Debug.Log("Rewarded ad loaded.");

                // Register ad events
                rewardedAd.OnAdFullScreenContentClosed += () =>
                {
                    Debug.LogError("Rewarded ad closed.");
                    LoadRewardedAd();
                };
                rewardedAd.OnAdFullScreenContentFailed += (error) =>
                {
                    Debug.LogError("Rewarded ad failed to show:" + error.GetMessage());
                };
            });


        }


        public void ShowRewardedAd()
        {
            if (rewardedAd != null && rewardedAd.CanShowAd())
            {
                rewardedAd.Show((Reward reward) =>
                {
                    Debug.Log($"User earned reward: {reward.Amount} {reward.Type}");

                });
            }
            else
            {
                Debug.Log("Rewarded ad not ready");
            }
        }





        // Update is called once per frame
        void Update()
        {

        }
    }
}
