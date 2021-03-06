using System;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    public static Board Instance;

    [SerializeField] private GameObject squarePrefab, squaresParent;
    public GameObject whitePiecesParent, blackPiecesParent;
    public GameObject spritesParent;

    public Color lightSquareColor, darkSquareColor, selectionColor;

    public PieceTypeSO[] pieceTypeSOArray = new PieceTypeSO[5]; //ORDER MUST BE: pawn,knight,bishop,rook,queen,king

    public static Dictionary<Piece.PieceType, PieceTypeSO> PieceTypeToSO = new Dictionary<Piece.PieceType, PieceTypeSO>();
    public static Dictionary<int, Square> SquareNumberToSquare = new Dictionary<int, Square>();

    public const string startingFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR"; //in case you lose the string: rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR

    private const int playerCount = 2;
    
    public const int fileCount = 8;
    public const int rankCount = 8;
    private const int squareCount = fileCount * rankCount;

    public const int maxFile = fileCount - 1;
    public const int maxRank = rankCount - 1;
    public const int maxSquare = squareCount - 1;

    private static List<Piece> WhitePieces
    {
        get
        {
            return GetPieces(Piece.PieceColor.White);
        }
    }
    private static List<Piece> BlackPieces
    {
        get
        {
            return GetPieces(Piece.PieceColor.Black);
        }
    }


    void Start()
    {
        if (Instance != null)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }


        for (int i = 0; i < Enum.GetNames(typeof(Piece.PieceType)).Length; i++)
        {
            PieceTypeToSO.Add((Piece.PieceType)i, pieceTypeSOArray[i]);
        }

        CreateBoard();
        CreatePositionFromFen(startingFen);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            ClearBoard();
        }
        else if (Input.GetKeyDown(KeyCode.S)) //starting pos
        {
            CreatePositionFromFen(startingFen);
        }
        else if (Input.GetKeyDown(KeyCode.R)) //random pos
        {
            CreatePositionFromFen(GenerateRandomStartingFen());
        }
        #region
        /*else if (Input.GetKeyDown(KeyCode.E)) //Generate with the specified exceptions
        {
            ClearBoard();

            int counter = 0;
            string fen;

            List<char> validCharacters = UI.GetToggledPieces();

            OUTER:
            while (true)
            {
                counter++;

                fen = GenerateRandomStartingFen();

                foreach (char character in fen)
                {
                    if (!validCharacters.Contains(character) && character != '/' && character != '8')
                    {
                        if (counter == 1000000)
                        {
                            print(counter + " tries and the position still hasn't been reached! Incredible!");
                            goto EndOfLoop;
                        }

                        goto OUTER;
                    }
                }

                CreatePositionFromFen(fen);
                print("It took " + counter + " tries to get to this position");
                break;
            }
            EndOfLoop:;
        }*/
        #endregion
    }

    private static void CreateBoard()
    {
        if (SquareNumberToSquare.Count != 0)
        {
            SquareNumberToSquare.Clear();
        }

        for (int rank = 0; rank < rankCount; rank++) //file = column
        {
            for (int column = 0; column < fileCount; column++) //rank = row
            {
                Vector2 spawnPos = new Vector2(column, rank);
                Color squareColor = (column + rank) % 2 == 0 ? Instance.darkSquareColor : Instance.lightSquareColor; //if (column + rank) is even, that means that square is black, otherwise it is white
                int squareNumber = (rank * fileCount) + column;
                
                CreateSquare(spawnPos, squareColor, squareNumber);
            }
        }
    }
    
    private static void ClearBoard()
    {
        MoveManager.ResetFields();

        foreach (Square squareClass in SquareNumberToSquare.Values)
        {
            if (squareClass.piece?.gameObj != null)
            {
                Destroy(squareClass.piece.gameObj);
            }

            squareClass.Unoccupy();
        }
    }

    private static void CreateSquare(Vector2 position, Color squareColor, int squareNumber)
    {
        GameObject squareObject = Instantiate(Instance.squarePrefab, position, Quaternion.identity, Instance.squaresParent.transform);

        squareObject.name = squareNumber.ToString();
        squareObject.GetComponent<SpriteRenderer>().color = squareColor;

        Square squareClass = squareObject.GetComponent<Square>();
        squareClass.squareNumber = squareNumber;
        squareClass.color = squareColor;

        SquareNumberToSquare.Add(squareNumber, squareClass);
    }

    public static void CreatePositionFromFen(string fen)
    {
        ClearBoard();

        MoveManager.playerTurn = Piece.PieceColor.White;

        //fen starts from square 56 for a 8x8 board
        int currentSquareNumber = squareCount - fileCount;

        foreach(char character in fen)
        {
            if (character == '/')
            {
                //next rank
                currentSquareNumber -= fileCount * 2;
            }
            else if (int.TryParse(character.ToString(), out int number))
            {
                currentSquareNumber += number;
            }
            else
            {
                Piece.PieceColor color = char.IsUpper(character) ? Piece.PieceColor.White : Piece.PieceColor.Black;
                Square square = SquareNumberToSquare[currentSquareNumber];

                switch (Piece.GetPieceTypeFromLetter(char.ToLower(character)))
                { 
                    case Piece.PieceType.Pawn:
                        new Pawn(color, square);
                        break;
                    case Piece.PieceType.Knight:
                        new Knight(color, square);
                        break;
                    case Piece.PieceType.Bishop:
                        new Bishop(color, square);
                        break;
                    case Piece.PieceType.Rook:
                        new Rook(color, square);
                        break;
                    case Piece.PieceType.Queen:
                        new Queen(color, square);
                        break;
                    case Piece.PieceType.King:
                        new King(color, square);
                        break;
                    default:
                        throw new Exception();
                }
                
                currentSquareNumber++;
            }
        }
    }
    private static string GenerateRandomStartingFen()
    {
        string fen = "";

        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < fileCount * 2; j++)
            {
                if (j == fileCount)
                {
                    fen += '/';
                }

                int randomIndex = UnityEngine.Random.Range(0, 6);
                char character = "pnbrqk"[randomIndex];

                if (i == 0) fen += character;
                else fen += char.ToUpper(character);
            }

            if (i == 0) fen += $"/{fileCount}/{fileCount}/{fileCount}/{fileCount}/";
        }

        //print("The random fen produced is: " + fen);
        return fen;
    }
    public static bool IsFenValid(string fen)
    {
        string pieceLetters = "pnbrqk";

        int overallCounter = 0;
        int counter = 0;
        
        foreach (char character in fen)
        {
            if ((counter >= fileCount && character != '/') || (counter < fileCount && character == '/'))
            {
                return false;
            }

            if (!pieceLetters.Contains(character.ToString().ToLower()))
            {
                if (!int.TryParse(character.ToString(), out int parsedCharacter) || parsedCharacter <= 0 || parsedCharacter > fileCount)
                {
                    if (character != '/')
                    {
                        return false;
                    }
                    else
                    {
                        counter = 0;
                    }
                }
                else
                {
                    counter += parsedCharacter;
                    overallCounter += parsedCharacter;
                }
            }
            else
            {
                counter++;
                overallCounter++;
            }
        }

        return overallCounter == squareCount;
    }


    /// <summary>
    /// Gets the first instance of the passed in piece type
    /// </summary>
    /// <returns></returns>
    public static Piece GetPiece(Piece.PieceColor color, Piece.PieceType type)
    {
        foreach(Square square in SquareNumberToSquare.Values)
        {
            if (square.piece?.type == type && square.piece?.color == color)
            {
                return square.piece;
            }
        }

        return null;
    }
    /// <summary>
    /// Tries to get the first instance of the passed in piece type, if it couldn't, returns false
    /// </summary>
    /// <returns></returns>
    public static bool TryGetPiece(Piece.PieceColor color, Piece.PieceType type, out Piece piece)
    {
        foreach (Square square in SquareNumberToSquare.Values)
        {
            if (square.piece.type == type && square.piece.color == color)
            {
                piece = square.piece;
                return true;
            }
        }

        piece = null;
        return false;
    }
    public static King GetKing(Piece.PieceColor color)
    {
        return (King)GetPiece(color, Piece.PieceType.King);
    }
    public static List<Piece> GetPieces(Piece.PieceColor? color = null, Piece.PieceType? type = null)
    {
        var pieces = new List<Piece>();

        foreach (Square square in SquareNumberToSquare.Values)
        {
            if ((color == null || square.piece?.color == color) && (type == null || square.piece?.type == type))
            {
                pieces.Add(square.piece);
            }
        }

        return pieces;
    }
}
