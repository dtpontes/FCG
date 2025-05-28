using FCG.Domain.Core.Notifications;
using FCG.Domain.Interfaces.Commons;
using FCG.Service.DTO.Request;
using FCG.Service.DTO.Response;
using FCG.Service.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCG.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GameController : BaseController
    {
        private readonly IGameService _gameService;

        public GameController(IGameService gameService,
                              IMediatorHandler mediator,
                              INotificationHandler<DomainNotification> notifications) : base(notifications, mediator)
        {
            _gameService = gameService;
        }

        /// <summary>
        /// Registra um novo jogo.
        /// </summary>
        /// <param name="gameRequestDto">Dados do jogo</param>
        /// <returns>Jogo criado ou erros de validação</returns>
        [Authorize(Roles = "Admin")]
        [HttpPost("register")]
        [ProducesResponseType(typeof(GameResponseDto), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> RegisterGame(GameRequestDto gameRequestDto)
        {
            var game = await _gameService.CreateGameAsync(gameRequestDto);

            return game != null ? Response(game) : Response();
        }

        /// <summary>
        /// Obtém um jogo pelo ID.
        /// </summary>
        /// <param name="id">ID do jogo</param>
        /// <returns>Dados do jogo ou erro</returns>
        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(GameResponseDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetGameById(long id)
        {
            var game = await _gameService.GetGameByIdAsync(id);

            return game != null ? Response(game) : NotFound();
        }

        /// <summary>
        /// Obtém todos os jogos.
        /// </summary>
        /// <returns>Lista de jogos</returns>
        [Authorize(Roles = "Admin")]
        [HttpGet("all")]
        [ProducesResponseType(typeof(IEnumerable<GameResponseDto>), 200)]
        public async Task<IActionResult> GetAllGames()
        {
            var games = await _gameService.GetAllGamesAsync();

            return Response(games);
        }

        /// <summary>
        /// Atualiza um jogo pelo ID.
        /// </summary>
        /// <param name="id">ID do jogo</param>
        /// <param name="gameRequestDto">Dados atualizados do jogo</param>
        /// <returns>Status da operação</returns>
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateGame(long id, GameRequestDto gameRequestDto)
        {
            var result = await _gameService.UpdateGameAsync(id, gameRequestDto);

            if (result == null)
                return BadRequest();

            return result.Value ? Response() : NotFound();
        }

        /// <summary>
        /// Exclui um jogo pelo ID.
        /// </summary>
        /// <param name="id">ID do jogo</param>
        /// <returns>Status da operação</returns>
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteGame(long id)
        {
            var result = await _gameService.DeleteGameAsync(id);

            return result ? Response() : NotFound();
        }
    }
}