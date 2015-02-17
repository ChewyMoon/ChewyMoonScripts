using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace EasyPeasyRivenSqueezy
{
    class WallJumper
    {

        public static void DoWallJump()
        {
            
            if (Riven.QCount != 2)
            {
                NotificationHandler.ShowWarningAlert("You need 2 Q's!");
                return;
            }

            var point = VectorHelper.GetFirstWallPoint(Riven.Player.ServerPosition, Game.CursorPos, 20);

            if (point == null)
            {
                NotificationHandler.ShowInfo("No spot to jump to!");
                return;
            }

            if (Riven.E.IsReady())
            {
                Riven.E.Cast((Vector2)point);
            }
            else
            {
                Riven.Player.IssueOrder(
               GameObjectOrder.MoveTo,
               Riven.Player.ServerPosition.Extend((Vector3)point, Riven.Player.BoundingRadius + 20));
            }

            Utility.DelayAction.Add(500, () => Riven.Q.Cast((Vector2)point));
            

        }

    }

    // Credits to hellsing who let me borrow this
    class VectorHelper
    {
        public static Vector2? GetFirstWallPoint(Vector3 from, Vector3 to, float step = 25)
        {
            return GetFirstWallPoint(from.To2D(), to.To2D(), step);
        }

        public static Vector2? GetFirstWallPoint(Vector2 from, Vector2 to, float step = 25)
        {
            var direction = (to - from).Normalized();

            for (float d = 0; d < from.Distance(to); d = d + step)
            {
                var testPoint = from + d * direction;
                var flags = NavMesh.GetCollisionFlags(testPoint.X, testPoint.Y);
                if (flags.HasFlag(CollisionFlags.Wall) || flags.HasFlag(CollisionFlags.Building))
                {
                    return from + (d - step) * direction;
                }
            }

            return null;
        }
    }
}
