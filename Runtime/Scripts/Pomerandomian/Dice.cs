using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Pomerandomian {
    public class Dice {
        /// <summary>
        /// Number of dice to roll.
        /// </summary>
        public readonly int Count;
        /// <summary>
        /// Number of sides on the dice.
        /// </summary>
        public readonly int Sides;
        /// <summary>
        /// Type of the roll: advantage (take the higher of two), disadvantage (lower of two), standard (none)
        /// </summary>
        public readonly RollType Type;
        /// <summary>
        /// Flat value to add to the roll.
        /// </summary>
        public readonly int Modifier;

        public enum RollType {
            Standard,
            Advantage,
            Disadvantage
        }

        public Dice(int count, int sides, RollType type = RollType.Standard, int modifier = 0) {
            Count = count;
            Sides = sides;
            Type = type;
            Modifier = modifier;
        }

        private int IndividualRoll(IRandom random) {
            return random.Next(0, Sides) + 1;
        }

        public int Roll(IRandom random) {
            int result = 0;
            for (int i = 0; i < Count; i++) {
                int thisRoll = random.Next(0, Sides) + 1;
                switch (Type) {
                    case RollType.Advantage:
                        thisRoll = Math.Max(thisRoll, random.Next(0, Sides) + 1);
                        break;
                    case RollType.Disadvantage:
                        thisRoll = Math.Min(thisRoll, random.Next(0, Sides) + 1);
                        break;
                }
                result += thisRoll;
            }
            return result + Modifier;
        }

        public DiceResult RollDetailed(IRandom random) {
            SingleRollResult[] rollResults = new SingleRollResult[Count];
            for (int i = 0; i < Count; i++) {
                rollResults[i] = SubRoll(this, random);
            }

            return new DiceResult(this, rollResults);
        }

        private SingleRollResult SubRoll(Dice dice, IRandom random) {
            if (Type == RollType.Standard) {
                int roll = IndividualRoll(random);
                return new SingleRollResult(roll, roll);
            }
            int roll1 = IndividualRoll(random);
            int roll2 = IndividualRoll(random);
            if (Type == RollType.Advantage) {
                return new SingleRollResult(Math.Max(roll1, roll2), roll1, roll2);
            } else {
                return new SingleRollResult(Math.Min(roll1, roll2), roll1, roll2);
            }
        }

        public int MinRoll {
            get => Count + Modifier;
        }

        public int MaxRoll {
            get => Count * Sides + Modifier;
        }

        public static bool TryParse(string input, out Dice dice, Dice defaultValue = null) {
            Dice parsed = FromString(input);
            dice = parsed ?? defaultValue;
            return parsed != null;
        }

        public static Dice FromString(string input) {
            input = input.ToLower().Trim();
            input = Regex.Replace(input, @"\s+", "");
            Regex regex = new Regex(@"^(?<numDice>\d+)d(?<sides>\d+)(?<type>[AHDL]?)(?<modifier>[+-]\d+)?$");
            Match match = regex.Match(input);
            if (!match.Success) {
                return null;
            }

            Group diceGroup = match.Groups["numDice"];
            Group sidesGroup = match.Groups["sides"];
            Group dropGroup = match.Groups["drop"];
            Group modifierGroup = match.Groups["modifier"];

            if (!int.TryParse(diceGroup.Value, out int numDice)) {
                return null;
            }
            if (!int.TryParse(sidesGroup.Value, out int sides)) {
                return null;
            }
            RollType type = RollType.Standard;
            if (dropGroup.Success && dropGroup.Value.Length > 0) {
                switch (dropGroup.Value) {
                    case "A":
                    case "H":
                        type = RollType.Advantage;
                        break;
                    case "D":
                    case "L":
                        type = RollType.Disadvantage;
                        break;
                    default:
                        return null;
                }
            }
            bool isDisadvantage = match.Groups[3].Value == "L";
            int modifier = 0;
            if (modifierGroup.Success && modifierGroup.Length > 0) {
                if (!int.TryParse(modifierGroup.Value, out modifier)) {
                    return null;
                }
            }
            return new Dice(numDice, sides, type, modifier);
        }
    }

    public class DiceResult {
        public readonly Dice Dice;
        public readonly SingleRollResult[] Rolls;
        public readonly int FinalResult;

        public DiceResult(Dice source, params SingleRollResult[] rolls) {
            Dice = source;
            Rolls = rolls;
            FinalResult = rolls.Sum(x => x.SubResult) + Dice.Modifier;
        }

        public override string ToString() {
            string rollsString = Rolls.Length == 1 ? Rolls[0].ToString() : "( " + string.Join(" + ", Rolls.Select(roll => roll.ToString())) + " )";
            if (Dice.Modifier > 0) return $"{rollsString} + {Dice.Modifier}";
            else return rollsString;
        }
    }

    public class SingleRollResult {
        public readonly int[] Rolls;
        public readonly int SubResult;

        public SingleRollResult(int result, params int[] rolls) {
            Rolls = rolls;
            SubResult = result;
        }

        public override string ToString() {
            if (Rolls.Length == 1) return SubResult.ToString();
            else return $"({string.Join(", ", Rolls.Select(roll => roll == SubResult ? $"[{roll}]" : roll.ToString()))})";
        }
    }
}