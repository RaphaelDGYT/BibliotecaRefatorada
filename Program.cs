namespace BibliotecaRefatorada
{
    // Entidades
    public class Livro
    {
        public required string Titulo { get; set; }
        public required string Autor { get; set; }
        public required string ISBN { get; set; }
        public bool Disponivel { get; set; } = true;
    }

    public class Usuario
    {
        public required string Nome { get; set; }
        public int ID { get; set; }
    }

    public class Emprestimo
    {
        public required Livro Livro { get; set; }
        public required Usuario Usuario { get; set; }
        public DateTime DataEmprestimo { get; set; }
        public DateTime DataDevolucaoPrevista { get; set; }
        public DateTime? DataDevolucaoEfetiva { get; set; }
    }

    // Interfaces
    public interface INotificador
    {
        void Notificar(string destinatario, string assunto, string mensagem);
    }

    public interface IUsuarioRepository
    {
        void Adicionar(Usuario usuario);
        Usuario BuscarPorId(int id);
        List<Usuario> BuscarTodos();
    }

    public interface ILivroRepository
    {
        void Adicionar(Livro livro);
        Livro BuscarPorISBN(string isbn);
        List<Livro> BuscarTodos();
    }

    public interface IEmprestimoRepository
    {
        void Adicionar(Emprestimo emprestimo);
        Emprestimo BuscarEmprestimoAtivo(string isbn, int usuarioId);
        List<Emprestimo> BuscarTodos();
    }

    // Implementações Simples
    public class EmailNotificador : INotificador
    {
        public void Notificar(string destinatario, string assunto, string mensagem)
        {
            Console.WriteLine($"[Email] Para: {destinatario} | Assunto: {assunto} | Msg: {mensagem}");
        }
    }

    public class SMSNotificador : INotificador
    {
        public void Notificar(string destinatario, string assunto, string mensagem)
        {
            Console.WriteLine($"[SMS] Para: {destinatario} | Msg: {mensagem}");
        }
    }

    // Repositórios Simples (Memória)
    public class LivroRepository : ILivroRepository
    {
        private List<Livro> livros = new();
        public void Adicionar(Livro livro)
        {
            livros.Add(livro);
        }

        public Livro BuscarPorISBN(string isbn)
        {
#pragma warning disable CS8603 // Possível retorno de referência nula.
            return livros.FirstOrDefault(l => l.ISBN == isbn);
#pragma warning restore CS8603 // Possível retorno de referência nula.
        }

        public List<Livro> BuscarTodos()
        {
            return livros;
        }
    }

    public class UsuarioRepository : IUsuarioRepository
    {
        private List<Usuario> usuarios = new();
        public void Adicionar(Usuario usuario)
        {
            usuarios.Add(usuario);
        }

        public Usuario BuscarPorId(int id)
        {
#pragma warning disable CS8603 // Possível retorno de referência nula.
            return usuarios.FirstOrDefault(u => u.ID == id);
#pragma warning restore CS8603 // Possível retorno de referência nula.
        }

        public List<Usuario> BuscarTodos()
        {
            return usuarios;
        }
    }

    public class EmprestimoRepository : IEmprestimoRepository
    {
        private List<Emprestimo> emprestimos = new();
        public void Adicionar(Emprestimo e)
        {
            emprestimos.Add(e);
        }

        public Emprestimo BuscarEmprestimoAtivo(string isbn, int usuarioId)
        {
#pragma warning disable CS8603 // Possível retorno de referência nula.
            return emprestimos.FirstOrDefault(e =>
                e.Livro.ISBN == isbn && e.Usuario.ID == usuarioId && e.DataDevolucaoEfetiva == null);
#pragma warning restore CS8603 // Possível retorno de referência nula.
        }

        public List<Emprestimo> BuscarTodos()
        {
            return emprestimos;
        }
    }

    // Serviço de multa
    public class MultaCalculator
    {
        public double CalcularMulta(Emprestimo emprestimo)
        {
            if (emprestimo.DataDevolucaoEfetiva > emprestimo.DataDevolucaoPrevista)
            {
                var diasAtraso = (emprestimo.DataDevolucaoEfetiva.Value - emprestimo.DataDevolucaoPrevista).Days;
                return diasAtraso * 1.0;
            }
            return 0;
        }
    }

    // Serviço principal
    public class BibliotecaService
    {
        private readonly ILivroRepository livroRepo;
        private readonly IUsuarioRepository usuarioRepo;
        private readonly IEmprestimoRepository emprestimoRepo;
        private readonly List<INotificador> notificadores;
        private readonly MultaCalculator multaCalc = new();

        public BibliotecaService(
            ILivroRepository livroRepo,
            IUsuarioRepository usuarioRepo,
            IEmprestimoRepository emprestimoRepo,
            List<INotificador> notificadores)
        {
            this.livroRepo = livroRepo;
            this.usuarioRepo = usuarioRepo;
            this.emprestimoRepo = emprestimoRepo;
            this.notificadores = notificadores;
        }

        public void CadastrarUsuario(string nome, int id)
        {
            var usuario = new Usuario { Nome = nome, ID = id };
            usuarioRepo.Adicionar(usuario);
            NotificarTodos(usuario.Nome, "Bem-vindo!", "Você foi cadastrado com sucesso.");
        }

        public void CadastrarLivro(string titulo, string autor, string isbn)
        {
            livroRepo.Adicionar(new Livro { Titulo = titulo, Autor = autor, ISBN = isbn });
        }

        public bool RealizarEmprestimo(int usuarioId, string isbn, int dias)
        {
            var usuario = usuarioRepo.BuscarPorId(usuarioId);
            var livro = livroRepo.BuscarPorISBN(isbn);

            if (livro == null || usuario == null || !livro.Disponivel) return false;

            livro.Disponivel = false;

            var emprestimo = new Emprestimo
            {
                Livro = livro,
                Usuario = usuario,
                DataEmprestimo = DateTime.Now,
                DataDevolucaoPrevista = DateTime.Now.AddDays(dias)
            };

            emprestimoRepo.Adicionar(emprestimo);
            NotificarTodos(usuario.Nome, "Empréstimo Realizado", $"Livro: {livro.Titulo}");

            return true;
        }

        public double RealizarDevolucao(string isbn, int usuarioId)
        {
            var emprestimo = emprestimoRepo.BuscarEmprestimoAtivo(isbn, usuarioId);
            if (emprestimo == null) return -1;

            emprestimo.DataDevolucaoEfetiva = DateTime.Now;
            emprestimo.Livro.Disponivel = true;

            var multa = multaCalc.CalcularMulta(emprestimo);

            if (multa > 0)
            {
                NotificarTodos(emprestimo.Usuario.Nome, "Multa por Atraso", $"Valor da multa: R$ {multa}");
            }

            return multa;
        }

        private void NotificarTodos(string destinatario, string assunto, string mensagem)
        {
            foreach (var notificador in notificadores)
            {
                notificador.Notificar(destinatario, assunto, mensagem);
            }
        }
    }

    class Program
    {
        static void Main()
        {
            var livroRepo = new LivroRepository();
            var usuarioRepo = new UsuarioRepository();
            var emprestimoRepo = new EmprestimoRepository();
            var notificadores = new List<INotificador> { new EmailNotificador(), new SMSNotificador() };

            var sistema = new BibliotecaService(livroRepo, usuarioRepo, emprestimoRepo, notificadores);

            sistema.CadastrarLivro("Clean Code", "Robert C. Martin", "123");
            sistema.CadastrarUsuario("João", 1);
            sistema.RealizarEmprestimo(1, "123", 7);

            var multa = sistema.RealizarDevolucao("123", 1);
            Console.WriteLine($"Multa: R$ {multa}");
        }
    }
}
