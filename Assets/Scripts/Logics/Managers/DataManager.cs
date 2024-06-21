using GSDM;
using System.Collections.Generic;

public static class DataManager 
{
    public static Stack<MatchMakingUser> Opponents = new Stack<MatchMakingUser>();

    public static void PushOpponent(MatchMakingUser opponent) => Opponents.Push(opponent);
    public static MatchMakingUser PopOpponent() => Opponents.Pop();
}