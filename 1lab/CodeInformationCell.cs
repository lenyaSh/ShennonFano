using System;
using System.Collections.Generic;
using System.Text;

namespace _1lab {
    class CodeInformationCell {
        char symbol;
        double frequency;
        String codeWord;
        int lenghtCodeWord;

        public CodeInformationCell() { }

        public CodeInformationCell(char symbol, double frequency, String codeWord, int lenghtCodeWord) {
            this.symbol = symbol;
            this.frequency = frequency;
            this.codeWord = codeWord;
            this.lenghtCodeWord = lenghtCodeWord;
        }

        public override string ToString() {
            String output = String.Format("{0} : {1} :: {2} :: {3};", symbol, frequency, codeWord, lenghtCodeWord);
            return output;
        }

        // функция на вывод
        public static String ListToString(List<CodeInformationCell> list) {
            String output = String.Empty;

            foreach (var o in list)
                output += o.ToString() + Environment.NewLine;

            return output;
        }

        // средняя длина кодового слова
        public static double MedLenghtList(List<CodeInformationCell> list) {
            int sumLenght = 0;

            foreach (var c in list)
                sumLenght += c.lenghtCodeWord;

            double medLenght = (double)sumLenght / list.Count;


            return medLenght;
        }

        public static bool CraftMacmillan(List<CodeInformationCell> list) {
            bool flag = false;

            double v = 0.0;

            foreach (var c in list)
                v += Math.Pow(2, -c.lenghtCodeWord);

            flag = (v <= 1);

            return flag;
        }
    }
}
