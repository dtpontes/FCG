namespace FCG.Service.DTO.Response
{
    /// <summary>
    /// DTO para cadastro de usuário administrador.
    /// </summary>
    public class RegisterUserResponseDto
    {
        /// <summary>
        /// Id do usuário
        /// </summary>        
        public Guid? Id { get; set; }


        /// <summary>
        /// E-mail do usuário.
        /// </summary>        
        public string? Email { get; set; }

        
    }
}
