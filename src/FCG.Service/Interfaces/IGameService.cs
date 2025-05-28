using FCG.Domain.Entities;
using FCG.Service.DTO.Request;
using FCG.Service.DTO.Response;

namespace FCG.Service.Interfaces
{
    public interface IGameService
    {
        Task<GameResponseDto?> CreateGameAsync(GameRequestDto gameRequestDto);

        Task<GameResponseDto?> GetGameByIdAsync(long id);

        Task<IEnumerable<GameResponseDto>> GetAllGamesAsync();

        Task<bool?> UpdateGameAsync(long id, GameRequestDto gameRequestDto);

        Task<bool> DeleteGameAsync(long id);



    }
}
