﻿using Microsoft.AspNetCore.Mvc;
using TestGraphModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace TestGraphServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GraphController : ControllerBase
    {
        private static Graph graph = new Graph("Test");

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

        //Отправка модели графа клиенту при подключении
        [HttpGet("sendgraph")]
        public IActionResult SendGraph()
        {
            Console.WriteLine("Граф отправлен клиенту.");
            return Ok(graph);
        }

        //Получение состояния графа передаваемое клиентом
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
            if (node.Ports.Count == 0)
            {
                node.Ports = PortCreator(node.PortsNumber);
            }
            else
            {
                foreach(var port in node.Ports)
                {
                    port.Id = nextPortId++;
                }
            }
            graph.AddVertex(node);
            Console.WriteLine($"Узел {node.NodeName} с Id {node.Id} добавлен в граф.");
            return Ok(graph);
        }

        [HttpPost("editnode")]
        public IActionResult EditNode([FromBody] Node newNode)
        {
            //проверка существования узла с заданным именем
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
            Node sourceNode = graph.Vertices.FirstOrDefault(n => n.Id == _edge.SourceId);
            Port sourcePort = sourceNode.Ports.FirstOrDefault(p => p.Id == _edge.SourcePortId);
            sourcePort.X = _edge.X_source;
            sourcePort.Y = _edge.Y_source;
            sourcePort.InputNodeName = _edge.InputToSourcePort;
            sourcePort.InputPortNumber = _edge.InputToSourceNumber;
            var edg = graph.Edges.FirstOrDefault(e => e.PortTarget.Id == sourcePort.Id);
            if (string.IsNullOrEmpty(sourcePort.InputNodeName) && edg != null)
            {
                sourcePort.InputNodeName = edg.PortSource.InputNodeName;
            }
            if (sourcePort.InputPortNumber == 0 && edg != null)
            {
                sourcePort.InputPortNumber = edg.PortSource.InputPortNumber;
            }


            //Узел приемник
            Node targetNode = graph.Vertices.FirstOrDefault(n => n.Id == _edge.TargetId);
            Port targetPort = targetNode.Ports.FirstOrDefault(p => p.Id == _edge.TargetPortId);
            targetPort.X = _edge.X_target;
            targetPort.Y = _edge.Y_target;
            targetPort.InputNodeName = _edge.InputToTargetPort;
            targetPort.InputPortNumber = _edge.InputToTargetNumber;
            edg = graph.Edges.FirstOrDefault(e => e.PortTarget.Id == targetPort.Id);
            if (string.IsNullOrEmpty(targetPort.InputNodeName) && edg != null)
            {
                targetPort.InputNodeName = edg.PortTarget.InputNodeName;
            }
            if (targetPort.InputPortNumber == 0 && edg != null)
            {
                targetPort.InputPortNumber = edg.PortTarget.InputPortNumber;
            }

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

            ////Проверка создания связи с уже занятым целевым портом
            //if (!string.IsNullOrEmpty(targetNode.Ports.FirstOrDefault(p => p.Id == _edge.TargetPortId).InputNodeName) && targetPort.InputPortNumber != 0)
            //{
            //    Console.WriteLine("Целевой порт уже занят");
            //    return BadRequest("Целевой порт уже занят");
            //}
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
                if (temp != null)
                {
                    temp.PortSource.InputPortNumber = 0;
                    temp.PortSource.InputNodeName = null;
                    temp.PortTarget.InputPortNumber = 0;
                    temp.PortTarget.InputNodeName = null;
                    graph.RemoveEdge(temp);
                }
            }

            sourcePort.InputPortNumber = targetPort.Id;
            sourcePort.InputNodeName = targetNode.NodeName;
            targetPort.InputPortNumber = sourcePort.Id;
            targetPort.InputNodeName = sourceNode.NodeName;

            //Cоздание/редактирование ребра
            var newEdge = new Edge(nextEdgeId++, sourceNode, targetNode, sourcePort, targetPort);

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
            edge.PortSource.InputNodeName = "";
            edge.PortTarget.InputNodeName = "";
            edge.PortSource.InputPortNumber = 0;
            edge.PortTarget.InputPortNumber = 0;
            graph.RemoveEdge(edge);
            Console.WriteLine($"Ребро с id {edgeId} удалено.");
            return Ok(graph);
        }

        List<Port> PortCreator(int _portsCount)
        {
            List<Port> tempPorts = new List<Port>();
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
                            InputNodeName = p.InputNodeName,
                            IsLeftSidePort = p.IsLeftSidePort,
                            X = p.X,
                            Y = p.Y
                        }).ToList()
                    };

                    _graph.AddVertex(node);
                }

                // Добавление ребер в граф
                foreach (var edgeDto in graphDto.Edges)
                {
                    var sourceNode = _graph.Vertices.FirstOrDefault(n => n.Id == edgeDto.Source.Id);
                    var targetNode = _graph.Vertices.FirstOrDefault(n => n.Id == edgeDto.Target.Id);
                    var sourcePortDto = (graphDto.Edges.FirstOrDefault(p => p.PortSource.Id == edgeDto.PortSource.Id)).PortSource;
                    var targetPortDto = (graphDto.Edges.FirstOrDefault(p => p.PortTarget.Id == edgeDto.PortTarget.Id)).PortTarget;
                    Port sourcePort = new Port()
                    {
                        Id = sourcePortDto.Id,
                        LocalId = sourcePortDto.LocalId,
                        InputPortNumber = sourcePortDto.InputPortNumber,
                        InputNodeName = sourcePortDto.InputNodeName,
                        IsLeftSidePort = sourcePortDto.IsLeftSidePort,
                        X = sourcePortDto.X,
                        Y = sourcePortDto.Y

                    };
                    Port targetPort = new Port()
                    {
                        Id = targetPortDto.Id,
                        LocalId = targetPortDto.LocalId,
                        InputPortNumber = targetPortDto.InputPortNumber,
                        InputNodeName = targetPortDto.InputNodeName,
                        IsLeftSidePort = targetPortDto.IsLeftSidePort,
                        X = targetPortDto.X,
                        Y = targetPortDto.Y
                    };

                    if (sourceNode != null && targetNode != null && sourcePort != null && targetPort != null)
                    {
                        var edge = new Edge(edgeDto.Id, sourceNode, targetNode, sourcePort, targetPort);
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