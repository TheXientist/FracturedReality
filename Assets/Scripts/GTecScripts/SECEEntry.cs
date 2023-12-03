using System;

[Serializable]
public class SECEEntry
{
    public string time;
    public string SECEdata;
    public string currentEvent = "Empty";
    public string playerPosition = "";
    public float playerVelocity = 0.0f;
    public float playerHealth = 0;
    public float bossHealth = 0;
}