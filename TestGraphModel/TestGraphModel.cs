using QuikGraph;
namespace TestGraphModel
{
    public class Graph : BidirectionalGraph<Node, Edge>
    {
        //Добавлены как второстепенные поля
        public int Id { get; set; }
        public string GraphName { get; set; }
        // Конструктор по умолчанию
        public Graph() : base() { }

        // Для тестов
        public Graph(string _test) : base()
        {
            Id = 0;
            GraphName = "TestGraph";
            Node n1 = new Node
            {
                Id = 1,
                NodeName = "Node1",
                SimpleData = new NodeData() { SomeText = "Хардкод1", SomeValue = 42 },
                X = 100,
                Y = 100,
                Ports = new List<Port>
            {
                new Port { Id = 1, InputPortNumber = 1, IsLeftSidePort = false, LocalId = 1 },
                new Port { Id = 2, InputPortNumber = 2 , IsLeftSidePort = true, LocalId = 2 },
                new Port { Id = 3, InputPortNumber = 3, IsLeftSidePort = false, LocalId = 3 }
            }
            };

            Node n2 = new Node
            {
                Id = 2,
                NodeName = "Node2",
                SimpleData = new NodeData() { SomeText = "Хардкод2", SomeValue = 888 },
                X = 300,
                Y = 300,
                Ports = new List<Port>
            {
                new Port { Id = 4, InputPortNumber = 1, IsLeftSidePort = false, LocalId = 1},
                new Port { Id = 5, InputPortNumber = 2, IsLeftSidePort = true, LocalId = 2},
                new Port { Id = 6, InputPortNumber = 3, IsLeftSidePort = false, LocalId = 3},
                new Port { Id = 7, InputPortNumber = 4, IsLeftSidePort = true, LocalId = 4}
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
        public string NodeName { get; set; }
        public NodeData SimpleData { get; set; }
        public List<Port> Ports { get; set; } = new List<Port>();
        public double X { get; set; }
        public double Y { get; set; }
    }


    public class Port
    {
        public int Id { get; set; }
        public int LocalId { get; set; }
        public int InputPortNumber { get; set; }
        public string? InputNodeName { get; set; }
        public bool IsLeftSidePort { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
    }



    public class Edge(int _id, Node source, Node target, Port portSource, Port portTarget) : IEdge<Node>
    {
        public int Id { get; set; } = _id;
        public Node Source { get; set; } = source;
        public Node Target { get; set; } = target;
        public Port PortSource { get; set; } = portSource;
        public Port PortTarget { get; set; } = portTarget;
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
        public string NodeName { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public NodeDataDto SimpleData { get; set; }
        public List<PortDto> Ports { get; set; } = new List<PortDto>();
    }

    public class PortDto
    {
        public int Id { get; set; }
        public int LocalId { get; set; }
        public int InputPortNumber { get; set; }
        public string? InputNodeName { get; set; }
        public bool IsLeftSidePort { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
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
        public double X_source { get; set; }
        public double Y_source { get; set; }
        public double X_target { get; set; }
        public double Y_target { get; set; }
        public string? InputToSourcePort { get; set; }
        public string? InputToTargetPort { get; set; }
        public int InputToSourceNumber { get; set; }
        public int InputToTargetNumber { get; set; }
    }

    public class NodeDataDto
    {
        public string SomeText { get; set; }
        public int SomeValue { get; set; }
    }

}
