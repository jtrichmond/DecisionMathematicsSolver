using System;
using System.Linq; //for .Contains() and .OrderBy()
using System.Collections.Generic; 
using System.Diagnostics; //for feedback when debugging
using Exceptions_Library;

namespace Graph_Library
{
    public interface INetwork
    {
        //unweighted graphs are not considered, therefore all graph objects shall be considered networks
        //Some methods are not used, as they were written to provide general functionality
        //Development did not reach the point where they were needed
        
        string[] GetNodeNames();
        bool IsDirected();
        bool SetNode(string nodeName, IDictionary<string, IList<double>> arcsFromNode);
        //dictionary contains info on all the arcs from that node

        bool SetArcs(string startNode, string endNode, IList<double> arcList);
        //replaces list of arcs from node with one provided

        bool AddNode(string nodeName, IDictionary<string, IList<double>> arcsFromNode);

        bool AddArc(string startNode, string endNode, double arcWeight);

        IDictionary<string, IList<double>> GetNode(string nodeName);
        //Gets data of all arcs from that node

        bool TryGetNode(string trialNode, out IDictionary<string, IList<double>> outNode);
        //Returns bool value for whether node is in graph
        //returns data if that node is in the graph

        IList<double> GetArcs(string startNode, string endNode);

        double? GetShortestArc(string startNode, string endNode);
        //can be null

        bool RemoveNode(string nodeName);

        bool RemoveArc(string startNode, string endNode, double arcWeight);

        (string[],double[],string[]) Dijkstra(string startNode);
        //returns names, shortest path weights, and previous nodes
    }

    internal interface IDijkstraNode
    {
        string Name { get; set; }
        double Weight { get; set; }
        string PreviousNode { get; set; }
    }

    public enum GraphAlgorithm
    //could be expanded later if more algorithms implemented
    {
        Dijkstra
    };


    public class Network : INetwork
    {
        //attributes
        private IDictionary<string, IDictionary<string, IList<double>>> _graphData;
        private bool _directed;

        //constructors
        public Network(IDictionary<string, IDictionary<string, IList<double>>> graphData, bool isDirected)
        {
            string errorMessage = "";
            if (this.SetGraphData(graphData) == false)
            {
                errorMessage = errorMessage + $"Failed to set graph data: {graphData} \n";
            }

            this.SetDirected(isDirected);
            //do not need to check, as bool values cannot be null. will fail beforehand

            if (errorMessage != "")
            {
                throw new NetworkConstructionException(errorMessage);
            }
        }


        public Network(string[] names, IList<double>[,] weights, bool isDirected)
        {
            //array of lists used to hold arcs
            string errorMessage = "";
            if (names.GetLength(0) != weights.GetLength(0))
            {
                errorMessage += $"Name and weight arrays not the same size: Name array has length {names.GetLength(0)} " +
                    $"and weight array has length {weights.GetLength(0)} \n";
            }

            if (weights.GetLength(0) != weights.GetLength(1))
            {
                errorMessage += $"Weight matrix not square: {weights.GetLength(0)} by {weights.GetLength(1)} \n";
            }

            if (errorMessage != "")
            {
                throw new NetworkConstructionException("Network construction failed: \n" + errorMessage);
                //thrown before Dictionary construction so that useful errorMessage displayed
                //rather than failing due to indexing, as well as at end
            }

            this._graphData = new Dictionary<string, IDictionary<string, IList<double>>>();
            for (int i = 0; i < names.GetLength(0); i++)
            {
                IDictionary<string, IList<double>> nodeDictionary = new Dictionary<string, IList<double>>();
                for (int j = 0; j < names.GetLength(0); j++)
                {
                    nodeDictionary.Add(names[j], weights[i, j]);
                }

                if (this.AddNode(names[i], nodeDictionary) == false)
                {
                    errorMessage += $"Failed to add node {names[i]}, with arcs {nodeDictionary}";
                }
            }

            this.SetDirected(isDirected);
            // do not need to check, bool cannot be null

            if (errorMessage != "")
            {
                throw new NetworkConstructionException("Network construction failed: \n" + errorMessage);
            }
        }

        //debugging methods

        public override bool Equals(object obj)
        {
            if (obj is null or not INetwork)
                return false;
            INetwork trial = (INetwork) obj;
            string[] thisNodeNames = this.GetNodeNames();
            if (thisNodeNames.GetLength(0) != trial.GetNodeNames().GetLength(0))
            {
                return false;
            }

            foreach (string node in thisNodeNames)
            {
                IDictionary<string, IList<double>> thisNode = this.GetNode(node);

                if (!(trial.TryGetNode(node, out IDictionary<string, IList<double>> trialNode)) ||
                    (thisNode.Count != trialNode.Count))
                {
                    return false;
                }

                foreach (var keyValuePair in thisNode)
                {
                    if (!(thisNode.TryGetValue(keyValuePair.Key, out IList<double> thisNodeArcs) &&
                          trialNode.TryGetValue(keyValuePair.Key, out IList<double> trialNodeArcs)))
                    {
                        return false;
                    }

                    if (thisNodeArcs.Count != trialNodeArcs.Count)
                    {
                        return false;
                    }
                    else
                    {
                        for (int i = 0; i < thisNodeArcs.Count; i++)
                        {
                            if (thisNodeArcs[i] - trialNodeArcs[i] != 0)
                                //possible loss of precision by rounding during equality comparison of floating points
                            {
                                return false;
                            }
                        }
                    }
                }
            }

            return true;
        }

        public override string ToString()
        {
            string returnString = "";
            foreach (var keyValuePair in this._graphData)
            {
                returnString = returnString + "Node: " + keyValuePair.Key + " Arcs: ";
                foreach (var nodeArcPair in keyValuePair.Value)
                {
                    string arcsString = string.Join(",", nodeArcPair.Value);
                    returnString += $"[{nodeArcPair.Key}: {arcsString}],";
                }

                returnString += "\n";
            }

            return returnString;
        }

        //private methods

        private bool SetGraphData(IDictionary<string, IDictionary<string, IList<double>>> graphData)
        {
            if (graphData != null) //could add additional checks here if needed
            {
                this._graphData = graphData;
                return true;
            }
            else
            {
                return false;
            }
        }

        private void SetDirected(bool isDirected) //isDirected cannot be null
        {
            this._directed = isDirected;
        }

        private bool CheckForInvalidArcs(IList<double> arcList)
        {
            //negative or 0 arcs are not possible in a network
            //0 arcs would mean two separate nodes were the same
            foreach (double arc in arcList)
            {
                if (arc <= 0)
                    return false;
            }

            return true;
        }


        //public methods
        public string[] GetNodeNames()
        {
            string[] nodeNames = new string[this._graphData.Count];
            this._graphData.Keys.CopyTo(nodeNames, 0);
            return nodeNames;
        }

        public bool IsDirected()
        {
            return this._directed;
            //would be used in the route inspection algorithm (Susie, et al., 2017)
        }

        public bool SetNode(string nodeName, IDictionary<string, IList<double>> arcsFromNode)
        {
            string[] nodeNames = this.GetNodeNames();
            if (!(nodeNames.Contains(nodeName)) || (arcsFromNode == null))
                return false;
            foreach (var endNode in arcsFromNode)
            {
                if (this.CheckForInvalidArcs(endNode.Value) == false)
                    return false;
            }

            this._graphData[nodeName] = arcsFromNode;
            return true;
        }

        public bool SetArcs(string startNode, string endNode, IList<double> arcList)
        {
            string[] nodeNames = this.GetNodeNames();
            if (!(nodeNames.Contains(startNode) && nodeNames.Contains(endNode)))
            {
                return false;
            }

            if (arcList == null || this.CheckForInvalidArcs(arcList) == false)
            {
                return false;
            }

            this._graphData[startNode][endNode] = arcList;
            return true;
        }

        public bool AddNode(string nodeName, IDictionary<string, IList<double>> arcsFromNode)
        {
            if (this.GetNodeNames().Contains(nodeName) || arcsFromNode == null)
            {
                return false;
            }

            foreach (var endNode in arcsFromNode)
            {
                if (endNode.Value is null)
                    continue;
                
                if (this.CheckForInvalidArcs(endNode.Value) == false)
                    return false;
                
            }

            this._graphData[nodeName] = arcsFromNode;
            return true;
        }

        public bool AddArc(string startNode, string endNode, double arcWeight)
        {
            string[] nodeNames = this.GetNodeNames();
            if (!(nodeNames.Contains(startNode) && nodeNames.Contains(endNode)))
            {
                return false;
            }

            if (arcWeight <= 0)
                return false;

            if (!(this._graphData[startNode].Keys.Contains(endNode)) ||
                this._graphData[startNode][endNode] == null) //ensures there is a list to add to
                this._graphData[startNode][endNode] = new List<double>();

            this._graphData[startNode][endNode].Add(arcWeight);
            return true;
        }

        public IDictionary<string, IList<double>> GetNode(string nodeName)
            //will fail if node not in network, use TryGetNode otherwise
        {
            return this._graphData[nodeName];
        }

        public bool TryGetNode(string trialNode, out IDictionary<string, IList<double>> outNode)
        {
            return this._graphData.TryGetValue(trialNode, out outNode);
        }

        public IList<double> GetArcs(string startNode, string endNode)
        {
            return !(this._graphData[startNode].Keys.Contains(endNode))
                ? null
                : this._graphData[startNode][endNode]; 
            //will return null if there are no arcs between the nodes
            //preventing dictionary trying to access non-existent key
        }

        public double? GetShortestArc(string startNode, string endNode)
        {
            IList<double> arcsBetweenNodes = this.GetArcs(startNode, endNode);
            if (arcsBetweenNodes is null)
                return null; //list constructor does not accept null values
            List<double> arcList = new List<double>(arcsBetweenNodes);

            arcList.Sort();
            return arcList[0]; //first element in sorted list
        }

        public bool RemoveNode(string nodeName)
        {
            return this._graphData.Remove(nodeName);
        }

        public bool RemoveArc(string startNode, string endNode, double arcWeight)
        {
            return this._graphData[startNode][endNode].Remove(arcWeight);
        }

        public (string[],double[],string[]) Dijkstra(string startNode)
            //returns tuple rather than DijkstraList so that DijkstraList is internal
            //external uses therefore do not depend on its implementation
        {
            DijkstraQueue queue = new DijkstraQueue(this.GetNodeNames(), startNode);
            Trace.WriteLine($"Queue is {queue}");
            IList<IDijkstraNode> returnList = new List<IDijkstraNode>();

            while (!(queue.IsEmpty()))
            {
                IDijkstraNode current = queue.DeQueue();
                returnList.Add(current);
                Trace.WriteLine($"Current node is {current}");
                IList<IDijkstraNode> nodeList = queue.List;
                foreach (var node in nodeList)
                {
                    double? arc = this.GetShortestArc(current.Name, node.Name);
                    if (arc is { } arcWeight) //not null pattern
                    {
                        double weightThroughCurrentNode = current.Weight + arcWeight;
                        if (weightThroughCurrentNode < node.Weight)
                        {
                            queue.SetNode(node.Name, weightThroughCurrentNode, current.Name);
                            Trace.WriteLine($"Node {node.Name} updated: new weight {weightThroughCurrentNode}," +
                                $" with previous node {node.PreviousNode}");
                        }
                    }
                }

                queue.Sort(); //sorts here rather than in SetNode so that foreach does not miss nodes.
                Trace.WriteLine($"Return List is {string.Join(", ", returnList)}");
            }

            string[] names = new string[returnList.Count];
            double[] weights = new double[returnList.Count];
            string[] previousNodes = new string[returnList.Count];
            for (int i = 0; i < returnList.Count; i++)
            {
                names[i] = returnList[i].Name;
                weights[i] = returnList[i].Weight;
                previousNodes[i] = returnList[i].PreviousNode;
            }

            return (names, weights, previousNodes);
        }
    }

    internal class DijkstraNode : IDijkstraNode //container for Dijkstra's
    {
        private string _name;
        private double _weight;
        private string _previousNode;

        //constructor
        public DijkstraNode(string name, double weight, string previousNode)
        {
            Name = name;
            Weight = weight;
            PreviousNode = previousNode;
        }

        //debugging methods
        public override bool Equals(object obj)
        {
            if (obj is IDijkstraNode trial)
            {
                if (this.Name == trial.Name && this.Weight - trial.Weight == 0 &&
                    this.PreviousNode == trial.PreviousNode)
                    return true;
                return false;
            }
            else
            {
                return false;
            }
        }

        public override string ToString()
        {
            return ($"Name {this.Name}: " +
                $"Weight {(double.IsPositiveInfinity(this.Weight) ? "Infinity" : this.Weight)}: " +
                $"PreviousNode {this.PreviousNode}");
        }

        //properties
        public string Name
        {
            set
            {
                if (value != null)
                    this._name = value;
            }

            get => this._name;
        }

        public double Weight
        {
            set
            {
                if (value >= 0)
                    this._weight = value;
            }
            get => this._weight;
        }

        public string PreviousNode
        {
            set
            {
                if (value != this._name)
                    this._previousNode = value;
            }
            get => this._previousNode;
        }
    }

    internal class DijkstraQueue
    {
        //constructor
        internal IList<IDijkstraNode> List { get; private set; }

        internal DijkstraQueue(string[] nodeNames, string startNode)
        {
            this.List = new List<IDijkstraNode>();
            foreach (string node in nodeNames)
            {
                this.List.Add(node == startNode
                    ? new DijkstraNode(node, 0, "None")
                    : new DijkstraNode(node, double.PositiveInfinity, startNode));
            }

            this.Sort();
        }

        //debugging methods
        public override string ToString()
        {
            return string.Join(", ", this.List);
        }

        public override bool Equals(object obj)
        {
            if (obj is null or not DijkstraQueue)
                return false;
            DijkstraQueue trial = (DijkstraQueue)obj;
            if (this.List != trial.List)
                return false;
            else return true;
        }

        //functional methods


        internal bool SetNode(string name, double weight, string previousNode)
        {
            bool found = false;
            int i = 0;

            while (!(found) && i < this.List.Count)
            {
                if (this.List[i].Name == name)
                {
                    this.List[i].Weight = weight;
                    this.List[i].PreviousNode = previousNode;
                    found = true;
                }
                else
                {
                    i += 1;
                }
            }

            return found;
        }

        internal bool IsEmpty()
        {
            return (this.List.Count == 0);
        }

        internal IDijkstraNode DeQueue()
        {
            IDijkstraNode returnNode = this.List[0];
            this.List.Remove(returnNode);
            return returnNode;
        }

        internal void Sort()
        {
            this.List = this.List.OrderBy(node => node.Weight).ToList();
            //sorts the nodes by their weight, in ascending order
        }
        
        //these methods allowed for the queue to be viewed easily when debugging
        //they are not necessary for the final solution

        internal string[] GetNames()
        {
            string[] names = new string[List.Count];
            for (int i = 0; i < List.Count; i++)
            {
                names[i] = List[i].Name;
            }
            return names;
        }

        internal double[] GetWeights()
        {
            double[] weights = new double[List.Count];
            for (int i = 0; i < List.Count; i++)
            {
                weights[i] = List[i].Weight;
            }
            return weights;
        }

        internal string[] GetPreviousNodes()
        {
            string[] previousNodes = new string[List.Count];
            for (int i = 0; i < List.Count; i++)
            {
                previousNodes[i] = List[i].PreviousNode;
            }
            return previousNodes;
        }

    }

    public struct ArrayNetworkRowInput
    {
        //for DataGrid input
        public string NodeName { get; init; }
        public string[] Values { get; set; }

        public override string ToString()
        {
            string returnString = NodeName + ":";
            foreach (string value in Values)
            {
                if (value is "" or null)
                    returnString += "-";
                else
                    returnString += value;
            }
            return returnString;
        }
    }

    public static class NetworkFactory
    {
        private static IList<double> ParseArrayToDoubleList(string[] array)
        {
            //converts an array of strings into a list of doubles
            IList<double> doubleList = new List<double>();

            foreach (string trial in array)
            {
                if (trial is null or "" or " ")
                    continue;
                //stops empty spaces being parsed

                if (double.TryParse(trial, out double value))
                {
                    doubleList.Add(value);
                }
                else
                    throw new ParseException($"Failed to parse {trial} into a number");
            }

            return doubleList;
        }

        private static string[] SplitWeights (string rawString, char splitCharacter)
        {
            return rawString.Contains(splitCharacter) is false 
                ? new [] {rawString} 
                : rawString.Split(splitCharacter);
            //turns the string into a 1 item array if there are no items to split
            //otherwise returns array of the items split
        }

        public static (string[] Names, IList<double>[,] Weights) ExtractFromInput
            (ArrayNetworkRowInput[] input, char splitCharacter)
        {
            string[] names = new string[input.GetLength(0)];
            IList<double>[,] weights = new IList<double>[input.GetLength(0), input.GetLength(0)];

            for (int i = 0; i < input.GetLength(0); i++)
            {
                names[i] = input[i].NodeName;
                for (int j = 0; j < input.GetLength(0); j++)
                {
                    if (input[i].Values[j] is not null)
                    {
                        try
                        {
                            string[] stringWeights = SplitWeights(input[i].Values[j], splitCharacter);
                            weights[i, j] = ParseArrayToDoubleList(stringWeights);
                        }
                        catch (Exception error)
                        {
                            if (error is SplitException or ParseException)
                            {
                                throw new NetworkConversionException
                                    ($"Failed to convert data into a network: \n {error.Message}");
                                //easier exception handling in code-behind for GUI if only one error thrown
                            }
                            else throw; //only those exceptions can be handled safely
                        }
                    }
                    
                }
            }

            return (names, weights);
        }
    }
}