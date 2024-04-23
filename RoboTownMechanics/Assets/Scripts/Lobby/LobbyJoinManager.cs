using LocalMultiplayer.Player;
using StateMachines.GlobalStateMachine;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LocalMultiplayer.Lobby
{
    public class LobbyJoinManager : SingletonBehaviour<LobbyJoinManager>
    {
        //--------------------Private--------------------//
        [Header("UpperLeft")]
        [SerializeField]
        private GameObject _upperLeft;
        [SerializeField]
        private Color _upperLeftColor;
        
        [Header("UpperRight")]
        [SerializeField]
        private GameObject _upperRight;
        [SerializeField] 
        private Color _upperRightColor;
        
        [Header("LowerLeft")]
        [SerializeField]
        private GameObject _lowerLeft;
        [SerializeField]
        private Color _lowerLeftColor;
        
        [Header("LowerRight")]
        [SerializeField] 
        private GameObject _lowerRight;
        [SerializeField]
        private Color _lowerRightColor;

        private StateMachine _stateMachine;

        [SerializeField]
        private List<LobbySpot> _lobbySpots = new();

        private PlayerSpawner _playerSpawner;

        private bool _lobbyHasStarted;

        //--------------------Functions--------------------//
        private void Start()
        {
            _playerSpawner = PlayerSpawner.Instance;
            _stateMachine = StateMachine.Instance;
        }

        /// <summary>
        /// A function to join the localmultiplayer lobby with your playermaster
        /// </summary>
        /// <param name="master">The master to join into the lobby</param>
        public void JoinLobby(PlayerMaster master)
        {
            if (!CheckSpotFree())
                return;

            JoinLowestSpot(master);
        }

        private bool CheckSpotFree()
        {
            bool _spotIsFree;

            if(_lobbySpots.Count < 4) 
                _spotIsFree = true;
            else
                _spotIsFree = false;

            return _spotIsFree;
        }

        private void JoinLowestSpot(PlayerMaster playerMaster)
        {
            int lowestAvailableIndex = 5;
            Color color = Color.black;

            if(_lobbySpots.Count == 0)
            {
                lowestAvailableIndex = 1;
            }
            else
            {
                int[] possibleIndexes = new int[4] {1, 2, 3, 4};

                foreach (LobbySpot spot in _lobbySpots)
                {
                    for (int i = 0; i < possibleIndexes.Length; i++)
                    {
                        if (possibleIndexes[i] == spot.SpotIndex)
                            possibleIndexes[i] = 0;
                    }
                }

                foreach (int i in possibleIndexes)
                {
                    if (i == 0)
                        continue;
                    else
                        lowestAvailableIndex = i;
                    break;
                }
            }
            
            if(lowestAvailableIndex == 1)
                playerMaster.PlayerInputComponent.OnInteractInputAction.performed += StartGame;

            switch (lowestAvailableIndex)
            {
                case 1:
                    color = _upperLeftColor;
                    _upperLeft.SetActive(true);
                    break;
                case 2:
                    color = _upperRightColor;
                    _upperRight.SetActive(true);
                    break;
                case 3:
                    color = _lowerLeftColor;
                    _lowerLeft.SetActive(true);
                    break;
                case 4:
                    color = _lowerRightColor;
                    _lowerRight.SetActive(true);
                    break;
            }

            playerMaster.PlayerColor = color;
            _lobbySpots.Add(new LobbySpot(playerMaster, color, lowestAvailableIndex));
        }

        /// <summary>
        /// A function to leave the lobby with the given master
        /// </summary>
        /// <param name="master">The master to leave the lobby with</param>
        public void LeaveLobby(PlayerMaster master)
        {
            LobbySpot masterSpot = new LobbySpot();
            int masterSpotIndex = 0;

            foreach (LobbySpot spot in _lobbySpots)
            {
                if (spot.CurrentPlayerMaster == master)
                {
                    masterSpot = spot;
                    masterSpotIndex = masterSpot.SpotIndex;
                }
            }

            master.HasJoinedLobby = true;

            if (masterSpotIndex == 1)
                master.PlayerInputComponent.OnInteractInputAction.performed -= StartGame;

            switch (masterSpotIndex)
            {
                case 1:
                    _upperLeft.SetActive(false);
                    break;
                case 2:
                    _upperRight.SetActive(false);
                    break;
                case 3:
                    _lowerLeft.SetActive(false);
                    break;
                case 4:
                    _lowerRight.SetActive(false);
                    break;
            }

            _lobbySpots.Remove(masterSpot);
        }

        private void StartGame(InputAction.CallbackContext context)
        {
            if (_lobbyHasStarted)
                return;
            
            List<PlayerMaster> masters = new();
            foreach (LobbySpot spot in _lobbySpots)
            {
                masters.Add(spot.CurrentPlayerMaster);
            }

            _playerSpawner.SpawnPlayers(masters);

            _stateMachine.SetState(_stateMachine.GameStateInstance);
            _lobbyHasStarted = true;
        }
    }

    [Serializable]
    public struct LobbySpot
    {
        //--------------------Private--------------------//
        private PlayerMaster _currentPlayerMaster;
        [SerializeField]
        private Color _spotColor;
        [SerializeField]
        private int _spotIndex;

        //--------------------Public--------------------//
        public PlayerMaster CurrentPlayerMaster => _currentPlayerMaster;
        
        public Color SpotColor => _spotColor;
        
        public int SpotIndex => _spotIndex;

        //--------------------Functions--------------------//
        public LobbySpot(PlayerMaster playerMaster, Color spotColor, int spotIndex)
        {
            _currentPlayerMaster = playerMaster;
            _spotColor = spotColor;
            _spotIndex = spotIndex;
        }
    }
}