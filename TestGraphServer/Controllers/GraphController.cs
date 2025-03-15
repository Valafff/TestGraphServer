using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using TestGraphModel;
using QuikGraph;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace TestGraphServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GraphController : ControllerBase
    {
        private static Graph graph = new Graph("hardCode");


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

        //Отправка состояния графа передаваемое клиентом
        [HttpPost("setgraphstate")] 
        public IActionResult SetGraphState([FromBody] string _graph)
        {
            if (_graph == null)
            {
                return BadRequest("Граф не может быть null.");
            }
            GraphDtoToGrath(_graph, ref graph, out bool _error);
            if (!_error)
            {
                Console.WriteLine("Граф получен от клиента.");
                return Ok();
            }
            else
            {
                Console.WriteLine("Ошибка получения графа.");
                return BadRequest();
            }
        }


        [HttpPost("createnode")]
        public IActionResult CreateNode([FromBody] Node node)
        {
            //if (graph.Nodes.Any(n => n.NodeName == node.NodeName))
            if (graph.Vertices.Any(n => n.NodeName == node.NodeName))
            {
                Console.WriteLine($"Узел именем {node.NodeName} уже существует.");
                return BadRequest($"Узел именем {node.NodeName} уже существует.");
            }

            node.Id = nextNodeId++;
            node.Ports = PortCreator(node.PortsNumber);
            //graph.Nodes.Add(node);
            graph.AddVertex(node);
            Console.WriteLine($"Узел {node.NodeName} с Id {node.Id} добавлен в граф.");
            return Ok(graph);
        }

        [HttpPost("editnode")]
        public IActionResult EditNode([FromBody] Node newNode)
        {
            //проверка существования узла с заданным именем
            //Node editNode = graph.Nodes.FirstOrDefault(n => n.Id == newNode.Id);
            Node editNode = graph.Vertices.FirstOrDefault(n => n.Id == newNode.Id);
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
        public IActionResult EdgeWorks([FromBody] EdgeDtoIdOnly _edge)

        {
            //Узел источник
            //Node sourceNode = graph.Nodes.FirstOrDefault(n => n.Id == _edge.SourceNodeId);
            Node sourceNode = graph.Vertices.FirstOrDefault(n => n.Id == _edge.SourceId);
            Port sourcePort = sourceNode.Ports.FirstOrDefault(p => p.Id == _edge.SourcePortId);

            //Port sourcePort = _edge.PortSource;
            //Узел приемник
            Node targetNode = graph.Vertices.FirstOrDefault(n => n.Id == _edge.TargetId);
            Port targetPort = targetNode.Ports.FirstOrDefault(p => p.Id == _edge.TargetPortId);
            //Port targetPort = _edge.PortTarget;

            //Проверка корректности входных данных
            if (sourceNode == null || targetNode == null || sourcePort == null || targetPort == null)
            {
                Console.WriteLine("Узел или порт ребра отсутствует или задан не верно.");
                return BadRequest("Узел или порт ребра отсутствует или задан не верно.");
            }

            //Проверка существования ребра
            Edge testEdge = graph.Edges.FirstOrDefault(e => e.Source.Id == _edge.SourceId
            && e.Target.Id == _edge.TargetId
            && e.PortSource.Id == _edge.SourcePortId
            && e.PortTarget.Id == _edge.TargetPortId);

            if (testEdge != null)
            {
                Console.WriteLine("Ребро уже создано.");
                return BadRequest("Ребро уже создано.");
            }

            //Проверка установления связи с портами одного и того же узла
            if (_edge.SourceId == _edge.TargetId)
            {
                Console.WriteLine("Попытка установить связь между портами одного и того же узла.");
                return BadRequest("Попытка установить связь между портами одного и того же узла.");
            }

            //Проверка создания связи с уже занятым целевым портом
            if (!string.IsNullOrEmpty(targetNode.Ports.FirstOrDefault(p => p.Id == _edge.TargetPortId).InputNodeName))
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
                graph.RemoveEdge(temp);
            }

            sourcePort.InputPortNumber = targetPort.Id;
            sourcePort.InputNodeName = targetNode.NodeName;
            targetPort.InputPortNumber = sourcePort.Id;
            targetPort.InputNodeName = sourceNode.NodeName;

            //Cоздание/редактирование ребра
            var newEdge = new Edge(sourceNode, targetNode)
            {
                Id = nextEdgeId++,
                PortSource = sourcePort,
                PortTarget = targetPort
            };

            graph.AddEdge(newEdge);
            Console.WriteLine($"Ребро создано между узлом {sourceNode.NodeName} портом {sourcePort.Id} и узлом {targetNode.NodeName} портом {targetPort.Id}.");
            Console.WriteLine($"Порт источник содержит: связанный порт {newEdge.PortSource.InputPortNumber} имя связанного узла {newEdge.PortSource.InputNodeName}");
            Console.WriteLine($"Порт приемник содержит: связанный порт {newEdge.PortTarget.InputPortNumber} имя связанного узла {newEdge.PortSource.InputNodeName}");
            return Ok(graph);

        }

        [HttpDelete("deleteedge/{edgeId}")]
        public IActionResult DeleteEdge(int edgeId)
        {
            var edge = graph.Edges.FirstOrDefault(e => e.Id == edgeId);
            if (edge == null)
            {
                Console.WriteLine($"Ребро с id {edgeId} не найдено.");
                return NotFound("Ребро не найдено.");
            }

            graph.RemoveEdge(edge);
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

        void GraphDtoToGrath(string _content, ref Graph _graph, out bool _error)
        {
            try
            {
                if (_graph != null)
                {
                    _graph.Clear();
                }
                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore, // Игнорировать null-значения
                    MissingMemberHandling = MissingMemberHandling.Ignore, // Игнорировать отсутствующие свойства
                    ContractResolver = new CamelCasePropertyNamesContractResolver() // Использовать camelCase
                };
                var graphDto = JsonConvert.DeserializeObject<GraphDto>(_content);
                // Добавление узлов в граф
                foreach (var nodeDto in graphDto.Vertices)
                {
                    Node node = new Node
                    {
                        Id = nodeDto.Id,
                        PortsNumber = nodeDto.PortsNumber,
                        NodeName = nodeDto.NodeName,
                        X = nodeDto.X,
                        Y = nodeDto.Y,
                        SimpleData = new NodeData() { SomeText = nodeDto.SimpleData.SomeText, SomeValue = nodeDto.SimpleData.SomeValue },
                        Ports = nodeDto.Ports.Select(p => new Port
                        {
                            Id = p.Id,
                            LocalId = p.LocalId,
                            InputPortNumber = p.InputPortNumber,
                            IsLeftSidePort = p.IsLeftSidePort
                        }).ToList()
                    };

                    _graph.AddVertex(node);
                }

                // Добавление ребер в граф
                foreach (var edgeDto in graphDto.Edges)
                {
                    var sourceNode = _graph.Vertices.FirstOrDefault(n => n.Id == edgeDto.Source.Id);
                    var targetNode = _graph.Vertices.FirstOrDefault(n => n.Id == edgeDto.Target.Id);

                    if (sourceNode != null && targetNode != null)
                    {
                        var edge = new Edge(sourceNode, targetNode);
                        _graph.AddEdge(edge);
                    }
                }
                _error = false;
            }
            catch (System.Text.Json.JsonException ex)
            {
                _error = true;
                throw;
            }
        }
    }
}