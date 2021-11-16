using System;
using System.Collections.Generic;
using static _1lab.FunctionClass;

namespace _1lab {
    class Huff {
    }

    class CellHaffman : IComparable<CellHaffman> {
        public List<char> listSymbols; // лист символов
        public double frequency; // общая вероятность

        public CellHaffman() {
            listSymbols = new List<char>();
        }

        public CellHaffman(List<char> list, double frequency) : this() {
            this.frequency = frequency;
            foreach (var c in list)
                listSymbols.Add(c);
        }

        public static CellHaffman operator +(CellHaffman a, CellHaffman b) {
            List<char> listChar = new List<char>();

            foreach (var c in a.listSymbols)
                listChar.Add(c);

            foreach (var c in b.listSymbols)
                listChar.Add(c);


            double frequency = a.frequency + b.frequency;

            CellHaffman ch = new CellHaffman(listChar, frequency);
            return ch;
        }

        // для сортировки
        public int CompareTo(CellHaffman other) {
            if (this.frequency == other.frequency)
                return 0;
            else if (this.frequency > other.frequency)
                return -1;
            else
                return 1;
        }
    }

    class HaffmanCoding {
        public static List<CodeInformationCell> Code() {
            string answer = string.Empty;

            var sortDict = DictionaryFerquencySort<double>(keyPair => keyPair.Value, SortParametr.Descending);

            List<CellHaffman> listHaffman = new List<CellHaffman>();

            // заполнение структуры
            foreach (var c in sortDict)
                listHaffman.Add(new CellHaffman(new List<char>() { c.Key }, c.Value));

            List<CodeInformationCell> list = ForwardRun(listHaffman, sortDict);

            return list;
        }

        private static List<CodeInformationCell> ForwardRun(List<CellHaffman> list, Dictionary<char, double> d) {

            List<CodeInformationCell> listAnswer = new List<CodeInformationCell>();
            int countIteration = d.Count;
            Dictionary<char, string> dictCode = new Dictionary<char, string>(); // коды символов

            foreach (var c in d)
                dictCode.Add(c.Key, String.Empty);


            for (int i = 2; i <= countIteration; i++) {
                List<CellHaffman> copyList = CopyMethod(list);
                // закрутка
                while (true) {
                    copyList.Sort();
                    // условие выхода
                    if (copyList.Count == i)
                        break;
                    else {

                        //объединение двух записей в одну

                        int ind1 = copyList.Count - 1;
                        int ind2 = copyList.Count - 2;
                        CellHaffman ch = copyList[ind1] + copyList[ind2];

                        copyList.RemoveAt(ind1);
                        copyList.RemoveAt(ind2);

                        copyList.Add(ch);

                    }
                }
                // расставление кодов

                int index1 = copyList.Count - 1; // к этим добавить 1
                int index2 = copyList.Count - 2; // к этим добавить 0


                CellHaffman up = copyList[index2];
                CellHaffman down = copyList[index1];

                foreach (var c in up.listSymbols)
                    dictCode[c] += "0";
                foreach (var c in down.listSymbols)
                    dictCode[c] += "1";
            }

            foreach (var c in d)
                listAnswer.Add(new CodeInformationCell(c.Key, c.Value, dictCode[c.Key], dictCode[c.Key].Length));

            return listAnswer;
        }

        private static List<CellHaffman> CopyMethod(List<CellHaffman> listCH) {
            List<CellHaffman> listCopy = new List<CellHaffman>();

            foreach (var c in listCH) {
                List<char> listChars = new List<char>();

                foreach (var s in c.listSymbols)
                    listChars.Add(s);

                listCopy.Add(new CellHaffman(listChars, c.frequency));
            }

            return listCopy;
        }

        public static string InformationsCode(List<CodeInformationCell> list) {
            string answer = String.Empty;
            answer += Environment.NewLine + "Энтропия = " + Entropy().ToString();
            answer += Environment.NewLine + "Средняя длина = " + CodeInformationCell.MedLenghtList(list);
            answer += Environment.NewLine + "неравенство Крафта-Макмиллана : " + CodeInformationCell.CraftMacmillan(list);
            return answer;
        }

    }
}
