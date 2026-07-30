using AcademiaDoZe.Domain.Entities.Base;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Domain.Entities
{
    public abstract class Pessoa : Entity
    {
        public string Nome { get; private set; }
        public Cpf Cpf { get; private set; }
        public Email Email { get; private set; }
        public Telefone Telefone { get; private set; }
        public DateTime DataNascimento { get; private set; }
        public Endereco Endereco { get; private set; }

        protected Pessoa(string nome, Cpf cpf, Email email, Telefone telefone, DateTime dataNascimento, Endereco endereco)
        {
            Nome = nome;
            Cpf = cpf;
            Email = email;
            Telefone = telefone;
            DataNascimento = dataNascimento;
            Endereco = endereco;
        }
    }
}