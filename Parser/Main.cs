using edu.stanford.nlp.ling;
using edu.stanford.nlp.parser.lexparser;
using edu.stanford.nlp.process;
using edu.stanford.nlp.trees;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Console = System.Console;

namespace Parser
{
    public static class Main
    {
        private readonly static string[] nounLabels = new string[] { "NN", "NNS" };

        public static List<string> ExtractNounsFromSemantics(string sentence)
        {
            string assemblyPath = Assembly.GetExecutingAssembly().GetName().CodeBase;
            string projectPath = Directory.GetParent(new Uri(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(assemblyPath)))).LocalPath).FullName;
            string modelsDirectory = Path.GetFullPath(projectPath + @"\Parser\CoreNLP-3.9.1-Models\edu\stanford\nlp\models");

            // Loading english PCFG parser from file
            LexicalizedParser lp = LexicalizedParser.loadModel(modelsDirectory + @"\lexparser\englishPCFG.ser.gz");

            // This shows loading and using an explicit tokenizer
            var tokenizerFactory = PTBTokenizer.factory(new CoreLabelTokenFactory(), "");
            var sent2Reader = new java.io.StringReader(sentence);
            var rawWords = tokenizerFactory.getTokenizer(sent2Reader).tokenize();
            sent2Reader.close();
            var tree = lp.apply(rawWords);
            return tree.toArray().Cast<LabeledScoredTreeNode>().Where(n => n.isLeaf() && nounLabels.Contains(n.parent(tree).label().value())).Select(n => n.label().ToString()).ToList();
        }
    }
}
