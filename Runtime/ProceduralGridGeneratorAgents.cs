using System.Collections.Generic;
using UnityEngine;

namespace MParysz.ProceduralGridGenerator2D
{
  public enum AgentSpawnType
  {
    RANDOM_POS,
    AGENT_POS,
    CENTER
  }

  internal class Agent
  {
    public Vector2Int Position
    {
      private set; get;
    }
    public Vector2Int Direction
    {
      private set; get;
    }

    public Agent(Vector2Int position, Vector2Int direction)
    {
      Position = position;
      Direction = direction;
    }

    public void SetDirection(Vector2Int direction)
    {
      Direction = direction;
    }

    public void SetPosition(Vector2Int position)
    {
      Position = position;
    }

    public void UpdatePosition(Vector2Int step)
    {
      Position += step;
    }
  }

  public class ProceduralGridGeneratorAgents : ProceduralGridGeneratorBase
  {
    private SquareType[,] _grid;
    private List<Agent> _agents;
    private bool _addBorder;
    private bool _removeSingleFillSquares;
    private AgentSpawnType _agentSpawnType = AgentSpawnType.RANDOM_POS;
    private readonly int _maxIterations = 1000000;
    private int _maxAgents = 20;
    private int _currentCornerIndex = -1;
    private int _numberOfEmptySquares;
    private float _emptySquaresPercentage = 0.55f;
    private float _changeDirectionChance = 0.7f;
    private float _addNewAgentChance = 0.3f;
    private float _removeAgentChance = 0.1f;

    public ProceduralGridGeneratorAgents(int roomWidth, int roomHeight) : base(roomWidth, roomHeight) { }
    public ProceduralGridGeneratorAgents(
      int roomWidth,
      int roomHeight,
      int maxAgents,
      bool addBorder = true,
      bool removeSingleFillSquares = false,
      AgentSpawnType agentSpawnType = AgentSpawnType.RANDOM_POS
    ) : base(roomWidth, roomHeight)
    {
      _maxAgents = maxAgents;
      _addBorder = addBorder;
      _removeSingleFillSquares = removeSingleFillSquares;
      _agentSpawnType = agentSpawnType;
    }
    public ProceduralGridGeneratorAgents(
      int roomWidth,
      int roomHeight,
      int maxAgents,
      float emptySquaresPercentage,
      float changeDirectionChance,
      float addNewAgentChance,
      float removeAgentChance,
      bool addBorder = true,
      bool removeSingleFillSquares = false,
      AgentSpawnType agentSpawnType = AgentSpawnType.RANDOM_POS
    ) : base(roomWidth, roomHeight)
    {
      _maxAgents = maxAgents;
      _addBorder = addBorder;
      _emptySquaresPercentage = emptySquaresPercentage;
      _changeDirectionChance = changeDirectionChance;
      _addNewAgentChance = addNewAgentChance;
      _removeAgentChance = removeAgentChance;
      _removeSingleFillSquares = removeSingleFillSquares;
      _agentSpawnType = agentSpawnType;
    }

    public override SquareType[,] GenerateGrid()
    {
      Setup();
      Generate();
      RemoveSingleWalls();
      AddBorder();

      return _grid;
    }

    public override SquareType[,] NextIteration()
    {
      if (_grid == null)
      {
        return GenerateGrid();
      }

      return _grid;
    }

    private void Setup()
    {
      _grid = new SquareType[roomWidth, roomHeight];

      for (var i = 0; i < roomWidth; i++)
      {
        for (var j = 0; j < roomHeight; j++)
        {
          _grid[i, j] = SquareType.FILL;
        }
      }

      _agents = new List<Agent>();
      _agents.Add(CreateAgentAtCenter());
    }

    private void Generate()
    {
      int iteration = 0;

      do
      {
        foreach (var agent in _agents)
        {
          if (_grid[agent.Position.x, agent.Position.y] == SquareType.EMPTY)
          {
            continue;
          }

          _grid[agent.Position.x, agent.Position.y] = SquareType.EMPTY;
          _numberOfEmptySquares++;
        }

        foreach (var agent in _agents)
        {
          if (agent.Position.x + agent.Direction.x > 0 &&
              agent.Position.x + agent.Direction.x < roomWidth - 1 &&
              agent.Position.y + agent.Direction.y > 0 &&
              agent.Position.y + agent.Direction.y < roomHeight - 1)
          {
            agent.UpdatePosition(agent.Direction);
          }

          if (Random.value < _changeDirectionChance)
          {
            agent.SetDirection(GetRandomDirection());
          }
        }

        for (var i = 0; i < _agents.Count; i++)
        {
          if (Random.value > _removeAgentChance || _agents.Count <= 1)
          {
            continue;
          }

          _agents.RemoveAt(i);
          break;
        }

        for (var i = 0; i < _agents.Count; i++)
        {
          if (Random.value > _addNewAgentChance || _agents.Count >= _maxAgents)
          {
            continue;
          }

          _agents.Add(CreateAgent(_agents[i]));
          break;
        }

        var emptySquaresPercentageValue = (float)_numberOfEmptySquares / (float)_grid.Length;

        if (emptySquaresPercentageValue >= _emptySquaresPercentage)
        {
          break;
        }

        iteration++;
      } while (iteration < _maxIterations);
    }

    private void RemoveSingleWalls()
    {
      if (!_removeSingleFillSquares)
      {
        return;
      }

      for (int row = 0; row < roomWidth - 1; row++)
      {
        for (int col = 0; col < roomHeight - 1; col++)
        {
          var cell = _grid[row, col];

          if (cell != SquareType.FILL)
          {
            continue;
          }

          var allEmptySquares = true;

          for (int checkRow = -1; checkRow <= 1; checkRow++)
          {
            for (int checkCol = -1; checkCol <= 1; checkCol++)
            {
              if (checkRow + row < 0 ||
                checkRow + row > roomWidth - 1 ||
                checkCol + col < 0 ||
                checkCol + col > roomHeight - 1)
              {
                continue;
              }

              if ((checkRow == 0 && checkCol == 0))
              {
                continue;
              }

              if (_grid[row + checkRow, col + checkCol] == SquareType.FILL)
              {

                allEmptySquares = false;
              }
            }
          }

          if (allEmptySquares)
          {
            _grid[row, col] = SquareType.EMPTY;
          }
        }
      }
    }

    private void AddBorder()
    {
      if (!_addBorder)
      {
        return;
      }

      for (var i = 0; i < roomWidth; i++)
      {
        for (var j = 0; j < roomHeight; j++)
        {
          if (i == 0 || j == 0 || i == roomWidth - 1 || j == roomHeight - 1)
          {
            _grid[i, j] = SquareType.FILL;
          }
        }
      }
    }


    private Agent CreateAgent(Agent currentAgent = null)
    {
      switch (_agentSpawnType)
      {
        case AgentSpawnType.RANDOM_POS:
          return CreateAgentAtRandomPos();
        case AgentSpawnType.AGENT_POS:
          return CreateAgentAtCurrentAgentPos(currentAgent);
        case AgentSpawnType.CENTER:
          return CreateAgentAtCenter();
      }

      return CreateAgentAtRandomPos();
    }

    private Agent CreateAgentAtRandomPos()
    {
      Vector2Int corner = Vector2Int.zero;

      _currentCornerIndex++;
      if (_currentCornerIndex == 4)
      {
        _currentCornerIndex = 0;
      }

      switch (_currentCornerIndex)
      {
        case 0:
          corner = new Vector2Int(0, 0);
          break;
        case 1:
          corner = new Vector2Int(0, 1);
          break;
        case 2:
          corner = new Vector2Int(1, 1);
          break;
        case 3:
          corner = new Vector2Int(1, 0);
          break;

      }

      var (boundryWidth, boundryHeight) = GetBoundry(corner);

      return new Agent(
        new Vector2Int(Random.Range(boundryWidth.x, boundryWidth.y),
        Random.Range(boundryHeight.x, boundryHeight.y)), GetRandomDirection()
      );
    }

    private Agent CreateAgentAtCurrentAgentPos(Agent currentAgent = null)
    {
      if (currentAgent == null)
      {
        return CreateAgentAtCenter();
      }

      return new Agent(currentAgent.Position, GetRandomDirection());
    }

    private Agent CreateAgentAtCenter()
    {
      return new Agent(new Vector2Int(Mathf.FloorToInt(roomWidth / 2), Mathf.FloorToInt(roomHeight / 2)), GetRandomDirection());
    }

    private (Vector2Int, Vector2Int) GetBoundry(Vector2Int corner)
    {
      var width = roomWidth;
      var height = roomHeight;

      var topHeight = new Vector2Int(Mathf.CeilToInt(height / 2), height);
      var bottomHeigh = new Vector2Int(0, Mathf.CeilToInt(height / 2));
      var rightWidth = new Vector2Int(Mathf.CeilToInt(width / 2), width);
      var leftWidth = new Vector2Int(0, Mathf.CeilToInt(width / 2));

      if (corner == new Vector2Int(1, 1))
      {
        return (rightWidth, topHeight);
      }
      else if (corner == new Vector2Int(0, 0))
      {
        return (leftWidth, bottomHeigh);
      }
      else if (corner == new Vector2Int(0, 1))
      {
        return (leftWidth, topHeight);
      }
      else if (corner == new Vector2Int(1, 0))
      {
        return (rightWidth, bottomHeigh);
      }

      return (new Vector2Int(0, 0), new Vector2Int(width, height));
    }

    private Vector2Int GetRandomDirection()
    {
      var rand = Random.Range(1, 5);

      switch (rand)
      {
        case 1:
          return Vector2Int.up;
        case 2:
          return Vector2Int.left;
        case 3:
          return Vector2Int.down;
        default:
          return Vector2Int.right;
      }
    }
  }
}
