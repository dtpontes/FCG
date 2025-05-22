namespace FCG.Service.DTO.Response
{
    /// <summary>
    /// DTO para cadastro de cliente.
    /// </summary>
    public class RegisterClientResponseDto 
    {

        /// <summary>
        /// Id do cliente.        
        /// </summary>
        public required long Id { get; set; }

        /// <summary>
        /// E-mail do cliente.        
        /// </summary>
        public required string Email { get; set; }        

        /// <summary>
        /// Nome do cliente.        
        /// </summary>
        public required string Name { get; set; }
        
    }
}
