using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MTI830_Projet
{
    public static class Utilities
    {
        public static async Task<IEnumerable<T1>> SelectManyAsync<T, T1>(this IEnumerable<T> enumeration, Func<T, Task<IEnumerable<T1>>> func)
        {
            return (await Task.WhenAll(enumeration.Select(func))).SelectMany(s => s);
        }

        public static void ToGephiCSV(this WordNode node, string filenameNodes, string filenameEdges, bool fromRoot = true)
        {
            WordNode initialNode = fromRoot ? node.GetRoot() : node;
            string nodecsv = "Id,Label,Size\n";
            string edgecsv = "Source,Target\n";

            initialNode.Traverse().Where(n => n.Parent != null).OrderBy(n => n.Depth).ToList().AssignIds().ForEach(n =>
            {
                nodecsv = string.Concat(nodecsv, n.Id, ",", n.Entry.Word, ",", n.Depth, "\n");
                edgecsv = string.Concat(edgecsv, n.Parent.Id, ",", n.Id, "\n");
            });

            using (StreamWriter sw = new StreamWriter(filenameNodes))
            {
                sw.Write(nodecsv);
            }

            using (StreamWriter sw = new StreamWriter(filenameEdges))
            {
                sw.Write(edgecsv);
            }
        }

        private static List<WordNode> AssignIds(this List<WordNode> tree)
        {
            for (int i = 0; i < tree.Count; i++)
                tree[i].AssignId(i);

            return tree;
        }
    }
}
