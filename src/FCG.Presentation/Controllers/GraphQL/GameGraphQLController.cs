using FCG.Domain.Core.Notifications;
using FCG.Domain.Interfaces.Commons;
using FCG.Service.DTO.Response;
using FCG.Service.Interfaces;
using GraphQL.AspNet.Attributes;
using GraphQL.AspNet.Controllers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FCG.Presentation.Controllers.GraphQL
{
    /// <summary>
    /// Controlador GraphQL para operações relacionadas a jogos.
    /// </summary>
    public class GameGraphQLController : GraphController
    {
        private readonly IGameService _gameService;

        /// <summary>
        /// Inicializa uma nova instância de <see cref="GameGraphQLController"/>.
        /// </summary>
        /// <param name="gameService">Serviço responsável pelas operações de jogos.</param>
        public GameGraphQLController(IGameService gameService)
        {
            _gameService = gameService;
        }

        /// <summary>
        /// Consulta para recuperar todos os jogos.
        /// </summary>
        /// <returns>Uma lista de jogos cadastrados.</returns>
        [QueryRoot("games")]
        public async Task<IEnumerable<GameResponseDto>> RetrieveAllGames()
        {
            var games = await _gameService.GetAllGamesAsync();

            return games;
        }

        /// <summary>
        /// Consulta para recuperar um jogo pelo ID.
        /// </summary>
        /// <param name="id">ID do jogo.</param>
        /// <returns>Os detalhes do jogo correspondente ao ID.</returns>
        [QueryRoot("game")]
        public async Task<GameResponseDto> RetrieveById(long id)
        {
            var game = await _gameService.GetGameByIdAsync(id);

            return game;
        }
    }
}