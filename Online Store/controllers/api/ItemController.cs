using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Models;
using Dapper;

namespace Online_Store.controllers.api
{
    [Route("/[controller]")]
    [ApiController]
    public class ItemController : ControllerBase
    {
        private IConfiguration _configuration;
        public ItemController(IConfiguration configuration) //allows us to get connection string
        {
            _configuration = configuration;
        }

        [HttpGet("[Action]")]
        public IEnumerable<Items> GetItems()
        {
            SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("SQL"));
            sqlConnection.Open();
            IEnumerable<Items> items = sqlConnection.Query<Items>("select * from [Items]");
            sqlConnection.Close();
            return items;
        }

        [HttpDelete("/Admin/[Controller]/[Action]/{id}")]
        public void DeleteItem(int id)
        {
            SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("SQL"));
            sqlConnection.Open();
            sqlConnection.Execute("delete from [Items] where id = @id", new {@id = id});
            sqlConnection.Close();
        }

        [HttpGet("[Action]")]
        public IEnumerable<Category> GetCategories()
        {
            SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("SQL"));
            sqlConnection.Open();
            IEnumerable<Category> categories = sqlConnection.Query<Category>("select * from [Categories]");
            sqlConnection.Close();
            return categories;
        }
    }
}
