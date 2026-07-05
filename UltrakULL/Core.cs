using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using TMPro;
using UltrakULL.Harmony_Patches;
using UltrakULL.json;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UltrakULL.CommonFunctions;

namespace UltrakULL
{
    public static class Core
    {
        public static Font VcrFont;
        public static GameObject ultrakullLogo = null;
        
        public static bool GlobalFontReady;
        public static bool TMPFontReady;
        
        public static Font GlobalFont;
        public static Font MuseumFont;
        public static TMP_FontAsset GlobalFontTMP;
        public static TMP_FontAsset MuseumFontTMP;
        public static TMP_FontAsset CJKFontTMP;
        public static TMP_FontAsset JaFontTMP;
        public static TMP_FontAsset ArabicFontTMP;
        public static TMP_FontAsset HebrewFontTMP;
        public static Material GlobalFontTMPOverlayMat;
        public static Material CJKFontTMPOverlayMat;
        public static Material jaFontTMPOverlayMat;
        public static Sprite[] CustomRankImages;

        // Custom fonts loaded from external files
        public static TMP_FontAsset CustomMainFontTMP;
        public static TMP_FontAsset CustomMuseumFontTMP;
        public static TMP_FontAsset CustomTerminalFontTMP;
        public static TMP_FontAsset CustomSecretTerminalFontTMP;
        
        // Materials for custom TMP fonts (overlay)
        public static Material CustomMainFontTMPOverlayMat;
        public static Material CustomMuseumFontTMPOverlayMat;
        public static Material CustomTerminalFontTMPOverlayMat;
        public static Material CustomSecretTerminalFontTMPOverlayMat;

        private static bool ultrakullDropdownExpanded = false;

        public static Sprite ArabicUltrakillLogo;

		public static bool wasLanguageReset = false;
        
        //Encapsulation function to patch all of the front end.
        public static void PatchFrontEnd(GameObject frontEnd)
        {
            MainMenu.Patch(frontEnd);
            Options options = new Options(ref frontEnd);
        }

        //Patches all text strings in the pause menu.
        public static void PatchPauseMenu(ref GameObject canvasObj)
        {
            try
            {
                GameObject pauseMenu = GetGameObjectChild(canvasObj, "PauseMenu");

                //Title
                TextMeshProUGUI pauseText = GetTextMeshProUGUI(GetGameObjectChild(pauseMenu, "Text"));
                pauseText.text = "-- " + LanguageManager.CurrentLanguage.pauseMenu.pause_title + " --";

                //Resume
                TextMeshProUGUI continueText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(pauseMenu, "Resume"), "Text"));
                continueText.text = LanguageManager.CurrentLanguage.pauseMenu.pause_resume;

                //Checkpoint
                TextMeshProUGUI checkpointText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(pauseMenu, "Restart Checkpoint"), "Text"));
                checkpointText.text = LanguageManager.CurrentLanguage.pauseMenu.pause_respawn;
                //SKIP button 
                if (GetCurrentSceneName().Contains("Intermission"))
                {
                    TextMeshProUGUI skipText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(pauseMenu, "Restart Checkpoint (1)"), "Text"));
                    skipText.text = LanguageManager.CurrentLanguage.pauseMenu.pause_skip;
                }
                //Restart mission
                TextMeshProUGUI restartText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(pauseMenu, "Restart Mission"), "Text"));
                restartText.text = LanguageManager.CurrentLanguage.pauseMenu.pause_restart;

                //Options
                TextMeshProUGUI optionsText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(pauseMenu, "Options"), "Text"));
                optionsText.text = LanguageManager.CurrentLanguage.pauseMenu.pause_options;

                //Quit
                TextMeshProUGUI quitText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(pauseMenu, "Quit Mission"), "Text"));
                quitText.text = LanguageManager.CurrentLanguage.pauseMenu.pause_quit;

                //Quit+Restart windows
                GameObject pauseDialogs = GetGameObjectChild(canvasObj, "PauseMenuDialogs");

                //Quit
                GameObject quitDialog = GetGameObjectChild(GetGameObjectChild(pauseDialogs, "Quit Confirm"), "Panel");
                TextMeshProUGUI quitDialogText = GetTextMeshProUGUI(GetGameObjectChild(quitDialog, "Text (2)"));
                quitDialogText.text = LanguageManager.CurrentLanguage.pauseMenu.pause_quitConfirm;

                TextMeshProUGUI quitDialogTooltip = GetTextMeshProUGUI(GetGameObjectChild(quitDialog, "Text (1)"));
                quitDialogTooltip.text = LanguageManager.CurrentLanguage.pauseMenu.pause_disableWindow;

                TextMeshProUGUI quitDialogYes = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(quitDialog, "Confirm"), "Text"));
                quitDialogYes.text = LanguageManager.CurrentLanguage.pauseMenu.pause_quitConfirmYes;

                TextMeshProUGUI quitDialogNo = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(quitDialog, "Cancel"), "Text"));
                quitDialogNo.text = LanguageManager.CurrentLanguage.pauseMenu.pause_quitConfirmNo;

                //Restart
                GameObject restartDialog = GetGameObjectChild(GetGameObjectChild(pauseDialogs, "Restart Confirm"), "Panel");

                TextMeshProUGUI restartDialogText = GetTextMeshProUGUI(GetGameObjectChild(restartDialog, "Text"));
                restartDialogText.text = LanguageManager.CurrentLanguage.pauseMenu.pause_restartConfirm;

                TextMeshProUGUI restartDialogTooltip = GetTextMeshProUGUI(GetGameObjectChild(restartDialog, "Text (1)"));
                restartDialogTooltip.text = LanguageManager.CurrentLanguage.pauseMenu.pause_disableWindow;

                TextMeshProUGUI restartDialogYes = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(restartDialog, "Confirm"), "Text"));
                restartDialogYes.text = LanguageManager.CurrentLanguage.pauseMenu.pause_restartConfirmYes;

                TextMeshProUGUI restartDialogNo = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(restartDialog, "Cancel"), "Text"));
                restartDialogNo.text = LanguageManager.CurrentLanguage.pauseMenu.pause_restartConfirmNo;
            }
            catch (Exception e)
            {
                Logging.Error("Failed to patch pause menu.");
                Logging.Error(e.ToString());
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="fontPath">Full path to the font file (TTF, OTF, etc.)</param>
        /// <param name="samplingPointSize">Sampling point size for the font atlas (default 90)</param>
        /// <param name="padding">Padding between glyphs in the atlas (default 9)</param>
        /// <param name="renderMode">Glyph render mode (default SDFAA)</param>
        /// <param name="atlasWidth">Width of the atlas texture (default 1024)</param>
        /// <param name="atlasHeight">Height of the atlas texture (default 1024)</param>
        /// <param name="atlasPopulationMode">Atlas population mode (default Dynamic)</param>
        /// <returns>TMP_FontAsset if successful, null otherwise</returns>

        /// <summary>
        /// Reloads custom fonts based on the current language.
        /// Call this after changing language.
        /// </summary>

        private static readonly HttpClient KoreanClient = new HttpClient()
        {
            Timeout = TimeSpan.FromSeconds(5)
        };

        public static async Task InitializeKoreanLanguage()
        {
            string url =
                "https://raw.githubusercontent.com/zer0pacity/ULL-korean/master/ko-kr.json";

            string localPath = Path.Combine(BepInEx.Paths.ConfigPath, "ultrakull", "ko-kr.json");

            Directory.CreateDirectory(Path.GetDirectoryName(localPath));

            try
            {
                string remoteJson = await KoreanClient.GetStringAsync(url);

                if (!File.Exists(localPath) ||
                    File.ReadAllText(localPath) != remoteJson)
                {
                    File.WriteAllText(localPath, remoteJson);

                    Logging.Info("Korean language file updated.");
                }

                JsonFormat korean =
                    JsonConvert.DeserializeObject<JsonFormat>(remoteJson);

                LanguageManager.allLanguages["ko-KR"] = korean;

                Logging.Info("Korean language initialized.");
            }
            catch (Exception e)
            {
                Logging.Error("Failed to initialize Korean language.");
                Logging.Error(e.ToString());
            }
        }

        public static void LoadFonts()
        {
            Logging.Message("Loading font resource bundle...");
            //Will load from the same directory that the dll is in.
            AssetBundle fontBundle = AssetBundle.LoadFromFile(Path.Combine(MainPatch.ModFolder,"ullfont.resource"));

			if (fontBundle == null)
            {
                Logging.Error("FAILED TO LOAD");
            }
            else
            {
                Logging.Message("Font bundle loaded.");
                Logging.Message("Loading fonts from bundle...");
                
                Font font1 = fontBundle.LoadAsset<Font>("VCR_OSD_MONO_EXTENDED");
                Font font2 = fontBundle.LoadAsset<Font>("EBGaramond-Regular");
                TMP_FontAsset font1TMP = fontBundle.LoadAsset<TMP_FontAsset>("VCR_OSD_MONO_EXTENDED_TMP");
                TMP_FontAsset font2TMP = fontBundle.LoadAsset<TMP_FontAsset>("EBGaramond-Regular_TMP");
                Material font1TMPTopMat = fontBundle.LoadAsset<Material>("VCR_OSD_MONO_EXTENDED_TMP_Overlay_Material");
                
                TMP_FontAsset cjkFontTMP = fontBundle.LoadAsset<TMP_FontAsset>("NotoSans-CJK_TMP");
                TMP_FontAsset jafontTMP = fontBundle.LoadAsset<TMP_FontAsset>("JF-Dot-jiskan16s-2000_TMP");
                Material cjkFontTMPTopMat = fontBundle.LoadAsset<Material>("NotoSans-CJK_TMP_Overlay_Material");
                Material jaFontTMPTopMat = fontBundle.LoadAsset<Material>("JF-Dot-jiskan16s-2000_TMP_Overlay_Material");
                if (font1 && font2)
                {
                    Logging.Warn("Normal fonts loaded.");
                    GlobalFont = font1;
                    MuseumFont = font2;
                    GlobalFontReady = true;
                }
                else
                {
                    Logging.Error("FAILED TO LOAD NORMAL FONTS");
                    GlobalFontReady = false;
                }
                if(font1TMP && font2TMP && cjkFontTMP && jafontTMP && font1TMPTopMat && cjkFontTMPTopMat && jaFontTMPTopMat)
                {
                    Logging.Warn("Normal TMP fonts loaded.");
                    GlobalFontTMP = font1TMP;
                    MuseumFontTMP = font2TMP;
                    CJKFontTMP = cjkFontTMP;
                    JaFontTMP = jafontTMP;
                    GlobalFontTMPOverlayMat = font1TMPTopMat;
                    CJKFontTMPOverlayMat = cjkFontTMPTopMat;
                    jaFontTMPOverlayMat = jaFontTMPTopMat;
                    
                    TMPFontReady = true;

                    AssetBundle koreanFontBundle = AssetBundle.LoadFromFile(Path.Combine(MainPatch.ModFolder, "koreanfont"));

                    if (koreanFontBundle == null)
                    {
                        Logging.Error("FAILED TO LOAD KOREAN FONT BUNDLE");
                    }
                    else
                    {
                        CustomMainFontTMP =
                            koreanFontBundle.LoadAsset<TMP_FontAsset>("mainfont");

                        CustomMuseumFontTMP =
                            koreanFontBundle.LoadAsset<TMP_FontAsset>("museumfont");

                        CustomTerminalFontTMP =
                            koreanFontBundle.LoadAsset<TMP_FontAsset>("terminalfont");

                        CustomSecretTerminalFontTMP =
                            koreanFontBundle.LoadAsset<TMP_FontAsset>("secretterminalfont");

                        CustomMainFontTMPOverlayMat = GlobalFontTMPOverlayMat;
                        CustomMuseumFontTMPOverlayMat = GlobalFontTMPOverlayMat;
                        CustomTerminalFontTMPOverlayMat = CJKFontTMPOverlayMat;
                        CustomSecretTerminalFontTMPOverlayMat = CJKFontTMPOverlayMat;

                        Logging.Message("Korean TMP fonts loaded from koreanfont bundle.");
                    }
                }
                else
                {
                    Logging.Error("FAILED TO LOAD TMP FONTS");
                    TMPFontReady = false;
                }
            }
        }
        
        public static void HandleSceneSwitch(Scene scene,ref GameObject canvas)
        {

            //Logging.Message("Switching scenes...");
            string levelName = GetCurrentSceneName();
            if(levelName == "Intro" || levelName == "Bootstrap")
            { 
                //Don't do anything if we're still booting up the game.
                //Logging.Warn("In intro, not hooking yet");
                return;
            }
            
            //Each scene (level) has an object called Canvas. Most game objects are there.
            GameObject canvasObj = GetInactiveRootObject("Canvas");
            if (!canvasObj)
            {
                Logging.Fatal("UNABLE TO FIND CANVAS IN CURRENT SCENE");
                return;
            }
            else
            {
                TextMeshProFontSwap.ClearFontSwapCache();
                TextFontSwap.TextFontSwapper.ClearCache();

                switch (levelName)
                {
                    case "Intro": { break; }
                    case "Main Menu":
                        {
                            if (Core.wasLanguageReset)
                            {
                                Core.wasLanguageReset = false;
                                MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("<color=orange>The currently set language file could not be loaded.\nLanguage has been reset to English to avoid problems.</color>");
                            }

                            PatchFrontEnd(canvasObj);

                            if (ultrakullLogo != null)
                            {
                                GameObject.Destroy(ultrakullLogo);
                                ultrakullLogo = null;
                            }

                            ultrakullLogo = new GameObject("UltrakULL_Dropdown");
                            ultrakullLogo.transform.SetParent(canvasObj.transform, false);

                            RectTransform rootRect = ultrakullLogo.AddComponent<RectTransform>();
                            rootRect.anchorMin = new Vector2(1, 1);
                            rootRect.anchorMax = new Vector2(1, 1);
                            rootRect.pivot = new Vector2(1, 1);
                            rootRect.anchoredPosition = new Vector2(-20, -20);
                            rootRect.sizeDelta = new Vector2(250, 30);

                            Image buttonImage = ultrakullLogo.AddComponent<Image>();
                            buttonImage.color = new Color(0.2f, 0.2f, 0.2f, 0.7f);
                            Button button = ultrakullLogo.AddComponent<Button>();

                            GameObject buttonTextObj = new GameObject("ButtonText");
                            buttonTextObj.transform.SetParent(ultrakullLogo.transform, false);
                            RectTransform buttonTextRect = buttonTextObj.AddComponent<RectTransform>();
                            buttonTextRect.anchorMin = Vector2.zero;
                            buttonTextRect.anchorMax = Vector2.one;
                            buttonTextRect.offsetMin = Vector2.zero;
                            buttonTextRect.offsetMax = Vector2.zero;

                            TextMeshProUGUI buttonText = buttonTextObj.AddComponent<TextMeshProUGUI>();
                            buttonText.text = "UltrakULL ▼";
                            buttonText.alignment = TextAlignmentOptions.MidlineRight;
                            buttonText.fontSize = 16;
                            buttonText.color = Color.white;

                            GameObject panel = new GameObject("DropdownPanel");
                            panel.transform.SetParent(ultrakullLogo.transform, false);
                            RectTransform panelRect = panel.AddComponent<RectTransform>();
                            panelRect.anchorMin = new Vector2(1, 1);
                            panelRect.anchorMax = new Vector2(1, 1);
                            panelRect.pivot = new Vector2(1, 1);
                            panelRect.anchoredPosition = new Vector2(0, -30);
                            panelRect.sizeDelta = new Vector2(rootRect.sizeDelta.x, 55);

                            Image panelBg = panel.AddComponent<Image>();
                            panelBg.color = new Color(0f, 0f, 0f, 0.75f);

                            GameObject panelTextObj = new GameObject("PanelText");
                            panelTextObj.transform.SetParent(panel.transform, false);
                            RectTransform panelTextRect = panelTextObj.AddComponent<RectTransform>();
                            panelTextRect.anchorMin = new Vector2(0, 0);
                            panelTextRect.anchorMax = new Vector2(1, 1);
                            panelTextRect.offsetMin = new Vector2(5, 5);
                            panelTextRect.offsetMax = new Vector2(-5, -5);

                            TextMeshProUGUI panelText = panelTextObj.AddComponent<TextMeshProUGUI>();
                            panelText.text = "<color=white>UltrakULL loaded.\nVersion: " + MainPatch.GetVersion() + "\nCurrent locale: " + LanguageManager.CurrentLanguage.metadata.langName;
                            panelText.alignment = TextAlignmentOptions.TopRight;
                            panelText.fontSize = 16;
                            panelText.color = Color.white;

                            CanvasGroup panelGroup = panel.AddComponent<CanvasGroup>();
                            panelGroup.alpha = 0f;
                            panelGroup.interactable = false;
                            panelGroup.blocksRaycasts = false;

                            button.onClick.AddListener(() =>
                            {
                                ultrakullDropdownExpanded = !ultrakullDropdownExpanded;
                                panelGroup.alpha = ultrakullDropdownExpanded ? 1f : 0f;
                                panelGroup.interactable = ultrakullDropdownExpanded;
                                panelGroup.blocksRaycasts = ultrakullDropdownExpanded;
                                buttonText.text = ultrakullDropdownExpanded ? "UltrakULL ▲" : "UltrakULL ▼";
                            });

                            break;
                        }

                    default:
                        {
                            if (isUsingEnglish())
                            {
                                Logging.Warn("Current language is English, not patching.");
                                return;
                            }

                            Logging.Message("Regular scene");
                            Logging.Message("Attempting to patch base elements");
                            try { PatchPauseMenu(ref canvasObj); } catch (Exception e) { Console.WriteLine(e.ToString()); }
                            try { Cheats.PatchCheatConsentPanel(ref canvasObj); ; } catch (Exception e) { Console.WriteLine(e.ToString()); }
                            try { Sandbox.PatchAlterMenu(); } catch (Exception e) { Console.WriteLine(e.ToString()); }
                            try { HUDMessages.PatchDeathScreen(ref canvasObj); } catch (Exception e) { Console.WriteLine(e.ToString()); }
                            try { LevelStatWindow.PatchStats(ref canvasObj); } catch (Exception e) { Console.WriteLine(e.ToString()); }
                            try { HUDMessages.PatchMisc(ref canvasObj); } catch (Exception e) { Console.WriteLine(e.ToString()); }
                            try { Options options = new Options(ref canvasObj); } catch (Exception e) { Console.WriteLine(e.ToString()); }

                            Logging.Message("Base elements patched");
                        }


                        if (levelName.Contains("Tutorial"))
                        {
                            Logging.Message("Tutorial");
                        }
                        else if (AngryLevel.IsAngryCustomLevel() == true)
                        {
                            Logging.Message("Angry Custom Level");
                            AngryLevel.PatchAngry();
                        }
                        else if (levelName.Contains("-S"))
                        {
                            Logging.Message("Secret");
                            SecretLevels secretLevels = new SecretLevels(ref canvasObj);
                        }
                        if (levelName.Contains("0-") & !levelName.Contains("-E"))
                        {
                            Logging.Message("Prelude");
                            Prelude preludePatchClass = new Prelude(ref canvasObj);
                        }
                        else if ((levelName.Contains("1-") & !levelName.Contains("-E")) || (levelName.Contains("2-") & !levelName.Contains("-E")) || (levelName.Contains("3-") & !levelName.Contains("-E")))
                        {
                            Logging.Message("Act 1");
                            Act1.PatchAct1(ref canvasObj);
                        }
                        else if ((levelName.Contains("4-") & !levelName.Contains("-E")) || (levelName.Contains("5-") & !levelName.Contains("-E")) || (levelName.Contains("6-") & !levelName.Contains("-E")))
                        {
                            Logging.Message("Act 2");
                            Act2.PatchAct2(ref canvasObj);
                        }
                        else if ((levelName.Contains("7-") & !levelName.Contains("-E")) || (levelName.Contains("8-") & !levelName.Contains("-E")) || (levelName.Contains("9-") & !levelName.Contains("-E")))
                        {
                            Logging.Message("Act 3");
                            if (LanguageManager.CurrentLanguage.act3 != null)
                            {
                                Act3.PatchAct3(ref canvasObj);
                            }
                            else
                            {
                                Logging.Warn("Category is not found in the language file!");
                            }
                        }
                        else if (levelName.Contains("P-"))
                        {
                            Logging.Message("Prime");
                            PrimeSanctum primeSanctumClass = new PrimeSanctum();
                        }
                        else if (levelName.Contains("-E"))
                        {
                            Logging.Message("Encore");
                            if (LanguageManager.CurrentLanguage.encore != null)
                            {
                                Encore.PatchEncore(ref canvasObj);
                            }

                        }
                        else if (levelName == "uk_construct")
                        {
                            Logging.Message("Sandbox");
                            Sandbox sandbox = new Sandbox(ref canvasObj);
                        }
                        else if (levelName == "Endless")
                        {
                            Logging.Message("CyberGrind");
                            CyberGrind.PatchCg();
                        }
                        else if (levelName.Contains("Intermission") || levelName.Contains("EarlyAccessEnd"))
                        {
                            Logging.Message("Intermission");
                            Intermission intermission = new Intermission(ref canvasObj);
                        }
                        else if (levelName == "CreditsMuseum2")
                        {
                            Logging.Message("DevMuseum");
                            DevMuseum devMuseum = new DevMuseum();
                        }
                        break;
                }
            }
        }

        public static async void ApplyPostInitFixes(GameObject canvasObj)
        {
            await Task.Delay(250); // Fix warning about async without await
            /*if (GetCurrentSceneName() == "Main Menu")
            {
                //Open Language Folder button in Options->Language
                TextMeshProUGUI openLangFolderText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(canvasObj,"OptionsMenu"), "Language Page"),"Scroll Rect (1)"),"Contents"),"OpenLangFolder"),"Slot Text")); 
                openLangFolderText.text = "<color=#03fc07>Open language folder</color>";
                
            }*/
        }
    }
}
