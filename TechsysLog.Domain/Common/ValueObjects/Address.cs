namespace TechsysLog.Domain.Common.ValueObjects
{
    /// <summary>
    /// Value Object: representa o endereço de entrega.
    /// Não possui identidade própria (não é entidade).
    /// </summary>
    public sealed class Address
    {
        public string Cep { get; }
        public string Street { get; }
        public string Number { get; }
        public string District { get; }
        public string City { get; }
        public string State { get; }

        public Address(string cep, string street, string number, string district, string city, string state)
        {
            Cep = string.IsNullOrWhiteSpace(cep) ? throw new ArgumentException("CEP é obrigatório.") : cep.Trim();
            Street = string.IsNullOrWhiteSpace(street) ? throw new ArgumentException("Rua é obrigatória.") : street.Trim();
            Number = string.IsNullOrWhiteSpace(number) ? throw new ArgumentException("Número é obrigatório.") : number.Trim();
            District = string.IsNullOrWhiteSpace(district) ? throw new ArgumentException("Bairro é obrigatório.") : district.Trim();
            City = string.IsNullOrWhiteSpace(city) ? throw new ArgumentException("Cidade é obrigatória.") : city.Trim();
            State = string.IsNullOrWhiteSpace(state) ? throw new ArgumentException("Estado é obrigatório.") : state.Trim();
        }
    }
}
