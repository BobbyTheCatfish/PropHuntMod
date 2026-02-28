using PropHuntMod.Utils;
using System.Timers;

namespace PropHuntMod.Networking.Server
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
                Server.instance.Announce("[Seekers]: Ready or not, here we come!");
                Server.GameState = GameState.Playing;
                ServerNetwork.BroadcastSeekerStart();

            }
            else if (seconds <= 5)
            {
                Server.instance.Announce($"[Seekers]: {seconds}...");
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
