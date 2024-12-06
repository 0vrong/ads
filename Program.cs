using System;
List<Order> repo = [
    new Order(1, new(2020,10,10),"123","123","123","123","в ожидании"),
    new Order(2, new(2020,10,10),"123","123","123","123","в ожидании"),
    new Order(3, new(2023,10,10),"123","123","123","123","в ожидании")
];
var builder = WebApplication.CreateBuilder();
builder.Services.AddCors();
var app = builder.Build();

app.UseCors(o => o.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

string message = "";

app.MapGet("orders", (int param = 0) =>
{
    string buffer = message;
    message = "";
    if (param != 0)
        return new { repo = repo.FindAll(x => x.Number == param), message = buffer };
    return new { repo, message = buffer };
});
app.MapGet("add", ([AsParameters] Order dto) => repo.Add(dto));
app.MapGet("update", ([AsParameters] UpdateOrderDTO dto) =>
{
    var order = repo.Find(x => x.Number == dto.Number);
    if (order == null)
        return;
    if (dto.Status != order.Status && dto.Status != "")
    {
        order.Status = dto.Status;
        message += $"Статус заявки {order.Number} изменен";
        if (order.Status == "выполнено")
        {
            message += $"Заявка {order.Number} завершена";
            order.EndDate = DateOnly.FromDateTime(DateTime.Now);
        }
    }

    if (dto.Description != "")
        order.Description = dto.Description;
    if (dto.Master != "")
        order.Master = dto.Master;
    if (dto.Comment != "")
        order.Comments.Add(dto.Comment);
});

int CompleteCount() => repo.FindAll(x => x.Status == "выполнено").Count;

Dictionary<string, int> GetProblemTypeStat() =>
    repo.GroupBy(x => x.ProblemType)
    .Select(x => (x.Key, x.Count()))
    .ToDictionary(k => k.Key, v => v.Item2);

double GetAverage() =>
    CompleteCount() == 0 ? 0 :
    repo.FindAll(x => x.Status == "выполнено")
    .Select(x => x.EndDate.Value.DayNumber - x.StartDate.DayNumber)
    .Sum() / CompleteCount();

app.MapGet("avg", () =>new
 {
     completeCount = CompleteCount(),
     getproblemtypestat = GetProblemTypeStat(),
     GetAverage = GetAverage()
});

app.Run();


class Order
{
    public Order(int number, DateOnly startDate, string device, string problemType, string description, string client, string status)
    {
        Number = number;
        StartDate = startDate;
        Device = device;
        ProblemType = problemType;
        Description = description;
        Client = client;
        Status = status;
    }

    public int Number { get; set; }
    public DateOnly StartDate { get; set; }
    public string Device { get; set; }
    public string ProblemType { get; set; }
    public string Description { get; set; }
    public string Client { get; set; }
    public string Status { get; set; }
    public DateOnly? EndDate { get; set; } = null;
    public string? Master { get; set; } = "Не назначен";
    public List<string> Comments { get; set; } = [];



}

record class UpdateOrderDTO(
    int Number,
    string? Status = "",
    string? Description = "",
    string? Master = "",
    string? Comment="");