using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using TestGraphModel;

namespace TestGraphServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GraphController : ControllerBase
    {
        //Хардкод
        private static Graph graph = new Graph
        {
            Nodes = new List<Node>
            {
                new Node { Id = 1, NodeName = "Node1", Ports =
                    new List<Port> { new Port { Id = 1, InputPortNumber = 0 },
                    new Port { Id = 2, InputPortNumber = 0 }, 
                    new Port { Id = 3, InputPortNumber = 0 } } },
                new Node { Id = 2, NodeName = "Node2", Ports =
                    new List<Port> { new Port { Id = 4, InputPortNumber = 0 }, 
                    new Port { Id = 5, InputPortNumber = 0 }, 
                    new Port { Id = 6, InputPortNumber = 0 }, 
                    new Port { Id = 7, InputPortNumber = 0 } } }
            },
            Edges = new List<Edge>()
        };


        private static int nextNodeId = 3; // Следующий Id для нового узла
        private static int nextPortId = 8; // Следующий Id для нового порта
        private static int nextEdgeId = 1; // Следующий Id для нового ребра

        //Проверка соединения
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            Console.WriteLine("Клиент  проверяет подключение");
            return Ok("pong");
        }

        //Отправка модели графа клиенту при первом подключении
        [HttpGet("sendgraph")]
        public IActionResult SendGraph()
        {
            Console.WriteLine("Граф отправлен клиенту.");
            return Ok(graph);
        }

        [HttpPost("createnode")]
        public IActionResult CreateNode([FromBody] Node node)
        {
            if (graph.Nodes.Any(n => n.NodeName == node.NodeName))
            {
                Console.WriteLine($"Узел именем {node.NodeName} уже существует.");
                return BadRequest($"Узел именем {node.NodeName} уже существует.");
            }

            node.Id = nextNodeId++;
            node.Ports = PortCreator(node.PortsNumber);
            graph.Nodes.Add(node);
            Console.WriteLine($"Узел {node.NodeName}с Id {node.Id} добавлен в граф.");
            return Ok(graph);
        }

        [HttpPost("editnode")]
        public IActionResult EditNode([FromBody] Node newNode)
        {
            //проверка существования узла с заданным именем
            Node editNode = graph.Nodes.FirstOrDefault(n => n.Id == newNode.Id);
            if (editNode == null)
            {
                Console.WriteLine($"Узел с id {newNode.Id} не найден.");
                return NotFound($"Узел с id {newNode.Id} не найден.");
            }
            //Проверка имени узала в ребрах
            if (newNode.NodeName != editNode.NodeName)
            {
                editNode.NodeName = newNode.NodeName;
                foreach (var edge in graph.Edges)
                {
                    if (edge.PortSource.InputNodeName == editNode.NodeName)
                    {
                        edge.PortSource.InputNodeName = newNode.NodeName;
                    }
                    if (edge.PortTarget.InputNodeName == editNode.NodeName)
                    {
                        edge.PortTarget.InputNodeName = newNode.NodeName;
                    }
                }
            }
            editNode.SimpleData = newNode.SimpleData;
            Console.WriteLine($"Узел {newNode.NodeName} обновлен.");
            return Ok(graph);
        }

        [HttpPost("edgeworks")]
        public IActionResult EdgeWorks([FromBody] Edge _edge)
        {
            try
            {
                if (_edge.PortSource != null && _edge.PortTarget!= null)
                {
                    //Узел источник
                    Node sourceNode = graph.Nodes.FirstOrDefault(n => n.Id == _edge.SourceNodeId);
                    Port sourcePort = sourceNode.Ports.FirstOrDefault(p => p.Id == _edge.PortSource.Id);

                    //Port sourcePort = _edge.PortSource;
                    //Узел приемник
                    Node targetNode = graph.Nodes.FirstOrDefault(n => n.Id == _edge.TargetNodeId);
                    Port targetPort = targetNode.Ports.FirstOrDefault(p => p.Id == _edge.PortTarget.Id);
                    //Port targetPort = _edge.PortTarget;



                    //Проверка корректности входных данных
                    if (sourceNode == null || targetNode == null || sourcePort == null || targetPort == null)
                    {
                        Console.WriteLine("Узел или порт ребра отсутствует или задан не верно.");
                        return BadRequest("Узел или порт ребра отсутствует или задан не верно.");
                    }

                    //Проверка существования ребра
                    Edge testEdge = graph.Edges.FirstOrDefault(e => e.SourceNodeId == _edge.SourceNodeId
                    && e.TargetNodeId == _edge.TargetNodeId
                    && e.PortSource.Id == _edge.PortSource.Id
                    && e.PortTarget.Id == _edge.PortTarget.Id);

                    if (testEdge != null)
                    {
                        Console.WriteLine("Ребро уже создано.");
                        return BadRequest("Ребро уже создано.");
                    }

                    //Проверка установления связи с портами одного и того же узла
                    if (_edge.SourceNodeId == _edge.TargetNodeId)
                    {
                        Console.WriteLine("Попытка установить связь между портами одного и того же узла.");
                        return BadRequest("Попытка установить связь между портами одного и того же узла.");
                    }

                    //Проверка создания связи с уже занятым целевым портом
                    if (!string.IsNullOrEmpty(targetNode.Ports.FirstOrDefault(p => p.Id == _edge.PortTarget.Id).InputNodeName))
                    {
                        Console.WriteLine("Целевой порт уже занят");
                        return BadRequest("Целевой порт уже занят");
                    }

                    //Удаление данных о связи в старом targetPort если она была
                    if (!string.IsNullOrEmpty(sourcePort.InputNodeName))
                    {
                        Edge temp = graph.Edges.FirstOrDefault(e => e.PortSource.Id == sourcePort.Id || e.PortTarget.Id == sourcePort.Id);
                        temp.PortSource.InputPortNumber = 0;
                        temp.PortSource.InputNodeName = null;
                        temp.PortTarget.InputPortNumber = 0;
                        temp.PortTarget.InputNodeName = null;
                        graph.Edges.Remove(temp);
                    }

                    sourcePort.InputPortNumber = targetPort.Id;
                    sourcePort.InputNodeName = targetNode.NodeName;
                    targetPort.InputPortNumber = sourcePort.Id;
                    targetPort.InputNodeName = sourceNode.NodeName;

                    //Cоздание/редактирование ребра
                    _edge = new Edge()
                    {
                        Id = nextEdgeId++,
                        PortSource = sourcePort,
                        PortTarget = targetPort,
                        SourceNodeId = sourceNode.Id,
                        TargetNodeId = targetNode.Id
                    };

                    graph.Edges.Add(_edge);
                    Console.WriteLine($"Ребро создано между узлом {sourceNode.NodeName} портом {sourcePort.Id} и узлом {targetNode.NodeName} портом {targetPort.Id}.");
                    Console.WriteLine($"Порт источник содержит: связанный порт {_edge.PortSource.InputPortNumber} имя связанного узла {_edge.PortSource.InputNodeName}");
                    Console.WriteLine($"Порт приемник содержит: связанный порт {_edge.PortTarget.InputPortNumber} имя связанного узла {_edge.PortSource.InputNodeName}");
                    return Ok(graph);
                }
                return BadRequest("Ошибка создания ребра");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка создания/редактирования ребра: {ex}");
                return BadRequest($"Ошибка создания/редактирования ребра: {ex}");
            }
           
        }

        [HttpDelete("deleteedge/{edgeId}")]
        public IActionResult DeleteEdge(int edgeId)
        {
            Edge edge = graph.Edges.FirstOrDefault(e => e.Id == edgeId);
            if (edge == null)
            {
                Console.WriteLine($"Ребро с id {edgeId} не найдено.");
                return NotFound("Ребро не найдено.");
            }

            graph.Edges.Remove(edge);
            Console.WriteLine($"Ребро с id {edgeId} удалено.");
            return Ok(graph);
        }

        List<Port> PortCreator(int _portsCount)
        {
            List<Port> tempPorts = new List<Port>();
            // Назначаем Id портам
            //_ports.ForEach(p =>  p.Id = _nextPortId++);
            int localId = 1;
            while (_portsCount-- > 0)
            {
                Port port = new Port();
                port.Id = nextPortId++;
                port.LocalId = localId++;

                if (port.Id % 2 == 0)
                {
                    port.IsLeftSidePort = false;
                }
                else
                    port.IsLeftSidePort = true;
                tempPorts.Add(port);
            }
            return tempPorts;
        }
    }
}