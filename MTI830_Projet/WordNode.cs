using MTI830_Projet.DTO;
using System.Collections.Generic;
using System.Linq;

namespace MTI830_Projet
{
    public class WordNode
    {
        private readonly bool uniqueChildren;
        private List<WordNode> children;

        public int Id { get; private set; }
        public EntryDTO Entry { get; }
        public WordNode Parent { get; private set; }
        public List<WordNode> Children {
            get { return children ?? new List<WordNode>(); }
            set {
                if (uniqueChildren)
                    this.children = value ?? new List<WordNode>();//this.NotInTree(value).ToList();
                else
                    this.children = value ?? new List<WordNode>();
                value.ForEach(c => InitChild(c));
            }
        }
        public int Depth { get; private set; }

        public WordNode(EntryDTO entry, bool uniqueChildren = true)
        {
            this.Id = -1;
            this.uniqueChildren = uniqueChildren;
            this.Entry = entry;
            this.Parent = null;
            this.children = new List<WordNode>();
            this.Depth = 1;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is WordNode node) || Entry == null || node.Entry == null)
                return false;

            return Entry.Word == node.Entry.Word;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public WordNode GetRoot()
        {
            WordNode node = this;
            while (node.Parent != null)
                node = node.Parent;
            return node;
        }

        public IEnumerable<WordNode> Traverse()
        {
            var stack = new Stack<WordNode>();
            stack.Push(this);
            while (stack.Count > 0)
            {
                var current = stack.Pop();
                yield return current;
                foreach (var child in current.Children)
                    stack.Push(child);
            }
        }

        public bool Contains(string word, bool fromRoot = false)
        {
            var stack = new Stack<WordNode>();
            stack.Push(fromRoot ? GetRoot() : this);
            while (stack.Count > 0)
            {
                var current = stack.Pop();
                if (current.Entry.Word == word)
                    return true;
                foreach (var child in current.Children)
                    stack.Push(child);
            }
            return false;
        }

        public IEnumerable<WordNode> NotInTree(IEnumerable<WordNode> words, bool fromRoot = false)
        {
            return words.Except((fromRoot ? GetRoot() : this).Traverse().Intersect(words));
        }

        public void AddChildren(IEnumerable<WordNode> children)
        {
            this.Children.AddRange(children);
            foreach (WordNode child in children)
                InitChild(child);
        }

        private void InitChild(WordNode child)
        {
            child.Parent = this;
            child.Depth = this.Depth + 1;
        }

        public void AssignId(int id)
        {
            this.Id = id;
        }
    }
}
