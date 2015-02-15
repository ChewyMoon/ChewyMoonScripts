using System;
using System.Reflection;
using LeagueSharp.Common;
using SharpDX;

namespace EasyPeasyRivenSqueezy
{
    class NotificationHandler
    {
        private static Notification _modeNotification;
        private static Notification _rModeNotification;

        private static int _lastInfoNotification;
        private static int _lastGapcloserAlert;
        private static int _lastInterrupterAlert;
        private static int _lastWarningAlert;

        private const int NotificationDelay = 1000;

        private static bool NotificationsEnabled
        {
            get
            {
                return Riven.GetBool("Notifications");
            }
        }

        public static void ShowWelcome()
        {
            var notification = new Notification("EasyPeasyRivenSqueezy v" + Assembly.GetExecutingAssembly().GetName().Version + " loaded!", 10000);
            Notifications.AddNotification(notification);
        }

        public static void ShowGapcloserAlert()
        {
            if (Environment.TickCount - _lastGapcloserAlert >= NotificationDelay || !NotificationsEnabled)
            {
                return;
            }

            var notification = new Notification("GAPCLOSER!", 2500)
            {
                TextColor = new ColorBGRA(255, 0, 0, 255)
            };

            notification.Flash(500);
            Notifications.AddNotification(notification);

            _lastGapcloserAlert = Environment.TickCount;
        }

        public static void ShowInterrupterAlert(bool interrupted)
        {
            if (Environment.TickCount - _lastInterrupterAlert >= NotificationDelay || !NotificationsEnabled)
            {
                return;
            }

            var notification = new Notification(interrupted ? "Interrupted!" : "Failed to interrupt!", 2500)
            {
                TextColor = interrupted ? new ColorBGRA(0, 255, 0, 255) : new ColorBGRA(255, 0, 0, 255)
            };

            notification.Flash(500);
            Notifications.AddNotification(notification);

            _lastInterrupterAlert = Environment.TickCount;
        }

        public static void ShowWarningAlert(string text)
        {
            if (Environment.TickCount - _lastWarningAlert >= NotificationDelay || !NotificationsEnabled)
            {
                return;
            }

            var notification = new Notification(text, 5000)
            {
                TextColor = new ColorBGRA(255, 255, 0, 255)
            };

            Notifications.AddNotification(notification);

            _lastWarningAlert = Environment.TickCount;
        }

        public static void ShowInfo(string text)
        {
            if (Environment.TickCount - _lastInfoNotification >= NotificationDelay || !NotificationsEnabled)
            {
                return;
            }

            var notification = new Notification(text, 5000);
            Notifications.AddNotification(notification);

            _lastInfoNotification = Environment.TickCount;
        }

        public static void Update()
        {
            if (!NotificationsEnabled)
            {
                // Remove mode notification if user turned notifications off
                if (_modeNotification != null)
                {
                    Notifications.RemoveNotification(_modeNotification);
                    _modeNotification = null;
                }

                if (_rModeNotification != null)
                {
                    Notifications.RemoveNotification(_rModeNotification);
                    _rModeNotification = null;
                }

                return;
            }

            if (_modeNotification == null)
            {
                _modeNotification = new Notification("Mode: " + Riven.Orbwalker.ActiveMode)
                {
                    TextColor = new ColorBGRA(124, 252, 0, 255)
                };

                Notifications.AddNotification(_modeNotification);
            }

            if (_rModeNotification == null)
            {
                _rModeNotification = new Notification("R Mode: " + Riven.Menu.Item("UseROption").GetValue<StringList>().SelectedValue)
                {
                    TextColor = new ColorBGRA(0, 153, 250, 255)
                };

                Notifications.AddNotification(_rModeNotification);
            }

            if (Riven.Menu.Item("FleeActive").IsActive())
            {
                _modeNotification.Text = "Mode: Flee";
            }
            else
            {
                _modeNotification.Text = "Mode: " + Riven.Orbwalker.ActiveMode;
            }

            _rModeNotification.Text = "R Mode: " + Riven.Menu.Item("UseROption").GetValue<StringList>().SelectedValue;
        }
    }
}
