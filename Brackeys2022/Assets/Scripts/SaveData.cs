using System.Collections.Generic;
[System.Serializable]

/// <summary>A class to store a player's save data.</summary>
public class SaveData
{
    /// <summary>Shows whether the player has gained cat powers.</summary>
    public bool hasCat;    
    
    /// <summary>Shows whether the player has gained birb powers.</summary>
    public bool hasBirb;

    /// <summary>Shows whether the player has gained mole powers.</summary>
    public bool hasMole;

    /// <summary>Shows whether the player has gained goat powers.</summary>
    public bool hasGoat;

    /// <summary>Shows whether the player has gained monkey powers.</summary>
    public bool hasMonkey;


    /// <summary>The index of the last checkPoint.</summary>
    public int checkPointIndex;

    /// <summary> Sound volume</summary>
    public float volume;

    /// <summary> Preferred language</summary>
    public int languageIndex;

    /// <summary>Creates a brand new, empty save file.</summary>
    public SaveData()
    {
        hasCat = hasBirb = hasMole = hasGoat = hasMonkey = false;
        checkPointIndex = 0;
        volume = 1;
        languageIndex = 0;
    }

    /// <summary>Creates a new save file with the given sound and language.</summary>
    /// <param name="soundVolume">Determines what volume to set the sound at</param>
    /// <param name="language">Determined what the language should be</param>
    public SaveData(float soundVolume, int language)
    {
        hasCat = hasBirb = hasMole = hasGoat = hasMonkey = false;
        checkPointIndex = 0;
        volume = soundVolume;
        languageIndex = language;
    }



    /// <summary>    /// <summary>Creates a save file from the given save information</summary>
    /// <param name="cat">Determines whether the cat power is on</param>
    /// <param name="birb">Determines whether the birb power is on</param>
    /// <param name="mole">Determines whether the mole power is on</param>
    /// <param name="goat">Determines whether the goat power is on</param>
    /// <param name="monkey">Determines whether the monkey power is on</param>
    /// <param name="checkPoint">Determines which chekpoint to spawn at</param>
    /// <param name="soundVolume">Determines what volume to set the sound at</param>
    /// <param name="language">Determined what the language should be</param>
    public SaveData(bool cat, bool birb, bool mole, bool goat, bool monkey, int checkPoint, float soundVolume, int language)
    {
        hasCat = cat;
        hasBirb = birb;
        hasMole = mole;
        hasGoat = goat;
        hasMonkey = monkey;
        checkPointIndex = checkPoint;
        volume = soundVolume;
        languageIndex = language;
    }
}

