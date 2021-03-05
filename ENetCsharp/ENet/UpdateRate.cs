using System.Diagnostics;

namespace ENetCsharp
{
    internal class UpdateRate
    {
        private int _updateRate;
        private int _frameMS;
        private Stopwatch _stopwatch;

        public UpdateRate(int updateRate)
        {
            _updateRate = updateRate;
            _frameMS = (int)(1000f / (float)updateRate);
            _stopwatch = new Stopwatch();
        }

        public void Start()
        {
            if (_stopwatch.IsRunning)
            {
                _stopwatch.Restart();
            }
            else
            {
                _stopwatch.Start();
            }
        }

        public void Stop()
        {
            _stopwatch.Stop();
        }

        public bool CanRunAfterFrame()
        {
            if (_stopwatch.ElapsedMilliseconds >= _frameMS)
            {
                _stopwatch.Restart();
                return true;
            }
            return false;
        }
    }
}
