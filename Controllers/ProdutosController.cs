using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using api_rest.Models;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace api_rest.Data
{
    [Route("api/v1/[controller]")] 
    [ApiController]
    [Authorize(Roles = "Gerente ")]
    public class ProdutosController : ControllerBase
    {
        private readonly ApplicationDbContext database;

        public ProdutosController(ApplicationDbContext database){
            this.database = database;
        }

        [HttpGet("claims-teste")]
        public ActionResult<IEnumerable<string>> TesteClaims()
        {
            return Ok(HttpContext.User.Claims.First(claim => claim.Type.ToString().Equals("id", StringComparison.InvariantCultureIgnoreCase)).Value);            
        }

        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            try{
                var produtos = database.Produtos.ToList();
                return Ok(produtos);
            }catch(Exception e){
                Response.StatusCode = 404;
                return new ObjectResult("");
            }
            
        }

        [HttpPost]
        public IActionResult Post([FromBody] ProdutoTemp pTemp){
            
            /* Validação */
            if(pTemp.Preco <= 0){
                Response.StatusCode = 400;
                return new ObjectResult(new {msg = "O preço do produto não pode ser menor ou igual a 0."});
            }

            if(pTemp.Nome.Length <= 1){
                Response.StatusCode = 400;
                return new ObjectResult(new {msg = "O nome do produto precisa ter mais de um caracter."});
            }

            Produto p = new Produto();
            
            p.Nome = pTemp.Nome;
            p.Preco = pTemp.Preco;

            database.Produtos.Add(p);
            database.SaveChanges();

            Response.StatusCode = 201;
            return new ObjectResult("");
        }

        
        [HttpGet("{id}")]
        public ActionResult Get(int id)
        {
            var produto = database.Produtos.First(p => p.Id == id);
            return Ok(produto);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            try{
                Produto produto = database.Produtos.First(p => p.Id == id);
                database.Produtos.Remove(produto);
                database.SaveChanges();
                return Ok();
            }catch(Exception e){
                Response.StatusCode = 404;
                return new ObjectResult("");
            }
        }
        

        [HttpPatch]
        public IActionResult Patch([FromBody] Produto produto){

            if(produto.Id > 0){
                
                try{
                    var p = database.Produtos.First(ptemp => ptemp.Id == produto.Id);

                    if(p != null){
                        p.Nome = produto.Nome != null ? produto.Nome : p.Nome;
                        p.Preco = produto.Preco != 0 ? produto.Preco : p.Preco; 

                        database.SaveChanges();
                        return Ok();

                    }else{
                        Response.StatusCode = 400;
                        return new ObjectResult(new {msg = "Produto não encontrado"});
                    }

                }catch{
                    Response.StatusCode = 400;
                    return new ObjectResult(new {msg = "Produto não encontrado"});
                }
                
            }else{
                Response.StatusCode = 400;
                return new ObjectResult(new {msg = "O Id do produto é inválido!"});
            }
        }

         public class ProdutoTemp{
            public string Nome {get; set;}
            public float Preco {get; set;}
        }
    }
}