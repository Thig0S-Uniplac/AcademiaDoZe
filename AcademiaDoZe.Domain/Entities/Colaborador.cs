using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Domain.Entities
{
    public class Colaborador : Pessoa
    {
        public ColaboradorTipo Tipo { get; private set; }
        public ColaboradorVinculo Vinculo { get; private set; }
        public Senha Senha { get; private set; }

        public Colaborador(
            string nome,
            Cpf cpf,
            Email email,
            Telefone telefone,
            DateTime dataNascimento,
            Endereco endereco,
            ColaboradorTipo tipo,
            ColaboradorVinculo vinculo,
            Senha senha)
            : base(nome, cpf, email, telefone, dataNascimento, endereco)
        {
            Tipo = tipo;
            Vinculo = vinculo;
            Senha = senha;
        }
    }
}