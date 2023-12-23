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
        public GenFiles(object objeto, string path)
        {
            this.objeto = objeto;
            this.path = path;

            DefinirTipoNomePropriedades(objeto);
        }

        private void DefinirTipoNomePropriedades(object instancia)
        {
            try
            {
                Type tipo = instancia.GetType();

                nameClass = tipo.Name;

                Assembly assembly = Assembly.GetExecutingAssembly();

                nameNamespace = assembly.GetName().Name;

                PropertyInfo[] propriedades = tipo.GetProperties(BindingFlags.Instance | BindingFlags.Public);

                Dictionary<Type, string> tipoAlias = new Dictionary<Type, string>
        {
            { typeof(int), "int" },
            { typeof(string), "string" },
            { typeof(double), "double" },
            { typeof(Decimal), "decimal" },
            { typeof(bool),"bool" }
        };
                for (int i = 0; i < propriedades.Length; i++)
                {
                    Type propriedadeTipo = propriedades[i].PropertyType;
                    string tipoAliasString = propriedadeTipo.Name; // Use o nome como padrão

                    if (tipoAlias.ContainsKey(propriedadeTipo))
                    {
                        tipoAliasString = tipoAlias[propriedadeTipo];
                    }

                    nomesDasPropriedades.Add(propriedades[i].Name, tipoAliasString);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private string GetCampos()
        {
            var str = new StringBuilder();
            foreach (var item in nomesDasPropriedades)
            {
                str.AppendLine("c." + item.Key + "=m." + item.Key + ";");
            }
            return str.ToString();
        }

        public void GenAllFiles(){
            Console.WriteLine("Deseja gerar Controller? (S/N)");
            string resp = Console.ReadLine();
            if(resp.ToUpper()=="S"){
                this.GenFileController();
            }
            Console.WriteLine("Deseja gerar Repositories? (S/N)");
            resp = Console.ReadLine();
            if(resp.ToUpper()=="S"){
                this.GenFileRepositories();
            }

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