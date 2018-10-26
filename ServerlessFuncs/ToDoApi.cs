
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace ServerlessFuncs
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Build.Framework;

    public static class ToDoApi
    {
        private static List<Todo> _items = new List<Todo>();

        [FunctionName("CreateTodo")]
        public static async Task<IActionResult> CreateToDo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "todo")]
            HttpRequest req, TraceWriter log)
        {
            log.Info("creating a new todo list item");
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var input = JsonConvert.DeserializeObject<TodoCreateModel>(requestBody);

            var todo = new Todo {TaskDescription = input.TaskDescription};
            _items.Add(todo);

            return new OkObjectResult(todo);
        }

        [FunctionName("GetTodos")]
        public static IActionResult GetTodos([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todo")]
            HttpRequest req, TraceWriter log)
        {
            log.Info("Getting todo list items");
            return new OkObjectResult(_items);
        }

        [FunctionName("GetTodoById")]
        public static IActionResult GetTodoById([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todo/{id}")]
            HttpRequest req, TraceWriter log, string id)
        {
            log.Info($"Getting todo item with id - {id}");
            var todo = _items.FirstOrDefault(t => t.Id == id);
            if (todo == null)
            {
                return new NotFoundResult();
            }

            return new OkObjectResult(todo);
        }

        [FunctionName("UpdateTodo")]
        public static async Task<IActionResult> UpdateTodo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "todo/{id}")]
            HttpRequest req, TraceWriter log, string id)
        {
            log.Info($"Updating todo with id - {id}");

            var todo = GetToDo(id);
            if (todo == null)
            {
                return new NotFoundResult();
            }

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var updated = JsonConvert.DeserializeObject<TodoUpdateModel>(requestBody);

            todo.IsCompleted = updated.IsCompleted;
            if (!string.IsNullOrEmpty(updated.TaskDescription))
            {
                todo.TaskDescription = updated.TaskDescription;
            }

            return new OkObjectResult(todo);
        }

        private static Todo GetToDo(string id)
        {
            return _items.FirstOrDefault(t => t.Id == id);
        }

        [FunctionName("DeleteTodo")]
        public static async Task<IActionResult> DeleteTodo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "todo/{id}")]
            HttpRequest req, TraceWriter log, string id)
        {
            log.Info($"Deleting todo with id - {id}");

            var todo = GetToDo(id);
            if (todo == null)
            {
                return new NotFoundResult();
            }

            _items.Remove(todo);
            return new OkResult();
        }
    }
}