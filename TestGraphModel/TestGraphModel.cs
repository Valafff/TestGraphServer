using QuikGraph;
namespace TestGraphModel
{
    public class Graph : BidirectionalGraph<Node, Edge>
    {
        // Конструктор по умолчанию
        public Graph() : base() { }

        // Для тестов
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
        public  string NodeName { get; set; }
        public  NodeData SimpleData { get; set; }
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
        public  Port PortSource { get; set; }
        public  Port PortTarget { get; set; }
    }

    public class NodeData
    {
        public string SomeText { get; set; }
        public int SomeValue { get; set; }
    }

    public class GraphDto
    {
        public List<NodeDto> Vertices { get; set; }
        public List<EdgeDto> Edges { get; set; }
    }

    public class NodeDto
    {
        public int Id { get; set; }
        public int PortsNumber { get; set; }
        public  string NodeName { get; set; }
        public  NodeDataDto SimpleData { get; set; }
        public List<PortDto> Ports { get; set; } = new List<PortDto>();
    }

    public class PortDto
    {
        public int Id { get; set; }
        public int LocalId { get; set; }
        public int InputPortNumber { get; set; }
        public string? InputNodeName { get; set; }
        public bool IsLeftSidePort { get; set; }
    }

    public class EdgeDto
    {
        public int Id { get; set; }
        public NodeDto Source { get; set; } 
        public NodeDto Target { get; set; } 
        public PortDto PortSource { get; set; } 
        public PortDto PortTarget { get; set; } 
    }

    public class EdgeDtoIdOnly
    {
        public int Id { get; set; }
        public int SourceId { get; set; } 
        public int TargetId { get; set; } 
        public int SourcePortId { get; set; } 
        public int TargetPortId { get; set; } 
    }

    public class NodeDataDto
    {
        public string SomeText { get; set; }
        public int SomeValue { get; set; }
    }

}
