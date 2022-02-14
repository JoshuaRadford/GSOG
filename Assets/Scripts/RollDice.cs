using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DiceRoll
{
    public struct DiceRollResult
    {
        public int[] rolls;
        public int total;
    }
    public class RollDice
    {
        public DiceRollResult Roll(int numDice, int numSides)
        {
            int total = 0;
            int[] rolls = new int[numDice];

            for (int i = 0; i < rolls.Length; i++)
            {
                int result = Random.Range(1, numSides + 1);
                rolls[i] = result;
                total += result;
            }

            DiceRollResult diceRollResult = new DiceRollResult();
            diceRollResult.rolls = rolls;
            diceRollResult.total = total;

            return diceRollResult;
        }
    }
}