using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using QuickGraph.Contracts;
using QuickGraph.Collections;
using System.Linq;
using QuickGraph.Algorithms.MinimumSpanningTree;
using System.Reflection;
using System.Diagnostics;


namespace QuickGraph.Algorithms
{
    /// <summary>
    /// Various extension methods to build algorithms
    /// </summary>
    public static class AlgorithmExtensions
    {
        /// <summary>
        /// Returns the method that implement the access indexer.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dictionary"></param>
        /// <returns></returns>
        public static Func<TKey, TValue> GetIndexer<TKey, TValue>(Dictionary<TKey, TValue> dictionary)
        {
            Contract.Requires(dictionary != null);
            Contract.Ensures(Contract.Result<Func<TKey, TValue>>() != null);

#if!SILVERLIGHT
            var method = dictionary.GetType().GetProperty("Item").GetGetMethod();
            return (Func<TKey, TValue>)Delegate.CreateDelegate(typeof(Func<TKey, TValue>), dictionary, method, true);
#else
            return key => dictionary[key];
#endif
        }

        /// <summary>
        /// Gets the vertex identity.
        /// </summary>
        /// <remarks>
        /// Returns more efficient methods for primitive types,
        /// otherwise builds a dictionary
        /// </remarks>
        /// <typeparam name="TVertex">The type of the vertex.</typeparam>
        /// <param name="graph">The graph.</param>
        /// <returns></returns>
        public static VertexIdentity<TVertex> GetVertexIdentity<TVertex>(
#if !NET20
this 
#endif
            IVertexSet<TVertex> graph)
        {
            Contract.Requires(graph != null);

            // simpler identity for primitive types
            switch (Type.GetTypeCode(typeof(TVertex)))
            {
                case TypeCode.String:
                case TypeCode.Boolean:
                case TypeCode.Byte:
                case TypeCode.Char:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.SByte:
                case TypeCode.Single:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return (v) => v.ToString();
            }

            // create dictionary
            var ids = new Dictionary<TVertex, string>(graph.VertexCount);
            return v =>
                {
                    string id;
                    if (!ids.TryGetValue(v, out id))
                        ids[v] = id = ids.Count.ToString();
                    return id;
                };
        }

        /// <summary>
        /// Gets the edge identity.
        /// </summary>
        /// <typeparam name="TVertex">The type of the vertex.</typeparam>
        /// <typeparam name="TEdge">The type of the edge.</typeparam>
        /// <param name="graph">The graph.</param>
        /// <returns></returns>
        public static EdgeIdentity<TVertex, TEdge> GetEdgeIdentity<TVertex, TEdge>(
#if !NET20
this 
#endif
            IEdgeSet<TVertex, TEdge> graph)
            where TEdge : IEdge<TVertex>
        {
            Contract.Requires(graph != null);

            // create dictionary
            var ids = new Dictionary<TEdge, string>(graph.EdgeCount);
            return e =>
            {
                string id;
                if (!ids.TryGetValue(e, out id))
                    ids[e] = id = ids.Count.ToString();
                return id;
            };
        }


        /// <summary>
        /// Gets the list of sink vertices
        /// </summary>
        /// <typeparam name="TVertex">type of the vertices</typeparam>
        /// <typeparam name="TEdge">type of the edges</typeparam>
        /// <param name="visitedGraph"></param>
        /// <returns></returns>
        public static IEnumerable<TVertex> Sinks<TVertex, TEdge>(
#if !NET20
this 
#endif
            IVertexListGraph<TVertex, TEdge> visitedGraph)
            where TEdge : IEdge<TVertex>
        {
            Contract.Requires(visitedGraph != null);
            return SinksIterator<TVertex, TEdge>(visitedGraph);
        }

        [DebuggerHidden]
        private static IEnumerable<TVertex> SinksIterator<TVertex, TEdge>(
            IVertexListGraph<TVertex, TEdge> visitedGraph)
            where TEdge : IEdge<TVertex>
        {
            foreach (var v in visitedGraph.Vertices)
                if (visitedGraph.IsOutEdgesEmpty(v))
                    yield return v;
        }


        /// <summary>
        /// Gets the list of root vertices
        /// </summary>
        /// <typeparam name="TVertex">type of the vertices</typeparam>
        /// <typeparam name="TEdge">type of the edges</typeparam>
        /// <param name="visitedGraph"></param>
        /// <returns></returns>
        public static IEnumerable<TVertex> Roots<TVertex, TEdge>(
#if !NET20
this 
#endif
            IBidirectionalGraph<TVertex, TEdge> visitedGraph)
            where TEdge : IEdge<TVertex>
        {
            Contract.Requires(visitedGraph != null);
            return RootsIterator(visitedGraph);
        }

        [DebuggerHidden]
        private static IEnumerable<TVertex> RootsIterator<TVertex, TEdge>(
            IBidirectionalGraph<TVertex, TEdge> visitedGraph)
            where TEdge : IEdge<TVertex>
        {
            foreach (var v in visitedGraph.Vertices)
                if (visitedGraph.IsInEdgesEmpty(v))
                    yield return v;
        }

        /// <summary>
        /// Gets the list of isolated vertices (no incoming or outcoming vertices)
        /// </summary>
        /// <typeparam name="TVertex">type of the vertices</typeparam>
        /// <typeparam name="TEdge">type of the edges</typeparam>
        /// <param name="visitedGraph"></param>
        /// <returns></returns>
        public static IEnumerable<TVertex> IsolatedVertices<TVertex, TEdge>(
#if !NET20
this 
#endif
            IBidirectionalGraph<TVertex, TEdge> visitedGraph)
            where TEdge : IEdge<TVertex>
        {
            Contract.Requires(visitedGraph != null);
            return IsolatedVerticesIterator(visitedGraph);
        }

        [DebuggerHidden]
        private static IEnumerable<TVertex> IsolatedVerticesIterator<TVertex, TEdge>(
            IBidirectionalGraph<TVertex, TEdge> visitedGraph)
            where TEdge : IEdge<TVertex>
        {
            foreach (var v in visitedGraph.Vertices)
                if (visitedGraph.Degree(v) == 0)
                    yield return v;
        }


        /// <summary>
        /// Clones a graph to another graph
        /// </summary>
        /// <typeparam name="TVertex">type of the vertices</typeparam>
        /// <typeparam name="TEdge">type of the edges</typeparam>
        /// <param name="g"></param>
        /// <param name="vertexCloner"></param>
        /// <param name="edgeCloner"></param>
        /// <param name="clone"></param>
        public static void Clone<TVertex, TEdge>(
#if !NET20
this 
#endif
            IVertexAndEdgeListGraph<TVertex, TEdge> g,
            Func<TVertex, TVertex> vertexCloner,
            Func<TEdge, TVertex, TVertex, TEdge> edgeCloner,
            IMutableVertexAndEdgeSet<TVertex, TEdge> clone)
            where TEdge : IEdge<TVertex>
        {
            Contract.Requires(g != null);
            Contract.Requires(vertexCloner != null);
            Contract.Requires(edgeCloner != null);
            Contract.Requires(clone != null);

            var vertexClones = new Dictionary<TVertex, TVertex>(g.VertexCount);
            foreach (var v in g.Vertices)
            {
                var vc = vertexCloner(v);
                clone.AddVertex(vc);
                vertexClones.Add(v, vc);
            }

            foreach (var edge in g.Edges)
            {
                var ec = edgeCloner(
                    edge,
                    vertexClones[edge.Source],
                    vertexClones[edge.Target]);
                clone.AddEdge(ec);
            }
        }


        /// <summary>
        /// Create a collection of odd vertices
        /// </summary>
        /// <param name="g">graph to visit</param>
        /// <returns>colleciton of odd vertices</returns>
        /// <exception cref="ArgumentNullException">g is a null reference</exception>
        public static List<TVertex> OddVertices<TVertex, TEdge>(
#if !NET20
this 
#endif
            IVertexAndEdgeListGraph<TVertex, TEdge> g)
            where TEdge : IEdge<TVertex>
        {
            Contract.Requires(g != null);

            var counts = new Dictionary<TVertex, int>(g.VertexCount);
            foreach (var v in g.Vertices)
                counts.Add(v, 0);

            foreach (var e in g.Edges)
            {
                ++counts[e.Source];
                --counts[e.Target];
            }

            var odds = new List<TVertex>();
            foreach (var de in counts)
            {
                if (de.Value % 2 != 0)
                    odds.Add(de.Key);
            }

            return odds;
        }


        /// <summary>
        /// Given a edge cost map, computes 
        /// the predecessor cost.
        /// </summary>
        /// <typeparam name="TVertex">type of the vertices</typeparam>
        /// <typeparam name="TEdge">type of the edges</typeparam>
        /// <param name="predecessors"></param>
        /// <param name="edgeCosts"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static double ComputePredecessorCost<TVertex, TEdge>(
            IDictionary<TVertex, TEdge> predecessors,
            IDictionary<TEdge, double> edgeCosts,
            TVertex target
            )
            where TEdge : IEdge<TVertex>
        {
            Contract.Requires(predecessors != null);
            Contract.Requires(edgeCosts != null);

            double cost = 0;
            TVertex current = target;
            TEdge edge;

            while (predecessors.TryGetValue(current, out edge))
            {
                cost += edgeCosts[edge];
                current = edge.Source;
            }

            return cost;
        }

        public static IDisjointSet<TVertex> ComputeDisjointSet<TVertex, TEdge>(
#if !NET20
this 
#endif
            IUndirectedGraph<TVertex, TEdge> visitedGraph)
            where TEdge : IEdge<TVertex>
        {
            Contract.Requires(visitedGraph != null);

            var ds = new ForestDisjointSet<TVertex>(visitedGraph.VertexCount);
            foreach (var v in visitedGraph.Vertices)
                ds.MakeSet(v);
            foreach (var e in visitedGraph.Edges)
                ds.Union(e.Source, e.Target);

            return ds;
        }

        class PrimRelaxer
            : IDistanceRelaxer
        {
            public static readonly IDistanceRelaxer Instance = new PrimRelaxer();

            public double InitialDistance
            {
                get { return double.MaxValue; }
            }

            public int Compare(double a, double b)
            {
                return a.CompareTo(b);
            }

            public double Combine(double distance, double weight)
            {
                return weight;
            }
        }
    }
}
