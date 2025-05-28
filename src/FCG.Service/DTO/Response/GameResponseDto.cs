using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCG.Service.DTO.Response
{
    /// <summary>
    /// DTO para resposta de informações de jogos.
    /// </summary>
    public class GameResponseDto
    {
        /// <summary>
        /// Id do jogo.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Nome do jogo.
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Descrição do jogo.
        /// </summary>
        public string Description { get; set; } = null!;

        /// <summary>
        /// Data de lançamento do jogo.
        /// </summary>
        public DateTime DateRelease { get; set; }

        /// <summary>
        /// Data da última atualização do jogo.
        /// </summary>
        public DateTime DateUpdate { get; set; }
    }
}
