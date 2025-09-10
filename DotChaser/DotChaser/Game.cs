using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using DotChaser.Interfaces;
using DotChaser.Maps;

namespace DotChaser
{
    public class Game
    {
        Dictionary<Vector2, int> grid = new();
        private readonly Dictionary<int, Player> _players = new();
        private IOutputProvider _outputProvider;
        private readonly Map _map;
        private bool _allDead;

        public Game(IEnumerable<Player> players, Map map, IOutputProvider outputProvider)
        {
            _map = map;
            _outputProvider = outputProvider;
            
            for (int y = _map.Height - 1; y >= 0; y--)
            {
                for (int x = 0; x < _map.Width; x++)
                {
                    grid[new Vector2(x, y)] = _map.Grid[y * _map.Width + x];
                }
            }
            
            // players
            foreach (var player in players)
            {
                var spawnPosition = player.Position;
                if (spawnPosition.X < 0 || spawnPosition.X >= _map.Width || spawnPosition.Y < 0 ||
                    spawnPosition.Y >= _map.Height)
                    throw new Exception("Player out of bounds");
                _players[player.ID] = player;
                grid[spawnPosition] = player.ID;
            }
        }


        public void Simulate()
        {
            if(_allDead)
                return;
            foreach (var player in _players.Values)
            {
                var newPosition = player.Position + player.Direction;
                if(newPosition.X < 0 || newPosition.X >= _map.Width || newPosition.Y < 0 || newPosition.Y >= _map.Height)
                    continue;
                if(grid[newPosition] == 1)
                    continue;

                if (grid[newPosition] == 2)
                    player.CollectDot();
                
                if (grid[newPosition] == 3)
                    player.Died();
                
                grid[player.Position] = 0;
                grid[newPosition] = player.ID;
                player.UpdatePosition(newPosition);
            }

            _allDead = _players.Values.All(x=>!x.Alive);
        }
        
        public void Render()
        {
            _outputProvider.Clear();
            if(_allDead)
            {
                _outputProvider.Print($"Game Over! Dots: {_players[1000].Dots}");
                _outputProvider.NewLine();
            }
            else
            {
                _outputProvider.Print($"Dots: {_players[1000].Dots}");
                _outputProvider.NewLine();
            }
                
            for (int y = _map.Height - 1; y >= 0; y--)
            {
                for (int x = 0; x < _map.Width; x++)
                {
                    var location = new Vector2(x, y);
                    switch (grid[location])
                    {
                        case >= 1000:
                            _outputProvider.Print(" o");
                            break;
                        case 3:
                            _outputProvider.Print(" ü");
                            break;
                        case 2:
                            _outputProvider.Print(" *");
                            break;
                        case 1:
                            _outputProvider.Print(" #");
                            break;
                        default:
                            _outputProvider.Print(" .");
                            break;
                    }
                }
                _outputProvider.NewLine();
            }
        }

        public void ChangePlayerDirection(Vector2 inputChange)
        {
            var player = _players[1000];
            player.ChangeDirection(inputChange);
        }

        public bool ValidatePosition(Vector2 vector2, int entity)
        {
            return grid[vector2] == entity;
        }
    }
}