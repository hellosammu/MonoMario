using System;
using System.Collections.Generic;
using System.Linq;

namespace SMWEngine.Source
{
    /**
     * Although created by me, this Timer class is HEAVILY
     * inspired by the way that the Flixel library. It can
     * do nearly everything that that timer system does.
     */
    public class Timer
    {
        // All timers
        public static List<Timer> timers = new List<Timer>();

        // Visible non-setter time left
        public float timeLeft { get => _timeLeft; }

        // How many times the timer loops (0 = infinite, 1 = default)
        public int loops = 1;
        public int loopsLeft;

        // If timer is finished
        public bool finished = false;

        // Actual time left (only modifiable in object)
        private float _timeLeft { get; set; }

        // Time elapsed, calculated from length of timer and how much time is left
        public float elapsedTime { get => time - timeLeft; }

        // The length of the timer
        public float time { get; set; }

        // If the timer is paused or not
        public bool active { get; set; } = false;

        // Visible start delay
        /*public float startDelay
        {
            get
            {
                return _startDelay;
            }
            set
            {
                _startDelay = value;
                startDelayCounter = value;
            }
        }*/
        // Starting delay time
        //private float _startDelay = 0;

        // Start delay counter
        //private float startDelayCounter = 0;
        public bool started = false;

        public Del onComplete;
        //public Del onStart;
        public Del onUpdate;

        public Timer()
        {

        }

        public Timer Start(float time) => Start(time, null, 1);
        public Timer Start(float time, Del onComplete) => Start(time, onComplete, 1);
        public Timer Start(float time, int loops) => Start(time, null, loops);
        public Timer Start(float time, Del onComplete, int loops = 1)
        {
            if (!timers.Contains(this))
            {
                timers.Add(this);
            }
            this.time = time;
            this._timeLeft = time;
            this.onComplete = onComplete;
            this.loops = loops;
            this.loopsLeft = loops;

            this.active = true;
            this.finished = false;

            return this;
        }

        public delegate void Del();

        public static void Update()
        {
            // Iterate through each timer
            timers.ToList().ForEach(delegate (Timer timer)
            {
                // Don't do anything if the timer is paused
                if (!timer.active)
                    return;
                // Subtract from the time left on the timer
                if (timer.timeLeft > 0)
                {
                    // Subtract counter
                    timer._timeLeft --;
                    // Trigger update method
                    if (timer.onUpdate != null)
                        timer.onUpdate();
                    // Don't complete on the frame that the timer reaches 0
                    return;
                }
                // If timer is finished
                if (timer.timeLeft <= 0)
                {
                    // Trigger on-complete function
                    if (timer.onComplete != null)
                    {
                        timer.onComplete();
                    }
                    // Iterate loops process if not set to infinitely loop
                    if (timer.loops > 0)
                    {
                        // If loops are left, subtract from them
                        if (timer.loopsLeft > 0)
                        {
                            timer.loopsLeft--;
                            timer._timeLeft = timer.time;
                        }
                        // If there's no loops left, set to inactive & remove timer
                        else
                        {
                            timers.Remove(timer);
                            timer.active = false;
                        }
                    }
                }
            });
        }
    }
}
