using UnityEngine;
using MParysz.ProceduralGridGenerator2D;
using UnityEngine.UI;

internal enum ProceduralGridGenerator2DType
{
  CELLULAR_AUTOMATA,
  AGENTS
}

public class Example : MonoBehaviour
{
  [Header("Input")]
  [SerializeField] private ProceduralGridGenerator2DType generationType = ProceduralGridGenerator2DType.CELLULAR_AUTOMATA;
  [SerializeField] private int roomHeight = 50;
  [SerializeField] private int roomWidth = 50;

  [Header("References")]
  [SerializeField] private GameObject emptySquare;
  [SerializeField] private GameObject fillSquare;
  [SerializeField] private GameObject squareParent;
  [SerializeField] private Button generateButton;
  [SerializeField] private Button nextButton;

  private ProceduralGridGeneratorBase generator;
  private SquareType[,] grid;

  private void Awake()
  {
    generateButton.onClick.AddListener(() => Generate());
    nextButton.onClick.AddListener(() => NextIteration());
  }

  private void Generate()
  {
    CleanSquareParent();
    PickGenerationType();

    grid = generator.GenerateGrid();

    CreateGrid(grid);
  }

  private void NextIteration()
  {
    CleanSquareParent();

    grid = generator.NextIteration();

    CreateGrid(grid);
  }

  private void CreateGrid(SquareType[,] grid)
  {
    for (var i = 0; i < roomWidth; i++)
    {
      for (var j = 0; j < roomHeight; j++)
      {
        GameObject square = null;

        switch (grid[i, j])
        {
          case SquareType.FILL:
            square = Instantiate(fillSquare, new Vector2(i, j), Quaternion.identity);
            break;
          case SquareType.EMPTY:
            square = Instantiate(emptySquare, new Vector2(i, j), Quaternion.identity);
            break;
        }

        if (square == null)
        {
          continue;
        }

        square.transform.SetParent(squareParent.transform);
      }
    }
  }

  private void PickGenerationType()
  {
    switch (generationType)
    {
      case ProceduralGridGenerator2DType.CELLULAR_AUTOMATA:
        generator = new ProceduralGridGeneratorCellularAutomata(roomWidth, roomHeight, 1, 0.45f);
        break;
      case ProceduralGridGenerator2DType.AGENTS:
        generator = new ProceduralGridGeneratorAgents(this.roomWidth, this.roomHeight, 1, 0.5f, 0.35f, 0, 0, true, true, AgentSpawnType.AGENT_POS);
        break;
    }
  }

  private void CleanSquareParent()
  {
    foreach (Transform child in squareParent.transform)
    {
      Destroy(child.gameObject);
    }
  }
}
