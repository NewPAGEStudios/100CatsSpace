using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateManager
{
    public enum GameState
    {
        InGame = 0, InMenu = 1, InChat = 2, InConsole = 3, EndGame = 4, CatEndGame = 5, InReplay = 6
    }
    public enum GameMode
    {
        None, fillColor, findCat
    }

    public enum InputScheme
    {
        None, Keyboard_MouseScheme, GamePadScheme
    }

    public enum MainMenuState
    {
        mainMenu ,setting, hostOnlineLobby, Map, AYS,Wait,colorPick,AYSaveWipe,CreditSection, LeaderBoard,Tutorial
    }
    public enum LobbyState
    {
        None,main, mapSelect
    }
    public enum TutActions
    {
        None, Start, MoveMouse, Radio, FindCat, Hint, ColorMode, OpenOnMap, ColorMenu, PaintRegion, ZoomAndMoveCam , EndGame
    }

}
