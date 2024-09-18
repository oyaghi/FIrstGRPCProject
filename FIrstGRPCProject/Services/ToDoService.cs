using FIrstGRPCProject.Data;
using FIrstGRPCProject.Models;
using Grpc.Core;
using Microsoft.AspNetCore.Authentication.OAuth.Claims;
using Microsoft.EntityFrameworkCore;

namespace FIrstGRPCProject.Services
{
    public class ToDoService: TodoIt.TodoItBase
    {
        private readonly ApplicationDbContext _context;
        public ToDoService(ApplicationDbContext appContext)
        {
            _context = appContext;
        }

        public override async Task<CreateToDoResponse> CreateToDo(CreateToDoRequest request, ServerCallContext context)
        {
            if (request.Title == string.Empty || request.Description == string.Empty)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "You must fill the all the fields"));
            }
            var toDoItem = new ToDoItem
            {
                Title = request.Title,
                Description = request.Description,
            };
            await _context.AddAsync(toDoItem);
            await _context.SaveChangesAsync();
            return await Task.FromResult(new CreateToDoResponse
            {
                Id = toDoItem.Id
            });
        }

        public override async Task<ReadToDoResponse> ReadToDo(ReadToDoRequest request, ServerCallContext context)
        {
            if (request.Id <= 0)
                throw new RpcException(new Status(StatusCode.InvalidArgument, "resouce index must be greater than 0"));

            var toDoItem = await _context.ToDoItems.SingleOrDefaultAsync(t => t.Id == request.Id);

            if (toDoItem != null)
            {
                return await Task.FromResult(new ReadToDoResponse
                {
                    Id = toDoItem.Id,
                    Title = toDoItem.Title,
                    Description = toDoItem.Description,
                    ToDoStatus = toDoItem.ToDoStatus
                });
            }

            throw new RpcException(new Status(StatusCode.NotFound, $"No Task with id {request.Id}"));
        }


        public override async Task<GetAllResponse> ReadListToDo(GetAllRequest request, ServerCallContext context)
        {
            var response = new GetAllResponse();
            var toDoItems = await _context.ToDoItems.ToListAsync();

            foreach (var toDo in toDoItems)
            {
                response.ToDo.Add(new ReadToDoResponse
                {
                    Id = toDo.Id,
                    Title = toDo.Title,
                    Description = toDo.Description,
                    ToDoStatus = toDo.ToDoStatus
                });
            }

            return await Task.FromResult(response);
        }

        public override async Task<UpdateToDoResponse> UpdateToDo(UpdateToDoRequest request, ServerCallContext context)
        {
            if (request.Id <= 0 || request.Title == string.Empty || request.Description == string.Empty)
                throw new RpcException(new Status(StatusCode.InvalidArgument, "You must suppply a valid object"));

            var toDoItem = await _context.ToDoItems.FirstOrDefaultAsync(t => t.Id == request.Id);

            if (toDoItem == null)
                throw new RpcException(new Status(StatusCode.NotFound, $"No Task with Id {request.Id}"));

            toDoItem.Title = request.Title;
            toDoItem.Description = request.Description;
            toDoItem.ToDoStatus = request.ToDoStatus;

            await _context.SaveChangesAsync();

            return await Task.FromResult(new UpdateToDoResponse
            {
                Id = toDoItem.Id
            });
        }

        public override async Task<DeleteToDoResponse> DeleteToDo(DeleteToDoRequest request, ServerCallContext context)
        {
            if (request.Id <= 0)
                throw new RpcException(new Status(StatusCode.InvalidArgument, "resouce index must be greater than 0"));

            var toDoItem = await _context.ToDoItems.FirstOrDefaultAsync(t => t.Id == request.Id);

            if (toDoItem == null)
                throw new RpcException(new Status(StatusCode.NotFound, $"No Task with Id {request.Id}"));

            _context.Remove(toDoItem);

            await _context.SaveChangesAsync();

            return await Task.FromResult(new DeleteToDoResponse
            {
                Id = toDoItem.Id
            });

        }
    }
}
