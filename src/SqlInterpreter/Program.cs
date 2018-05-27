using System;

namespace SqlInterpreter
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            var sqlExecuter = new SqlExecuter();
            InitialLoad(sqlExecuter);

            string command = Console.ReadLine();
            while (string.CompareOrdinal(command, "exit") != 0)
            {
                try
                {
                    sqlExecuter.Execute(command);
                }
                catch (Exception e)
                {
                    DisplayError(e.Message);
                }
                command = Console.ReadLine();
            }
        }

        private static void DisplayError(string message)
        {
            var beforeColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            try
            {
                Console.WriteLine(message);
            }
            finally
            {
                Console.ForegroundColor = beforeColor;
            }
        }

        private static void InitialLoad(SqlExecuter sqlExecuter)
        {
            sqlExecuter.Execute("insert into Pessoa {Id: 1, Nome: 'Danilo Carvalho', Idade: 31, ProfissaoId: 1}");
            sqlExecuter.Execute("insert into Pessoa {Id: 2, Nome: 'Lorena Carvalho', Idade: 27, ProfissaoId: 2}");
            sqlExecuter.Execute("insert into Pessoa {Id: 3, Nome: 'Ivana Sueli', Idade: 54, ProfissaoId: 3}");
            sqlExecuter.Execute("insert into Pessoa {Id: 4, Nome: 'Simone Santos', Idade: 31, ProfissaoId: 2}");
            sqlExecuter.Execute("insert into Pessoa {Id: 5, Nome: 'Joao Carlos', Idade: 56, ProfissaoId: 1}");

            sqlExecuter.Execute("insert into Profissao {Id: 1, Nome: 'Desenvolvedor', Salario: 6000}");
            sqlExecuter.Execute("insert into Profissao {Id: 2, Nome: 'Administrador', Salario: 5000}");
            sqlExecuter.Execute("insert into Profissao {Id: 3, Nome: 'Engenheiro', Salario: 4000}");

            //sqlExecuter.Execute("from a in Pessoa select a");
            //sqlExecuter.Execute("from a in Profissao select a");
            //sqlExecuter.Execute("from a in Profissao select {Nome: a.Nome}");
            //sqlExecuter.Execute("from a in Profissao where a.Salario > 4000 select {Nome: a.Nome}");
            //sqlExecuter.Execute("from a in Profissao where a.Salario <= 4000 select {Nome: a.Nome}");
            //sqlExecuter.Execute("from a in Pessoa where a.Idade >= 18 and a.Idade <= 35 select {Nome: a.Nome, Idade: a.Idade}");
            //sqlExecuter.Execute("from a in Pessoa from b in Profissao select {Pessoa: a, Profissao: b}");
            //sqlExecuter.Execute(@"
                //from a in Pessoa 
                //from b in (from c in Profissao where c.Id = a.ProfissaoId select c) 
                //select {Pessoa: a, Profissao: b}");
            //sqlExecuter.Execute(@"
                //from a in Pessoa 
                //from b in (from c in Profissao where c.Id = a.ProfissaoId select c) 
                //orderby b.Salario desc
                //select {Nome: a.Nome, Salario: b.Salario}");
            //sqlExecuter.Execute(@"
                //from a in Pessoa 
                //from b in (from c in Profissao where c.Id = a.ProfissaoId select c) 
                //groupby b.Salario as sal into g
                //orderby sal desc
                //select {Salario: sal, MediaIdade: avg(from x in g select {Idade: x.a.Idade})}");

            //sqlExecuter.Execute("delete from Pessoa where Idade > 40");
            //sqlExecuter.Execute("delete from Profissao");
        }
    }
}
