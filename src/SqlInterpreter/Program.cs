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
                sqlExecuter.Execute(command);
                command = Console.ReadLine();
            }
        }

        private static void InitialLoad(SqlExecuter sqlExecuter)
        {
            sqlExecuter.Execute("insert into Pessoa {Id: 1, Nome: 'Danilo Carvalho', Idade: 31, ProfissaoId: 1}");
        }
    }
}
