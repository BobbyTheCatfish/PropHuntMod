using PropHuntMod.Utils;
using PropHuntMod.Utils.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace PropHuntMod.Server
{
    public class SeekerTimer
    {
        public int seconds;
        Timer timer;

        void SetTimer(int seconds)
        {
            this.seconds = seconds;
            timer = new Timer(1000);
            timer.Elapsed += new ElapsedEventHandler((a, b) => {
                OnSecond();
            });

            timer.AutoReset = true;
            timer.Start();
        }

        void OnSecond()
        {
            seconds--;

            if (seconds <= 0)
            {
                // stop timer
                timer.Stop();
                timer = null;

                // send data
                PropHuntServer.instance.Announce("[Seekers]: Ready or not, here we come!");
                PropHuntServer.GameState = GameState.Playing;
                ServerNetwork.BroadcastSeekerStart();

            }
            else if (seconds <= 5)
            {
                PropHuntServer.instance.Announce($"[Seekers]: {seconds}...");
            }
        }

        public void CancelTimer()
        {
            if (timer != null)
            {
                timer.Stop();
                timer = null;
            }
        }

        public SeekerTimer(int seconds)
        {
            SetTimer(seconds);
        }
    }
}
