using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;
namespace IntelligentScissors
{
    public enum PFmode
    {
        exist, update
    }
    #region PERIORITY QUEUE
    /// <summary>
    /// the periority queue heap based implementation
    /// </summary>
    public class PeriorityQueue
    {
        private List<Edge> Heap = new List<Edge>(); //array of edges
        private void swap(int x, int y)
        {
            int temp = x;
            x = y;
            y = temp;
        }
        private int Left(int Node)
        {
            //private function returns the index of the expected left child of the node at index Node 
            return Node * 2 + 1;
            //
        }
        private int Right(int Node)
        {
            //private function returns the index of the expected right child of the node at index Node 
            return Node * 2 + 2;
        }
        private int Parent(int Node)
        {
            //private function returns the index of the expected parent of the node at index Node 
            return (Node - 1) / 2;
        }
        /// <summary>
        /// the function that modify our tree(Heap) after adding elements
        /// </summary>
        /// <param name="Node"></param>
        private void SiftUp(int Node)
        {
            //in the case of you are a root (position 0) 
            //or your value is bigger than the value of your parent
            // you have no thing to do here, just return 
            if (Node == 0 || Heap[Node].Weight >= Heap[Parent(Node)].Weight)
                return;
            //swap(Heap[Parent(Node)], Heap[Node]);
            Edge temp = Heap[Parent(Node)];
            Heap[Parent(Node)] = Heap[Node];
            Heap[Node] = temp;
            //
            //now let's continue positioning-fitting process of the position of the given node ( which became a parent ) 
            SiftUp(Parent(Node));
        }
        /// <summary>
        /// the function that modify our tree(Heap) after deleting(poping) elements
        /// </summary>
        /// <param name="Node"></param>
        private void SiftDown(int Node)
        {
            //in case of: you have no left child ( absolutely you will not have even right one 
            //or you have a only left child (no right one) which is in its accurate position(it is smaller than his child on left)
            //or you have 2 children which are both greater than you
            // therefore, you have no thing to do, just return home :D
            if (Left(Node) >= Heap.Count
                || (Left(Node) < Heap.Count && Right(Node) >= Heap.Count && Heap[Left(Node)].Weight >= Heap[Node].Weight)
                || (Left(Node) < Heap.Count && Right(Node) < Heap.Count && Heap[Left(Node)].Weight >= Heap[Node].Weight &&
                Heap[Right(Node)].Weight >= Heap[Node].Weight))
                return;
            //in case of you have a right child, and this child is smaller than his brother on left
            //just rise this cute small up :D
            if (Right(Node) < Heap.Count && Heap[Right(Node)].Weight <= Heap[Left(Node)].Weight)
            {
                Edge temp = Heap[Right(Node)];
                Heap[Right(Node)] = Heap[Node];
                Heap[Node] = temp;
                SiftDown(Right(Node));
            }
            //in case of you have not a right child
            //just rise your only left child up he is cute too :D
            else
            {
                Edge temp = Heap[Left(Node)];
                Heap[Left(Node)] = Heap[Node];
                Heap[Node] = temp;
                SiftDown(Left(Node));
            }
        }
        public PeriorityQueue()
        {
            //
        }
        /// <summary>
        /// Add element to the periority queue
        /// </summary>
        /// <param name="Node"></param>
        public void Push(Edge Node)
        {
            Heap.Add(Node);//add him at the end
            SiftUp(Heap.Count - 1);//now modify the heap, as the last position made the heap not well modified
        }
        /// <summary>
        /// Remove the smallest element from your heap, and then modify it to have the next smallest element in the front
        /// </summary>
        /// <returns></returns>
        public Edge Pop()
        {
            Edge temp = Heap[0];//hold the element before killing him :D
            Heap[0] = Heap[Heap.Count - 1];//remove the smallest one,overwrite it the last element in the heap
            Heap.RemoveAt(Heap.Count - 1);//remove this guy at the back, he came in the front of the heap :D
            SiftDown(0);//NOW, we have a non-accurate heap, let's put this guy at the frontm in his acctual(expected position) :D
            return temp;//whom did you delete now :D ?
        }
        /// <summary>
        /// returns if the heap still has elements or not
        /// </summary>
        /// <returns></returns>
        public bool IsEmpty()
        {
            if (Heap.Count == 0)//if you had run out of them , say true :D
                return true;
            return false;
        }
        /// <summary>
        /// What is your size ? 
        /// </summary>
        /// <returns></returns>
        public int Size()
        {
            return Heap.Count;
        }
        /// <summary>
        /// returns the first and smallest element in the heap
        /// </summary>
        /// <returns></returns>
        public Edge Top()
        {
            return Heap[0];
        }
    }
    #endregion

    public static class ShortestPath_Operations
    {
        /// <summary>
        /// generate the shortest path from source node to all other nodes
        /// </summary>
        /// <param name="Graph"></param>
        /// <param name="Source"></param>
        /// <param name="Dest"></param>
        /// <returns></returns>
        public static List<Point> GenerateShortestPath(int Source, int Dest, RGBPixel[,] ImageMatrix)
        {
            // return sortest path btween src & all other nodes 
            List<int> Previous_list = Dijkstra(Source, Dest, ImageMatrix);
            // Backtracking shortest path to (Dest)node  from previous list 
            return Backtracking(Previous_list, Dest, ImageOperations.GetWidth(ImageMatrix));
        }
        /// <summary>
        /// backtracing the shortestpath btween 2 nodes src, dest
        /// </summary>
        /// <param name="All_Paths"></param>
        /// <param name="Source"></param>
        /// <param name="Dest"></param>
        /// <returns>shortestpath</returns>
        public static List<Point> Backtracking(List<int> Previous_list, int Dest, int matrix_width)
        {
            List<Point> ShortestPath = new List<Point>(); // shortest path bteewn Source node and destination
            Stack<int> RevPath = new Stack<int>();   //the reversed shortest path bteewn Source node and destination           
            RevPath.Push(Dest); // push the destination node 
            int previous = Previous_list[Dest]; // previous of the destination (current node)
            // backtracking the shortest path from all paths
            while (previous != -1)
            {
                RevPath.Push(previous); // push last node in the path   
                previous = Previous_list[previous]; //previous of current node
            }
            //revrese the reversed path 
            while (RevPath.Count != 0)
            {
                var p = Helper.Unflatten(RevPath.Pop(), matrix_width);
                Point point = new Point((int)p.X, (int)p.Y);
                ShortestPath.Add(point);
            }
            // return shortest path bteewn Source node and destination
            return ShortestPath;
        }
        /// <summary>
        /// Dijkstra algorithm
        /// </summary>
        /// <param name="Graph"></param>
        /// <param name="Source"></param>
        /// <returns>list of all shortest paths btween a source node and all nodes </returns>
        /// 
        #region DIJKSTRA ALGORITHMS
        public static List<int> Dijkstra(int Source, RGBPixel[,] ImageMatrix)
        {
            const double oo = 10000000000000000000; // infity value
            //Distance : the minimum cost between the source node and all the others nodes
            //initialized with infinty value
            int Width = ImageOperations.GetWidth(ImageMatrix);
            int Height = ImageOperations.GetHeight(ImageMatrix);
            int nodes_number = Width * Height;
            List<double> Distance = new List<double>();
            Distance = Enumerable.Repeat(oo, nodes_number).ToList();
            //Previous : saves the previous node that lead to the shortest path from the src node to current node 
            List<int> Previous = new List<int>();
            Previous = Enumerable.Repeat(-1, nodes_number).ToList();
            // SP between src and it self costs 0 
            // PeriorityQueue : always return the shortest bath btween src node and specific node  
            PeriorityQueue MinimumDistances = new PeriorityQueue();
            MinimumDistances.Push(new Edge(-1, Source, 0));
            while (!MinimumDistances.IsEmpty())
            {
                // get the shortest path so far 
                Edge CurrentEdge = MinimumDistances.Top();
                MinimumDistances.Pop();
                // check if this SP is vaild (i didn't vist this node with a less cost)
                if (CurrentEdge.Weight >= Distance[CurrentEdge.To])
                    continue;
                // save the previous 
                Distance[CurrentEdge.To] = CurrentEdge.Weight;
                Previous[CurrentEdge.To] = CurrentEdge.From;
                // Relaxing 
                List<Edge> neibours = GraphOperations.Get_neighbours(CurrentEdge.To, ImageMatrix);
                for (int i = 0; i < neibours.Count; i++)
                {
                    Edge HeldEdge = neibours[i];
                    // if the relaxed path cost of a neighbour node is less than  it's previous one
                    if (Distance[HeldEdge.To] > Distance[HeldEdge.From] + HeldEdge.Weight)
                    {
                        // set the relaxed cost to Distance  && pash it to the PQ
                        HeldEdge.Weight = Distance[HeldEdge.From] + HeldEdge.Weight;
                        MinimumDistances.Push(HeldEdge);
                    }
                }
            }
            return Previous;  // re turn th shortest paths from src to all nodes
        }
        public static List<int> Dijkstra(int Source, int dest, RGBPixel[,] ImageMatrix)
        {
            const double oo = 10000000000000000000; // infity value
            //Distance : the minimum cost between the source node and all the others nodes
            //initialized with infinty value
            int Width = ImageOperations.GetWidth(ImageMatrix);
            int Height = ImageOperations.GetHeight(ImageMatrix);
            int nodes_number = Width * Height;
            List<double> Distance = new List<double>();
            Distance = Enumerable.Repeat(oo, nodes_number).ToList();

            //Previous : saves the previous node that lead to the shortest path from the src node to current node 
            List<int> Previous = new List<int>();
            Previous = Enumerable.Repeat(-1, nodes_number).ToList();

            // PeriorityQueue : always return the shortest bath btween src node and specific node  
            PeriorityQueue MinimumDistances = new PeriorityQueue();
            MinimumDistances.Push(new Edge(-1, Source, 0));
            while (!MinimumDistances.IsEmpty())
            {
                // get the shortest path so far 
                Edge CurrentEdge = MinimumDistances.Top();
                MinimumDistances.Pop();
                // check if this SP is vaild (i didn't vist this node with a less cost)
                if (CurrentEdge.Weight >= Distance[CurrentEdge.To])
                    continue;
                // save the previous 
                Previous[CurrentEdge.To] = CurrentEdge.From;
                Distance[CurrentEdge.To] = CurrentEdge.Weight;
                if (CurrentEdge.To == dest) break;
                // Relaxing 
                List<Edge> neibours = GraphOperations.Get_neighbours(CurrentEdge.To, ImageMatrix);
                for (int i = 0; i < neibours.Count; i++)
                {
                    Edge HeldEdge = neibours[i];
                    // if the relaxed path cost of a neighbour node is less than  it's previous one
                    if (Distance[HeldEdge.To] > Distance[HeldEdge.From] + HeldEdge.Weight)
                    {
                        // set the relaxed cost to Distance  && pash it to the PQ
                        HeldEdge.Weight = Distance[HeldEdge.From] + HeldEdge.Weight;
                        MinimumDistances.Push(HeldEdge);
                    }
                }
            }
            return Previous;  // re turn th shortest paths from src to all nodes
        }
        #endregion
        // return bondry limit of dikstra
        public static Boundary Square_Boundary(int Src, int Width, int Height)
        {
            Vector2D Src2d = Helper.Unflatten(Src, Width + 1); //src node  x y
            Boundary bondry = new Boundary();
            int max_dist = 200;
            if (Src2d.X > max_dist)
                bondry.MIN_X = (int)Src2d.X - max_dist;
            else
                bondry.MIN_X = 0;
            if (Width - Src2d.X > max_dist)
                bondry.MAX_X = (int)Src2d.X + max_dist;
            else
                bondry.MAX_X = Width;
            if (Src2d.Y > max_dist)
                bondry.MIN_Y = (int)Src2d.Y - max_dist;
            else
                bondry.MIN_Y = 0;
            if (Height - Src2d.Y > max_dist)
                bondry.MAX_Y = (int)Src2d.Y + max_dist;
            else
                bondry.MAX_Y = Height;
            return bondry;
        }
    }
}
