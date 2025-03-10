using QuikGraph;
namespace TestGraphModel
{
    //public class Graph
    //{
    //    public List<Node> Nodes { get; set; } = new List<Node>();
    //    public List<Edge> Edges { get; set; } = new List<Edge>();
    //}

    //public class Node
    //{
    //    public int Id { get; set; }
    //    public int PortsNumber { get; set; }
    //    public string NodeName { get; set; }
    //    public NodeData SimpleData { get; set; }
    //    public List<Port> Ports { get; set; } = new List<Port>();
    //}

    //public class Port
    //{
    //    public int Id { get; set; }
    //    public int LocalId { get; set; }
    //    public int InputPortNumber { get; set; }
    //    public string? InputNodeName { get; set; }
    //    public bool IsLeftSidePort { get; set; }
    //}



    //public class Edge
    //{
    //    public int Id { get; set; }
    //    public int SourceNodeId { get; set; }
    //    public Port? PortSource { get; set; }
    //    public int TargetNodeId { get; set; } 
    //    public Port? PortTarget { get; set; }
    //}

    //public class NodeData
    //{
    //    public string? SomeText { get; set; }
    //    public int SomeValue { get; set; }
    //}


    public class Graph : BidirectionalGraph<Node, Edge>
    {
        // Конструктор по умолчанию
        public Graph() : base() { }

        // Пользовательский конструктор
        public Graph(string _test) : base()
        {
            Node n1 = new Node
            {
                Id = 1,
                NodeName = "Node1",
                SimpleData = new NodeData(),
                Ports = new List<Port>
            {
                new Port { Id = 1, InputPortNumber = 0 },
                new Port { Id = 2, InputPortNumber = 0 },
                new Port { Id = 3, InputPortNumber = 0 }
            }
            };

            Node n2 = new Node
            {
                Id = 2,
                NodeName = "Node2",
                SimpleData = new NodeData(),
                Ports = new List<Port>
            {
                new Port { Id = 4, InputPortNumber = 0 },
                new Port { Id = 5, InputPortNumber = 0 },
                new Port { Id = 6, InputPortNumber = 0 },
                new Port { Id = 7, InputPortNumber = 0 }
            }
            };

            this.AddVertex(n1);
            this.AddVertex(n2);
        }
    }

    public class Node
    {
        public int Id { get; set; }
        public int PortsNumber { get; set; }
        public required string NodeName { get; set; }
        public required NodeData SimpleData { get; set; }
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



    public class Edge(Node source, Node target) : IEdge<Node>
    {
        public int Id { get; set; }
        public Node Source { get; set; } = source;
        public Node Target { get; set; } = target;

        //public int SourceNodeId { get; set; }
        public required Port PortSource { get; set; }
        //public int TargetNodeId { get; set; }
        public required Port PortTarget { get; set; }
    }

    public class NodeData
    {
        public string SomeText { get; set; }
        public int SomeValue { get; set; }
    }

    //Data Transfer Object
    //public class GraphDto
    //{
    //    public int EdgeCapacity { get; set; }
    //    public required string VertexType { get; set; }
    //    public required string EdgeType { get; set; }
    //    public bool IsDirected { get; set; }
    //    public bool AllowParallelEdges { get; set; }
    //    public bool IsVerticesEmpty { get; set; }
    //    public int VertexCount { get; set; }
    //    public required List<NodeDto> Vertices { get; set; }
    //    public bool IsEdgesEmpty { get; set; }
    //    public int EdgeCount { get; set; }
    //    public required List<EdgeDto> Edges { get; set; }
    //}

    //public class NodeDto
    //{
    //    public int Id { get; set; }
    //    public int PortsNumber { get; set; }
    //    public required string NodeName { get; set; }
    //    public required List<PortDto> Ports { get; set; }
    //}

    public class GraphDto
    {
        public List<NodeDto> Nodes { get; set; } = new List<NodeDto>();
        public List<EdgeDto> Edges { get; set; } = new List<EdgeDto>();
    }

    public class NodeDto
    {
        public int Id { get; set; }
        public int PortsNumber { get; set; }
        public required string NodeName { get; set; }
        public required NodeDataDto SimpleData { get; set; }
        public List<PortDto> Ports { get; set; } = new List<PortDto>();
    }

    //public class PortDto
    //{
    //    public int Id { get; set; }
    //    public int LocalId { get; set; }
    //    public int InputPortNumber { get; set; }
    //    public bool IsLeftSidePort { get; set; }
    //}
    public class PortDto
    {
        public int Id { get; set; }
        public int LocalId { get; set; }
        public int InputPortNumber { get; set; }
        public string? InputNodeName { get; set; }
        public bool IsLeftSidePort { get; set; }
    }

    //public class EdgeDto
    //{
    //    public int SourceId { get; set; }
    //    public int TargetId { get; set; }
    //    public int SourcePortId { get; set; }
    //    public int TargetPortId { get; set; }
    //}
    public class EdgeDto
    {
        public int Id { get; set; }
        public int SourceId { get; set; } // Используем ID вместо объекта Node
        public int TargetId { get; set; } // Используем ID вместо объекта Node
        public int SourcePortId { get; set; }
        public int TargetPortId { get; set; }
        public required PortDto PortSource { get; set; }
        public required PortDto PortTarget { get; set; }
    }

    public class NodeDataDto
    {
        public string SomeText { get; set; }
        public int SomeValue { get; set; }
    }

}
