namespace TechsysLog.Application.Exceptions
{
    /// <summary>
    /// Exceção de aplicação para erros de regra/fluxo.
    /// A Api transformará isso em HTTP 400/404/409 conforme o tipo.
    /// </summary>
    public class AppException : Exception
    {
        public string Code { get; }
        public int? HttpStatusHint { get; }

        public AppException(string code, string message, int? httpStatusHint = null) : base(message)
        {
            Code = code;
            HttpStatusHint = httpStatusHint;
        }
    }
}
