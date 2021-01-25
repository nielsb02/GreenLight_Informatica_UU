﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenLight
{
    public class Dijkstra
    {

        private static readonly int NO_PARENT = -1;
        public List<int> vertices = new List<int>();
        public List<int> distancelist = new List<int>();
        public List<List<int>> path = new List<List<int>>();
        public int pathnr = 0;

        // Function that implements Dijkstra's  
        // single source shortest path  
        // algorithm for a graph represented  
        // using adjacency matrix  
        // representation  
        public void dijkstra(int[,] adjacencyMatrix,
                                            int startVertex)
        {
            int nVertices = adjacencyMatrix.GetLength(0);

            // shortestDistances[i] will hold the  
            // shortest distance from src to i  
            int[] shortestDistances = new int[nVertices];

            // added[i] will true if vertex i is  
            // included / in shortest path tree  
            // or shortest distance from src to  
            // i is finalized  
            bool[] added = new bool[nVertices];

            // Initialize all distances as  
            // INFINITE and added[] as false  
            for (int vertexIndex = 0; vertexIndex < nVertices;
                                                vertexIndex++)
            {
                shortestDistances[vertexIndex] = int.MaxValue;
                added[vertexIndex] = false;
            }

            // Distance of source vertex from  
            // itself is always 0  
            shortestDistances[startVertex] = 0;

            // Parent array to store shortest  
            // path tree  
            int[] parents = new int[nVertices];

            // The starting vertex does not  
            // have a parent  
            parents[startVertex] = NO_PARENT;

            // Find shortest path for all  
            // vertices  
            for (int i = 1; i < nVertices; i++)
            {

                // Pick the minimum distance vertex  
                // from the set of vertices not yet  
                // processed. nearestVertex is  
                // always equal to startNode in  
                // first iteration.  
                int nearestVertex = -1;
                int shortestDistance = int.MaxValue;
                for (int vertexIndex = 0;
                        vertexIndex < nVertices;
                        vertexIndex++)
                {
                    if (!added[vertexIndex] &&
                        shortestDistances[vertexIndex] <
                        shortestDistance)
                    {
                        nearestVertex = vertexIndex;
                        shortestDistance = shortestDistances[vertexIndex];
                    }
                }

                // Mark the picked vertex as  
                // processed  
                if (nearestVertex >= 0)
                {
                    added[nearestVertex] = true;
                    // Update dist value of the  
                    // adjacent vertices of the  
                    // picked vertex.  
                    for (int vertexIndex = 0;
                            vertexIndex < nVertices;
                            vertexIndex++)
                    {
                        int edgeDistance = adjacencyMatrix[nearestVertex, vertexIndex];

                        if (edgeDistance > 0
                            && ((shortestDistance + edgeDistance) <
                                shortestDistances[vertexIndex]))
                        {
                            parents[vertexIndex] = nearestVertex;
                            shortestDistances[vertexIndex] = shortestDistance +
                                                            edgeDistance;
                        }
                    }
                }
            }

            printSolution(startVertex, shortestDistances, parents);
        }

        // A utility function to print  
        // the constructed distances  
        // array and shortest paths  
        private void printSolution(int startVertex,
                                        int[] distances,
                                        int[] parents)
        {
            int nVertices = distances.Length;
            //Console.Write("Vertex\t Distance\tPath");

            for (int vertexIndex = 0;
                    vertexIndex < nVertices;
                    vertexIndex++)
            {
                if (vertexIndex != startVertex)
                {
                    //Console.Write("\n" + startVertex + " -> ");
                    //Console.Write(vertexIndex + " \t\t ");
                    vertices.Add(vertexIndex);
                    //Console.Write(distances[vertexIndex] + "\t\t");
                    distancelist.Add(distances[vertexIndex]);
                    path.Add(new List<int>());
                    printPath(vertexIndex, parents);
                    pathnr++;
                }
            }
        }

        // Function to print shortest path  
        // from source to currentVertex  
        // using parents array  
        private void printPath(int currentVertex,
                                    int[] parents)
        {
            //Console.WriteLine("CurrentVertex: " + currentVertex);
            // Base case : Source node has  
            // been processed  
            if (currentVertex <= NO_PARENT)
            {
                return;
            }
            if (currentVertex == parents[currentVertex])
            {
                //Console.Write(currentVertex + " ");
                return;
            }
            printPath(parents[currentVertex], parents);
            //Console.Write(currentVertex + " ");
            path[pathnr].Add(currentVertex);
        }
    }
}
// This code has been contributed by 29AjayKumar