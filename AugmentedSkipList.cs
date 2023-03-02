using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Based on http://igoro.com/archive/skip-lists-are-fascinating/
// Augmentation based on https://opendatastructures.org/newhtml/ods/latex/skiplists.html
// Algorithm to update lengths based on https://code.activestate.com/recipes/576930/

namespace SkipLists
{
    // Interface for SkipList

    interface ISkipList<T> where T : IComparable
    {
        void Insert(T item);     // Inserts item into the skip list (duplicates are permitted)
        bool Contains(T item);   // Returns true if item is found; false otherwise
        void Remove(T item);     // Removes one occurrence of item (if possible) from the skip list

        // Augmented methods
        T RankI(int i);          // Return the item with the given rank
        int RankII(T item);       // Return the rank of a given item
    }

    // Class SkipList

    class SkipList<T> : ISkipList<T> where T : IComparable
    {
        private Node head;          // Header node of height 32
        private int maxHeight;     // Maximum height among non-header nodes
        private Random rand;          // For generating random heights
        // *** UPDATE (just for testing) ***
        public int size { get; set; }

        // Class Node (used by SkipList)

        private class Node
        {
            public T Item { get; set; }
            public int Height { get; set; }
            public Node[] Next { get; set; }
            // *** UPDATE ***
            // array of integers to keep length/ how many nodes are skipped by each arrow at each level
            public int[] Length { get; set; }

            // Constructor
            public Node(T item, int height)
            {
                Item = item;
                Height = height;
                Next = new Node[Height];
                Length = new int[Height];
                for (int i = 0; i < Height; i++)
                {
                    Next[i] = null;
                    Length[i] = 0;
                }
            }
        }

        // Constructor
        // Creates an empty skip list with a header node of height 32
        // Time complexity: O(1)

        public SkipList()
        {
            head = new Node(default(T), 32);    // Set to NIL by default
            maxHeight = 0;                      // Current maximum height of the skip list
            rand = new Random();
            size = 0;
        }


        //Public Insert
        //Initiate random height and inserts the given item into the skip list
        //Duplicate items are permitted
        //Expected time complexity: O(log n)

        public void Insert(T item)
        {
            // Randomly determine height of a node
            int height = 0;
            int R = rand.Next();   // R is a random 32-bit positive integer
            while ((height < maxHeight) && ((R & 1) == 1))
            {
                height++;   // Equal to the number consecutive lower order 1-bits
                R >>= 1;    // Right shift one bit
            }
            if (height == maxHeight) maxHeight++;


            // Create and insert node recursively
            Node newNode = new Node(item, height + 1);

            //  *** UPDATE *** 
            // pos keeps track of the current node's position
            // floor starts from the maxHeight
            int pos = 0, floor = maxHeight - 1;
            Stack<int> steps = new Stack<int>(); // second stack acts like a queue to store the number of pointers from the first floor
            Node cur = head;
            // recursive insert call
            Insert(cur, newNode, ref pos, floor, steps);

            size++; // update the count of items in the skip list
        }

        // *** UPDATE ***
        // Private Insert
        // Performs post-order traversal to insert given item to skip list and update lengths for each pointer
        private void Insert(Node cur, Node newNode, ref int pos, int floor, Stack<int> steps)
        {
            if (floor >= 0)
            {
                int stepCount = 0; // restart count for steps (arrows) for each floor

                // find the insert position from the top
                while (cur.Next[floor] != null && cur.Next[floor].Item.CompareTo(newNode.Item) < 0)
                {
                    stepCount += cur.Length[floor];
                    cur = cur.Next[floor];
                }
                // if you go down a floor, insertion is going to happen somewhere after the current node
                // if there is a node after the current one, then the length of the pointer increases by one from the insertion
                if(floor >= newNode.Height && cur.Next[floor] != null)
                    cur.Length[floor] += 1;
                steps.Push(stepCount); // push the distance of the current level into the stack

                // go down the floor of the previous node before insertion 
                Insert(cur, newNode, ref pos, floor - 1, steps);

                if (floor < newNode.Height)
                {
                    // adjust the pointers for each floor
                    newNode.Next[floor] = cur.Next[floor];
                    cur.Next[floor] = newNode;
                    // update number of nodes skipped for the new node and the node before the insertion
                    newNode.Length[floor] = cur.Length[floor] - pos; 
                    cur.Length[floor] = pos + 1;
                    // update the position counter using the steps taken at each floor (in reverse order)
                    // e.g., if we're at floor 0, then steps's dequeue is the steps at maxHeight - 1
                    pos += steps.Pop(); 
                }
            }
        }


        // Contains
        // Returns true if the given item is found in the skip list; false otherwise
        // Expected time complexity: O(log n)

        public bool Contains(T item)
        {
            Node cur = head;
            for (int i = maxHeight - 1; i >= 0; i--)  // for each level
            {
                while (cur.Next[i] != null && cur.Next[i].Item.CompareTo(item) < 0)
                    cur = cur.Next[i];

                if (cur.Next[i] != null && cur.Next[i].Item.CompareTo(item) == 0)
                    return true;
            }
            return false;
        }

        // Public Remove (post-order recursion)
        // Removes one occurrence (if possible) of the given item from the skip list
        // Expected time complexity: O(log n)

        public void Remove(T item)
        {
            Node cur = head;
            int floor = maxHeight - 1;
            int height = int.MaxValue;

            if (Remove(item, cur, floor, ref height) != null)
                size -= 1; // update the size of the skip list if removal is successful
        }


        // Private Remove 
        // Traverse to the node to be removed.
        // Post order recursion is done so that the height of the node to be removed can be first determined
        // The height is used in order to maintain the lengths of the pointers

        private Node Remove(T item, Node cur, int floor, ref int height)
        {
            Node removed = null;
            if (floor >= 0)
            {
                // traverse to the node before the one to be removed
                while (cur.Next[floor] != null && cur.Next[floor].Item.CompareTo(item) < 0)
                    cur = cur.Next[floor];
                // recursive call. Travel to the floor 0 from maxHeight - 1 before processing
                Remove(item, cur, floor - 1, ref height);


                // Processing removal for each floor starting from 0
                if(cur.Next[floor] != null && cur.Next[floor].Item.CompareTo(item) == 0)
                {
                    height = cur.Next[floor].Height; // get the height of the node to be removed
                    removed = cur.Next[floor]; // keeps the removed node here to be returned
                    cur.Next[floor].Height -= 1; // reduce height by one for each floor removed
                    cur.Length[floor] += cur.Next[floor].Length[floor] - 1; // update length of pointer
                    cur.Next[floor] = cur.Next[floor].Next[floor];    // Cut off reference at level i
                }

                // decrease length by one for floors over the height of the node removed
                if (floor >= height && cur.Next[floor] != null)
                   cur.Length[floor] -= 1;

                // Decrease maximum height by 1 when the number of nodes at height i is 0
                if (head.Next[floor] == null)
                    maxHeight--;
            }
            return removed;
        }


        // Print
        // Outputs the item in sorted order
        // Time complexity:  O(n)

        public void Print()
        {
            Node cur = head.Next[0];
            while (cur != null)
            {
                Console.Write(cur.Item + " ");
                cur = cur.Next[0];
            }
            Console.WriteLine();
        }

        // Profile
        // Prints out a "skyline" of the skip list
        // Time complexity: O(n)

        public void Profile()
        {
            Node cur = head;

            while (cur != null)
            {
                Console.WriteLine(new string('*', cur.Height));  // Outputs a string of *s
                cur = cur.Next[0];
            }
        }

        // Public Rank I
        // Calls private Rank which returns the item with rank i
        // Expected time complexity:  O(log n)

        public T RankI(int i)
        {
            return RankI(head, i);
        }

        // Private Rank I
        // Returns the item with the given rank i
        // Expected time complexity:  O(log n)

        private T RankI(Node p, int i)
        {
            int r = 0;
            if (i > 0 && i <= size)                  // Check for correct input
            {
                for (int x = maxHeight - 1; x >= 0; x--)  // for each level
                {
                    // traverse to the given rank
                    while (p.Next[x] != null && (r + p.Length[x]) <= i)
                    {
                        r += p.Length[x]; // traverse and update the current rank
                        p = p.Next[x];
                    }
                }
                return p.Item;
            }
            else
                // i out of range
                return default(T);
        }


        // Rank II
        // Calculates and returns the rank of the given item if found; -1 otherwise
        // Expected time complexity:  O(log n)

        public int RankII(T item)
        {
            Node cur = head;
            int r = 0;
            for (int i = maxHeight - 1; i >= 0; i--)  // for each level
            {
                while (cur.Next[i] != null && cur.Next[i].Item.CompareTo(item) < 0)
                {
                    r += cur.Length[i]; // count total lengths traversed
                    cur = cur.Next[i];
                }
                if (cur.Next[i] != null && cur.Next[i].Item.CompareTo(item) == 0)
                    return r += cur.Length[i];
            }
            return -1;              // Return -1 if item is not found

        }

    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Initiating Skip List...");

            SkipList<int> S = new SkipList<int>();

            //int i;
            ////for (i = 1; i <= 10; i++) { S.Insert(i);}
            //for (i = 1; i <= 6; i++) { S.Insert(i); S.Insert(7 - i); }

            //S.Print();
            //S.Profile();
            //for(i = 1; i <= 6; i++)
            //    Console.WriteLine("Rank of " + i + "        : " + S.RankII(i));
            //Console.WriteLine("Min and max      : " + S.RankI(1) + " " + S.RankI(S.size));

            //for (i = 1; i <= 2; i++) { S.Remove(6); S.Remove(3); }
            //for (i = 1; i <= 6; i++) Console.WriteLine(S.Contains(i));

            //for (i = 1; i <= 6; i++)
            //    Console.WriteLine("Rank of " + i + "        : " + S.RankII(i));
            //Console.WriteLine("Min and max        : " + S.RankI(1) + " " + S.RankI(S.size));
            //S.Print();
            //S.Profile();

            //Console.ReadKey();

            System.Threading.Thread.Sleep(100);

            bool run = true;
            while (run == true) //repeat until quit
            {
                Console.Write(
                "\nMenu:" +
                "\nInitialize default Skip List    (S):" +
                "\nInsert new item                 (N):" +
                "\nDelete existing item            (D):" +
                "\nFind item in Skip List          (F):" +
                "\nFind item of Rank (Rank I)      (I):" +
                "\nFind Rank of item (Rank II)     (R):" +
                "\nQuit                            (Q):" +
                "\nInput? :");

                bool valid = false; //validation
                char input = default;
                int item;

                while (valid == false)
                {
                    try
                    {
                        input = Char.ToUpper(Convert.ToChar(Console.ReadLine())); //in case of caps lock
                        valid = true;
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Input error, please try again");
                        Console.Write("Input :");
                    }
                }

                switch (input) //operation based on menu input
                {
                    // initiate default skip list
                    case 'S':
                        Console.WriteLine("Initializing default Skip List ... ");
                        System.Threading.Thread.Sleep(100);
                        for (int i = 10; i <= 60; i++) { S.Insert(i); S.Insert(70 - i); i += 9; }
                        S.Print();
                        S.Profile();
                        break;

                    // call Insert()
                    case 'N':
                        Console.Write("Please enter the item you would like to insert >> ");
                        while (!int.TryParse(Console.ReadLine(), out item))
                            Console.WriteLine("Fail to insert item."
                                + "\nPlease re-enter item or enter '0' to return to main menu:");
                        if (item != 0)
                        {
                            S.Insert(item);
                            S.Print();
                            S.Profile();
                        }
                        break;

                    // call Remove()
                    case 'D':
                        Console.Write("Please enter the item you would like to remove >> ");
                        while (!int.TryParse(Console.ReadLine(), out item))
                            Console.WriteLine("Input not valid."
                                + "\nPlease re-enter item or enter '0' to return to main menu:");
                        if (item != 0)
                        {
                            S.Remove(item);
                            S.Print();
                            S.Profile();
                        }
                        break;

                    // call Contains()
                    case 'F':
                        Console.Write("Please enter the item >> ");
                        while (!int.TryParse(Console.ReadLine(), out item))
                            Console.WriteLine("Input not valid."
                                + "\nPlease re-enter item or enter '0' to return to main menu:");
                        if (item != 0)
                        {
                            if (S.Contains(item))
                                Console.WriteLine(item + " found at rank " + S.RankII(item));
                            else
                                Console.WriteLine(item + " not found");
                        }
                        break;

                    // call RankI()
                    case 'I':
                        Console.WriteLine("Current items in the Skip List are: ");
                        S.Print();
                        Console.Write("Please enter the Rank >> ");
                        while (!int.TryParse(Console.ReadLine(), out item))
                            Console.WriteLine("Input not valid."
                                + "\nPlease re-enter item or enter '0' to return to main menu:");
                        int rankedItem = S.RankI(item);
                        if (rankedItem != 0)
                            Console.WriteLine("The item of rank " + item + " is " + rankedItem);
                        else
                            Console.WriteLine("The rank entered is out of bounds of the skip list");
                        break;

                    // call RankII()
                    case 'R':
                        Console.WriteLine("Current items in the Skip List are: ");
                        S.Print();
                        Console.Write("Please enter the item you would like to be ranked >> ");
                        while (!int.TryParse(Console.ReadLine(), out item))
                            Console.WriteLine("Input not valid."
                                + "\nPlease re-enter item or enter '0' to return to main menu:");
                        int rank = S.RankII(item);
                        if (rank > 0)
                            Console.WriteLine("The rank of " + item + " is " + rank);
                        else
                            Console.WriteLine(item + " is not found in the Skip List.");
                        break;

                    // stop program
                    case 'Q': //quit
                        run = false; //main loop control variable
                        break;


                    default: //if menu input was a char but didn't match up with any valid menu operations
                        Console.WriteLine("Unrecognised input, please try again");
                        break;
                }

                System.Threading.Thread.Sleep(1000);
            }

            Console.WriteLine("\nThank you for using the Skip List. Goodbye!");
            Console.ReadLine();
        }
    }
}
