using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KalML
{
    public class Network
    {

        Random random { get; }
        private Layer[] layers;
        private int[] sizes;
        public Network(int[] sizes)
        {
            random = new Random();
            this.sizes = sizes;
            layers = new Layer[sizes.Length];
            layers[0] = new Layer(sizes[0], 1);
            for (int i = 1; i < sizes.Length; i++)
            {
                layers[i] = new Layer(sizes[i], sizes[i-1]);
            }
        }
        private Network() { }//убрал возможность вызова пустого конструктора
        public Network CopyWithChanges()//я знаю про BackPropagation но ради эксперимента хочу сделать так
        {
            Network NewNetwork = new Network(sizes);

            for (int i = 1; i < layers.Length; i++)
            {
                for (int j = 0; j < layers[i].nodes.Length; j++)
                {
                    this.layers[i].nodes[j].Weights.CopyTo(NewNetwork.layers[i].nodes[j].Weights,0);
                }
            }
            for (int i = 0; i < 100; i++)
            {
                int x = 0;
                int y = 0;
                NewNetwork.layers[x = random.Next() % NewNetwork.layers.Length]
                    .nodes[y = random.Next() % NewNetwork.layers[x].nodes.Length]
                    .Weights[random.Next() % NewNetwork.layers[x].nodes[y].Weights.Length] += 0.25 - random.NextDouble()*0.5;
            }
            if (random.Next() % 5 == 0)
            {
                NewNetwork.layers[random.Next() % NewNetwork.layers.Length].Bias += 2 - random.NextDouble() * 4;
            }

            return NewNetwork;

        }

        public void FillRandomWeights()
        {
            Random random = new Random();
            for (int i = 1; i < layers.Length; i++)
            {
                for (int j = 0; j < layers[i].nodes.Length; j++)
                {
                    for (int z = 0; z < layers[i-1].nodes.Length; z++)
                    {
                        layers[i].nodes[j].Weights[z] = 4.0 - (random.NextDouble() * 8.0);
                    }
                }
            }
        }

        private double CalculateValue(Layer prewLayer, double[] Weights)
        {
            double value = 0;

            for (int i = 0; i < prewLayer.nodes.Length; i++)
            {
                value += Weights[i] * prewLayer.nodes[i].Value;
            }
            value += prewLayer.Bias;
            value = 1/(1+Math.Pow(Math.E, -value));//пенисоида
            if (value == double.PositiveInfinity )
            {
                value = 0;
            }
            else if (value == double.NegativeInfinity)
            {
                value = 1;
            }
            return value;
        }

        public Node[] Calculate(double[] Input)
        {
            for (int i = 0; i < layers[0].nodes.Length; i++)
            {
                if (Input.Length > i)
                {
                    layers[0].nodes[i].Value = Input[i];
                }
                else
                {
                    layers[0].nodes[i].Value = 0;
                }
            }

            for (int i = 1; i < layers.Length; i++)
            {
                for (int j = 0; j < layers[i].nodes.Length; j++)
                {
                    layers[i].nodes[j].Value = CalculateValue(layers[i-1], layers[i].nodes[j].Weights);
                }
            }

            return layers[layers.Length - 1].nodes;
        }
            
    }

    public struct Layer
    {
        public Node[] nodes;
        public double Bias;
        public Layer(int size, int prewLayerSize)
        {
            Bias = 0;
            nodes = new Node[size];
            Random random = new Random();
            for(int i = 0; i < size; i++)
            {
                nodes[i] = new Node(prewLayerSize);
            }
        }
    }

    public struct Node
    {
        public double Value;
        public double[] Weights;
        public Node(int size)
        {
            this.Value = 0;
            this.Weights = new double[size];
            for(int i = 0; i < size; i++)
            {
                Weights[i] = 1;
            }
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
