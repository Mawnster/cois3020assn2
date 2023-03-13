
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrieRWayTree
{

    public interface IContainer<T>
    {
        void MakeEmpty();
        bool Empty();
        int Size();
    }

    //-------------------------------------------------------------------------

    public interface ITrie<T> : IContainer<T>
    {
        bool Insert(string key, T value);
        bool Remove(string key);
        T Value(string key);
    }

    //-------------------------------------------------------------------------

    class Trie<T> : ITrie<T>
    {
        private Node root;          // Root node of the Trie

        private class Node
        {
            public T value;         // Value associated with a key; otherwise default
            public int numValues;   // Number of descendent values of a Node 
            public Node[] child;    // Branch for each letter 'a' .. 'z'

            // Node
            // Creates an empty Node
            // All children are set to null by default
            // Time complexity:  O(1)

            public Node()
            {
                value = default(T);
                numValues = 0;
                child = new Node[26];
            }
        }

        // Trie
        // Creates an empty Trie
        // Time complexity:  O(1)

        public Trie()
        {
            MakeEmpty();
        }

        // Public Insert
        // Calls the private Insert which carries out the actual insertion
        // Returns true if successful; false otherwise

        public bool Insert(string key, T value)
        {
            return Insert(root, key, 0, value);
        }

        // Private Insert
        // Inserts the key/value pair into the Trie
        // Returns true if the insertion was successful; false otherwise
        // Note: Duplicate keys are ignored
        // Time complexity:  O(L) where L is the length of the key

        private bool Insert(Node p, string key, int j, T value)
        {
            int i;

            if (j == key.Length)
            {
                if (p.value.Equals(default(T)))
                {
                    // Sets the value at the Node
                    p.value = value;
                    p.numValues++;
                    return true;
                }
                // Duplicate keys are ignored (unsuccessful insertion)
                else
                    return false;
            }
            else
            {
                // Maps a character to an index
                i = Char.ToLower(key[j]) - 'a';

                // Creates a new Node if the link is null
                // Note: Node is initialized to the default value
                if (p.child[i] == null)
                    p.child[i] = new Node();

                // If the inseration is successful
                if (Insert(p.child[i], key, j + 1, value))
                {
                    // Increase number of descendant values by one
                    p.numValues++;
                    return true;
                }
                else
                    return false;
            }
        }

        // Value
        // Returns the value associated with a key; otherwise default
        // Time complexity:  O(min(L,M)) where L is the length of the given key and
        //                                     M is the maximum length of a key in the trie

        public T Value(string key)
        {
            int i;
            Node p = root;

            // Traverses the links character by character
            foreach (char ch in key)
            {
                i = Char.ToLower(ch) - 'a';
                if (p.child[i] == null)
                    return default(T);    // Key is too long
                else
                    p = p.child[i];
            }
            return p.value;               // Returns the value or default
        }

        // Public Remove
        // Calls the private Remove that carries out the actual deletion
        // Returns true if successful; false otherwise

        public bool Remove(string key)
        {
            return Remove(root, key, 0);
        }

        // Private Remove
        // Removes the value associated with the given key
        // Time complexity:  O(min(L,M)) where L is the length of the key
        //                               where M is the maximum length of a key in the trie

        private bool Remove(Node p, string key, int j)
        {
            int i;

            // Key not found
            if (p == null)
                return false;

            else if (j == key.Length)
            {
                // Key/value pair found
                if (!p.value.Equals(default(T)))
                {
                    p.value = default(T);
                    p.numValues--;
                    return true;
                }
                // No value with associated key
                else
                    return false;
            }

            else
            {
                i = Char.ToLower(key[j]) - 'a';

                // If the deletion is successful
                if (Remove(p.child[i], key, j + 1))
                {
                    // Decrease number of descendent values by one and
                    // Remove Nodes with no remaining descendents
                    if (p.child[i].numValues == 0)
                        p.child[i] = null;
                    p.numValues--;
                    return true;
                }
                else
                    return false;
            }
        }

        // MakeEmpty
        // Creates an empty Trie
        // Time complexity:  O(1)

        public void MakeEmpty()
        {
            root = new Node();
        }

        // Empty
        // Returns true if the Trie is empty; false otherwise
        // Time complexity:  O(1)

        public bool Empty()
        {
            return root.numValues == 0;
        }

        // Size
        // Returns the number of Trie values
        // Time complexity:  O(1)

        public int Size()
        {
            return root.numValues;
        }

        // Public Print
        // Calls private Print to carry out the actual printing

        public void Print()
        {
            Print(root, "");
        }

        // Private Print
        // Outputs the key/value pairs ordered by keys
        // Time complexity:  O(n) where n is the number of nodes in the trie

        private void Print(Node p, string key)
        {
            int i;

            if (p != null)
            {
                if (!p.value.Equals(default(T)))
                    Console.WriteLine(key + " " + p.value + " " + p.numValues);
                for (i = 0; i < 26; i++)
                    Print(p.child[i], key + (char)(i + 'a'));
            }
        }



        //#####################################       Assnignment 2 algorithms       ##################################################################################



        public List<string> PartialMatch(string pattern)
        {
            // start at the root node
            Node temp = root;
            // empty list to append to
            List<string> keysMatched = new List<string>();
            // counter for the char
            int counter = 0;
            // to store a (potentially) complete word
            string keyValue = "";

            int i = Char.ToLower(pattern[0]) - 'a';
            // if the current character in pattern is the universal character *
            if (pattern[0] == '*')
            {
                // add all possible characters to the current string, and move to the next character
                for (int k = 0; k < 26; k++)
                    MatchFinder(temp.child[k], pattern, counter + 1, keyValue + (char)(k + 'a'));
            }
            else // if not *, add the specific character, and call move to the next character in pattern
                MatchFinder(temp.child[i], pattern, counter + 1, keyValue + (char)(i + 'a'));

            void MatchFinder(Node p, string pattern, int j, string key)
            {
                int i;
                if (p != null) // running condition
                {
                    if (j < pattern.Length)
                    {
                        i = Char.ToLower(pattern[j]) - 'a';
                        // if the current character in pattern is the universal character *
                        if (pattern[j] == '*')
                        {
                            // add all possible characters to the current string, and move to the next character
                            for (int k = 0; k < 26; k++)
                                MatchFinder(p.child[k], pattern, j + 1, key + (char)(k + 'a'));
                        }
                        else // if not *, add the specific character, and call move to the next character in pattern
                            MatchFinder(p.child[i], pattern, j + 1, key + (char)(i + 'a'));
                    }
                    // add only keys that are the same length as the pattern and has an assigned value
                    if ((key.Length == pattern.Length) && (!p.value.Equals(default(T))))
                        keysMatched.Add(key);
                }
            }
            return keysMatched;
        }

        public List<string> Autocomplete(string prefix)
        {
            // start at the root node
            Node temp = root;
            // empty list to append to
            List<string> keysMatched = new List<string>();
            // counter for the char
            int counter = 0;
            // to store a (potentially) complete word
            string keyValue = "";

            int i = Char.ToLower(prefix[0]) - 'a';
            // if the current character in pattern is the universal character *
            if (prefix[0] == '*')
            {
                // add all possible characters to the current string, and move to the next character
                for (int k = 0; k < 26; k++)
                    MatchFinder(temp.child[k], prefix, counter + 1, keyValue + (char)(k + 'a'));
            }
            else // if not *, add the specific character, and call move to the next character in pattern
                MatchFinder(temp.child[i], prefix, counter + 1, keyValue + (char)(i + 'a'));

            void MatchFinder(Node p, string pattern, int j, string key)
            {
                int i;
                if (p != null) // running condition
                {
                    if (j < pattern.Length)
                    {
                        i = Char.ToLower(pattern[j]) - 'a';
                        // if the current character in pattern is the universal character *
                        if (pattern[j] == '*')
                        {
                            // add all possible characters to the current string, and move to the next character
                            for (int k = 0; k < 26; k++)
                                MatchFinder(p.child[k], pattern, j + 1, key + (char)(k + 'a'));
                        }
                        else // if not *, add the specific character, and call move to the next character in pattern
                            MatchFinder(p.child[i], pattern, j + 1, key + (char)(i + 'a'));
                    }
                    // add only keys that are the same length as the pattern and has an assigned value
                    if (key.Length == pattern.Length)
                        FoundWords(p, pattern, key);
                }
            }
            void FoundWords(Node p, string pattern, string key)

            {
                if (p != null)
                {
                    if (!p.value.Equals(default(T)))
                    {
                        keysMatched.Add(key);
                    }
                    else
                    {
                        for (int k = 0; k < 26; k++)
                            FoundWords(p.child[k], pattern, key + (char)(k + 'a'));
                    }
                }
            }
            return keysMatched;
        }

        //################################################### End of Algos ####################################################################

    }

    class Program
    {
        static void Main(string[] args)
        {
            void PrintList(List<string> myList)
            {
                foreach (string item in myList)
                {
                    Console.WriteLine(item);
                }
            }

            Trie<int> T;
            T = new Trie<int>();
            List<string> matchedWords = new List<string>();

            String file = File.ReadAllText(System.IO.Directory.GetCurrentDirectory() + "/words.txt");


            // add every word from the file
            string insertWord = "";
            int keyValue = 10;
            foreach (string line in File.ReadLines("./words.txt"))
            {
                foreach (char ch in line)
                {
                    if (!(ch == ' ' || ch == '\''))
                    {
                        insertWord += ch;
                    }
                }
                T.Insert(insertWord, keyValue);
                keyValue += 10;
                insertWord = "";
            }

            /*
            //Testing for Partial Match
            // test that multiple results can be returned
            Console.WriteLine("testing ar*, return: arm, art");
            matchedWords = T.PartialMatch("ar*");  
            PrintList(matchedWords);

            // test that an exact string doesnt return words that have leading or tailing characters
            Console.WriteLine("testing act, return: act");
            matchedWords = T.PartialMatch("agent");  
            PrintList(matchedWords);

            // testing a beginning wildcard to return words with a different leading char
            Console.WriteLine("testing *ie, return: die, lie");
            matchedWords = T.PartialMatch("*ie");  
            PrintList(matchedWords);

            // testing a single wildcard to only return the only one char word
            Console.WriteLine("testing *, return: a");
            matchedWords = T.PartialMatch("*");  
            PrintList(matchedWords);

            // testing to obtain all 12 letter words with only wildcards
            Console.WriteLine("testing ************(* x 12), return: organization, particularly, professional, relationship");
            matchedWords = T.PartialMatch("************");  
            PrintList(matchedWords);
            */

            // Testing for Autocomplete
            Console.WriteLine("testing acti, return: action, activitiy");
            matchedWords = T.Autocomplete("acti");
            PrintList(matchedWords);



            Console.ReadKey();
        }
    }
}
