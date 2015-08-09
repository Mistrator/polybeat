using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Polybeat
{
    public static class KeyboardHandler
    {
        /// <summary>
        /// Näppäimistön tila viime tarkistuksella, järjestetty,
        /// vain käytössä olevia näppäimiä
        /// </summary>
        private static int[] LastPressed;

        /// <summary>
        /// Edelliset painetut aktiiviset näppäimet, järjestetty
        /// </summary>
        private static int[] LastPressedActiveKeys;

        static KeyboardHandler()
        {
            LastPressed = new int[0];
            LastPressedActiveKeys = new int[0];
        }

        /// <summary>
        /// Palauttaa näppäimistössä tapahtuneet muutokset.
        /// </summary>
        /// <returns></returns>
        public static KeyboardChange UpdateState()
        {
            KeyboardState state = Keyboard.GetState();
            Keys[] pressed = state.GetPressedKeys();

            if (pressed.Length == 0)
            {
                // ei painettu mitään
                LastPressed = new int[0];
                return KeyboardChange.NonePressed;
            }

            List<int> pressedValsList = new List<int>();

            for (int i = 0; i < pressed.Length; i++)
            {
                int v = GetValueForKey(pressed[i]);
                if (v != -1)
                    pressedValsList.Add(v);
            }

            pressedValsList.Sort();

            int[] pressedVals = pressedValsList.ToArray();

            if (pressedVals.Length == 0)
            {
                // painettu ainoastaan ei-aktiivisia näppäimiä
                LastPressed = pressedVals;
                return KeyboardChange.NonePressed;
            }

            if (CompareArrays(pressedVals, LastPressed))
            {
                // tilanne muuttumaton
                return KeyboardChange.Unchanged;
            }

            if (LastPressedActiveKeys.Length == 0)
            {
                // ensimmäinen painallus, ei edeltäviä näppäimiä
                LastPressed = pressedVals;
                LastPressedActiveKeys = pressedVals;
                return KeyboardChange.SamePressed;
            }

            // muuttunut edellisestä tarkistuksesta

            if (CompareArrays(pressedVals, LastPressedActiveKeys))
            {
                // painettu samaa kuin viimeksi näppäimen nostamisen jälkeen
                LastPressed = pressedVals;
                return KeyboardChange.SamePressed;
            }

            // painettu joko suurempaa, pienempää tai molempia

            if (pressedVals[0] < LastPressedActiveKeys[0] && pressedVals[pressedVals.Length - 1] > LastPressedActiveKeys[LastPressedActiveKeys.Length - 1])
            {
                // painettu suurempaa ja pienempää
                LastPressed = pressedVals;
                LastPressedActiveKeys = pressedVals;
                return KeyboardChange.BothHigherAndLower;
            }

            if (pressedVals[0] < LastPressedActiveKeys[0])
            {
                // painettu pienempää
                LastPressed = pressedVals;
                LastPressedActiveKeys = pressedVals;
                return KeyboardChange.Lower;
            }

            if (pressedVals[pressedVals.Length - 1] > LastPressedActiveKeys[LastPressedActiveKeys.Length - 1])
            {
                LastPressed = pressedVals;
                LastPressedActiveKeys = pressedVals;
                return KeyboardChange.Higher;
            }

            // tätä ei pitäis tulla
            LastPressed = pressedVals;
            LastPressedActiveKeys = pressedVals;
            return KeyboardChange.SamePressed;
        }

        private static bool CompareArrays(int[] first, int[] second)
        {
            if (first.Length != second.Length)
                return false;

            for (int i = 0; i < first.Length; i++)
            {
                if (first[i] != second[i])
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Palauttaa ASCII-kirjainta vastaavan numeroarvon.
        /// Arvot alkavat nollasta ja kasvavat vasemmalta oikealle, riveissä alhaalta ylöspäin.
        /// Muu näppäin palauttaa arvon -1.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private static int GetValueForKey(Keys key)
        {
            switch (key)
            {
                case Keys.A:
                    return 7;
                case Keys.B:
                    return 4;
                case Keys.C:
                    return 2;
                case Keys.D:
                    return 9;
                case Keys.E:
                    return 20;
                case Keys.F:
                    return 10;
                case Keys.G:
                    return 11;
                case Keys.H:
                    return 12;
                case Keys.I:
                    return 25;
                case Keys.J:
                    return 13;
                case Keys.K:
                    return 14;
                case Keys.L:
                    return 15;
                case Keys.M:
                    return 6;
                case Keys.N:
                    return 5;
                case Keys.O:
                    return 26;
                case Keys.P:
                    return 27;
                case Keys.Q:
                    return 16;
                case Keys.R:
                    return 19;
                case Keys.S:
                    return 8;
                case Keys.T:
                    return 20;
                case Keys.U:
                    return 22;
                case Keys.V:
                    return 3;
                case Keys.W:
                    return 17;
                case Keys.X:
                    return 1;
                case Keys.Y:
                    return 21;
                case Keys.Z:
                    return 0;
                case Keys.Space:
                    return 28;
                default:
                    return -1;
            }
        }

        /// <summary>
        /// Onko jokin aktiivinen näppäin painettuna.
        /// </summary>
        /// <param name="change"></param>
        /// <returns></returns>
        public static bool IsKeyPressed(KeyboardChange change)
        {
            switch (change)
            {
                case KeyboardChange.Unchanged:
                    return false;
                case KeyboardChange.SamePressed:
                    return true;
                case KeyboardChange.Higher:
                    return true;
                case KeyboardChange.Lower:
                    return true;
                case KeyboardChange.BothHigherAndLower:
                    return true;
                case KeyboardChange.NonePressed:
                    return false;
                default:
                    return false;
            }
        }
    }
}