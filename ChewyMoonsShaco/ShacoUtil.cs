#region

using System.Collections.Generic;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

#endregion

namespace ChewyMoonsShaco
{
    internal class ShacoUtil
    {
        public static Vector3 GetQPos(Obj_AI_Hero target, bool serverPos, int distance = 150)
        {
            var enemyPos = serverPos ? target.ServerPosition : target.Position;
            var myPos = serverPos ? ObjectManager.Player.ServerPosition : ObjectManager.Player.Position;

            return enemyPos + Vector3.Normalize(enemyPos - myPos) * distance;
        }

        public static Vector2 GetShortestWayPoint(List<Vector2> waypoints)
        {
            var shortestPos = new Vector2();
            foreach (var waypoint in waypoints)
            {
                if (shortestPos == new Vector2())
                {
                    shortestPos = waypoint;
                }
                else
                {
                    if (waypoint.Distance(ObjectManager.Player) < shortestPos.Distance(ObjectManager.Player))
                    {
                        shortestPos = waypoint;
                    }
                }
            }

            return shortestPos;
        }
    }
}