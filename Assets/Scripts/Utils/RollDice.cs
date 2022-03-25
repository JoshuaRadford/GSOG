using UnityEngine;

namespace DiceRoll {
    public struct DiceRollResult {
        public int[] rolls;
        public int total;
    }
    public class RollDice {
        public void Roll(int numDice, int numSides, out int[] rolls, out int total) {
            total = 0;
            rolls = new int[numDice];

            for (int i = 0; i < rolls.Length; i++) {
                int result = Random.Range(1, numSides + 1);
                rolls[i] = result;
                total += result;
            }
        }
    }
}