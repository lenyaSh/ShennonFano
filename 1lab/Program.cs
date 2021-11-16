using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace _1lab {
    class Program {

        static (Dictionary<byte, double>, int) GetBytesFromFile(string path, Dictionary<byte, double> dict) {
            List<byte> b = new List<byte>();
            int temp;
            char symbol;

            using (FileStream fstream = new FileStream(path, FileMode.OpenOrCreate)) {
                fstream.Seek(0, SeekOrigin.Begin);

                temp = fstream.ReadByte();
                symbol = (char)temp;
                while (temp != -1) {
                    b.Add((byte)temp);
                    temp = fstream.ReadByte();
                    symbol = (char)temp;
                }
            }

            foreach (byte elem in b) {
                if (dict.ContainsKey(elem)) {
                    dict[elem]++;
                }
                else {
                    dict.Add(elem, 1);
                }
            }

            return (dict, b.Count);
        }
        static (Dictionary<byte, double> probability, double entropy, int sizeFile) GetEntropy(string pathToFile) {
            Dictionary<byte, double> result = new Dictionary<byte, double>(), temp = new Dictionary<byte, double>();

            int sizeFileInBytes;

            (result, sizeFileInBytes) = GetBytesFromFile(pathToFile, result);

            double probability;
            double entr = 0;

            foreach (KeyValuePair<byte, double> elem in result) {
                probability = elem.Value / sizeFileInBytes;
                entr -= probability * Math.Log2(probability);

                temp.Add(elem.Key, probability);
            }

            return (temp, entr, sizeFileInBytes);
        }

        /// <summary>
        /// Алгоритм Шеннона Фано
        /// </summary>
        /// <param name="probability">Вероятность встретить байтовый символ</param>
        /// <param name="leftIndex">Левый индекс для формирования кода</param>
        /// <param name="rightIndex">Правый индекс для формирования кода</param>
        /// <param name="code">Формирует код для конкретного байта</param>
        /// <returns>Словарь байт файла - значение</returns>
        static Dictionary<byte, string> ShennonFano(Dictionary<byte, double> probability, int leftIndex, int rightIndex, StringBuilder[] code) {
            int averageValueInArray;
            double[] probabilities = probability.Values.ToArray();
            byte[] keys = probability.Keys.ToArray();
            Dictionary<byte, string> result = new Dictionary<byte, string>();

            if (leftIndex < rightIndex) {
                averageValueInArray = GetMiddleIndex(leftIndex, rightIndex, probabilities);

                for (int i = leftIndex; i <= rightIndex; i++) {
                    code[i].Append(i > averageValueInArray ? '0' : '1');
                }

                ShennonFano(probability, leftIndex, averageValueInArray, code);
                ShennonFano(probability, averageValueInArray + 1, rightIndex, code);

                //формирование словаря байт - битовый код в строке
                for (int i = 0; i < code.Length; i++) {
                    if (result.ContainsKey(keys[i])) {
                        result[keys[i]] = code[i].ToString();
                    }
                    else {
                        result.Add(keys[i], code[i].ToString());
                    }
                }
            }
            else if (code.Length == 1) {
                code[0].Append("0");
            }

            return result;
        }

        static int GetMiddleIndex(int leftIndex, int rightIndex, double[] sortedProbability) {
            double sumFromLeftToRightIndex = 0;
            for (int i = leftIndex; i < rightIndex; i++) {
                sumFromLeftToRightIndex += sortedProbability[i];
            }

            double schet2 = sortedProbability[rightIndex];

            while (sumFromLeftToRightIndex >= schet2) {
                rightIndex--;
                sumFromLeftToRightIndex -= sortedProbability[rightIndex];
                schet2 += sortedProbability[rightIndex];
            }

            return rightIndex;
        }

        /// <summary>
        /// Для кодирования используется словарь, в котором ключ - байт, который нужно закодировать, а значение - биты в строке, на которые нужно заменить
        /// </summary>
        /// <param name="pathToFile">Путь до закодированного файла</param>
        /// <param name="keys">Словарь - результат работы алгоритма Шеннона Фано</param>
        static void Encode(string pathToFile, Dictionary<byte, string> keys) {
            string newPath = pathToFile.Split('.')[0] + "_encode" + '.' + pathToFile.Split('.')[1];
            int bitOfFile;

            List<byte> allFile = new List<byte>();
            using (FileStream stream = new FileStream(pathToFile, FileMode.Open)) {
                bitOfFile = stream.ReadByte();

                // читаем и запоминаем весь файл
                while (bitOfFile != -1) {
                    allFile.Add((byte)bitOfFile);
                    bitOfFile = stream.ReadByte();
                }
            }

            BitArray codeShannonFanoForOneBit;
            BitArray resultBitSequence = new BitArray(0);

            // перевели весь файл в последовательность кодов Шеннона
            foreach (var elem in allFile) {
                codeShannonFanoForOneBit = new BitArray(keys[elem].Length);
                codeShannonFanoForOneBit = GetBitArrayFromString(keys[elem]);

                resultBitSequence = AppendToBitArray(resultBitSequence, codeShannonFanoForOneBit);
            }

            // TODO: одна из проблем: дополняя новыми битами, мы добавляем новые символы (добавляя 2 нуля в конец - новый символ)
            // Дополнение до кратности 8
            if (resultBitSequence.Length % 8 != 0) {
                resultBitSequence = AppendToBitArray(resultBitSequence, new BitArray(8 - resultBitSequence.Length % 8));
            }

            using StreamWriter writer = new StreamWriter(newPath);
            byte[] buff = new byte[1];
            BitArray eightBit = new BitArray(8);

            for (int i = 0; i < resultBitSequence.Length - 7; i += 8) {
                // формирование буфера
                for (int j = i; j < i + 8; j++) {
                    eightBit[j % 8] = resultBitSequence[j];
                }

                ReverseBitArray(eightBit).CopyTo(buff, 0);
                writer.Write((char)buff[0]);
            }
        }

        /// <summary>
        /// Перевод из битовой строки в BitArray
        /// </summary>
        /// <param name="bitCode">Битовая строка</param>
        /// <returns>Массив бит, соответствующий введенной строке</returns>
        static BitArray GetBitArrayFromString(string bitCode) {
            BitArray result = new BitArray(bitCode.Length);

            // идем с конца, т.к. это особенность BitArray
            for (int i = 0; i < result.Length; i++) {
                result[i] = bitCode[i] == '1';
            }

            return result;
        }

        /// <summary>
        /// Сложение BitArray'ев
        /// </summary>
        /// <param name="result">Левая часть</param>
        /// <param name="partOfAllCode">Правая часть</param>
        static BitArray AppendToBitArray(BitArray result, BitArray partOfAllCode) {
            if (result.Length == 0) {
                result = new BitArray(partOfAllCode);
            }
            else {
                BitArray temp = new BitArray(result);
                result = new BitArray(result.Length + partOfAllCode.Length);

                // копирование старого значения
                for (int i = 0; i < temp.Length; i++) {
                    result[i] = temp[i];
                }
                // добавление в конец нового
                for (int i = temp.Length; i < result.Length; i++) {
                    result[i] = partOfAllCode[i % temp.Length];
                }
            }

            return result;
        }

        /// <summary>
        /// Метод разворота BitArray для правильного перевода в byte
        /// </summary>
        /// <param name="text">Исходный BitArray</param>
        /// <returns>Перевернутый BitArray</returns>
        static BitArray ReverseBitArray(BitArray text) {
            bool temp;
            for (int i = 0; i < text.Length / 2; i++) {
                temp = text[i];
                text[i] = text[text.Length - i - 1];
                text[text.Length - i - 1] = temp;
            }

            return text;
        }

        static void Decode(string pathToFile, Dictionary<byte, string> keys) {
            string newPath = pathToFile.Split('.')[0] + "_decode" + '.' + pathToFile.Split('.')[1];

            // весь файл в битах
            BitArray code = new BitArray(0);
            byte[] byteFromFile;
            string fileContent;
            using (StreamReader fs = new StreamReader(pathToFile)) {
                fileContent = fs.ReadToEnd();
            }

            foreach(var symb in fileContent) {
                code = AppendToBitArray(code, ReverseBitArray(new BitArray(new byte []{ (byte) symb })));
            }

            foreach(bool elem in code) {
                Console.Write(elem == true ? '1' : '0');
            }
            
            // для кодов
            StringBuilder codeOnString = new StringBuilder(code[0] == true ? '1' : '0');
            int searchedByte;

            using StreamWriter writer = new StreamWriter(newPath);

            for (int i = 1; i < code.Length; i++) {
                searchedByte = GetKeyBasedOnValue(keys, codeOnString.ToString());

                if (searchedByte != -1) {
                    writer.Write((char)searchedByte);
                    codeOnString = new StringBuilder();
                }

                codeOnString.Append(code[i] == true ? '1' : '0');
            }
        }

        static int GetKeyBasedOnValue(Dictionary<byte, string> pairs, string searchedElement) {
            int value = -1;

            foreach (var elem in pairs) {
                if (elem.Value == searchedElement) {
                    return elem.Key;
                }
            }

            return value;
        }

        static void Main() {
            Console.WriteLine("=============Алгоритм Шеннона-Фано=============");

            Dictionary<byte, double> symbols_probability = new Dictionary<byte, double>();
            double entropy;
            int countBytesInFile;
            (symbols_probability, entropy, countBytesInFile) = GetEntropy(@"D:\code.txt");

            // сортировка по убыванию вероятности появления символа
            symbols_probability = symbols_probability.OrderByDescending(pair => pair.Value).ToDictionary(pair => pair.Key, pair => pair.Value);

            Console.WriteLine("1. Вывести вероятности символов? (Да - 1, Нет - 2)");
            if (Console.ReadLine() == "1") {
                Console.WriteLine("Пары значение : вероятность");

                foreach (var elem in symbols_probability) {
                    Console.WriteLine($"{elem.Key} : {elem.Value}");
                }
            }

            // хранит коды для всех байтов внутри словаря
            StringBuilder[] code = new StringBuilder[symbols_probability.Count];
            for (int i = 0; i < code.Length; i++) {
                code[i] = new StringBuilder();
            }

            Console.WriteLine("2. Вывести энтропию исходного файла? (Да - 1, Нет - 2)");
            if (Console.ReadLine() == "1") {
                Console.WriteLine($"Энтропия: {entropy}");
            }

            Dictionary<byte, string> byteCodesPairs = ShennonFano(symbols_probability, 0, symbols_probability.Count - 1, code);

            Console.WriteLine("3. Вывести результат работы алгоритма Шеннона-Фано? (Да - 1, Нет - 2)");
            if (Console.ReadLine() == "1") {
                Console.WriteLine("Пары символ : код символа");

                foreach (var elem in byteCodesPairs) {
                    Console.WriteLine($"{(char)elem.Key} : {elem.Value}");
                }
            }

            Console.WriteLine("Выполняется кодирование...");
            Encode(@"D:\code.txt", byteCodesPairs);
            Console.WriteLine("Готово!");

            (symbols_probability, entropy, countBytesInFile) = GetEntropy(@"D:\code_encode.txt");

            Console.WriteLine("4. Вывести энтропию сжатого файла? (Да - 1, Нет - 2)");
            if (Console.ReadLine() == "1") {
                Console.WriteLine($"Энтропия сжатого файла: {entropy}");
            }

            Console.WriteLine("Выполняется декодирование...");
            Decode(@"D:\code_encode.txt", byteCodesPairs);
            Console.WriteLine("Готово!");

        }
    }
}
