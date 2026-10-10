using UnityEngine;

public static class SelectionData
{
    public static int Player1Top = 0;
    public static int Player1Mid = 0;
    public static int Player1Tip = 0;

    public static int Player2Top = 0;
    public static int Player2Mid = 0;
    public static int Player2Tip = 0;

    public static void ResetSelections()
    {
        Player1Top = 0;
        Player1Mid = 0;
        Player1Tip = 0;

        Player2Top = 0;
        Player2Mid = 0;
        Player2Tip = 0;
    }
}
