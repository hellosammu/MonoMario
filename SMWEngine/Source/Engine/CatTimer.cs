using System;
using System.Collections.Generic;
using System.Linq;

namespace SMWEngine.Source
{
    /**
     * Although created by me, this Timer class is HEAVILY
     * inspired by the way that the Flixel timer works. It can
     * do nearly everything that that timer system does.
     */
    public class CatTimer
    {

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

        public delegate void Del();
        public Del onComplete;
        public Del onUpdate;

        /**
         * Nullify the timer
         */
        public void Cancel()
        {
            if (CatTimer.timers.Contains(this))
                CatTimer.timers.Remove(this);
            finished = true;
            active = false;
        }

        /**
         * Simple pause wrappers, not necessary (can just use active)
         */
        public void Pause() => active = false;
        public void Resume() => active = true;

        /**
         * Easy way to reset the timer mid-way before finishing
         */
        public CatTimer Reset() => Reset(0);
        public CatTimer Reset(float time)
        {

            // If the time given is more than 0, feed the new time in
            if (time > 0)
                this.time = time;

            // Start a new timer
            Start(this.time, onComplete, loops);

            // Return this timer object
            return this;
        }

        public CatTimer Start(float time) => Start(time, null, 1);
        public CatTimer Start(float time, Del onComplete) => Start(time, onComplete, 1);
        public CatTimer Start(float time, int loops) => Start(time, null, loops);
        public CatTimer Start(float time, Del onComplete, int loops = 1)
        {
            // Add object to timers
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

        // Global timer stuff
        public static List<CatTimer> timers = new List<CatTimer>();
        public static void Update(float elapsed)
        {
            // Iterate through each timer
            timers.ToList().ForEach(delegate (CatTimer timer)
            {
                // Don't do anything if the timer is paused
                if (!timer.active)
                    return;
                // Subtract from the time left on the timer
                if (timer.timeLeft > 0)
                {
                    // Subtract counter
                    timer._timeLeft -= (elapsed * SMW.multiplyFPS);
                    // Trigger update method
                    if (timer.onUpdate != null)
                        timer.onUpdate();
                    // Don't complete on the frame that the timer reaches 0
                    if (timer._timeLeft <= 0)
                        timer._timeLeft = 0;
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
                        if (timer.loopsLeft > 1)
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
                    else
                    {
                        timer.Start(timer.time, timer.onComplete, timer.loops);
                    }
                }
            });
        }
    }
}
