using AcademiaDoZe.Domain.Entities.Base;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.ValueObjects;
//thiago kovaslki

namespace AcademiaDoZe.Domain.Entities
{
    public abstract class Pessoa : Entity
    {
        public string Nome { get; private set; }
        public Cpf Cpf { get; private set; }
        public Email Email { get; private set; }
        public Telefone Telefone { get; private set; }
        public DateOnly DataNascimento { get; private set; }
        public Endereco Endereco { get; private set; }

        protected Pessoa(int id,
        string nome,
        Cpf cpf,
        DateOnly dataNascimento,
        Telefone telefone,
        Email email,
        Endereco endereco,
        Senha senha,
        Arquivo foto
       )
        {
            Id = id;
            Nome = nome;
            Cpf = cpf;
            Email = email;
            Telefone = telefone;
            DataNascimento = dataNascimento;
            Endereco = endereco;
        }
    }
}