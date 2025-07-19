﻿using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using BepInEx;
using HarmonyLib;

namespace BeanBattlesMapMaker
{
    [BepInPlugin("flarfo.beanbattles.mapmaker", "Bean Battles Map Maker", "0.0.1")]
    public class MapMakerPlugin : BaseUnityPlugin
    {
        private readonly Harmony harmony = new Harmony("Bean Battles Map Maker");

        public bool showGUI = true;

        public static string[] mapDirectories;

        public static Dictionary<string, string> mapsList = new Dictionary<string, string>();

        GameObject selectorUI;
        GameObject mapButton;

        GameObject mapsScreen;
        GameObject optionsScreen;

        GameObject selectedButton;

        public static Toggle doGrace;
        public static Slider graceValue;

        public static Dictionary<GameObject, Tuple<string, int>> mapsDict = new Dictionary<GameObject, Tuple<string,int>>();

        public static int graceTime = 0;
        public static int windowCount = 0;
        public static bool open = true;

        public static RectTransform winSize;
        public static RectTransform winButton;

        public void Awake()
        {
            Logger.LogInfo("Running Bean Battles Map Maker!");

            harmony.PatchAll();

            mapDirectories = Directory.GetDirectories(Path.Combine(Directory.GetParent(Application.dataPath).ToString(), "Maps"));

            InstantiateUI();
        }

        public void InstantiateUI()
        {
            AssetBundle uiBundle = GetAssetBundleFromResource("mapselector");

            selectorUI = Instantiate(uiBundle.LoadAsset<GameObject>("MapSelectorRoot.prefab"));
            selectorUI.transform.SetParent(GameObject.Find("MenuCanvas").transform, false);

            mapButton = uiBundle.LoadAsset<GameObject>("MapButton.prefab");

            uiBundle.Unload(false);

            GameObject.Find("Header").AddComponent<Draggable>().target = GameObject.Find("MapSelector").transform;

            mapsScreen = GameObject.Find("MapScreen");

            optionsScreen = GameObject.Find("OptionScreen");
            doGrace = GameObject.Find("GracePeriodButton").GetComponent<Toggle>();
            graceValue = GameObject.Find("GracePeriodSlider").GetComponent<Slider>();

            winSize = GameObject.Find("Holder").GetComponent<RectTransform>();
            winButton = GameObject.Find("TWindowButton").GetComponent<RectTransform>();


            graceValue.onValueChanged.AddListener(delegate {
                graceTime = (int)(graceValue.value*100);
                graceValue.gameObject.GetComponentInChildren<Text>().text = "Grace Time: " + graceTime;
            });

            optionsScreen.SetActive(false);

            Button mapsButton = GameObject.Find("MapsButton").GetComponent<Button>();
            mapsButton.onClick.AddListener(delegate ()
            {
                optionsScreen.SetActive(false);
                mapsScreen.SetActive(true);
            });

            Button settingsButton = GameObject.Find("SettingsButton").GetComponent<Button>();
            settingsButton.onClick.AddListener(delegate ()
            {
                mapsScreen.SetActive(false);
                optionsScreen.SetActive(true);
            });

            Button minimizeButton = GameObject.Find("TWindowButton").GetComponent<Button>();
            minimizeButton.onClick.AddListener(delegate ()
            {
                if (open)
                {
                    //Make it squash
                    winSize.localScale = new Vector3(1, 0, 1);
                    winButton.localRotation = Quaternion.Euler(0, 0, 180);
                    open = false;
                }
                else if (!open)
                {
                    // Put it back to normal
                    winSize.localScale = new Vector3(1, 1, 1);
                    winButton.localRotation = Quaternion.Euler(0, 0, 0);
                    open = true;
                }
            });

            for (int i = 0; i < mapDirectories.Length; i++)
            {
                GameObject newMapButton = Instantiate(mapButton);

                RawImage mapImage = newMapButton.GetComponent<RawImage>();

                byte[] fileData = File.ReadAllBytes(Directory.GetFiles(Path.Combine(mapDirectories[i], "icon"))[0]);

                Texture2D tex = new Texture2D(2, 2);
                tex.LoadImage(fileData);

                mapImage.texture = tex;

                GameObject selectorGrid = GameObject.Find("SelectorGrid");
                newMapButton.transform.SetParent(selectorGrid.transform, false);

                string buttonText = Path.GetFileName(mapDirectories[i]);

                int mapButtonNumber = 0;

                if (int.TryParse(buttonText[buttonText.Length - 1].ToString(), out int result))
                {
                    mapButtonNumber = result;
                }
                else
                {
                    buttonText += " 0";
                }

                mapsList.Add(buttonText, mapDirectories[i]);

                mapsDict.Add(newMapButton, new Tuple<string, int>(mapDirectories[i], mapButtonNumber));

                newMapButton.transform.GetChild(0).gameObject.GetComponent<Text>().text = buttonText.Remove(buttonText.Length - 2);

                newMapButton.GetComponent<Button>().onClick.AddListener(delegate ()
                {
                    if (!selectedButton)
                    {
                        selectedButton = newMapButton;
                        selectedButton.transform.GetChild(1).gameObject.SetActive(true);
                        SetupMap.selectedMap = mapsDict[newMapButton].Item1; 
                        SetupMap.selectedMapName = buttonText;

                        GameObject.Find("NetworkManager").GetComponent<CustomNetworkManager>().ChangeMap(mapsDict[selectedButton].Item2);
                    }
                    else if (selectedButton == newMapButton)
                    {
                        selectedButton.transform.GetChild(1).gameObject.SetActive(false);
                        selectedButton = null;
                        SetupMap.selectedMap = null;
                        SetupMap.selectedMapName = null;
                    }
                    else
                    {
                        selectedButton.transform.GetChild(1).gameObject.SetActive(false);
                        selectedButton = newMapButton;
                        selectedButton.transform.GetChild(1).gameObject.SetActive(true);
                        SetupMap.selectedMap = mapsDict[newMapButton].Item1;
                        SetupMap.selectedMapName = buttonText;

                        GameObject.Find("NetworkManager").GetComponent<CustomNetworkManager>().ChangeMap(mapsDict[selectedButton].Item2);
                    }
                    //button press method stuff, make button selected, make cur map selected this button, make selected button this button, 
                    //reference selected button when changing checkmark/highlight. change current selected button to not have checkmark,
                    //then make this one new selected button
                });

                if (i % 4 == 1 && i > 8)
                {
                    Vector2 layoutSize = selectorGrid.GetComponent<RectTransform>().sizeDelta;
                    selectorGrid.GetComponent<RectTransform>().sizeDelta = new Vector2(layoutSize.x, layoutSize.y + 200);
                }
            }

            mapsScreen.GetComponent<ScrollRect>().verticalNormalizedPosition = 1f;
        }

        public static AssetBundle GetAssetBundleFromResource(string fileName)
        {
            var execAssembly = Assembly.GetExecutingAssembly();

            var resourceName = execAssembly.GetManifestResourceNames().Single(str => str.EndsWith(fileName));

            Debug.Log($"Resource Name: {resourceName}");

            using (var stream = execAssembly.GetManifestResourceStream(resourceName))
            {
                return AssetBundle.LoadFromStream(stream);
            }
        }
    }
}
