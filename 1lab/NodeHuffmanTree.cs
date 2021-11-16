using System.Collections.Generic;

namespace _1lab {
    public class NodeHuffmanTree {

        public byte Symbol { get; set; }
        public string Code { get; set; }
        public double Frequency { get; set; }
        public NodeHuffmanTree Right { get; set; }
        public NodeHuffmanTree Left { get; set; }

        public List<bool> Traverse(byte symbol, List<bool> data) {
            if (Right == null && Left == null) {
                return symbol.Equals(Symbol) ? data : null;
            }
            else {
                List<bool> left = null;
                List<bool> right = null;

                if (Left != null) {
                    List<bool> leftPath = new List<bool>();
                    leftPath.AddRange(data);
                    leftPath.Add(false);

                    left = Left.Traverse(symbol, leftPath);
                }

                if (Right != null) {
                    List<bool> rightPath = new List<bool>();
                    rightPath.AddRange(data);
                    rightPath.Add(true);
                    right = Right.Traverse(symbol, rightPath);
                }

                return left ?? right;
            }
        }
    }
}
