namespace TestGraphModel
{
    public class Graph
    {
        public List<Node> Nodes { get; set; } = new List<Node>();
        public List<Edge> Edges { get; set; } = new List<Edge>();
    }

    public class Node
    {
        public int Id { get; set; }
        public int PortsNumber { get; set; }
        public string NodeName { get; set; }
        public NodeData SimpleData { get; set; }
        public List<Port> Ports { get; set; } = new List<Port>();
    }

    public class Port
    {
        public int Id { get; set; }
        public int LocalId { get; set; }
        public int InputPortNumber { get; set; }
        public string? InputNodeName { get; set; }
        public bool IsLeftSidePort { get; set; }
    }



    public class Edge
    {
        public int Id { get; set; }
        public int SourceNodeId { get; set; }
        public Port? PortSource { get; set; }
        public int TargetNodeId { get; set; }
        public Port? PortTarget { get; set; }
    }

    public class NodeData
    {
        public string? SomeText { get; set; }
        public int SomeValue { get; set; }
    }
}
