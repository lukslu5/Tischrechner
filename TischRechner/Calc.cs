using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TischRechner
{
    public class Calc
    {
        public List<double> numbers = new List<double>();
        public List<char> operators = new List<char>();
        public double solution;
        public int indexPos;
        public void AddNumber(string number, string modifier)
        {
            numbers.Add(double.Parse(number) * (modifier == "+" ? 1 : -1));
        }
        public void AddOperator(string handler)
        {
            operators.Add(char.Parse(handler));
        }
        public void Calculate(int indexPos)
        {
            List<double> tempNumbers = new List<double>(numbers);
            List<char> tempOperators = new List<char>(operators);

            // Sonderzeichenrechnung
            for (int i = 0; i < tempOperators.Count; i++)
            {
                if (tempOperators[i] == '^' || tempOperators[i] == '√')
                {
                    if (tempOperators[i] == '^')
                        tempNumbers[i] = Math.Pow(tempNumbers[i], tempNumbers[i + 1]);
                    else // '√'
                        tempNumbers[i] = Math.Pow(tempNumbers[i+1], 1 / tempNumbers[i]);

                    tempNumbers.RemoveAt(i + 1);
                    tempOperators.RemoveAt(i);

                    i--;
                }
            }
            // Punktrechnung
            for (int i = 0;i < tempOperators.Count; i++)
            {
                if (tempOperators[i] == 'x' || tempOperators[i] == '/')
                {
                    if (tempOperators[i] == 'x')
                        tempNumbers[i] = tempNumbers[i] * tempNumbers[i + 1];
                    else // '/'
                        tempNumbers[i] = tempNumbers[i] / tempNumbers[i + 1];

                    tempNumbers.RemoveAt(i + 1);
                    tempOperators.RemoveAt(i);

                    i--;
                }
            }
            // Strichrechnung
            for (int i = 0; i < tempOperators.Count; i++)
            {
                if (tempOperators[i] == '+' || tempOperators[i] == '-')
                {
                    if (tempOperators[i] == '+')
                        tempNumbers[i] = tempNumbers[i] + tempNumbers[i + 1];
                    else // '-'
                        tempNumbers[i] = tempNumbers[i] - tempNumbers[i + 1];

                    tempNumbers.RemoveAt(i + 1);
                    tempOperators.RemoveAt(i);

                    i--;
                }
            }
            solution = tempNumbers[0];
            this.indexPos = indexPos;
        }
    }
}