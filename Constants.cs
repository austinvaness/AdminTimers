using Sandbox.Game;
using Sandbox.ModAPI;
using System.Collections.Generic;
using System.Linq;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.Utils;

namespace avaness.AdminTimers
{
    public static class Constants
    {
        public static bool IsOffline => MyAPIGateway.Session.OnlineMode == MyOnlineModeEnum.OFFLINE;
        public static bool IsServer => MyAPIGateway.Session.IsServer || IsOffline;
        public static bool IsDedicated => IsServer && MyAPIGateway.Utilities.IsDedicated;
        public static bool IsPlayer => !IsDedicated;

        public const int maxCmdLen = 32;

        public const ushort chatPacket = 10665;

        public static IMyPlayer GetPlayer(ulong steamId)
        {
            if (steamId == 0)
                return null;
            List<IMyPlayer> temp = new List<IMyPlayer>(1);
            MyAPIGateway.Players.GetPlayers(temp, p => p.SteamUserId == steamId);
            return temp.FirstOrDefault();
        }

        public static IMyPlayer GetPlayer(long playerId)
        {
            if (playerId == 0)
                return null;
            List<IMyPlayer> temp = new List<IMyPlayer>(1);
            MyAPIGateway.Players.GetPlayers(temp, p => p.IdentityId == playerId);
            return temp.FirstOrDefault();
        }

        public static bool IsAdmin(long playerId)
        {
            ulong steamId = MyAPIGateway.Players.TryGetSteamId(playerId);
            if (steamId != 0)
                return MyAPIGateway.Session.IsUserAdmin(steamId);
            return false;
        }

        public static bool IsAdmin(IMyPlayer p)
        {
            return MyAPIGateway.Session.IsUserAdmin(p.SteamUserId);
        }
    }
}
