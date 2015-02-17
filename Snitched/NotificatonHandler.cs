using LeagueSharp.Common;
using SharpDX;

namespace Snitched
{
    class NotificationHandler
    {
        private static Notification _modeNotificationHandler;

        public static void Update()
        {
            var text = "Snitcher: " + (Program.Menu.Item("Enabled").GetValue<bool>() ||
                Program.Menu.Item("EnabledKeybind").GetValue<KeyBind>().Active ? "Enabled" : "Disabled");
            
            if (_modeNotificationHandler == null)
            {
                _modeNotificationHandler = new Notification("Snitcher: " + text)
                {
                    TextColor = new ColorBGRA(124, 252, 0, 255)
                };

                Notifications.AddNotification(_modeNotificationHandler);
            }

            _modeNotificationHandler.Text = text;
        }
    }
}
