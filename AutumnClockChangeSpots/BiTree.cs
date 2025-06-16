using System;

namespace AutumnClockChangeSpots
{
    public class BiTree

    {
        /// <summary>
        /// for use with sql db just save hash as bigint to ref into a dictionary
        /// </summary>
        public Int64 LongHash { get; set; }
        public Char NodeValue { get; set; }
        //true false nodes
        public BiTree IsTrue { get; set; } = null;
        public BiTree IsFalse { get; set; } = null;
        public BiTree()
        {
        }
        public BiTree(char val)
        {
            NodeValue = val;
        }

        public void HashInsert(string newword, long IntHash)
        //public void HashInsert(string newword, string TimeStr)
        {

            BiTree node = this;

            foreach (char c in newword)
            {
                //make new node as required
                if (c == '1' && node.IsTrue == null)
                {
                    BiTree newnode = new BiTree(c);
                    node.IsTrue = newnode;

                }
                if (c == '0' && node.IsFalse == null)
                {
                    BiTree newnode = new BiTree(c);
                    node.IsFalse = newnode;
                }
                if (c == '1')
                    node = node.IsTrue;
                else
                    node = node.IsFalse;
            }
            node.LongHash = IntHash;
        }
    }
}