using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using DotChaser.Interfaces;

namespace DotChaser
{
    public class Game
    {
        Dictionary<Vector2, int> grid = new();
        private readonly Dictionary<int, Player> _players = new();
        private int _height;
        private int _width;
        private IOutputProvider _outputProvider;

        public Game(int height, int width, IEnumerable<Player> players, IOutputProvider outputProvider)
        {
            _outputProvider = outputProvider;
            _width = width;
            _height = height;
            foreach (var player in players)
            {
                _players[player.ID] = player;
                grid[player.Position] = player.ID;
            }

            for (int y = height - 1; y >= 0; y--)
            {
                for (int x = 0; x < width; x++)
                {
                    if (_players.Values.Any(p => p.Position == new Vector2(x, y)))
                        grid[new Vector2(x, y)] = 1000;
                    else
                        grid[new Vector2(x, y)] = 0;
                }
            }
        }


        public void Simulate()
        {
            foreach (var player in _players.Values)
            {
                var newPosition = player.Position + player.Direction;
                if(newPosition.X < 0 || newPosition.X >= _width || newPosition.Y < 0 || newPosition.Y >= _height)
                    continue;
                
                grid[player.Position] = 0;
                grid[newPosition] = player.ID;
                player.UpdatePosition(newPosition);
            }
        }
        
        public void Render()
        {
            _outputProvider.Clear();
            for (int y = _height - 1; y >= 0; y--)
            {
                for (int x = 0; x < _width; x++)
                {
                    if (_players.Values.Any(p => p.Position == new Vector2(x, y)))
                        _outputProvider.Print(" O");
                    else
                        _outputProvider.Print(" X");
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