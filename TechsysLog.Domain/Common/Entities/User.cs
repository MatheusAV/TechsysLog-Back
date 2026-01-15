namespace TechsysLog.Domain.Common.Entities
{
    public sealed class User : EntityBase
    {
        public string Name { get; private set; } = default!;
        public string Email { get; private set; } = default!;
        public string PasswordHash { get; private set; } = default!;
        
        private User() { }

        public User(string name, string email, string passwordHash)
        {
            SetName(name);
            SetEmail(email);
            SetPasswordHash(passwordHash);
        }

        public void SetName(string name)
        {
            Name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentException("Nome é obrigatório.") : name.Trim();
        }

        public void SetEmail(string email)
        {
            Email = string.IsNullOrWhiteSpace(email) ? throw new ArgumentException("E-mail é obrigatório.") : email.Trim().ToLowerInvariant();
        }

        public void SetPasswordHash(string passwordHash)
        {
            PasswordHash = string.IsNullOrWhiteSpace(passwordHash) ? throw new ArgumentException("Senha (hash) é obrigatória.") : passwordHash;
        }
    }
}
