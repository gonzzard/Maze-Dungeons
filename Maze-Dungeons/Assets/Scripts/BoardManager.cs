﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.UI;

public class BoardManager : MonoBehaviour {

	#region Atributos serializables

    /// <summary>
	/// Casilla de salida (escapatoria).
    /// </summary>
    [SerializeField]
    private GameObject exit;

    [SerializeField]
    private GameObject timeLess;

	public GameObject horizontal;

    /// <summary>
    /// Casillas del suelo.
    /// </summary>
    [SerializeField]
    private GameObject[] floorTiles;

	/// <summary>
	/// Casillas del borde del laberinto.
	/// </summary>
    [SerializeField]
    private GameObject[] outerWallsTiles;

	[SerializeField]
	private GameObject[] insideHorizontalWallsTiles;

    [SerializeField]
    private GameObject[] upWallsTiles;

    [SerializeField]
    private GameObject[] minimapWallsTiles;

    public int level;

	[SerializeField]
	private Text TextoPuntos;

	#endregion

	#region Atributos no serializables

	/// <summary>
	/// Número de columnas del laberinto.
	/// </summary>
	private double columns;

	/// <summary>
	/// Número de filas del laberinto.
	/// </summary>
	private double rows;

	/// <summary>
	/// The maze map.
	/// </summary>
    private List<List<Box>> mazeMap = new List<List<Box>>();

	/// <summary>
	/// The beepers.
	/// </summary>
	private List<Box> beepers = new List<Box>();

	/// <summary>
	/// The walls.
	/// </summary>
    private List<Box> walls = new List<Box>();

	/// <summary>
	/// The board holder.
	/// </summary>
	private Transform boardHolder;

	/// <summary>
	/// The remaining beepers.
	/// </summary>
	private int remainingBeepers;

	#endregion

	#region Clase de una casilla

	/// <summary>
	/// Clase que representa una casilla del mapa.
	/// </summary>
    public class Box
	{
		public bool isWall;
        public bool isWall2;
		public bool isPath;
		public bool isRightPath;
		public bool isEnd;
		public bool isBegin;
		public string tag;
		public double x;
		public double y;
        
		public Box(){ }
	}

	#endregion

	#region Algoritmos del laberinto

    /// <summary>
    /// Método para reemplazar los tags de una casilla
    /// </summary>
    /// <param name="toBeReplaced">Tag a ser reemplazado.</param>
    /// <param name="toReplace">Tag a reemplaazar.</param>
	private void ReplaceTags(string toBeReplaced, string toReplace)
	{
		for (int i = 0; i < mazeMap.Count; i++)
		{
			for(int j = 0; j < mazeMap[i].Count; j++)
			{
				if(!mazeMap[i][j].tag.Equals(toReplace) 
					&& mazeMap[i][j].tag.Equals(toBeReplaced))
				{
					mazeMap[i][j].tag = toReplace;
				}
			}
		}
	}

	/// <summary>
	/// Prepara la lista de casillas del laberinto. Cuadricula el mapa.
	/// </summary>
	private void PrepareMazeMap()
	{
		int tag = 0;

		// Filas
		for (int i = 0; i < rows; i++) {
			mazeMap.Add(new List<Box> ());
		}

		// Columnas
		for (int i = 0; i < rows; i++) {
			for (int j = 0; j < columns; j++) {
				mazeMap[i].Add(new Box ());
			}
		}

		double x = -2.56; // Factor de conversion 4 - 100 a 25 -> 4 - 100px -> 1 unidad
		double y = -2.56;

		for (int i = 0; i < mazeMap.Count; i++)
		{
			for(int j = 0; j < mazeMap[i].Count; j++)
			{
				if (i == 0 || j == 0 || i == mazeMap.Count - 1 || j == mazeMap[i].Count -1)
				{
					// Delimiter walls
					mazeMap[i][j].isWall = true;
					mazeMap[i][j].isPath = false;
					mazeMap[i][j].isRightPath = false;
					mazeMap[i][j].tag = "-1";
					mazeMap[i][j].x = j;
					mazeMap[i][j].y = i;
				}
				else if (i % 2 != 0 && j % 2 != 0)
				{
					// Path
					mazeMap[i][j].isWall = false;
					mazeMap[i][j].isPath = true;
					mazeMap[i][j].isRightPath = false;
					mazeMap[i][j].tag = tag.ToString();
					mazeMap[i][j].x = x;
					mazeMap[i][j].y = y;
					beepers.Add(mazeMap[i][j]);
				}
				else
				{
					// Walls
					mazeMap[i][j].isWall = true;
					mazeMap[i][j].isPath = false;
					mazeMap[i][j].isRightPath = false;
					mazeMap[i][j].tag = "-1";
					mazeMap[i][j].x = j;
					mazeMap[i][j].y = i;
					// A la lista
					walls.Add(mazeMap[i][j]);
				}
				tag++;
				y += 2.56;
			}
				y = -2.56;
			x += 2.56;
		}
	}

	/// <summary>
	/// Genera el laberinto mediante el algoritmo de kruskal.
	/// </summary>
	private void GeneratePaths()
	{
		int direction = 2;

		// Hasta que no haya muros en la lista
		while (walls.Count != 0)
		{
			int position;

			if (walls.Count == 1)
			{
				position = 0;
			}
			else
			{
				// Sacamos un numero al azar para la posicion.
				position = Random.Range(0, walls.Count - 1);
			}

			// Care with the coordinates and the array postion.
			int yWallPos = (int) System.Math.Round(walls[position].y, 2);
			int xWallPos = (int) System.Math.Round(walls[position].x, 2);

			// We alternate between horinzontal and vertical paths
			if(direction % 2 == 0)
			{
				// Horizontal 
				if (!mazeMap[yWallPos][xWallPos - 1].tag.Equals(mazeMap[yWallPos][xWallPos + 1].tag) // Distinto tag, no están comunicados
					&& !mazeMap[yWallPos][xWallPos - 1].tag.Equals("-1")
					&& !mazeMap[yWallPos][xWallPos + 1].tag.Equals("-1"))
				{
					// Join Paths
					ReplaceTags(mazeMap[yWallPos][xWallPos-1].tag, mazeMap[yWallPos][xWallPos + 1].tag); 
					mazeMap[yWallPos][xWallPos].isWall = false; // Quitamos el muro y lo añadimos al camino
					mazeMap[yWallPos][xWallPos].isPath = true;
					mazeMap[yWallPos][xWallPos].tag = mazeMap[yWallPos][xWallPos + 1].tag;
				}
				else
				{
					// Vertical
					if (!mazeMap[yWallPos - 1][xWallPos].tag.Equals(mazeMap[yWallPos + 1][xWallPos].tag)
						&& !mazeMap[yWallPos - 1][xWallPos].tag.Equals("-1")
						&& !mazeMap[yWallPos + 1][xWallPos].tag.Equals("-1"))
					{
						// Join Paths
						ReplaceTags(mazeMap[yWallPos - 1][xWallPos].tag, mazeMap[yWallPos + 1][xWallPos].tag);
						mazeMap[yWallPos][xWallPos].isWall = false; // Quitamos el muro y lo añadimos al camino
						mazeMap[yWallPos][xWallPos].isPath = true;
						mazeMap[yWallPos][xWallPos].tag = mazeMap[yWallPos + 1 ][xWallPos].tag;
					}
				}
				direction = 1;
			}
			else{
				// Vertical
				if (!mazeMap[yWallPos - 1][xWallPos].tag.Equals(mazeMap[yWallPos + 1][xWallPos].tag)
					&& !mazeMap[yWallPos - 1][xWallPos].tag.Equals("-1")
					&& !mazeMap[yWallPos + 1][xWallPos].tag.Equals("-1"))
				{
					// Join Paths
					ReplaceTags(mazeMap[yWallPos - 1][xWallPos].tag, mazeMap[yWallPos + 1][xWallPos].tag);
					mazeMap[yWallPos][xWallPos].isWall = false; // Quitamos el muro y lo añadimos al camino
					mazeMap[yWallPos][xWallPos].isPath = true;
					mazeMap[yWallPos][xWallPos].tag = mazeMap[yWallPos + 1 ][xWallPos].tag;
				}
				else
				{
					// Horinzontal
					if (!mazeMap[yWallPos][xWallPos - 1].tag.Equals(mazeMap[yWallPos][xWallPos + 1].tag)
						&& !mazeMap[yWallPos][xWallPos - 1].tag.Equals("-1")
						&& !mazeMap[yWallPos][xWallPos + 1].tag.Equals("-1"))
					{
						// Join Paths
						ReplaceTags(mazeMap[yWallPos][xWallPos-1].tag, mazeMap[yWallPos][xWallPos + 1].tag);
						mazeMap[yWallPos][xWallPos].isWall = false; // Quitamos el muro y lo añadimos al camino
						mazeMap[yWallPos][xWallPos].isPath = true;
						mazeMap[yWallPos][xWallPos].tag = mazeMap[yWallPos][xWallPos + 1].tag;
					}
				}
				direction = 2;
			}
			walls.RemoveAt(position);
		}
	}

    /// <summary>
    /// Cambia el estilo de pared para que sean mejores visualmente
    /// </summary>
    private void GenerateWallUp()
    {
        for (int i = 0; i < rows; i++)
        {
            for (int j = 1; j < columns; j++)
            {
                if (mazeMap[i][j].isWall == true && (mazeMap[i][j - 1].isWall == true || mazeMap[i][j - 1].isWall2 == true))
                {
                    mazeMap[i][j].isWall2 = true;
                    mazeMap[i][j].isWall = false;
                }
            }
        }
    }

    /// <summary>
    /// Instancia el laberinto en la escena del juego.
    /// </summary>
    private void PrintMaze()
	{
		GameObject instance = null;

		double x = -2.56;
		double y = -2.56;

		boardHolder = new GameObject("Board").transform;

		for (int i = 0; i < rows; i++)
		{
			for(int j = 0; j < columns; j++)
			{
				GameObject toInstantiate = floorTiles[Random.Range(0, floorTiles.Length)];
				x = System.Math.Round(x, 2);
				y = System.Math.Round(y, 2);
                
				if (i == 0 || j == 0 || i == rows - 1 || j == columns - 1) 
				{
					toInstantiate = outerWallsTiles[0];
					instance = Instantiate(toInstantiate, new Vector3((float)x, (float)y, 0f), Quaternion.identity) as GameObject;
					instance.transform.SetParent(boardHolder);
				}
				else if (mazeMap[i][j].isWall)
				{
					toInstantiate = insideHorizontalWallsTiles[Random.Range(0, insideHorizontalWallsTiles.Length)];
					instance = Instantiate(toInstantiate, new Vector3((float) (x), (float) (y), 0f), Quaternion.identity) as GameObject;
					instance.transform.SetParent(boardHolder);
				}
                else if (mazeMap[i][j].isWall2)
                {
                    toInstantiate = upWallsTiles[Random.Range(0, upWallsTiles.Length)];
                    instance = Instantiate(toInstantiate, new Vector3((float)(x), (float)(y), 0f), Quaternion.identity) as GameObject;
                    instance.transform.SetParent(boardHolder);
                }
                else if (mazeMap[i][j].isRightPath)
				{
					// Floor
					instance = Instantiate(toInstantiate, new Vector3((float) x, (float) y, 0f), Quaternion.identity) as GameObject;
					instance.transform.SetParent(boardHolder);
				}
				else
				{
					// Floor
					instance = Instantiate(toInstantiate, new Vector3((float) x, (float) y, 0f), Quaternion.identity) as GameObject;
					instance.transform.SetParent(boardHolder);
				}
				y += 2.56;
			}
			y = -2.56;
			x += 2.56;
		}
	}

    /// <summary>
    /// Instancia el minimapa en la escena del juego.
    /// </summary>
    private void PrintMinimap()
    {
        GameObject instance = null;

        double x = -2.56;
        double y = -2.56;

        boardHolder = new GameObject("Board").transform;

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                GameObject toInstantiate = floorTiles[Random.Range(0, floorTiles.Length)];
                x = System.Math.Round(x, 2);
                y = System.Math.Round(y, 2);

				if (i == 0 || j == 0 || i == rows - 1 || j == columns - 1) 
				{
					toInstantiate = outerWallsTiles[0];
					instance = Instantiate(toInstantiate, new Vector3((float)x, (float)y, -100f), Quaternion.identity) as GameObject;
					instance.transform.SetParent(boardHolder);
				}
                else if (mazeMap[i][j].isWall)
                {
                    toInstantiate = minimapWallsTiles[0];
                    instance = Instantiate(toInstantiate, new Vector3((float)(x), (float)(y), -100f), Quaternion.identity) as GameObject;
                    instance.transform.SetParent(boardHolder);
                }
                else if (mazeMap[i][j].isWall2)
                {
                    toInstantiate = minimapWallsTiles[0];
                    instance = Instantiate(toInstantiate, new Vector3((float)(x), (float)(y), -100f), Quaternion.identity) as GameObject;
                    instance.transform.SetParent(boardHolder);
                }
                else if (mazeMap[i][j].isRightPath)
                {
                    // Floor
                    instance = Instantiate(toInstantiate, new Vector3((float)x, (float)y, -100f), Quaternion.identity) as GameObject;
                    instance.transform.SetParent(boardHolder);
                }
                else
                {
                    // Floor
                    instance = Instantiate(toInstantiate, new Vector3((float)x, (float)y, -100f), Quaternion.identity) as GameObject;
                    instance.transform.SetParent(boardHolder);
                }
                y += 2.56;
            }
            y = -2.56;
            x += 2.56;
        }
    }

    #endregion

    /// <summary>
    /// Genera la escena de un laberinto en un nivel concreto.
    /// </summary>
    /// <param name="level">Número del nivel en el que se encuentra el jugador.</param>
    public void SetUpScene(int level)
    {
		this.level = level;
		this.remainingBeepers = level * 5;
		this.columns = (level % 2 == 0) ? level + 25 : level + 25 + 1;
		this.rows = (level % 2 == 0) ? level + 25 : level + 25 + 1;
        PrepareMazeMap ();
		GeneratePaths ();
		PlaceBeepers (level * 10);
        GenerateWallUp();
		PrintMaze ();
		GameObject instance = Instantiate(horizontal, new Vector3((float) -25.6, (float) -25.6, 10f), Quaternion.identity) as GameObject;
        PrintMinimap();
    }

	/// <summary>
	/// Coloca en el mapa los objetos recogibles
	/// </summary>
	/// <param name="level">Número del nivel en el que se encuentra el jugador.</param>
	private void PlaceBeepers(int level)
	{
		placeBeepers(10);
		placeTimeLessBeepers(3);
	}

	/// <summary>
	/// Coloca en el mapa los objetos a recoger obligatoriamente.
	/// </summary>
	/// <param name="number">Número de objetos a colocar.</param>
	private void placeBeepers(int number)
	{
		TextoPuntos.text = "Objetos por recoger: " + number;
		GameObject beeper;
		for (int i = 0; i < number; i++)
		{
			Box box =  beepers[Random.Range (0, beepers.Count - 1)];
			beeper = Instantiate (exit, 
				new Vector3((float) box.x, (float) box.y, 0f), Quaternion.identity) as GameObject;
		}
	}

	/// <summary>
	/// Coloca en el mapa los objetos que reducen el tiempo.
	/// </summary>
	/// <param name="number">Número de objetos a colocar.</param>
	private void placeTimeLessBeepers(int number)
	{
		GameObject beeper;
		for (int i = 0; i < number; i++)
		{
			Box box = beepers[Random.Range (0, beepers.Count - 1)];
			beeper = Instantiate(timeLess,
				new Vector3((float)box.x, (float)box.y, 0f), Quaternion.identity) as GameObject;
		}
	}
}