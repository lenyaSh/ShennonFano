using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace _1lab {
    public class HuffmanTree {
        private List<NodeHuffmanTree> nodes = new List<NodeHuffmanTree>();
        public NodeHuffmanTree Root { get; set; }
        public Dictionary<byte, double> Frequencies = new Dictionary<byte, double>();

        public void Build(Dictionary<byte, double> freq) {
            Frequencies = freq;

            foreach (KeyValuePair<byte, double> symbol in Frequencies) {
                nodes.Add(new NodeHuffmanTree() { Symbol = symbol.Key, Frequency = symbol.Value });
            }

            while (nodes.Count > 1) {
                List<NodeHuffmanTree> orderedNodes = nodes.OrderBy(node => node.Frequency).ToList();

                if (orderedNodes.Count >= 2) {
                    List<NodeHuffmanTree> taken = orderedNodes.Take(2).ToList();

                    // Create a parent node by combining the frequencies
                    NodeHuffmanTree parent = new NodeHuffmanTree() {
                        Symbol = (byte)'*',
                        Frequency = taken[0].Frequency + taken[1].Frequency,
                        Left = taken[0],
                        Right = taken[1]
                    };

                    nodes.Remove(taken[0]);
                    nodes.Remove(taken[1]);
                    nodes.Add(parent);
                }

                Root = nodes.FirstOrDefault();
            }
        }

        public BitArray Encode(byte[] source) {
            List<bool> encodedSource = new List<bool>();

            for (int i = 0; i < source.Length; i++) {
                List<bool> encodedSymbol = Root.Traverse(source[i], new List<bool>());
                encodedSource.AddRange(encodedSymbol);
            }

            BitArray bits = new BitArray(encodedSource.ToArray());

            return bits;
        }

        public string Decode(BitArray bits) {
            NodeHuffmanTree current = this.Root;
            string decoded = "";

            foreach (bool bit in bits) {
                if (bit) {
                    if (current.Right != null) {
                        current = current.Right;
                    }
                }
                else {
                    if (current.Left != null) {
                        current = current.Left;
                    }
                }

                if (IsLeaf(current)) {
                    decoded += current.Symbol;
                    current = this.Root;
                }
            }

            return decoded;
        }

        public List<NodeHuffmanTree> GetNodes() {
            return nodes;
        }

        public bool IsLeaf(NodeHuffmanTree node) {
            return (node.Left == null && node.Right == null);
        }

    }
}
