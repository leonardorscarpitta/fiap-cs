using System.Data;
using Microsoft.Data.SqlClient;

/// <summary>
/// Classe de reposit�rio para gerenciar entidades Aluno no banco de dados.
/// </summary>
public class AlunoRepository : IRepository<Aluno>
{
    /// <summary>
    /// Obt�m ou define a string de conex�o com o banco de dados.
    /// </summary>
    public string ConnectionString { get; set; }

    /// <summary>
    /// Inicializa uma nova inst�ncia da classe <see cref="AlunoRepository"/>.
    /// </summary>
    /// <param name="connectionString">A string de conex�o com o banco de dados.</param>
    public AlunoRepository(string connectionString)
    {
        ConnectionString = connectionString;
    }

    /// <summary>
    /// Garante que o esquema do banco de dados para a tabela Aluno exista.
    /// </summary>
    public void GarantirEsquema()
    {
        const string ddl = @"
        IF OBJECT_ID('dbo.Alunos', 'U') IS NULL
        BEGIN
            CREATE TABLE dbo.Alunos (
                Id INT IDENTITY(1,1) PRIMARY KEY,
                Nome NVARCHAR(100) NOT NULL,
                Idade INT NOT NULL,
                Email NVARCHAR(100) NOT NULL,
                DataNascimento DATE NOT NULL
            );
        END";
        using var conn = new SqlConnection(ConnectionString);
        conn.Open();
        using var cmd = new SqlCommand(ddl, conn) { CommandType = CommandType.Text, CommandTimeout = 30 };
        cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// Insere um novo registro de Aluno no banco de dados.
    /// </summary>
    /// <param name="nome">O nome do Aluno.</param>
    /// <param name="idade">A idade do Aluno.</param>
    /// <param name="email">O email do Aluno.</param>
    /// <param name="dataNascimento">A data de nascimento do Aluno.</param>
    /// <returns>O ID do Aluno rec�m-inserido.</returns>
    public int Inserir(string nome, int idade, string email, DateTime dataNascimento)
    {
        const string sql = @"
        INSERT INTO dbo.Alunos (Nome, Idade, Email, DataNascimento)
        VALUES (@Nome, @Idade, @Email, @DataNascimento);
        SELECT CAST(SCOPE_IDENTITY() AS INT);";

        using var conn = new SqlConnection(ConnectionString);
        conn.Open();
        using var cmd = new SqlCommand(sql, conn) { CommandType = CommandType.Text, CommandTimeout = 30 };
        cmd.Parameters.Add("@Nome", SqlDbType.NVarChar, 100).Value = nome;
        cmd.Parameters.Add("@Idade", SqlDbType.Int).Value = idade;
        cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 100).Value = email;
        cmd.Parameters.Add("@DataNascimento", SqlDbType.Date).Value = dataNascimento.Date;

        var resultado = cmd.ExecuteScalar();
        return resultado is null or DBNull ? 0 : Convert.ToInt32(resultado);
    }

    /// <summary>
    /// Recupera uma lista de todos os registros de Aluno do banco de dados.
    /// </summary>
    /// <returns>Uma lista de entidades Aluno.</returns>
    public List<Aluno> Listar()
    {
        const string sql = @"
        SELECT Id, Nome, Idade, Email, DataNascimento
        FROM dbo.Alunos
        ORDER BY Id;";

        using var conn = new SqlConnection(ConnectionString);
        conn.Open();
        using var cmd = new SqlCommand(sql, conn) { CommandType = CommandType.Text, CommandTimeout = 30 };
        using var reader = cmd.ExecuteReader();

        var alunos = new List<Aluno>();
        while (reader.Read())
            alunos.Add(MapearAluno(reader));

        return alunos;
    }

    /// <summary>
    /// Atualiza os dados de um registro de Aluno no banco de dados.
    /// </summary>
    /// <param name="id">O ID do Aluno a ser atualizado.</param>
    /// <param name="nome">O novo nome do Aluno.</param>
    /// <param name="idade">A nova idade do Aluno.</param>
    /// <param name="email">O novo email do Aluno.</param>
    /// <param name="dataNascimento">A nova data de nascimento do Aluno.</param>
    /// <returns>O n�mero de linhas afetadas.</returns>
    public int Atualizar(int id, string nome, int idade, string email, DateTime dataNascimento)
    {
        const string sql = @"
        UPDATE dbo.Alunos
        SET Nome = @Nome,
            Idade = @Idade,
            Email = @Email,
            DataNascimento = @DataNascimento
        WHERE Id = @Id;";

        using var conn = new SqlConnection(ConnectionString);
        conn.Open();
        using var cmd = new SqlCommand(sql, conn) { CommandType = CommandType.Text, CommandTimeout = 30 };
        cmd.Parameters.Add("@Nome", SqlDbType.NVarChar, 100).Value = nome;
        cmd.Parameters.Add("@Idade", SqlDbType.Int).Value = idade;
        cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 100).Value = email;
        cmd.Parameters.Add("@DataNascimento", SqlDbType.Date).Value = dataNascimento.Date;
        cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;

        return cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// Exclui um registro de Aluno do banco de dados.
    /// </summary>
    /// <param name="id">O ID do Aluno a ser exclu�do.</param>
    /// <returns>O n�mero de linhas afetadas.</returns>
    public int Excluir(int id)
    {
        const string sql = "DELETE FROM dbo.Alunos WHERE Id = @Id;";

        using var conn = new SqlConnection(ConnectionString);
        conn.Open();
        using var cmd = new SqlCommand(sql, conn) { CommandType = CommandType.Text, CommandTimeout = 30 };
        cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;

        return cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// Busca registros de Aluno no banco de dados com base em um termo e valor.
    /// </summary>
    /// <param name="propriedade">A propriedade a ser pesquisada (coluna).</param>
    /// <param name="valor">O valor a ser pesquisado.</param>
    /// <returns>Uma lista de entidades Aluno correspondentes.</returns>
    public List<Aluno> Buscar(string propriedade, object valor)
    {
        if (string.IsNullOrWhiteSpace(propriedade))
            throw new ArgumentException("A propriedade de busca é obrigatória.", nameof(propriedade));

        if (valor is null)
            throw new ArgumentNullException(nameof(valor));

        // O nome da coluna não pode ser parametrizado, então só as colunas
        // conhecidas são aceitas para evitar injeção de SQL.
        var coluna = ColunasPermitidas.FirstOrDefault(c => c.Equals(propriedade.Trim(), StringComparison.OrdinalIgnoreCase))
            ?? throw new ArgumentException($"Propriedade '{propriedade}' inválida. Use: {string.Join(", ", ColunasPermitidas)}.", nameof(propriedade));

        // Colunas de texto usam busca parcial; as demais, igualdade.
        bool textual = coluna is "Nome" or "Email";
        string sql = $@"
        SELECT Id, Nome, Idade, Email, DataNascimento
        FROM dbo.Alunos
        WHERE {coluna} {(textual ? "LIKE" : "=")} @Valor
        ORDER BY Id;";

        using var conn = new SqlConnection(ConnectionString);
        conn.Open();
        using var cmd = new SqlCommand(sql, conn) { CommandType = CommandType.Text, CommandTimeout = 30 };
        cmd.Parameters.Add(CriarParametroBusca(coluna, valor, textual));
        using var reader = cmd.ExecuteReader();

        var alunos = new List<Aluno>();
        while (reader.Read())
            alunos.Add(MapearAluno(reader));

        return alunos;
    }

    /// <summary>
    /// Colunas da tabela dbo.Alunos que podem ser usadas em <see cref="Buscar"/>.
    /// </summary>
    private static readonly string[] ColunasPermitidas = { "Id", "Nome", "Idade", "Email", "DataNascimento" };

    /// <summary>
    /// Cria o parâmetro @Valor com o tipo correspondente à coluna pesquisada.
    /// </summary>
    /// <param name="coluna">A coluna já validada contra <see cref="ColunasPermitidas"/>.</param>
    /// <param name="valor">O valor informado pelo chamador.</param>
    /// <param name="textual">Indica se a busca usa LIKE.</param>
    /// <returns>O parâmetro pronto para ser adicionado ao comando.</returns>
    private static SqlParameter CriarParametroBusca(string coluna, object valor, bool textual)
    {
        if (textual)
            return new SqlParameter("@Valor", SqlDbType.NVarChar, 100) { Value = $"%{valor}%" };

        return coluna switch
        {
            "DataNascimento" => new SqlParameter("@Valor", SqlDbType.Date)
            {
                Value = valor is DateTime data
                    ? data.Date
                    : DateTime.Parse(Convert.ToString(valor) ?? string.Empty).Date
            },
            _ => new SqlParameter("@Valor", SqlDbType.Int) { Value = Convert.ToInt32(valor) }
        };
    }

    /// <summary>
    /// Converte a linha atual do leitor em uma entidade Aluno.
    /// </summary>
    /// <param name="reader">O leitor posicionado em uma linha válida.</param>
    /// <returns>A entidade Aluno correspondente.</returns>
    private static Aluno MapearAluno(SqlDataReader reader) => new(
        reader.GetInt32(0),
        reader.GetString(1),
        reader.GetInt32(2),
        reader.GetString(3),
        reader.GetDateTime(4));
}
