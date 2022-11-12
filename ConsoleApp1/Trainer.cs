using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Accord.DataSets;

namespace KalML
{
    internal class Trainer
    {

        MNIST mNIST = new MNIST();
        private double[] compresImage(double[] input)
        {
            double[] res = new double[49];
            for (int x = 0; x < 7; x++)
            {
                for (int y = 0; y < 7; y++)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        for (int j = 0; j < 4; j++)
                        {
                            if (((x * 4 + i) + 28 * (y * 4 + j))<input.Length)
                            {
                                res[x + (y * 7)] += input[(x * 4 + i) + 28 * (y * 4 + j)];
                            }
                        }
                    }
                    res[x + (y * 7)] /= 16;
                }
            }
            /*
            for (int i = 0; i < 7; i++)
            {
                for (int j = 0; j < 7; j++)
                {
                    if (res[j*7 + i]>0.5)
                    {
                        Console.Write(" ");
                    }
                    else
                    {
                        Console.Write("0");
                    }
                }
                Console.Write("\n");
            }
            Console.Write("\n");
            */
            return res;
        }
        public struct AnswerStruct
        {
            public bool value;
            public int sampleNumber;
            public int rightAnswer;
            public int resivedAnswer;
            public int answCount;
            public override string ToString()
            {
                if (answCount != 1)
                {
                    return "Всё переплетено но не предопределенно: " + answCount + " Правильный ответ: " + rightAnswer + "Ответ: " + value;
                }
                else
                {
                    return "Ответ: " + value + " Семпл: " + sampleNumber + " Правильный ответ: " + rightAnswer + " Полученный ответ " + resivedAnswer;
                }
            }
        }

        public struct ResultStruct
        {
            public double RightAnswersPercent;
            public double Score;

            public override string ToString()
            {
                return "Колво правильных ответов: " + RightAnswersPercent * 100.0 + " Счёт: " + Score;
            }
        }

        public AnswerStruct Test(int SampleNumber, Network NewNetwork)
        {
            double[] a = compresImage(mNIST.Training.Item1[SampleNumber].ToDense());
            Node[] Nodes = NewNetwork.Calculate(a);
            int Bigest = -1;
            int count = 0;

            for (int k = 0; k < Nodes.Length; k++)
            {
                if (Nodes[k].Value > 0.8)
                {
                    count++;
                    Bigest = k;
                }
            }
            AnswerStruct answer = new AnswerStruct();
            answer.resivedAnswer = -1;
            answer.value = false;
            answer.sampleNumber = SampleNumber;
            answer.rightAnswer = (int)mNIST.Training.Item2[SampleNumber];
            if (mNIST.Training.Item2[SampleNumber] == Bigest)
            {
                answer.resivedAnswer = Bigest;
                answer.value = true;
            }
            answer.answCount = count;
            return answer;
        }
        public void test()
        {
            Network BestNetwork = null;
            Tuple<Network, ResultStruct>[] NetworksAndRess = new Tuple<Network, ResultStruct>[50];
            for(int i = 0; i < NetworksAndRess.Length; i++)
            {
                NetworksAndRess[i] = new Tuple<Network, ResultStruct>(new Network(new int[] { 49, 25, 10 }),new ResultStruct());
                NetworksAndRess[i].Item1.FillRandomWeights();
            }
            int sampleNabor = 0;
            int SamplesAmount = 1000;
            bool IsFirstGen = true;
            while (true)
            {
                for (int i = 0; i < NetworksAndRess.Length; i++)
                {
                    Network NewNetwork;
                    if(IsFirstGen)
                    {
                        NewNetwork = NetworksAndRess[i].Item1;
                    }
                    else if (i == 0)
                    {
                        NewNetwork = BestNetwork;
                    }
                    else
                    {
                        NewNetwork = BestNetwork.CopyWithChanges();
                    }
                    
                    double Results = 0;
                    double Score = 0;

                    Parallel.For(sampleNabor * SamplesAmount, (sampleNabor + 1) * SamplesAmount, (z) =>
                    {
                        var temp = Test(z, NewNetwork);
                        if (temp.value)
                        {
                            //Console.WriteLine(item.Result);
                            Results++;
                            Score += 1.0 / temp.answCount;
                        }
                    });

                    Results = Results / SamplesAmount;
                    ResultStruct resultStruct = new ResultStruct();
                    resultStruct.RightAnswersPercent = Results;
                    resultStruct.Score = Score;
                    NetworksAndRess[i] = new Tuple<Network, ResultStruct>(NewNetwork, resultStruct);
                }
                IsFirstGen = false;
                Tuple<Network, ResultStruct> Best = new Tuple<Network, ResultStruct>(NetworksAndRess[0].Item1, NetworksAndRess[0].Item2);

                foreach (var item in NetworksAndRess)
                {
                    if (item.Item2.Score > Best.Item2.Score)
                    {
                        Best = item;
                    }
                }
                BestNetwork = Best.Item1;
                Console.WriteLine(Best.Item2);
                sampleNabor++;
                if (sampleNabor > (mNIST.Training.Item1.Length/SamplesAmount)-2)
                {
                    sampleNabor = 0;
                }
            }
        }
    }
}
