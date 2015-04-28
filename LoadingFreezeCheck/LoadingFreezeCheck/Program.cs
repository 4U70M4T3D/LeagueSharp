using System;
using System.Drawing;
using LeagueSharp;
using LeagueSharp.Common;

namespace LoadingFreezeCheck
{
    class Program
    {
        private static int _counter;
        private static int _colorChange = 15;
        private static bool _inGame;
        private static readonly Color[] Colors = { Color.Cyan, Color.Red, Color.Green, Color.Pink, Color.Yellow, Color.Blue, Color.White, Color.DarkOrange, Color.DarkMagenta };
        private static int _lastColor;

        static void Main()
        {
            Game.OnStart += GameStarted;
            CustomEvents.Game.OnGameLoad += GameStarted;
            Drawing.OnDraw += DrawNotFrozen;
        }
        private static void DrawNotFrozen(EventArgs args)
        {

            if (_inGame) return;
            _counter++;
            if (_counter > _colorChange)
            {
                _lastColor = new Random().Next(0, 8);
                Drawing.DrawText(100, 100, Colors[_lastColor], "LOADING");
                _colorChange += 15;
            }
            else
            {
                Drawing.DrawText(100, 100, Colors[_lastColor], "LOADING");
            }
        }
        private static void GameStarted(EventArgs args)
        {
            _inGame = true;
        }
    }
}
