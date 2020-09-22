using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerData
{
    public string playerId;
    public string playerName;

    //[wins, draws, losses, points]
    public float[] unlimited = new float[4];
    public float[] bullet = new float[4];
    public float[] blitz = new float[4];
    public float[] rapid = new float[4];
    public float[] custom = new float[4];

    public float[] light = new float[4];
    public float[] dark = new float[4];
}
