using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TableHandler : MonoBehaviour
{
    [SerializeField] private Vector2 tableSize;
    public TableActionsHandler ActionsHandler;
    private List<TablePlaceHandler> tablePlaces;
    private TablePlaceHandler[,] tableMatrix;

    private void Awake()
    {
        tablePlaces = FindObjectsOfType<TablePlaceHandler>().ToList();
        tablePlaces = tablePlaces.OrderBy(_tablePlace => _tablePlace.Id).ToList();
        tableMatrix = new TablePlaceHandler[(int)tableSize.x, (int)tableSize.y];
        List<TablePlaceHandler> _tablePlaces = tablePlaces.ToList();
        _tablePlaces.Remove(_tablePlaces.Find(_place => _place.Id == -1));
        _tablePlaces.Remove(_tablePlaces.Find(_place => _place.Id == 65));
        int _counter = 0;
        for (int _i = 0; _i < tableSize.x; _i++)
        {
            for (int _j = 0; _j < tableSize.y; _j++)
            {
                if (_counter == 0 || _counter == 64)
                {
                    _counter++;
                    _j--;
                    continue;
                }

                tableMatrix[_i, _j] = tablePlaces.Find(_place => _place.Id == _tablePlaces[_counter].Id);
                _counter++;
            }
        }
    }

    private void Start()
    {
        ActionsHandler.Setup();
    }

    public TablePlaceHandler GetPlace(int _id)
    {
        return tablePlaces.Find(_tablePlace => _tablePlace.Id == _id);
    }

    public TablePlaceHandler GetPlace(Vector2 _index)
    {
        try
        {
            return tableMatrix[(int)_index.x, (int)_index.y];
        }
        catch
        {
            return default;
        }
    }

    public List<TablePlaceHandler> GetPlacesAround(int _id, CardMovementType _movementType, int _range = 1,
        bool _includeCenter = false, bool _log=false)
    {
        List<TablePlaceHandler> _surroundingPlaces = new List<TablePlaceHandler>();
        TablePlaceHandler _centerPlace = GetPlace(_id);

        int _centerI = -1;
        int _centerJ = -1;
        bool _found = false;
        for (int _i = 0; _i < tableSize.x; _i++)
        {
            for (int _j = 0; _j < tableSize.y; _j++)
            {
                if (tableMatrix[_i, _j] == _centerPlace)
                {
                    _centerI = _i;
                    _centerJ = _j;
                    _found = true;
                    break;
                }
            }

            if (_found) break;
        }

        if (_centerI == -1 || _centerJ == -1) return _surroundingPlaces;

        for (int _x = -_range; _x <= _range; _x++)
        {
            for (int _y = -_range; _y <= _range; _y++)
            {
                if (_x == 0 && _y == 0)
                {
                    if (_includeCenter)
                    {
                        _surroundingPlaces.Add(_centerPlace);
                    }

                    continue;
                }

                if (_movementType == CardMovementType.FourDirections && (_x != 0 && _y != 0))
                    continue;

                int _newI = _centerI + _x;
                int _newJ = _centerJ + _y;

                if (_newI >= 0 && _newI < tableSize.x && _newJ >= 0 && _newJ < tableSize.y)
                {
                    if (tableMatrix[_newI, _newJ] != null && !tableMatrix[_newI, _newJ].IsAbility)
                        _surroundingPlaces.Add(tableMatrix[_newI, _newJ]);
                }
            }
        }
        
        return _surroundingPlaces;
    }

    public List<TablePlaceHandler> GetPlacesAroundNoCorners(int _id, CardMovementType _movementType, int _range = 1,
        bool _includeCenter = false)
    {
        List<TablePlaceHandler> _surroundingPlaces = new List<TablePlaceHandler>();
        TablePlaceHandler _centerPlace = GetPlace(_id);

        int _centerI = -1;
        int _centerJ = -1;
        bool _found = false;
        for (int _i = 0; _i < tableSize.x; _i++)
        {
            for (int _j = 0; _j < tableSize.y; _j++)
            {
                if (tableMatrix[_i, _j] == _centerPlace)
                {
                    _centerI = _i;
                    _centerJ = _j;
                    _found = true;
                    break;
                }
            }

            if (_found) break;
        }

        if (_centerI == -1 || _centerJ == -1) return _surroundingPlaces;

        for (int _x = -_range; _x <= _range; _x++)
        {
            for (int _y = -_range; _y <= _range; _y++)
            {
                if (_x == 0 && _y == 0)
                {
                    if (_includeCenter)
                    {
                        _surroundingPlaces.Add(_centerPlace);
                    }

                    continue;
                }

                // Check if the current space is within the desired "diamond" shape using Euclidean distance
                if (Math.Abs(_x) + Math.Abs(_y) > _range)
                    continue;

                if (_movementType == CardMovementType.FourDirections && (_x != 0 && _y != 0))
                    continue;

                int _newI = _centerI + _x;
                int _newJ = _centerJ + _y;

                if (_newI >= 0 && _newI < tableSize.x && _newJ >= 0 && _newJ < tableSize.y)
                {
                    if (tableMatrix[_newI, _newJ] != null && !tableMatrix[_newI, _newJ].IsAbility)
                        _surroundingPlaces.Add(tableMatrix[_newI, _newJ]);
                }
            }
        }

        return _surroundingPlaces;
    }


    public int DistanceBetweenPlaces(TablePlaceHandler _placeOne, TablePlaceHandler _placeTwo)
    {
        Vector2 _indexOfPlaceOne = GetIndexOfPlace(_placeOne);
        Vector2 _indexOfPlaceTwo = GetIndexOfPlace(_placeTwo);

        int _placeOneRow = (int)_indexOfPlaceOne.x;
        int _placeTwoRow = (int)_indexOfPlaceTwo.x;

        int _placeOneCol = (int)_indexOfPlaceOne.y;
        int _placeTwoCol = (int)_indexOfPlaceTwo.y;

        int _rowDifference = Math.Abs(_placeOneRow - _placeTwoRow);
        int _colDifference = Math.Abs(_placeOneCol - _placeTwoCol);

        int _distance = Math.Max(_rowDifference, _colDifference);

        return _distance;
    }

    public Vector2 GetIndexOfPlace(TablePlaceHandler _place)
    {
        for (int _i = 0; _i < tableSize.x; _i++)
        {
            for (int _j = 0; _j < tableSize.y; _j++)
            {
                if (_place == tableMatrix[_i, _j])
                {
                    return new Vector2(_i, _j);
                }
            }
        }

        throw new Exception("Cant find index for _place");
    }

    public TablePlaceHandler GetPlaceWithWallInPath(TablePlaceHandler _start, TablePlaceHandler _end)
    {
        Vector2 _startIndex = GetIndexOfPlace(_start);
        Vector2 _endIndex = GetIndexOfPlace(_end);

        if (_startIndex == _endIndex) return null;

        int _minX = (int)Mathf.Min(_startIndex.x, _endIndex.x);
        int _maxX = (int)Mathf.Max(_startIndex.x, _endIndex.x);
        int _minY = (int)Mathf.Min(_startIndex.y, _endIndex.y);
        int _maxY = (int)Mathf.Max(_startIndex.y, _endIndex.y);

        for (int _x = _minX; _x <= _maxX; _x++)
        {
            for (int _y = _minY; _y <= _maxY; _y++)
            {
                if (tableMatrix[_x, _y].ContainsWall)
                {
                    return tableMatrix[_x, _y];
                }
            }
        }

        return null;
    }

    public TablePlaceHandler GetPlaceWithCardInPath(TablePlaceHandler _start, TablePlaceHandler _end)
    {
        Vector2 _startIndex = GetIndexOfPlace(_start);
        Vector2 _endIndex = GetIndexOfPlace(_end);

        if (_startIndex == _endIndex) return null;

        int _minX = (int)Mathf.Min(_startIndex.x, _endIndex.x);
        int _maxX = (int)Mathf.Max(_startIndex.x, _endIndex.x);
        int _minY = (int)Mathf.Min(_startIndex.y, _endIndex.y);
        int _maxY = (int)Mathf.Max(_startIndex.y, _endIndex.y);

        for (int _x = _minX; _x <= _maxX; _x++)
        {
            for (int _y = _minY; _y <= _maxY; _y++)
            {
                TablePlaceHandler _place = tableMatrix[_x, _y];
                if (_place.IsOccupied)
                {
                    if (_place.ContainsMarker || _place.ContainsWall || _place.ContainsPortal || _place.ContainsWall)
                    {
                        continue;
                    }

                    if (_place.Id == _start.Id || _place.Id == _end.Id)
                    {
                        continue;
                    }

                    return tableMatrix[_x, _y];
                }
            }
        }

        return null;
    }

    public Card CheckForCardInFront(int _startId, int _endId)
    {
        Vector2 _inFrontIndex = GetFrontIndex(_startId, _endId);

        if (_inFrontIndex.x >= 0 && _inFrontIndex.x < tableSize.x && _inFrontIndex.y >= 0 &&
            _inFrontIndex.y < tableSize.y)
        {
            TablePlaceHandler _inFrontPlaceHandler = tableMatrix[(int)_inFrontIndex.x, (int)_inFrontIndex.y];
            if (_inFrontPlaceHandler)
            {
                return _inFrontPlaceHandler.GetCard();
            }
        }

        return null;
    }

    public Vector2 GetFrontIndex(int _startId, int _endId)
    {
        Vector2 _endIndex = GetIndexOfPlace(GetPlace(_endId));
        Vector2 _direction = GetDirection(_startId, _endId);

        Vector2 _inFrontIndex = _endIndex + _direction;

        return _inFrontIndex;
    }

    public Vector2 GetDirection(int _startId, int _endId)
    {
        Vector2 _startIndex = GetIndexOfPlace(GetPlace(_startId));
        Vector2 _endIndex = GetIndexOfPlace(GetPlace(_endId));

        int deltaX = (int)(_endIndex.x - _startIndex.x);
        int deltaY = (int)(_endIndex.y - _startIndex.y);

        int dirX = Math.Sign(deltaX);
        int dirY = Math.Sign(deltaY);

        return new Vector2(dirX, dirY);
    }

    public bool AreDiagonal(TablePlaceHandler _placeOne, TablePlaceHandler _placeTwo)
    {
        Vector2 _indexOfPlaceOne = GetIndexOfPlace(_placeOne);
        Vector2 _indexOfPlaceTwo = GetIndexOfPlace(_placeTwo);

        int _rowDifference = Mathf.Abs((int)_indexOfPlaceOne.x - (int)_indexOfPlaceTwo.x);
        int _colDifference = Mathf.Abs((int)_indexOfPlaceOne.y - (int)_indexOfPlaceTwo.y);

        return _rowDifference == _colDifference && _rowDifference == 1;
    }

    public Vector2 GetBehindIndex(int _startId, int _endId)
    {
        Vector2 _startIndex = GetIndexOfPlace(GetPlace(_startId));
        Vector2 _endIndex = GetIndexOfPlace(GetPlace(_endId));

        Vector2 _direction = new Vector2(_endIndex.x - _startIndex.x != 0 ? Math.Sign(_endIndex.x - _startIndex.x) : 0,
            _endIndex.y - _startIndex.y != 0 ? Math.Sign(_endIndex.y - _startIndex.y) : 0);

        Vector2 _behindIndex = _startIndex - _direction;

        return _behindIndex;
    }

    public void GetAbilityPosition(Action<int> _callBack)
    {
        // List<int> _myAbilityIndexes = new List<int>() { 1, 2, 3, 4, 5, 6, 7 };
        List<int> _myAbilityIndexes = new List<int>()
        {
            4,
            3,
            5,
            2,
            6,
            1,
            7
        };

        int _position = -1;

        for (int _i = 0; _i < _myAbilityIndexes.Count; _i++)
        {
            TablePlaceHandler _place = GetPlace(_myAbilityIndexes[_i]);
            if (_place.IsOccupied)
            {
                continue;
            }

            _position = _place.Id;
            break;
        }

        _callBack?.Invoke(_position);
    }

    public int GetTeleportExitIndex(int _entryIndex, int _teleportIndex1, int _teleportIndex2)
    {
        Vector2 _entryPosition = GetIndexOfPlace(GetPlace(_entryIndex));
        Vector2 _teleportPosition1 = GetIndexOfPlace(GetPlace(_teleportIndex1));
        Vector2 _teleportPosition2 = GetIndexOfPlace(GetPlace(_teleportIndex2));

        Vector2 _direction = _teleportPosition1 - _entryPosition;

        Vector2 _exitPosition = _teleportPosition2 + _direction;

        if (_exitPosition.x >= 0 && _exitPosition.x < tableSize.x && _exitPosition.y >= 0 &&
            _exitPosition.y < tableSize.y)
        {
            TablePlaceHandler _exitPlace = tableMatrix[(int)_exitPosition.x, (int)_exitPosition.y];
            if (_exitPlace != null)
            {
                return _exitPlace.Id;
            }
        }

        return -1;
    }

    public List<TablePlaceHandler> FindPath(TablePlaceHandler _start, TablePlaceHandler _end, CardMovementType _movementType)
    {
        Dictionary<TablePlaceHandler, TablePlaceHandler> _cameFrom = new Dictionary<TablePlaceHandler, TablePlaceHandler>();
        Dictionary<TablePlaceHandler, int> _costSoFar = new Dictionary<TablePlaceHandler, int>();

        List<TablePlaceHandler> _openList = new List<TablePlaceHandler>();

        _openList.Add(_start);
        _costSoFar[_start] = 0;

        while (_openList.Count > 0)
        {
            _openList.Sort((_a, _b) => _costSoFar[_a].CompareTo(_costSoFar[_b])); // Sort by least cost first

            var _current = _openList[0];
            _openList.RemoveAt(0); // Pop the first (lowest cost)

            if (_current == _end)
            {
                List<TablePlaceHandler> _path = new List<TablePlaceHandler>();
                while (_current != _start)
                {
                    _path.Add(_current);
                    _current = _cameFrom[_current];
                }

                _path.Add(_start);
                _path.Reverse();
                if (_path.Contains(_start))
                {
                    _path.Remove(_start);
                }

                if (_path.Contains(_end))
                {
                    _path.Remove(_end);
                }

                return _path;
            }

            foreach (var _neighbor in GetNeighbors(_current, _movementType))
            {
                int _newCost = _costSoFar[_current] + GetMovementCost(_current, _neighbor, _movementType);

                if (!_costSoFar.ContainsKey(_neighbor) || _newCost < _costSoFar[_neighbor])
                {
                    if (!_neighbor.HasLifeForce)
                    {
                        _costSoFar[_neighbor] = _newCost;
                        _openList.Add(_neighbor);
                        _cameFrom[_neighbor] = _current;
                    }
                }
            }
        }

        return new List<TablePlaceHandler>(); // Return empty list if no path found
    }

// This function assumes that the TablePlaceHandlers are laid out in a grid.
    private IEnumerable<TablePlaceHandler> GetNeighbors(TablePlaceHandler _place, CardMovementType _movementType)
    {
        List<TablePlaceHandler> _neighbors = GetPlacesAround(_place.Id, _movementType);
        return _neighbors;
    }

    private bool IsDiagonal(TablePlaceHandler current, TablePlaceHandler neighbor)
    {
        // Assuming each place has a grid position (x, y)
        // This part is pseudo-code since I don't know your exact structure for TablePlaceHandler.
        // You'll need to replace it to work with your actual setup.
        Vector2 _x = GetIndexOfPlace(current);
        Vector2 _y = GetIndexOfPlace(neighbor);
        var deltaX = Math.Abs(_x.x - _y.x);
        var deltaY = Math.Abs(_x.y - _y.y);
        return deltaX == 1 && deltaY == 1;
    }

    private int GetMovementCost(TablePlaceHandler current, TablePlaceHandler neighbor, CardMovementType movementType)
    {
        int baseCost = neighbor.IsOccupied ? 2 : 1;

        if (movementType == CardMovementType.FourDirections)
        {
            if (IsDiagonal(current, neighbor))
            {
                return int.MaxValue; // Make diagonal moves extremely costly for 4-direction movement
            }
        }

        return baseCost;
    }
}
