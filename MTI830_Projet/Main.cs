using MTI830_Projet.DTO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MTI830_Projet
{
    public static class Main
    {
        public static void Run()
        {
            new Thread(AnalyseWord).Start();
        }

        public static async void AnalyseWord()
        {
            string initialWord = "matter";

            Stopwatch sw = new Stopwatch();
            sw.Start();
            // Get initial entry
            EntryDTO entry = await APIManager.GetWordEntry(initialWord);
            WordNode root = new WordNode(entry);
            List<string> tested = new List<string>{ initialWord };
            GetWordTree(root);
            var a = root.Traverse().Select(n => n.Entry.Word);
            sw.Stop();
            var time = sw.Elapsed;
        }

        private static WordNode GetWordTree(WordNode initial, int maxDepth = int.MaxValue)
        {
            if (initial.Depth == maxDepth)
                return initial;

            Queue<WordNode> nodes = new Queue<WordNode>();
            nodes.Enqueue(initial);

            HashSet<string> tested = new HashSet<string>() { initial.Entry.Word };

            while (nodes.Any())
            {
                WordNode node = nodes.Dequeue();
                if (node.Depth == maxDepth) continue;

                // Get definitions
                List<string> defs = node.Entry.GetNounDefinitions();

                foreach (string def in defs)
                {
                    var definingWords = def.GetLemmatizedNouns(tested).GetAwaiter().GetResult().OfType<EntryDTO>();
                    List<WordNode> definingWordsNodes = definingWords.Select(e => new WordNode(e)).ToList();

                    if (definingWordsNodes.Any())
                    {
                        node.AddChildren(definingWordsNodes);
                        foreach (var defNode in definingWordsNodes)
                        {
                            tested.Add(defNode.Entry.Word);
                            nodes.Enqueue(defNode);
                        }
                    }
                }
            }

            return initial;
        }
    }
}
