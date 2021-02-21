using System;
using System.Collections.Generic;
using System.Threading;

namespace ClientTest
{
    public class Time
    {
        private static float _deltaTime;
        private static float _totalSec;
        private static long _frameCount;

        public static float deltaTime => _deltaTime;
        public static float realtimeSinceStartup => _totalSec;
        public static float timeSinceLevelLoad => _totalSec;
        public static long frameCount => _frameCount;

        public static void Update(long miliseconds)
        {
            _deltaTime = (float)miliseconds * 0.001f;
            _totalSec += miliseconds * 0.001f;
            _frameCount++;
        }
    }

    public static class InputListener
    {
        static Queue<string> _inputQueue = new Queue<string>();
        static readonly object _object = new object();
        static public bool Quited = false;

        private static Dictionary<string, Action<string[]>> _actionMethods = new Dictionary<string, Action<string[]>>();

        public static void Start()
        {
            Thread thr = new Thread(InputThread);
            thr.Start();

            Register("q", Quit);
        }

        private static void Quit(string[] obj)
        {
            Quited = true;
        }

        public static void Register(string key, Action<string[]> action)
        {
            _actionMethods.Add(key, action);
        }

        public static void Unregister(string key)
        {
            _actionMethods.Remove(key);
        }

        public static void Update()
        {
            string line = "";
            lock (_object)
            {
                if (_inputQueue.Count > 0)
                {
                    line = _inputQueue.Dequeue();
                }
            }

            if (!string.IsNullOrEmpty(line))
            {
                string[] commands = line.Split(' ');

                if (_actionMethods.ContainsKey(commands[0]))
                {
                    _actionMethods[commands[0]]?.Invoke(commands);
                }
            }
        }

        private static void InputThread(object obj)
        {
            while (!Quited)
            {
                string line = Console.ReadLine();
                lock (_object)
                {
                    _inputQueue.Enqueue(line);
                }
            }
        }
    }
}