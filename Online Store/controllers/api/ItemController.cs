using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Models;
using Dapper;
using System.Diagnostics;
using Azure.Storage.Blobs;
using Microsoft.EntityFrameworkCore;

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
            IEnumerable<Items> items = sqlConnection.Query<Items>("exec dbo.os_sp_getItems;");
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

        [HttpPost("[Action]")]
        public void AddItem([FromForm] Binders.Item item)
        {
            SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("SQL"));
            if(sqlConnection.Execute("select * from [Items] where name = @name AND supplier = @supplier", item) == 1)
            {
                Response.Redirect("/Admin/ItemManagement?success=false&reason=exists");
            }

            String imagesString = "";
            for (int i = 0; i < item.images.Length; i++)
            {
                Debug.WriteLine(item.images[i]);

                IFormFile file = item.images[i];
                BlobContainerClient container = new BlobContainerClient("DefaultEndpointsProtocol=https;AccountName=archieonlinestore;AccountKey=6+RID6LqA4+vqY9iG4kp+tmNQTVuHRYlz/QKkByWXMNxSLsecJvpf33rf2JmgfGiYzqkzMUJvS7E+AStv/qNNQ==;EndpointSuffix=core.windows.net", "images");
                BlobClient client = container.GetBlobClient(item.name + "_" + file.FileName);
                client.Upload(file.OpenReadStream(), true, default);

                imagesString += (item.name + "_" + file.FileName) + ";";
            }

            try
            {
                sqlConnection.Execute("exec dbo.os_sp_addItem @name, '" + imagesString + "', @price, @quantity, @supplier, @width, @height, @category, @color, @description, @notes;", new
                {
                    @name = item.name,
                    @description = item.description,
                    @price = item.Price,
                    @quantity = item.Quantity,
                    @supplier = item.supplier,
                    @width = item.width,
                    @height = item.height,
                    @category = item.category,
                    @color = item.color,
                    @notes = item.notes,
                });
            }catch(SqlException e)
            {
               Response.Redirect("/Admin/ItemManagement?success=false");
            }
            finally
            {
                sqlConnection.Close();
            }

            Response.Redirect("/Admin/ItemManagement?success=true");
        }
    }
}
