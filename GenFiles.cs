using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GenerateRepository
{
    public class GenFiles
    {
        private readonly string path;
        private readonly object objeto;
        private Dictionary<string, string> nomesDasPropriedades = new Dictionary<string, string>();
        private string nameClass = string.Empty;
        private string nameNamespace = string.Empty;

        private Stack<string> listClass = new Stack<string>();
        public GenFiles(string nameClass, string path="./", string folderModels="./Models/")
        {
            this.objeto = objeto;
            this.path = path;
            this.nameClass = nameClass;
            DefinirClasses(nameClass,folderModels);
        }
        public void GenAllFiles()
        {
            Console.WriteLine("Deseja gerar Controller? (S/N)");
            string resp = Console.ReadLine();
            if (resp.ToUpper() == "S")
            {
                this.GenFileController();
            }
            Console.WriteLine("Deseja gerar Repositories? (S/N)");
            resp = Console.ReadLine();
            if (resp.ToUpper() == "S")
            {
                this.GenFileRepositories();
            }

        }

        private void DefinirClasses(string Classe, string caminho = "./Models/")
        {
            var file = caminho + Classe + ".cs";

            if (!File.Exists(file))
            {
                Console.WriteLine(file + " não existe");
                throw new Exception("Arquivo não encontrado...");
            }
            var lines = File.ReadLines(file).ToList();

            for (int i = 0; i < lines.Count(); i++)
            {
                var linha = lines[i];
                var arrBase = linha.Split(":");
                if (arrBase.Length > 1)
                {
                    DefinirClasses(arrBase[1]);
                }
            }
            listClass.Push(file);
        }

        private string GetCampos()
        {
            var list = new List<string>();
            var str = new StringBuilder();
            foreach (var item in listClass)
            {
                var lines = File.ReadLines(item).ToList();
                for (var i = 0; i < lines.Count; i++)
                {
                    if (lines[i].Contains("get;"))
                    {
                        list.Add(GetName(lines[i]));
                    }
                }
            }

            foreach (var item in list)
            {
                str.AppendLine("c." + item + "=m." + item + ";");
            }

            return str.ToString();
        }

        private string GetName(string value)
        {
            var arr = value.Trim().Split(" ");
            return arr[2].Replace("?", "");
        }
        private string GetControllers()
        {
            var text = this.GetModeloControllers();
            return text.Replace("@@Class", nameClass).Replace("@@namespace", nameNamespace);
        }
        private string GetRepositories()
        {
            var text = this.GetModeloRepositories();
            return text.Replace("@@Class", nameClass).Replace("@@namespace", nameNamespace).Replace("@@SetsUpdate", this.GetCampos());
        }

        private void GenFileController()
        {
            var caminho = path + "Controllers/" + nameClass + "Controller.cs";
            if (!File.Exists(caminho))
            {
                File.WriteAllText(caminho, this.GetControllers());
            }
            else
            {
                Console.WriteLine("Não foi possível gerar: " + caminho + ". Já existe.");
            }
        }

        private void GenFileRepositories()
        {
            var caminho = path + "Repositories/" + nameClass + "Repositories.cs";
            if (!File.Exists(caminho))
            {
                File.WriteAllText(caminho, this.GetRepositories());
            }
            else
            {
                Console.WriteLine("Não foi possível gerar: " + caminho + ". Já existe.");
            }
        }
        private string GetModeloControllers()
        {

            string codigoCSharp = @"
                            using System;
                            using System.Collections.Generic;
                            using System.Linq;
                            using System.Threading.Tasks;
                            using @@namespace.Repositories;
                            using Microsoft.AspNetCore.Mvc;

                            namespace @@namespace.Controllers
                            {
                                [ApiController]
                                [Route(""api/[controller]"")]
                                public class @@ClassController : ControllerBase
                                {
                                    private readonly IRepositories<@@Class> _repositore;
                                    public @@ClassController(IRepositories<@@Class> repositore)
                                    {
                                        _repositore = repositore;
                                    }

                                    [HttpGet(""Get/{where?}"")]
                                    public async Task<ActionResult<IEnumerable<@@Class>>> Get(string where = """")
                                    {
                                        try
                                        {
                                            return Ok(await _repositore.Get(where));
                                        }
                                        catch (Exception e)
                                        {
                                            return BadRequest(new { erro = e.Message });
                                        }
                                    }

                                    [HttpGet(""GetAll"")]
                                    public async Task<ActionResult<IEnumerable<@@Class>>> GetAll()
                                    {
                                        try
                                        {
                                            return Ok(await _repositore.Get(""""));
                                        }
                                        catch (Exception e)
                                        {
                                            return BadRequest(new { erro = e.Message });
                                        }
                                    }

                                    [HttpPost]
                                    public async Task<ActionResult<@@Class>> Add(@@Class m)
                                    {
                                        try
                                        {
                                            if (m is null)
                                                throw new Exception(""@@Class was not given"");
                                            return  Ok(await _repositore.Add(m));
                                        }
                                        catch (Exception e)
                                        {
                                            return BadRequest(new { erro = e.Message });
                                        }
                                    }

                                    [HttpPut(""{id}"")]
                                    public async Task<ActionResult<@@Class>> Update([FromBody] @@Class m, int id)
                                    {
                                        try
                                        {
                                            if (m is null)
                                                throw new Exception(""@@Class was not given"");
                                            return Ok(await _repositore.Update(m));
                                        }
                                        catch (Exception e)
                                        {
                                            return BadRequest(new { erro = e.Message });
                                        }
                                    }

                                    [HttpDelete(""{id}"")]
                                    public async Task<ActionResult<@@Class>> Delete(int id)
                                    {
                                        try
                                        {
                                            if (m is null)
                                                throw new Exception(""@@Class was not given"");
                                            return Ok(await _repositore.Delete(id));
                                        }
                                        catch (Exception e)
                                        {
                                            return BadRequest(new { erro = e.Message });
                                        }
                                    }

                                    [HttpGet(""GetById/{id}"")]
                                    public async Task<ActionResult<@@Class>> GetById(int id)
                                    {
                                        try
                                        {
                                            return Ok(await _repositore.GetById(id));
                                        }
                                        catch (Exception e)
                                        {
                                            return BadRequest(new { erro = e.Message });
                                        }
                                    }
                                }
                            }
                            ";

            return codigoCSharp;
        }

        private string GetModeloRepositories()
        {
            return @"
                    using Microsoft.EntityFrameworkCore;
                    using Models.Models;


                    namespace @@namespace.Repositories
                    {
                        public class @@ClassRepositories : IRepositories<@@Class>
                        {
                            private readonly Db _db;
                            public @@ClassRepositories(Db db)
                            {
                                _db = db;
                            }

                            public async Task<@@Class> Add(@@Class m)
                            {
                                try
                                {
                                    m.Id = await NewId();

                                    this.Validate(m);

                                    await _db.ContextDb.@@Class.AddAsync(m);

                                    await _db.ContextDb.SaveChangesAsync();

                                    return m;
                                }
                                catch (Exception)
                                {

                                    throw;
                                }
                            }


                            public async Task<IEnumerable<@@Class>> Get(string param)
                            {
                                try
                                {
                                    var list = await _db.ContextDb.@@Class.Where(x => x.Nome.Contains(param)).ToListAsync();
                                    return list;
                                }
                                catch (Exception)
                                {
                                    throw;
                                }
                            }

                            public async Task<@@Class> GetById(int id)
                            {
                                try
                                {
                                    var model = await _db.ContextDb.@@Class.FirstOrDefaultAsync(x => x.Id == id);

                                    if (model is null)
                                        throw new Exception(""@@Class Not found"");

                                    return model;
                                }
                                catch (Exception)
                                {
                                    throw;
                                }

                            }

                            public Task<@@Class> GetById(string id)
                            {
                                throw new NotImplementedException();
                            }

                            public async Task<IEnumerable<dynamic>> GetGeneric(string sql)
                            {
                                try
                                {
                                    var result = await _db.Dapper.GetGeneric(sql);
                                    return result;
                                }
                                catch (Exception)
                                {
                                    throw new Exception(""Erro ao realizar consulta"");
                                }
                            }
                            public async Task<bool> Delete(int id)
                            {
                                try
                                {
                                    var modelo = await _db.ContextDb.@@Class.FirstOrDefaultAsync(x => x.Id == id);

                                    if (modelo == null)
                                        throw new Exception(""@@Class Not found"");

                                    _db.ContextDb.@@Class.Remove(modelo);

                                    await _db.ContextDb.SaveChangesAsync();

                                    return true;
                                }
                                catch (Exception)
                                {
                                    throw;
                                }
                            }
                            public async Task<@@Class> Update(@@Class m)
                            {
                                try
                                {
                                    var c = await _db.ContextDb.@@Class.FirstOrDefaultAsync(x => x.Id == m.Id);

                                    if (c == null)
                                        throw new Exception(""@@Class Not found"");

                            @@SetsUpdate

                                    this.Validate(c);

                                    await _db.ContextDb.SaveChangesAsync();

                                    return c;
                                }
                                catch (Exception)
                                {
                                    throw;
                                }
                            }

                            public void Validate(@@Class m)
                            {
                                if (string.IsNullOrEmpty(m.Id))
                                    throw new Exception(""Exception"");
                            }

                            public async Task<int> NewId()
                            {
                                try
                                {
                                    return await _db.ContextDb.@@Class.MaxAsync(x => x.Id) + 1;
                                }
                                catch
                                {
                                    return 1;
                                }
                            }
                        }
                    }

            ";
        }

    }
}