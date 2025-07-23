// System namespaces
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

// Unity
using UnityEngine;

// MelonLoader
using MelonLoader;

// BTD Mod Helper
using BTD_Mod_Helper;
using BTD_Mod_Helper.Api;
using BTD_Mod_Helper.Extensions;

// Il2Cpp and Bloons TD 6 game namespaces
using Il2Cpp;
using Il2CppAssets;
using Il2CppAssets.Scripts.Data;
using Il2CppAssets.Scripts.Data.Behaviors.Events.Triggers;
using Il2CppAssets.Scripts.Data.Behaviors.Weapons;
using Il2CppAssets.Scripts.Models;
using Il2CppAssets.Scripts.Models.Gameplay.Mods;
using Il2CppAssets.Scripts.Models.Towers.Weapons;
using Il2CppAssets.Scripts.Simulation;
using Il2CppAssets.Scripts.Simulation.Towers;
using Il2CppAssets.Scripts.Unity;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;

// Custom project namespaces
using DATAREADER;
using System.Runtime.CompilerServices;


[assembly: MelonInfo(typeof(DATAREADER.DATAREADER), ModHelperData.Name, ModHelperData.Version, ModHelperData.RepoOwner)]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]

namespace DATAREADER;

public class DATAREADER : BloonsTD6Mod
{
    public override void OnApplicationStart()
    {
        ModHelper.Msg<DATAREADER>("DATAREADER loaded!");
        managerDirectoryStuffs();
        MelonCoroutines.Start(CheckValueLoop());
        //GameDataJsonExample test = new GameDataJsonExample();
        //test.testlol();
    }
   
    /*
    public override void OnNewGameModel(GameModel result)
    {
        foreach(var weapon in result.GetDescendants<WeaponModel>().ToList() )
        {
            weapon.rate = 0.01f;
        }

    }*/
    public override void OnCashAdded(double amount, Simulation.CashType from, int cashIndex, Simulation.CashSource source, Tower tower)
    {
       // base.OnCashAdded(amount, from, cashIndex, source, tower);
        //ModHelper.Msg<DATAREADER>($"Cash Added: {amount}!"); 

    }
    public override void OnRoundEnd() 
        {
            ModHelper.Msg<DATAREADER>("round has ended!"); 

    }
    /*
    public override void OnUpdate()
    {
        try
        {
            if (InGame.instance != null && InGame.instance.GetSimulation() != null )
            {
                MelonLogger.Msg("Hola Soy");
                var cash = InGame.instance.GetCash();
                MelonLogger.Msg($"Current cash: {cash}");
            }
        } catch (Exception e)
        {
            ModHelper.Msg<DATAREADER>(e.Message);
        }
    }*/

    public override void OnMatchEnd()
    {
        ModHelper.Msg<DATAREADER>("Game Over");
    }

    public override void OnDefeat()
    {
        ModHelper.Msg<DATAREADER>("Defeated gamemover Over");
        gameOver = true;
        writeData();

    }

    public override void OnVictory()
    {

        wonGame = true;
        writeData();
    }

    public override void OnRestart()
    {
        gameOver = false;
        wonGame = false;
        writeData();
    }
    public class AbilityData
    {

        public Boolean IsReady { get; set; }
        public float CoolDownRemaining { get; set; }
        public AbilityData(bool isReady, float coolDownRemaining)
        {
            IsReady = isReady;
            CoolDownRemaining = coolDownRemaining;
        }
    }
    public class GameData
    {
        public bool isDefeated { get; set; }
        
        public bool gameWon { get; set; }
        public double Cash { get; set; }
        public int Round { get; set; }
        public int Lives { get; set; }
        public int TowersPlaced { get; set; }
        public int TotalAbilities { get; set; }
        
        public int LogNumber { get; set; }

        public int StartRound { get; set; }

        public List<AbilityData> Abilities { get; set; } = new List<AbilityData>();

    }
    bool gameOver = false;
    bool wonGame = false;
    int logIteration = 0;
    double cash = 0;
    int round = 0;
    double lives = 1;
    int  totalTowersPlaced = 0;
    int totalAbilities = 0;
    int startRound =0;
    List<AbilityData> AbilityInformations = new List<AbilityData>();

    string filePath = "gameData";
    private void writeData ()
    {
        GameData gameData = new GameData
        {
            isDefeated = gameOver,
            gameWon = wonGame,
            Cash = cash,
            Round = round,
            Lives = (int)lives,
            TowersPlaced = totalTowersPlaced,
            LogNumber = logIteration,
            StartRound = startRound,
            TotalAbilities = totalAbilities,
            Abilities = AbilityInformations

        };


        var options = new JsonSerializerOptions();

        options.WriteIndented = true;

        string jsonString = JsonSerializer.Serialize<GameData>(gameData, options);
        MelonLogger.Msg($"{jsonString}");
        
        File.WriteAllText(filePath + "/gameData.json", jsonString);
        logIteration += 1;
    }

    private void managerDirectoryStuffs()
    {
        MelonLogger.Msg("This game is running at the directory: " + Directory.GetCurrentDirectory());
        if (Directory.Exists(filePath))
        {
            MelonLogger.Msg(filePath + " Folder exists, will not be creating a new one..");

        } else
        {
            MelonLogger.Msg(filePath + " folder does not exist, will be creating a new one");
            Directory.CreateDirectory(filePath);
            MelonLogger.Msg("Sucessfully created folder at: " + Directory.GetCurrentDirectory() + filePath);
        }
    }
    private IEnumerator CheckValueLoop()
    {
        
        while (true)
        {
            
                



            yield return new WaitForSeconds(0.20f);
            if (InGame.instance != null && InGame.instance.GetSimulation() != null)
            {
                var simulation = InGame.instance.GetSimulation();

                cash = InGame.instance.GetCash();
                round = simulation.GetCurrentRound() + 1;
                lives = InGame.instance.GetHealth();
                var towers = InGame.instance.GetTowers();
                var towersPlaced = 0;

                foreach (var tower in towers)
                {
                    //MelonLogger.Msg($"Tower: {tower.towerModel.name}");
                    towersPlaced++;
                }
                totalTowersPlaced = towersPlaced;

                var abilities = InGame.instance.GetAbilities();
                var totalAbilities = 0;
                List<AbilityData> myAbilityList = new List<AbilityData>();


                foreach (var ability in abilities)
                {
                    MelonLogger.Msg($"Ability: {ability.tower} Can Use?: {ability.IsReady} CoolDownRemaining: {ability.CooldownRemaining}");
                    AbilityData newAbility = new AbilityData(ability.IsReady, ability.CooldownRemaining);
                    myAbilityList.Add(newAbility);
                    totalAbilities++;
                    MelonLogger.Msg(newAbility.ToString() );
                }
                AbilityInformations.Clear();
                AbilityInformations = myAbilityList;

                startRound = InGame.instance.GetStartRound();


                writeData();
               
                // MelonLogger.Msg($"Cash: {cash} Round: {round} Lives: {lives} TowersPlaced: {towersPlaced}");
            }
        }

    }

   


}